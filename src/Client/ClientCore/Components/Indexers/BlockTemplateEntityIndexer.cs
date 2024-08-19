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
using System.Linq;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Components.Indexers;

/// <summary>
///     Indexer which identifies block template entities.
/// </summary>
public class BlockTemplateEntityIndexer : BaseComponentIndexer<int>
{
    private readonly SortedSet<ulong> blockTemplateEntities = new();
    private bool modified;

    public BlockTemplateEntityIndexer(MaterialComponentCollection materials, BlockTemplateEntityFilter filter)
        : base(materials, filter)
    {
    }

    /// <summary>
    ///     Set of all block template entities currently in memory.
    /// </summary>
    public IReadOnlySet<ulong> BlockTemplateEntities => blockTemplateEntities;

    /// <summary>
    ///     Smallest block template entity ID.
    /// </summary>
    public ulong First => blockTemplateEntities.First();

    /// <summary>
    ///     Gets the next larger block template entity ID, if any.
    /// </summary>
    /// <param name="templateEntityId">Current block template entity ID.</param>
    /// <param name="nextTemplateEntityId">Next greater block template entity ID.</param>
    /// <returns>true if a larger block template entity ID was found, false otherwise.</returns>
    public bool TryGetNextLarger(ulong templateEntityId, out ulong nextTemplateEntityId)
    {
        try
        {
            nextTemplateEntityId = blockTemplateEntities
                .GetViewBetween(templateEntityId + 1, EntityConstants.LastTemplateEntityId).First();
            return true;
        }
        catch (Exception)
        {
            nextTemplateEntityId = 0;
            return false;
        }
    }

    /// <summary>
    ///     Gets the next smaller block template entity ID, if any.
    /// </summary>
    /// <param name="templateEntityId">Current block template entity ID.</param>
    /// <param name="nextTemplateEntityId">Next block template entity ID.</param>
    /// <returns>true if a smaller block template entity ID was found, false otherwise.</returns>
    public bool TryGetNextSmaller(ulong templateEntityId, out ulong nextTemplateEntityId)
    {
        try
        {
            nextTemplateEntityId = blockTemplateEntities
                .GetViewBetween(EntityConstants.FirstTemplateEntityId, templateEntityId - 1).Reverse().First();
            return true;
        }
        catch (Exception)
        {
            nextTemplateEntityId = 0;
            return false;
        }
    }

    protected override void ComponentAddedCallback(ulong entityId, int componentValue, bool isLoad)
    {
        blockTemplateEntities.Add(entityId);
        modified = true;
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        blockTemplateEntities.Remove(entityId);
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