// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
using System.Linq;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Components.Indexers;

/// <summary>
///     Filter that selects for item template entities.
/// </summary>
public class ItemTemplateEntityFilter : TemplateEntityComponentFilter<EntityType>
{
    private readonly EntityTypeComponentCollection entityTypes;

    public ItemTemplateEntityFilter(EntityTypeComponentCollection entityTypes)
        : base(entityTypes, entityTypes)
    {
        this.entityTypes = entityTypes;
    }

    protected override bool ShouldAccept(ulong entityId)
    {
        return base.ShouldAccept(entityId) && entityTypes.TryGetValue(entityId, out var entityType) &&
               entityType == EntityType.Item;
    }
}

/// <summary>
///     Indexer for item template entities.
/// </summary>
public class ItemTemplateEntityIndexer : BaseComponentIndexer<EntityType>
{
    private readonly SortedSet<ulong> itemTemplateEntities = new();
    private bool modified;

    public ItemTemplateEntityIndexer(EntityTypeComponentCollection entityTypes, ItemTemplateEntityFilter filter)
        : base(entityTypes, filter)
    {
    }

    /// <summary>
    ///     Set of all item template entities currently in memory.
    /// </summary>
    public IReadOnlySet<ulong> ItemTemplateEntities => itemTemplateEntities;

    /// <summary>
    ///     Smallest item template entity ID.
    /// </summary>
    public ulong First => itemTemplateEntities.First();

    /// <summary>
    ///     Gets the next larger item template entity ID, if any.
    /// </summary>
    /// <param name="templateEntityId">Current item template entity ID.</param>
    /// <param name="nextTemplateEntityId">Next greater item template entity ID.</param>
    /// <returns>true if a larger item template entity ID was found, false otherwise.</returns>
    public bool TryGetNextLarger(ulong templateEntityId, out ulong nextTemplateEntityId)
    {
        try
        {
            nextTemplateEntityId = itemTemplateEntities
                .GetViewBetween(templateEntityId + 1, EntityConstants.LastTemplateEntityId)
                .FirstOrDefault((ulong)0);
            return nextTemplateEntityId > 0;
        }
        catch (Exception)
        {
            nextTemplateEntityId = 0;
            return false;
        }
    }

    /// <summary>
    ///     Gets the next smaller item template entity ID, if any.
    /// </summary>
    /// <param name="templateEntityId">Current item template entity ID.</param>
    /// <param name="nextTemplateEntityId">Next item template entity ID.</param>
    /// <returns>true if a smaller item template entity ID was found, false otherwise.</returns>
    public bool TryGetNextSmaller(ulong templateEntityId, out ulong nextTemplateEntityId)
    {
        if (templateEntityId == EntityConstants.FirstTemplateEntityId)
        {
            nextTemplateEntityId = 0;
            return false;
        }

        nextTemplateEntityId = itemTemplateEntities
            .GetViewBetween(EntityConstants.FirstTemplateEntityId, templateEntityId - 1)
            .Reverse()
            .FirstOrDefault(0UL);
        return nextTemplateEntityId > 0UL;
    }

    protected override void ComponentAddedCallback(ulong entityId, EntityType componentValue, bool isLoad)
    {
        itemTemplateEntities.Add(entityId);
        modified = true;
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        itemTemplateEntities.Remove(entityId);
        modified = true;
    }

    protected override void EndUpdatesCallback()
    {
        if (modified) OnIndexModified?.Invoke();
    }

    /// <summary>
    ///     Event triggered when the block template entity index is modified.
    /// </summary>
    public event Action? OnIndexModified;
}