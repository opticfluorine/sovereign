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
using System.Numerics;
using Sovereign.EngineCore.World;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Indexes positioned block entities by their world segment.
/// </summary>
public class BlockWorldSegmentIndexer : BaseComponentIndexer<GridPosition>
{
    /// <summary>
    ///     Map from world segment index to the set of non-block entities positioned there.
    /// </summary>
    private readonly Dictionary<GridPosition, HashSet<ulong>> index = new();

    /// <summary>
    ///     Inverse map to index.
    /// </summary>
    private readonly Dictionary<ulong, GridPosition> inverse = new();

    private readonly WorldSegmentResolver resolver;

    public BlockWorldSegmentIndexer(BlockPositionComponentCollection blockPositions, WorldSegmentResolver resolver)
        : base(blockPositions, blockPositions)
    {
        this.resolver = resolver;
    }

    /// <summary>
    ///     Gets the entities in the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Set (possibly empty) of non-block entities in the world segment.</returns>
    /// <remarks>
    ///     This method returns a copy of the set of entities, and ownership of
    ///     the set is transferred to the caller. It is only guaranteed to be
    ///     current at the moment the method was called, and only if the method
    ///     was called during normal processing of a tick (i.e. not during the
    ///     component update step between ticks).
    /// </remarks>
    public HashSet<ulong> GetEntitiesInWorldSegment(GridPosition segmentIndex)
    {
        if (!index.ContainsKey(segmentIndex))
            index[segmentIndex] = new HashSet<ulong>();

        return new HashSet<ulong>(index[segmentIndex]);
    }

    /// <summary>
    ///     Event invoked when an entity changes its world segment.
    ///     The first GridPosition argument is the previous world segment.
    ///     The second GridPosition argument is the new world segment.
    /// </summary>
    public event Action<ulong, GridPosition, GridPosition>? OnChangeWorldSegment;

    protected override void ComponentAddedCallback(ulong entityId, GridPosition componentValue, bool isLoad)
    {
        AddEntity(entityId, componentValue);
    }

    protected override void ComponentModifiedCallback(ulong entityId, GridPosition componentValue)
    {
        var newSegmentIndex = resolver.GetWorldSegmentForPosition((Vector3)componentValue);
        var oldSegmentIndex = inverse[entityId];
        if (newSegmentIndex != oldSegmentIndex)
        {
            RemoveEntity(entityId);
            AddEntity(entityId, componentValue);
            OnChangeWorldSegment?.Invoke(entityId, oldSegmentIndex, newSegmentIndex);
        }
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        RemoveEntity(entityId);
    }

    /// <summary>
    ///     Adds a positioned entity to the index.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="position">Position.</param>
    private void AddEntity(ulong entityId, GridPosition position)
    {
        var segmentIndex = resolver.GetWorldSegmentForPosition((Vector3)position);
        if (!index.ContainsKey(segmentIndex))
            index[segmentIndex] = new HashSet<ulong>();

        index[segmentIndex].Add(entityId);
        inverse[entityId] = segmentIndex;
    }

    private void RemoveEntity(ulong entityId)
    {
        inverse.Remove(entityId, out var segmentIndex);
        index[segmentIndex].Remove(entityId);
    }
}