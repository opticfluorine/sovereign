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
using System.Collections.Generic;
using System.Threading;
using Sovereign.EngineUtil.Collections;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Tracks top-level information for all in-memory entities.
/// </summary>
public class EntityTable
{
    private const int InitialPendingBufferSize = 16384;

    /// <summary>
    ///     Empty set of ulongs.
    /// </summary>
    private readonly HashSet<ulong> emptyUlongSet = new();

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
    private readonly StructBuffer<EntityRemove> pendingRemoves = new(InitialPendingBufferSize);

    /// <summary>
    ///     Map from template ID to all loaded instances of that template.
    /// </summary>
    private readonly Dictionary<ulong, HashSet<ulong>> templateInstances = new();

    private readonly Lock templateLockHandle = new();

    /// <summary>
    ///     Set of all currently loaded entity IDs, including template entity IDs.
    /// </summary>
    public IReadOnlySet<ulong> EntityIds => entities;

    /// <summary>
    ///     Next unused template entity ID.
    /// </summary>
    public ulong NextTemplateEntityId { get; private set; } = EntityConstants.FirstTemplateEntityId;

    /// <summary>
    ///     Gets the loaded instances of the given template.
    /// </summary>
    /// <param name="templateId">Template entity ID.</param>
    /// <returns>Known instances, which may be an empty set.</returns>
    public IReadOnlySet<ulong> GetInstancesOfTemplate(ulong templateId)
    {
        return templateInstances.TryGetValue(templateId, out var instances) ? instances : emptyUlongSet;
    }

    /// <summary>
    ///     Marks the given template entity ID as used.
    /// </summary>
    /// <param name="templateEntityId">Template entity ID.</param>
    public void TakeTemplateEntityId(ulong templateEntityId)
    {
        lock (templateLockHandle)
        {
            if (templateEntityId >= NextTemplateEntityId) NextTemplateEntityId = templateEntityId + 1;
        }
    }

    /// <summary>
    ///     Takes the next available entity ID.
    /// </summary>
    /// <returns>Template entity ID.</returns>
    public ulong TakeNextTemplateEntityId()
    {
        lock (templateLockHandle)
        {
            return NextTemplateEntityId++;
        }
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
        var newAdd = new EntityAdd
            { EntityId = entityId, TemplateEntityId = templateEntityId, IsBlock = isBlock, IsLoad = isLoad };
        pendingAdds.Add(ref newAdd);
    }

    /// <summary>
    ///     Enqueues an entity to be removed from the table.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isUnload">If true, this is an unload rather than a remove.</param>
    public void Remove(ulong entityId, bool isUnload)
    {
        if (!Exists(entityId)) return;
        var newRemove = new EntityRemove { EntityId = entityId, IsUnload = isUnload };
        pendingRemoves.Add(ref newRemove);
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
            if (pendingAdd.TemplateEntityId > 0)
            {
                entityTemplates[entityId] = pendingAdd.TemplateEntityId;
                if (!templateInstances.TryGetValue(pendingAdd.TemplateEntityId, out var instances))
                {
                    instances = new HashSet<ulong>();
                    templateInstances[pendingAdd.TemplateEntityId] = instances;
                }

                instances.Add(entityId);
            }

            if (!pendingAdd.IsBlock)
            {
                nonBlockEntities.Add(entityId);
                OnNonBlockEntityAdded?.Invoke(entityId);
            }

            OnEntityAdded?.Invoke(entityId, pendingAdd.IsLoad);
            if (pendingAdd.TemplateEntityId > 0)
                OnTemplateSet?.Invoke(entityId, pendingAdd.TemplateEntityId, pendingAdd.IsLoad);
        }

        // Removals.
        for (var i = 0; i < pendingRemoves.Count; ++i)
        {
            ref var pendingRemove = ref pendingRemoves[i];
            var entityId = pendingRemove.EntityId;
            entities.Remove(entityId);
            if (nonBlockEntities.Contains(entityId))
            {
                nonBlockEntities.Remove(entityId);
                OnNonBlockEntityRemoved?.Invoke(entityId);
            }

            if (entityTemplates.Remove(entityId, out var templateId)) templateInstances[templateId].Remove(entityId);

            OnEntityRemoved?.Invoke(entityId, pendingRemove.IsUnload);
        }

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
        // Remove instance of old template.
        if (entityTemplates.TryGetValue(entityId, out var oldTemplateId))
            templateInstances[oldTemplateId].Remove(entityId);

        // Set new template.
        if (templateEntityId > 0)
        {
            if (!templateInstances.TryGetValue(templateEntityId, out var instances))
            {
                instances = new HashSet<ulong>();
                templateInstances[templateEntityId] = instances;
            }

            entityTemplates[entityId] = templateEntityId;
            instances.Add(entityId);
        }
        else
        {
            entityTemplates.Remove(entityId);
        }

        OnTemplateSet?.Invoke(entityId, templateEntityId, false);
    }

    /// <summary>
    ///     Event invoked when an entity has been added.
    ///     First parameter is entity ID.
    ///     Seocnd parameter is true if this is a load, false if not.
    /// </summary>
    public event Action<ulong, bool>? OnEntityAdded;

    /// <summary>
    ///     Event invoked when an entity has been removed.
    ///     Parameter is entity ID.
    ///     Second parameter is true if this is an unload, false if not.
    /// </summary>
    public event Action<ulong, bool>? OnEntityRemoved;

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
    ///     First parameter is entity ID, second parameter is template entity ID, third is load flag.
    /// </summary>
    public event Action<ulong, ulong, bool>? OnTemplateSet;

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

        /// <summary>
        ///     Flag indicating whether the new entity is loaded rather than created.
        /// </summary>
        public bool IsLoad;
    }

    /// <summary>
    ///     Contains information for an entity to be removed.
    /// </summary>
    private struct EntityRemove
    {
        /// <summary>
        ///     Entity ID.
        /// </summary>
        public ulong EntityId;

        /// <summary>
        ///     Flag indicating whether the entity is being unloaded rather than removed.
        /// </summary>
        public bool IsUnload;
    }
}