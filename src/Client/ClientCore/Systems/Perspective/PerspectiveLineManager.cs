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
using Castle.Core.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.World;

namespace Sovereign.ClientCore.Systems.Perspective;

/// <summary>
///     Manages the perspective lines used in rendering.
/// </summary>
/// <remarks>
///     The perspective lines are the set of lines which are arranged in a lattice on the
///     block positions at z=0 and are oriented such that when the 3/4 perspective transformation
///     is applied, the lines are perpendicular to the plane of the screen. Every block position
///     is intersected by exactly one perspective line.
///     Entities may be arranged on these lines and ordered by z-depth to support algorithms such
///     as mouse pointer target determination, obscuring entity elimination, and rendering
///     order determinations. This class monitors the set of known positioned entities and
///     assigns them to any perspective lines which they overlap, and provides access to
///     the perspective lines for algorithm use.
/// </remarks>
public class PerspectiveLineManager
{
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly KinematicComponentCollection kinematics;

    /// <summary>
    ///     Map from entity ID to set of overlapping perspective line indices.
    /// </summary>
    private readonly Dictionary<ulong, HashSet<Tuple<int, int>>> linesByEntity = new();

    /// <summary>
    ///     Active perspective lines indexed by their z-intercept (x, y) coordinates.
    /// </summary>
    private readonly Dictionary<Tuple<int, int>, PerspectiveLine> perspectiveLines = new();

    private readonly WorldSegmentResolver resolver;

    public PerspectiveLineManager(KinematicComponentCollection kinematics,
        BlockPositionComponentCollection blockPositions, WorldSegmentResolver resolver,
        EntityTable entityTable)
    {
        this.kinematics = kinematics;
        this.blockPositions = blockPositions;
        this.resolver = resolver;

        entityTable.OnEntityAdded += AddEntity;
        entityTable.OnEntityRemoved += RemoveEntity;
        kinematics.OnComponentModified += (entityId, k) => NonBlockEntityMoved(entityId, k.Position);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Called when the client is subscribed to a world segment. This activates any perspective
    ///     lines which intercept the newly subscribed world segment.
    /// </summary>
    /// <param name="segmentIndex">Subscribed world segment index.</param>
    public void OnWorldSegmentSubscribe(GridPosition segmentIndex)
    {
        var indices = GetIndicesForWorldSegment(segmentIndex);
        foreach (var index in indices)
        {
            if (!perspectiveLines.TryGetValue(index, out var line))
            {
                line = new PerspectiveLine();
                perspectiveLines[index] = line;
            }

            line.refCount++;
        }
    }

    /// <summary>
    ///     Called when the client is unsubscribed from a world segment. This deactivates any
    ///     perspective lines which no longer intersect any subscribed world segments.
    /// </summary>
    /// <param name="segmentIndex">Unsubscribed world segment index.</param>
    public void OnWorldSegmentUnsubscribe(GridPosition segmentIndex)
    {
        var indices = GetIndicesForWorldSegment(segmentIndex);
        foreach (var index in indices)
        {
            if (!perspectiveLines.TryGetValue(index, out var line))
            {
                Logger.WarnFormat("Perspective line {0} not found for world segment unsubscribe.", index);
                continue;
            }

            line.refCount--;
            if (line.refCount <= 0) perspectiveLines.Remove(index);
        }
    }

    /// <summary>
    ///     Called when a new entity is added.
    /// </summary>
    /// <param name="entityId"></param>
    private void AddEntity(ulong entityId)
    {
        if (blockPositions.HasComponentForEntity(entityId)) AddBlockEntity(entityId, blockPositions[entityId]);
        else if (kinematics.HasComponentForEntity(entityId)) AddNonBlockEntity(entityId, kinematics[entityId].Position);
    }

    /// <summary>
    ///     Adds a block entity to its perspective line.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="blockPosition">Block position.</param>
    private void AddBlockEntity(ulong entityId, GridPosition blockPosition)
    {
        // Block entities only overlap a single perspective line since they are always positioned
        // in the same block position lattice as the lines.
        var index = GetIndexForBlockPosition(blockPosition);
        AddEntityToLine(entityId, index, blockPosition.Z);
    }

    /// <summary>
    ///     Adds a non-block entity to any overlapping perspective line.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="position">Position.</param>
    private void AddNonBlockEntity(ulong entityId, Vector3 position)
    {
        // Non-block entities can overlap multiple perspective lines depending on their extent.
        // Entity extents aren't scheduled for inclusion until v0.5.0 however, so for the time
        // being they have point positions and will only intersect a single perspective line.
        var index = GetIndexForPosition(position);
        AddEntityToLine(entityId, index, position.Z);
    }

    /// <summary>
    ///     Adds the given entity to a perspective line.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="lineIndex">Perspective line index.</param>
    /// <param name="z">z position of entity.</param>
    private void AddEntityToLine(ulong entityId, Tuple<int, int> lineIndex, float z)
    {
        if (!perspectiveLines.TryGetValue(lineIndex, out var line))
        {
            // This shouldn't normally happen, but just in case, allocate a new line if one
            // doesn't already exist. The reference count will be picked up when the subscribe
            // event hits.
            line = new PerspectiveLine();
            perspectiveLines[lineIndex] = line;
        }

        line.EntitiesByZ.Add(z, entityId);

        // Keep a record of where this block is for easier removal later.
        if (!linesByEntity.TryGetValue(entityId, out var lineIndices))
        {
            lineIndices = new HashSet<Tuple<int, int>>();
            linesByEntity[entityId] = lineIndices;
        }

        lineIndices.Add(lineIndex);
    }

    /// <summary>
    ///     Removes an entity from any overlapping perspective lines.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void RemoveEntity(ulong entityId)
    {
    }

    /// <summary>
    ///     Called when a non-block entity moves.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="position">New position.</param>
    private void NonBlockEntityMoved(ulong entityId, Vector3 position)
    {
    }

    /// <summary>
    ///     Gets the perspective line indices that intersect the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Intersecting perspective line indices.</returns>
    private List<Tuple<int, int>> GetIndicesForWorldSegment(GridPosition segmentIndex)
    {
        var (minPosition, maxPosition) = resolver.GetRangeForWorldSegment(segmentIndex);
        var startX = (int)minPosition.X; // inclusive
        var startY = (int)minPosition.Y; // inclusive
        var startZ = (int)minPosition.Z; // inclusive
        var endX = (int)maxPosition.X; // exclusive
        var endY = (int)maxPosition.Y; // exclusive
        var endZ = (int)maxPosition.Z; // exclusive

        var indices = new List<Tuple<int, int>>();

        // Each perspective line will intersect two faces of the world segment cube.
        // To enumerate the perspective lines that intersect a world segment, we iterate the
        // block positions on the front and bottom face of the world segment, taking care not
        // to double-count the bottom row of the front face/front row of the bottom face
        // (the shared edge between the two faces).

        for (var x = startX; x < endX; x++)
        {
            // Front face: (startX..endX, startY, startZ..endZ)
            for (var z = startZ; z < endZ; z++)
            {
                indices.Add(GetIndexForBlockPosition(x, startY, z));
            }

            // Bottom face (excluding edge with front face): (startX..endX, startY+1..endY, startZ)
            for (var y = startY + 1; y < endY; y++)
            {
                indices.Add(GetIndexForBlockPosition(x, y, startZ));
            }
        }

        return indices;
    }

    /// <summary>
    ///     Gets the index of the perspective line that intersects the given block position.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="z">Z coordinate.</param>
    /// <returns>Perspective line index.</returns>
    private Tuple<int, int> GetIndexForBlockPosition(int x, int y, int z)
    {
        // Perspective lines have constant x and (y + z), so at the z-intercept this becomes...
        return Tuple.Create(x, y + z);
    }

    /// <summary>
    ///     Gets the index of the perspective line that intersects the given block position.
    /// </summary>
    /// <param name="blockPosition">Block position.</param>
    /// <returns>Perspective line index.</returns>
    private Tuple<int, int> GetIndexForBlockPosition(GridPosition blockPosition)
    {
        return GetIndexForBlockPosition(blockPosition.X, blockPosition.Y, blockPosition.Z);
    }

    /// <summary>
    ///     Gets the index of the perspective line that intersects the given position.
    /// </summary>
    /// <param name="position">Position.</param>
    /// <returns>Perspective line index.</returns>
    private Tuple<int, int> GetIndexForPosition(Vector3 position)
    {
        return GetIndexForBlockPosition((int)Math.Floor(position.X), (int)Math.Floor(position.Y),
            (int)Math.Floor(position.Z));
    }

    /// <summary>
    ///     Contains data for a single perspective line.
    /// </summary>
    private class PerspectiveLine
    {
        /// <summary>
        ///     Entities overlapping this perspective line, sorted by z coordinate.
        /// </summary>
        public readonly SortedList<float, ulong> EntitiesByZ = new();

        /// <summary>
        ///     Reference count.
        /// </summary>
        public uint refCount;
    }
}