// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Sovereign.EngineUtil.Collections;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Tracks top-level information for all in-memory entities.
/// </summary>
public class EntityTable
{
    private const int InitialPendingBufferSize = 16384;

    /// <summary>
    ///     Set of all entities that are currently held in memory.
    /// </summary>
    private readonly HashSet<ulong> entities = new();

    /// <summary>
    ///     Map from entity ID to its template entity ID.
    /// </summary>
    private readonly Dictionary<ulong, ulong> entityTemplates = new();

    /// <summary>
    ///     Set of non-block entities that are currently held in memory.
    /// </summary>
    private readonly HashSet<ulong> nonBlockEntities = new();

    /// <summary>
    ///     Set of all entities that are enqueued to be added to the table.
    /// </summary>
    private readonly StructBuffer<EntityAdd> pendingAdds = new(InitialPendingBufferSize);

    /// <summary>
    ///     Set of all entities that are enqueued to be removed from the table.
    /// </summary>
    private readonly ConcurrentBag<ulong> pendingRemoves = new();

    /// <summary>
    ///     Next unused template entity ID.
    /// </summary>
    public ulong NextTemplateEntityId { get; private set; } = EntityConstants.FirstTemplateEntityId;

    /// <summary>
    ///     Marks the given template entity ID as used.
    /// </summary>
    /// <param name="templateEntityId">Template entity ID.</param>
    public void TakeTemplateEntityId(ulong templateEntityId)
    {
        if (templateEntityId >= NextTemplateEntityId) NextTemplateEntityId = templateEntityId + 1;
    }

    /// <summary>
    ///     Checks whether the given entity is current in memory.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if in memory, false otherwise.</returns>
    public bool Exists(ulong entityId)
    {
        return entities.Contains(entityId);
    }

    /// <summary>
    ///     Enqueues an entity to be added to the table.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="templateEntityId">Template entity ID, or 0 for no template.</param>
    /// <param name="isBlock">If true, indicates the entity is a block entity.</param>
    /// <param name="isLoad">If true, treat the entity as loaded rather than newly added.</param>
    public void Add(ulong entityId, ulong templateEntityId, bool isBlock, bool isLoad)
    {
        if (Exists(entityId)) return;
        var newAdd = new EntityAdd { EntityId = entityId, TemplateEntityId = templateEntityId, IsBlock = isBlock };
        pendingAdds.Add(ref newAdd);
        if (!isLoad && templateEntityId > 0) OnTemplateSet?.Invoke(entityId, templateEntityId);
    }

    /// <summary>
    ///     Enqueues an entity to be removed from the table.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void Remove(ulong entityId)
    {
        if (!Exists(entityId)) return;
        pendingRemoves.Add(entityId);
    }

    /// <summary>
    ///     Applies all pending entity table updates.
    /// </summary>
    public void UpdateAllEntities()
    {
        // Additions.
        for (var i = 0; i < pendingAdds.Count; ++i)
        {
            ref var pendingAdd = ref pendingAdds[i];
            var entityId = pendingAdd.EntityId;
            entities.Add(entityId);
            if (pendingAdd.TemplateEntityId > 0) entityTemplates[entityId] = pendingAdd.TemplateEntityId;

            if (!pendingAdd.IsBlock)
            {
                nonBlockEntities.Add(entityId);
                OnNonBlockEntityAdded?.Invoke(entityId);
            }

            OnEntityAdded?.Invoke(entityId);
        }

        // Removals.
        foreach (var entityId in pendingRemoves)
        {
            entities.Remove(entityId);
            if (nonBlockEntities.Contains(entityId))
            {
                nonBlockEntities.Remove(entityId);
                OnNonBlockEntityRemoved?.Invoke(entityId);
            }

            OnEntityRemoved?.Invoke(entityId);
        }

        // Reset pending sets.
        pendingAdds.Clear();
        pendingRemoves.Clear();
    }

    /// <summary>
    ///     Gets the template entity ID, if any, for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="templateEntityId">Template entity ID.</param>
    /// <returns>true if there is a template, false otherwise.</returns>
    public bool TryGetTemplate(ulong entityId, out ulong templateEntityId)
    {
        return entityTemplates.TryGetValue(entityId, out templateEntityId);
    }

    /// <summary>
    ///     Sets or removes the template ID for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="templateEntityId">Template entity ID, or 0 for no template.</param>
    public void SetTemplate(ulong entityId, ulong templateEntityId)
    {
        if (templateEntityId > 0)
            entityTemplates[entityId] = templateEntityId;
        else
            entityTemplates.Remove(entityId);
        OnTemplateSet?.Invoke(entityId, templateEntityId);
    }

    /// <summary>
    ///     Event invoked when an entity has been added.
    ///     Parameter is entity ID.
    /// </summary>
    public event Action<ulong>? OnEntityAdded;

    /// <summary>
    ///     Event invoked when an entity has been removed.
    ///     Parameter is entity ID.
    /// </summary>
    public event Action<ulong>? OnEntityRemoved;

    /// <summary>
    ///     Event invoked when a non-block entity has been added.
    ///     Parameter is entity ID.
    /// </summary>
    public event Action<ulong>? OnNonBlockEntityAdded;

    /// <summary>
    ///     Event invoked when a non-block entity has been removed.
    ///     Parameter is entity ID.
    /// </summary>
    public event Action<ulong>? OnNonBlockEntityRemoved;


    /// <summary>
    ///     Event invoked when a template is set to an entity.
    ///     First parameter is entity ID, second parameter is template entity ID.
    /// </summary>
    public event Action<ulong, ulong>? OnTemplateSet;

    /// <summary>
    ///     Contains information for an entity to be added.
    /// </summary>
    private struct EntityAdd
    {
        /// <summary>
        ///     Entity ID.
        /// </summary>
        public ulong EntityId;

        /// <summary>
        ///     Entity ID of the template entity, if any, or 0 for no template.
        /// </summary>
        public ulong TemplateEntityId;

        /// <summary>
        ///     Flag indicating whether the new entity is a block entity.
        /// </summary>
        public bool IsBlock;
    }
}