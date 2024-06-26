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
using Sovereign.ClientCore.Rendering;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
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

    /// <summary>
    ///     Object pool of entity lists to minimize heap churn for vertically moving entities.
    /// </summary>
    private readonly ObjectPool<EntityList> entityListPool = new();

    /// <summary>
    ///     Object pool of index lists to minimize heap churn when considering entity overlap.
    /// </summary>
    private readonly ObjectPool<List<Tuple<int, int>>> indexListPool = new();

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

    /// <summary>
    ///     Cache of current z position of each tracked entity, used for efficient updates.
    /// </summary>
    private readonly Dictionary<ulong, float> zDepthByEntity = new();

    public PerspectiveLineManager(KinematicComponentCollection kinematics,
        BlockPositionComponentCollection blockPositions, WorldSegmentResolver resolver,
        EntityTable entityTable, DrawableLookup drawableLookup)
    {
        this.kinematics = kinematics;
        this.blockPositions = blockPositions;
        this.resolver = resolver;
        this.drawableLookup = drawableLookup;

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
                    Logger.WarnFormat("Perspective line {0} not found for world segment unsubscribe.", index);
                    continue;
                }

                line.ReferenceCount--;
                if (line.ReferenceCount <= 0) perspectiveLines.Remove(index);
            }
        }
        finally
        {
            indices.Clear();
            indexListPool.ReturnObject(indices);
        }
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
        foreach (var entities in line.ZDepths
                     .GetViewBetween(EntityList.ForComparison(minZ), EntityList.ForComparison(maxZ)).Reverse())
        {
            // If there are multiple entities at the highest depth, the priority is:
            //   1. Non-block entities (take first available)
            //   2. Block top face (will only ever be one at a given z)
            //   3. Block front face (will only ever be one at a given z)
            var foundTopFace = false;
            var projectedPoint = new Vector2(point.X, point.Y + point.Z);
            var foundAny = false;
            foreach (var entityInfo in entities.Entities)
            {
                if (!EntityOverlapsProjectedPoint(entityInfo.EntityId, entityInfo.EntityType, projectedPoint)) continue;

                foundAny = true;
                switch (entityInfo.EntityType)
                {
                    case EntityType.NonBlock:
                        // Always wins.
                        entityId = entityInfo.EntityId;
                        return true;

                    case EntityType.BlockTopFace:
                        foundTopFace = true;
                        entityId = entityInfo.EntityId;
                        break;

                    case EntityType.BlockFrontFace:
                        if (!foundTopFace) entityId = entityInfo.EntityId;
                        break;
                }
            }

            if (foundAny) return true;
        }

        // If we get here, nothing was found in the window.
        return false;
    }

    /// <summary>
    ///     Determines whether the given projected point overlaps the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="entityType">Entity type.</param>
    /// <param name="projectedPoint">Point projected onto the z=0 plane.</param>
    /// <returns>true if the projected point overlaps the entity, false otherwise.</returns>
    private bool EntityOverlapsProjectedPoint(ulong entityId, EntityType entityType, Vector2 projectedPoint)
    {
        var entityPosition = Vector2.Zero;
        var entityExtent = Vector2.One;

        switch (entityType)
        {
            case EntityType.NonBlock:
                var fullPos = kinematics[entityId].Position;
                entityPosition = new Vector2(fullPos.X, fullPos.Y + fullPos.Z);
                entityExtent = drawableLookup.GetEntityDrawableSizeWorld(entityId);
                break;

            case EntityType.BlockTopFace:
            {
                var blockPos = blockPositions[entityId];
                entityPosition = new Vector2(blockPos.X, blockPos.Y + blockPos.Z);
                break;
            }

            case EntityType.BlockFrontFace:
            {
                var blockPos = blockPositions[entityId];
                entityPosition = new Vector2(blockPos.X, blockPos.Y + blockPos.Z - 1);
                break;
            }
        }

        var entityRangeMin = entityPosition with { Y = entityPosition.Y - entityExtent.Y };
        var entityRangeMax = entityRangeMin + entityExtent;

        return RangeUtil.IsPointInRange(entityRangeMin, entityRangeMax, projectedPoint);
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
        // in the same block position lattice as the lines. However, their two faces differ in their y
        // coordinate and therefore fall on two different perspective lines.
        var indexTopFace = GetIndexForBlockPosition(blockPosition);
        var indexFrontFace = GetIndexForBlockPosition(blockPosition with { Y = blockPosition.Y - 1 });
        AddEntityToLine(entityId, indexTopFace, blockPosition.Z, EntityType.BlockTopFace);
        AddEntityToLine(entityId, indexFrontFace, blockPosition.Z, EntityType.BlockFrontFace);
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
            {
                AddEntityToLine(entityId, index, position.Z, EntityType.NonBlock);
            }
        }
        finally
        {
            indices.Clear();
            indexListPool.ReturnObject(indices);
        }
    }

    /// <summary>
    ///     Removes an entity from any overlapping perspective lines.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void RemoveEntity(ulong entityId)
    {
        if (!linesByEntity.TryGetValue(entityId, out var lineIndices)) return;
        if (!zDepthByEntity.TryGetValue(entityId, out var z))
        {
            Logger.ErrorFormat("No cached z value for entity {0}; skipping removal.", entityId);
            return;
        }

        foreach (var index in lineIndices)
        {
            if (!perspectiveLines.TryGetValue(index, out var line)) continue;
            if (!line.ZDepths.TryGetValue(EntityList.ForComparison(z), out var list))
            {
                Logger.ErrorFormat("Entity {0} not found in perspective line {1} for removal.", entityId, index);
                continue;
            }

            list.RemoveEntity(entityId);
            if (list.Entities.Count == 0)
            {
                line.ZDepths.Remove(list);
                entityListPool.ReturnObject(list);
            }
        }

        zDepthByEntity.Remove(entityId);
        linesByEntity.Remove(entityId);
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
            || !zDepthByEntity.TryGetValue(entityId, out var oldZ))
        {
            Logger.WarnFormat("Moved entity {0} not already tracked, treating as add.");
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
                    Logger.ErrorFormat("Perspective line {0} is missing.", oldIndex);
                    continue;
                }

                RemoveEntityFromLine(entityId, oldIndex, oldLine, oldZ);
            }

            foreach (var newIndex in newIndices)
                AddEntityToLine(entityId, newIndex, position.Z, EntityType.NonBlock);

            // Update state.
            zDepthByEntity[entityId] = position.Z;
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
    /// <param name="entityType">Entity type.</param>
    private void AddEntityToLine(ulong entityId, Tuple<int, int> lineIndex, float z, EntityType entityType)
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
        if (!line.ZDepths.TryGetValue(EntityList.ForComparison(z), out var entityList))
        {
            entityList = entityListPool.TakeObject();
            entityList.Z = z;
            entityList.Entities.Clear();
            line.ZDepths.Add(entityList);
        }

        entityList.Entities.Add(new EntityInfo
        {
            EntityId = entityId,
            EntityType = entityType
        });
        zDepthByEntity[entityId] = z;

        // Keep a record of where this block is for easier removal later.
        if (!linesByEntity.TryGetValue(entityId, out var lineIndices))
        {
            lineIndices = new HashSet<Tuple<int, int>>();
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
    private void RemoveEntityFromLine(ulong entityId, Tuple<int, int> lineIndex, PerspectiveLine line, float z)
    {
        if (line.ZDepths.TryGetValue(EntityList.ForComparison(z), out var list))
        {
            list.RemoveEntity(entityId);
            if (list.Entities.Count == 0)
            {
                line.ZDepths.Remove(list);
                entityListPool.ReturnObject(list);
            }
        }

        if (linesByEntity.TryGetValue(entityId, out var lineIndices))
        {
            lineIndices.Remove(lineIndex);
        }
    }

    /// <summary>
    ///     Gets the perspective line indices that intersect the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="indices">List of indices to update.</param>
    /// <returns>Intersecting perspective line indices.</returns>
    private void GetIndicesForWorldSegment(GridPosition segmentIndex, List<Tuple<int, int>> indices)
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
        return GetIndexForBlockPosition((int)Math.Floor(position.X), (int)Math.Ceiling(position.Y),
            (int)Math.Ceiling(position.Z));
    }

    /// <summary>
    ///     Gets the index of the perspective line that intersects the given projected position on the z=0 plane.
    /// </summary>
    /// <param name="projectedPosition">Position projected onto the z=0 plane.</param>
    /// <returns>Perspective line index.</returns>
    private Tuple<int, int> GetIndexForProjectedPosition(Vector2 projectedPosition)
    {
        return Tuple.Create((int)Math.Floor(projectedPosition.X), (int)Math.Ceiling(projectedPosition.Y));
    }

    /// <summary>
    ///     Gets the indices of any perspective lines overlapped by the given non-block entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="indices">Indices list to update.</param>
    private void GetNonBlockOverlappingLines(ulong entityId, List<Tuple<int, int>> indices)
    {
        if (!kinematics.HasComponentForEntity(entityId))
        {
            Logger.WarnFormat("No position for entity {0}.", entityId);
            return;
        }

        var position = kinematics[entityId].Position;
        var projectedPosition = new Vector2(position.X, position.Y + position.Z);
        var entityExtent = drawableLookup.GetEntityDrawableSizeWorld(entityId);

        // Determine line extent along projected x and y axes.
        var minPosition = projectedPosition with { Y = projectedPosition.Y - entityExtent.Y };
        var maxPosition = projectedPosition + entityExtent;
        var minIndices = GetIndexForProjectedPosition(minPosition);
        var maxIndices = GetIndexForProjectedPosition(maxPosition);

        for (var x = minIndices.Item1; x <= maxIndices.Item1; ++x)
        {
            for (var y = minIndices.Item2; y <= maxIndices.Item2; ++y)
            {
                indices.Add(Tuple.Create(x, y));
            }
        }
    }

    /// <summary>
    ///     Entity types that can be found on a perspective line.
    /// </summary>
    private enum EntityType
    {
        /// <summary>
        ///     Non-block entity.
        /// </summary>
        NonBlock,

        /// <summary>
        ///     Front face of a block entity.
        /// </summary>
        BlockFrontFace,

        /// <summary>
        ///     Top face of a block entity.
        /// </summary>
        BlockTopFace
    }

    /// <summary>
    ///     Information regarding an entity on a perspective line.
    /// </summary>
    private struct EntityInfo
    {
        /// <summary>
        ///     Entity ID.
        /// </summary>
        public ulong EntityId;

        /// <summary>
        ///     Entity type.
        /// </summary>
        public EntityType EntityType;
    }

    /// <summary>
    ///     Represents a list of entities at a common z-depth.
    /// </summary>
    private class EntityList
    {
        /// <summary>
        ///     Comparer for EntityLists.
        /// </summary>
        public static readonly IComparer<EntityList> Comparer =
            Comparer<EntityList>.Create((a, b) => Comparer<float>.Default.Compare(a.Z, b.Z));

        private static readonly List<EntityInfo> EmptyList = new(0);

        /// <summary>
        ///     Entities at this z-depth.
        /// </summary>
        public readonly List<EntityInfo> Entities;

        /// <summary>
        ///     Z depth of this entity list.
        /// </summary>
        public float Z;

        public EntityList()
        {
            Entities = new List<EntityInfo>();
        }

        /// <summary>
        ///     Constructs a new empty EntityList for lookups only.
        /// </summary>
        /// <param name="z">Z depth.</param>
        private EntityList(float z)
        {
            Z = z;
            Entities = EmptyList;
        }

        /// <summary>
        ///     Convenience method to improve code readability when creating a dummy list as a comparison key.
        /// </summary>
        /// <param name="z">Z depth.</param>
        /// <returns>Dummy list for use as a comparison key.</returns>
        public static EntityList ForComparison(float z)
        {
            return new EntityList(z);
        }

        /// <summary>
        ///     Removes all occurrences of an entity in this list.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        public void RemoveEntity(ulong entityId)
        {
            var toRemove = new List<int>();
            for (var i = 0; i < Entities.Count; ++i)
            {
                if (Entities[i].EntityId == entityId) toRemove.Add(i);
            }

            for (var i = toRemove.Count - 1; i >= 0; --i)
            {
                Entities.RemoveAt(toRemove[i]);
            }
        }
    }

    /// <summary>
    ///     Contains data for a single perspective line.
    /// </summary>
    private class PerspectiveLine
    {
        /// <summary>
        ///     Z values at which entities are located on this perspective line.
        /// </summary>
        public readonly SortedSet<EntityList> ZDepths = new(EntityList.Comparer);

        /// <summary>
        ///     Reference count.
        /// </summary>
        public uint ReferenceCount;
    }
}