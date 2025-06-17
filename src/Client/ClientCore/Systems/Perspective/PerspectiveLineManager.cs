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
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.World;
using Sovereign.EngineUtil.Collections;
using Sovereign.EngineUtil.Ranges;

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
    private readonly DrawableLookup drawableLookup;
    private readonly DrawableTagCollection drawables;

    /// <summary>
    ///     Object pool of entity lists to minimize heap churn for vertically moving entities.
    /// </summary>
    private readonly ObjectPool<List<EntityInfo>> entityListPool = new();

    /// <summary>
    ///     Object pool of index lists to minimize heap churn when considering entity overlap.
    /// </summary>
    private readonly ObjectPool<List<PerspectiveLineKey>> indexListPool = new();

    private readonly KinematicsComponentCollection kinematics;

    /// <summary>
    ///     Map from entity ID to set of overlapping perspective line indices.
    /// </summary>
    private readonly ConcurrentDictionary<ulong, HashSet<PerspectiveLineKey>> linesByEntity = new();

    private readonly ILogger<PerspectiveLineManager> logger;

    /// <summary>
    ///     Active perspective lines indexed by their z-intercept (x, y) coordinates.
    /// </summary>
    private readonly ConcurrentDictionary<PerspectiveLineKey, PerspectiveLine> perspectiveLines = new();

    private readonly WorldSegmentResolver resolver;

    /// <summary>
    ///     Cache of current z floor of each tracked entity, used for efficient updates.
    /// </summary>
    private readonly ConcurrentDictionary<ulong, int> zFloorByEntity = new();

    public PerspectiveLineManager(KinematicsComponentCollection kinematics,
        BlockPositionComponentCollection blockPositions, WorldSegmentResolver resolver,
        EntityTable entityTable, DrawableLookup drawableLookup, DrawableTagCollection drawables,
        ILogger<PerspectiveLineManager> logger)
    {
        this.kinematics = kinematics;
        this.blockPositions = blockPositions;
        this.resolver = resolver;
        this.drawableLookup = drawableLookup;
        this.drawables = drawables;
        this.logger = logger;

        entityTable.OnEntityAdded += AddEntity;
        entityTable.OnEntityRemoved += RemoveEntity;
        kinematics.OnComponentModified += (entityId, k) => NonBlockEntityMoved(entityId, k.Position);
    }

    /// <summary>
    ///     Called when the client is subscribed to a world segment. This activates any perspective
    ///     lines which intercept the newly subscribed world segment.
    /// </summary>
    /// <param name="segmentIndex">Subscribed world segment index.</param>
    public void OnWorldSegmentSubscribe(GridPosition segmentIndex)
    {
        var indices = indexListPool.TakeObject();
        try
        {
            GetIndicesForWorldSegment(segmentIndex, indices);
            foreach (var index in indices)
            {
                if (!perspectiveLines.TryGetValue(index, out var line))
                {
                    line = new PerspectiveLine();
                    perspectiveLines[index] = line;
                }

                line.ReferenceCount++;
            }
        }
        finally
        {
            indices.Clear();
            indexListPool.ReturnObject(indices);
        }
    }

    /// <summary>
    ///     Called when the client is unsubscribed from a world segment. This deactivates any
    ///     perspective lines which no longer intersect any subscribed world segments.
    /// </summary>
    /// <param name="segmentIndex">Unsubscribed world segment index.</param>
    public void OnWorldSegmentUnsubscribe(GridPosition segmentIndex)
    {
        var indices = indexListPool.TakeObject();
        try
        {
            GetIndicesForWorldSegment(segmentIndex, indices);
            foreach (var index in indices)
            {
                if (!perspectiveLines.TryGetValue(index, out var line))
                {
                    logger.LogWarning("Perspective line {Index} not found for world segment unsubscribe.", index);
                    continue;
                }

                line.ReferenceCount--;
                if (line.ReferenceCount <= 0) perspectiveLines.TryRemove(index, out _);
            }
        }
        finally
        {
            indices.Clear();
            indexListPool.ReturnObject(indices);
        }
    }

    /// <summary>
    ///     Gets the perspective line, if any, that intersects the given block position.
    /// </summary>
    /// <param name="blockPosition">Block position.</param>
    /// <param name="perspectiveLine"></param>
    /// <returns></returns>
    public bool TryGetPerspectiveLine(GridPosition blockPosition,
        [NotNullWhen(true)] out PerspectiveLine? perspectiveLine)
    {
        var lineIndex = GetIndexForBlockPosition(blockPosition);
        return perspectiveLines.TryGetValue(lineIndex, out perspectiveLine);
    }

    /// <summary>
    ///     Gets the perspective line, if any, that has the given index.
    /// </summary>
    /// <param name="lineIndex">Perspective line index.</param>
    /// <param name="perspectiveLine">Perspective line, or null if none found.</param>
    /// <returns>true if a perspective line was found, false otherwise.</returns>
    public bool TryGetPerspectiveLine(ValueTuple<int, int> lineIndex,
        [NotNullWhen(true)] out PerspectiveLine? perspectiveLine)
    {
        return perspectiveLines.TryGetValue(new PerspectiveLineKey(lineIndex.Item1, lineIndex.Item2),
            out perspectiveLine);
    }

    /// <summary>
    ///     Gets the highest z floor for the given perspective line index, if any.
    /// </summary>
    /// <param name="lineIndex">Perspective line index.</param>
    /// <param name="highestZFloor">Highest Z floor. Only meaningful if method returns true.</param>
    /// <returns>true if the perspective line contains at least one entity, false otherwise.</returns>
    public bool TryGetHighestZFloorForLine(ValueTuple<int, int> lineIndex, out int highestZFloor)
    {
        highestZFloor = 0;
        if (!perspectiveLines.TryGetValue(new PerspectiveLineKey(lineIndex.Item1, lineIndex.Item2), out var line))
            return false;

        if (line.ZFloors.Count == 0) return false;
        highestZFloor = line.ZFloors[0].ZFloor;
        return true;
    }

    /// <summary>
    ///     Gets the highest entity, if any, within a z window around a point.
    /// </summary>
    /// <param name="point">Point of interest in world coordinates.</param>
    /// <param name="minZ">Minimum z for search window.</param>
    /// <param name="maxZ">Maximum z for search window.</param>
    /// <param name="entityId">Entity ID if found, or defaults to 0 otherwise.</param>
    /// <returns>true if an entity was found, false otherwise.</returns>
    public bool TryGetHighestEntityAtPoint(Vector3 point, float minZ, float maxZ, out ulong entityId)
    {
        entityId = 0;

        var lineIndex = GetIndexForPosition(point);
        if (!perspectiveLines.TryGetValue(lineIndex, out var line)) return false;
        foreach (var zSet in line.ZFloors)
        {
            // If there are multiple entities at the highest depth, the priority is:
            //   1. Non-block entities (take first available)
            //   2. Block front face (will only ever be one at a given z floor)
            //   3. Block top face (will only ever be one at a given z floor)
            var foundFrontFace = false;
            var projectedPoint = new Vector2(point.X, point.Y + point.Z);
            var foundAny = false;
            foreach (var entityInfo in zSet.Entities)
            {
                if (!EntityOverlapsProjectedPoint(entityInfo.EntityId, entityInfo.PerspectiveEntityType,
                        projectedPoint)) continue;

                foundAny = true;
                switch (entityInfo.PerspectiveEntityType)
                {
                    case PerspectiveEntityType.NonBlock:
                        // Always wins.
                        entityId = entityInfo.EntityId;
                        return true;

                    case PerspectiveEntityType.BlockFrontFace:
                        foundFrontFace = true;
                        entityId = entityInfo.EntityId;
                        break;

                    case PerspectiveEntityType.BlockTopFace:
                        if (!foundFrontFace) entityId = entityInfo.EntityId;
                        break;
                }
            }

            if (foundAny) return true;
        }

        // If we get here, nothing was found in the window.
        return false;
    }

    /// <summary>
    ///     Iterates all current perspective lines.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<(int, int, PerspectiveLine)> GetAllLines()
    {
        // Take a snapshot before iterating in case the lines change during iteration.
        var copy = new Dictionary<PerspectiveLineKey, PerspectiveLine>(perspectiveLines);
        foreach (var kvp in copy) yield return (kvp.Key.X, kvp.Key.Yz, kvp.Value);
    }

    /// <summary>
    ///     Determines whether the given projected point overlaps the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="perspectiveEntityType">Entity type.</param>
    /// <param name="projectedPoint">Point projected onto the z=0 plane.</param>
    /// <returns>true if the projected point overlaps the entity, false otherwise.</returns>
    private bool EntityOverlapsProjectedPoint(ulong entityId, PerspectiveEntityType perspectiveEntityType,
        Vector2 projectedPoint)
    {
        var entityPosition = Vector2.Zero;
        var entityExtent = Vector2.One;

        switch (perspectiveEntityType)
        {
            case PerspectiveEntityType.NonBlock:
                var fullPos = kinematics[entityId].Position;
                entityPosition = new Vector2(fullPos.X, fullPos.Y + fullPos.Z);
                entityExtent = drawableLookup.GetEntityDrawableSizeWorld(entityId);
                break;

            case PerspectiveEntityType.BlockTopFace:
            {
                var blockPos = blockPositions[entityId];
                entityPosition = new Vector2(blockPos.X, blockPos.Y + blockPos.Z + 1);
                break;
            }

            case PerspectiveEntityType.BlockFrontFace:
            {
                var blockPos = blockPositions[entityId];
                entityPosition = new Vector2(blockPos.X, blockPos.Y + blockPos.Z);
                break;
            }
        }

        var entityRangeMin = entityPosition;
        var entityRangeMax = entityRangeMin + entityExtent;

        return RangeUtil.IsPointInRange(entityRangeMin, entityRangeMax, projectedPoint);
    }

    /// <summary>
    ///     Called when a new entity is added.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isLoad">Unused.</param>
    private void AddEntity(ulong entityId, bool isLoad)
    {
        if (!drawables.HasTagForEntity(entityId)) return;

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
        // in the same block position lattice as the lines. However, their two faces differ in their z
        // coordinate and therefore fall on two different perspective lines.
        var indexTopFace = GetIndexForBlockPosition(blockPosition with { Z = blockPosition.Z + 1 });
        var indexFrontFace = GetIndexForBlockPosition(blockPosition);
        AddEntityToLine(entityId, indexTopFace, blockPosition.Z + 1, PerspectiveEntityType.BlockTopFace,
            indexTopFace.OriginFlag);
        AddEntityToLine(entityId, indexFrontFace, blockPosition.Z, PerspectiveEntityType.BlockFrontFace,
            indexFrontFace.OriginFlag);

        zFloorByEntity[entityId] = blockPosition.Z;
    }

    /// <summary>
    ///     Adds a non-block entity to any overlapping perspective line.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="position">Position.</param>
    private void AddNonBlockEntity(ulong entityId, Vector3 position)
    {
        // Non-block entities can overlap multiple perspective lines depending on their extent.
        var indices = indexListPool.TakeObject();
        try
        {
            GetNonBlockOverlappingLines(entityId, indices);
            foreach (var index in indices)
                AddEntityToLine(entityId, index, position.Z, PerspectiveEntityType.NonBlock, index.OriginFlag);
        }
        finally
        {
            indices.Clear();
            indexListPool.ReturnObject(indices);
        }

        zFloorByEntity[entityId] = (int)Math.Floor(position.Z);
    }

    /// <summary>
    ///     Removes an entity from any overlapping perspective lines.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isUnload">Unused.</param>
    private void RemoveEntity(ulong entityId, bool isUnload)
    {
        if (!drawables.HasTagForEntity(entityId, true)) return;

        if (!linesByEntity.TryGetValue(entityId, out var lineIndices)) return;
        if (!zFloorByEntity.TryGetValue(entityId, out var zFloor))
        {
            logger.LogError("No cached z value for entity {EntityId:X}; skipping removal.", entityId);
            return;
        }

        var isBlock = blockPositions.HasComponentForEntity(entityId, true);

        foreach (var index in lineIndices)
        {
            if (!perspectiveLines.TryGetValue(index, out var line)) continue;

            // Blocks also appear in the next z-floor up (because of the top face).
            // Non-blocks only appear in their base z-floor.
            for (var zOff = 0; zOff < (isBlock ? 2 : 1); ++zOff)
            {
                if (!line.TryGetListForZFloor(zFloor + zOff, out var list))
                {
                    // This is expected for blocks (we overscan to simplify the removal logic),
                    // but not for non-block entities.
                    if (!isBlock)
                        logger.LogError("Entity {EntityID:X} not found in perspective line {LineIndex} for removal.",
                            entityId, index);
                    continue;
                }

                list.RemoveEntity(entityId);
                if (list.Entities.Count == 0)
                {
                    line.RemoveZFloor(zFloor + zOff);
                    entityListPool.ReturnObject(list.Entities);
                }
            }
        }

        zFloorByEntity.TryRemove(entityId, out _);
        linesByEntity.TryRemove(entityId, out _);
    }

    /// <summary>
    ///     Called when a non-block entity moves.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="position">New position.</param>
    private void NonBlockEntityMoved(ulong entityId, Vector3 position)
    {
        // Get prior state.
        if (!linesByEntity.TryGetValue(entityId, out var oldLines)
            || !zFloorByEntity.TryGetValue(entityId, out var oldZFloor))
        {
            logger.LogWarning("Moved entity {Id:X} not already tracked, treating as add.", entityId);
            AddNonBlockEntity(entityId, position);
            return;
        }

        var newIndices = indexListPool.TakeObject();
        try
        {
            GetNonBlockOverlappingLines(entityId, newIndices);

            // Remove from old perspective lines, then re-add.
            foreach (var oldIndex in oldLines)
            {
                if (!perspectiveLines.TryGetValue(oldIndex, out var oldLine))
                {
                    logger.LogError("Perspective line {Index} is missing.", oldIndex);
                    continue;
                }

                RemoveEntityFromLine(entityId, oldIndex, oldLine, oldZFloor);
            }

            foreach (var newIndex in newIndices)
                AddEntityToLine(entityId, newIndex, position.Z, PerspectiveEntityType.NonBlock, newIndex.OriginFlag);

            // Update state.
            zFloorByEntity[entityId] = (int)Math.Floor(position.Z);
        }
        finally
        {
            newIndices.Clear();
            indexListPool.ReturnObject(newIndices);
        }
    }

    /// <summary>
    ///     Adds the given entity to a perspective line.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="lineIndex">Perspective line index.</param>
    /// <param name="z">z position of entity.</param>
    /// <param name="perspectiveEntityType">Entity type.</param>
    /// <param name="originOnLine">Whether the sprite origin is on this perspective line.</param>
    private void AddEntityToLine(ulong entityId, PerspectiveLineKey lineIndex, float z,
        PerspectiveEntityType perspectiveEntityType,
        bool originOnLine)
    {
        if (!perspectiveLines.TryGetValue(lineIndex, out var line))
        {
            // This shouldn't normally happen, but just in case, allocate a new line if one
            // doesn't already exist. The reference count will be picked up when the subscribe
            // event hits.
            line = new PerspectiveLine();
            perspectiveLines[lineIndex] = line;
        }

        // Insert the entity into the correct z position on the perspective line.
        var zFloor = (int)Math.Floor(z);
        if (!line.TryGetListForZFloor(zFloor, out var entityList))
        {
            entityList = new EntityList(zFloor, entityListPool.TakeObject());
            entityList.Entities.Clear();
            line.AddZFloor(zFloor, entityList);
        }

        entityList.AddEntity(new EntityInfo
        {
            EntityId = entityId,
            PerspectiveEntityType = perspectiveEntityType,
            OriginOnLine = originOnLine,
            Z = z
        });

        // Keep a record of where this block is for easier removal later.
        if (!linesByEntity.TryGetValue(entityId, out var lineIndices))
        {
            lineIndices = new HashSet<PerspectiveLineKey>();
            linesByEntity[entityId] = lineIndices;
        }

        lineIndices.Add(lineIndex);
    }

    /// <summary>
    ///     Removes an entity from the given perspective line.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="lineIndex">Line index.</param>
    /// <param name="line">Perspective line.</param>
    /// <param name="z">z position of entity.</param>
    private void RemoveEntityFromLine(ulong entityId, PerspectiveLineKey lineIndex, PerspectiveLine line, float z)
    {
        var zFloor = (int)Math.Floor(z);
        if (line.TryGetListForZFloor(zFloor, out var list))
        {
            list.RemoveEntity(entityId);
            if (list.Entities.Count == 0)
            {
                line.RemoveZFloor(zFloor);
                entityListPool.ReturnObject(list.Entities);
            }
        }

        if (linesByEntity.TryGetValue(entityId, out var lineIndices)) lineIndices.Remove(lineIndex);
    }

    /// <summary>
    ///     Gets the perspective line indices that intersect the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="indices">List of indices to update.</param>
    /// <returns>Intersecting perspective line indices.</returns>
    private void GetIndicesForWorldSegment(GridPosition segmentIndex, List<PerspectiveLineKey> indices)
    {
        var (minPosition, maxPosition) = resolver.GetRangeForWorldSegment(segmentIndex);
        var startX = (int)minPosition.X; // inclusive
        var startY = (int)minPosition.Y; // inclusive
        var startZ = (int)minPosition.Z; // inclusive
        var endX = (int)maxPosition.X; // exclusive
        var endY = (int)maxPosition.Y; // exclusive
        var endZ = (int)maxPosition.Z; // exclusive

        // Each perspective line will intersect two faces of the world segment cube.
        // To enumerate the perspective lines that intersect a world segment, we iterate the
        // block positions on the front and bottom face of the world segment, taking care not
        // to double-count the bottom row of the front face/front row of the bottom face
        // (the shared edge between the two faces).

        indices.Clear();
        for (var x = startX; x < endX; x++)
        {
            // Front face: (startX..endX, startY, startZ..endZ)
            for (var z = startZ; z < endZ; z++) indices.Add(GetIndexForBlockPosition(x, startY, z));

            // Bottom face (excluding edge with front face): (startX..endX, startY+1..endY, startZ)
            for (var y = startY + 1; y < endY; y++) indices.Add(GetIndexForBlockPosition(x, y, startZ));
        }
    }

    /// <summary>
    ///     Gets the index of the perspective line that intersects the given block position.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="z">Z coordinate.</param>
    /// <returns>Perspective line index.</returns>
    private PerspectiveLineKey GetIndexForBlockPosition(int x, int y, int z)
    {
        // Perspective lines have constant x and (y + z), so at the z-intercept this becomes...
        return new PerspectiveLineKey(x, y + z);
    }

    /// <summary>
    ///     Gets the index of the perspective line that intersects the given block position.
    /// </summary>
    /// <param name="blockPosition">Block position.</param>
    /// <returns>Perspective line index.</returns>
    private PerspectiveLineKey GetIndexForBlockPosition(GridPosition blockPosition)
    {
        return GetIndexForBlockPosition(blockPosition.X, blockPosition.Y, blockPosition.Z);
    }

    /// <summary>
    ///     Gets the index of the perspective line that intersects the given position.
    /// </summary>
    /// <param name="position">Position.</param>
    /// <returns>Perspective line index.</returns>
    private PerspectiveLineKey GetIndexForPosition(Vector3 position)
    {
        return GetIndexForBlockPosition((int)Math.Floor(position.X), (int)Math.Floor(position.Y),
            (int)Math.Floor(position.Z));
    }

    /// <summary>
    ///     Gets the index of the perspective line that intersects the given projected position on the z=0 plane.
    /// </summary>
    /// <param name="projectedPosition">Position projected onto the z=0 plane.</param>
    /// <returns>Perspective line index.</returns>
    private Tuple<int, int> GetIndexForProjectedPosition(Vector2 projectedPosition)
    {
        return Tuple.Create((int)Math.Floor(projectedPosition.X), (int)Math.Floor(projectedPosition.Y));
    }

    /// <summary>
    ///     Gets the indices of any perspective lines overlapped by the given non-block entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="indices">Indices list to update.</param>
    private void GetNonBlockOverlappingLines(ulong entityId, List<PerspectiveLineKey> indices)
    {
        if (!kinematics.HasComponentForEntity(entityId))
        {
            logger.LogWarning("No position for entity {EntityId:X}.", entityId);
            return;
        }

        var position = kinematics[entityId].Position;
        var projectedPosition = new Vector2(position.X, position.Y + position.Z);
        var originX = (int)Math.Floor(projectedPosition.X);
        var originY = (int)Math.Floor(projectedPosition.Y);
        var entityExtent = drawableLookup.GetEntityDrawableSizeWorld(entityId);

        // Determine line extent along projected x and y axes.
        var minPosition = projectedPosition;
        var maxPosition = projectedPosition + entityExtent;
        var minIndices = GetIndexForProjectedPosition(minPosition);
        var maxIndices = GetIndexForProjectedPosition(maxPosition);

        for (var x = minIndices.Item1; x <= maxIndices.Item1; ++x)
        for (var y = minIndices.Item2; y <= maxIndices.Item2; ++y)
            indices.Add(new PerspectiveLineKey(x, y, x == originX && y == originY));
    }

    /// <summary>
    ///     Perspective line index.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="yz">Y+Z invariant.</param>
    private readonly struct PerspectiveLineKey(int x, int yz, bool originFlag = false) : IEquatable<PerspectiveLineKey>
    {
        public bool Equals(PerspectiveLineKey other)
        {
            return X == other.X && Yz == other.Yz;
        }

        public override bool Equals(object? obj)
        {
            return obj is PerspectiveLineKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Yz);
        }

        /// <summary>
        ///     X coordinate of line.
        /// </summary>
        public readonly int X = x;

        /// <summary>
        ///     (Y+Z) invariant of line.
        /// </summary>
        public readonly int Yz = yz;

        /// <summary>
        ///     Flag used internally to indicate that a perspective line contains the sprite origin.
        /// </summary>
        public readonly bool OriginFlag = originFlag;

        public static bool operator ==(PerspectiveLineKey left, PerspectiveLineKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PerspectiveLineKey left, PerspectiveLineKey right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"<{X}, {Yz}>";
        }
    }
}