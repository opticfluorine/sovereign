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
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.ClientCore.Components.Indexers;

/// <summary>
///     Indexer which identifies block template entities.
/// </summary>
public class BlockTemplateEntityIndexer : BaseComponentIndexer<int>
{
    private readonly HashSet<ulong> blockTemplateEntities = new();
    private bool modified;

    public BlockTemplateEntityIndexer(MaterialComponentCollection materials) : base(materials, materials)
    {
    }

    /// <summary>
    ///     Set of all block template entities currently in memory.
    /// </summary>
    public IReadOnlySet<ulong> BlockTemplateEntities => blockTemplateEntities;

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