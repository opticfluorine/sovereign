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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineUtil.Algorithms;

namespace Sovereign.ClientCore.Systems.Perspective;

/// <summary>
///     Manages the overhead block occupancy graph
/// </summary>
internal class OverheadBlockGraphManager
{
    private const int MinimumScheduleDelay = 2;
    private readonly BlockPositionComponentCollection blockPositions;

    private readonly HashSet<(int, int)> changeSet = new();

    private readonly ConcurrentDictionary<int, UnionFind2dGrid> graphs = new();
    private readonly ILogger<OverheadBlockGraphManager> logger;
    private readonly PerspectiveLineManager perspectiveLineManager;

    /// <summary>
    ///     Back buffer for dirty lattice points. Used to drive graph updates.
    /// </summary>
    private HashSet<(int, int)> dirtyLatticePointsBack = new();

    /// <summary>
    ///     Front buffer for dirty lattice points. Used to accumulate changes.
    /// </summary>
    private HashSet<(int, int)> dirtyLatticePointsFront = new();

    private Task? graphUpdateTask;
    private int maxZ;
    private int minZ;
    private int ticksSinceChange;

    public OverheadBlockGraphManager(BlockPositionComponentCollection blockPositions,
        PerspectiveLineManager perspectiveLineManager,
        ILogger<OverheadBlockGraphManager> logger)
    {
        this.blockPositions = blockPositions;
        this.perspectiveLineManager = perspectiveLineManager;
        this.logger = logger;
        blockPositions.OnComponentAdded += OnBlockAdded;
        blockPositions.OnComponentModified += OnBlockMoved;
        blockPositions.OnComponentRemoved += OnBlockRemoved;
    }

    /// <summary>
    ///     Gets the overhead block graph for the given z if it exists.
    /// </summary>
    /// <param name="z">Z.</param>
    /// <param name="graph">Graph, or null if no graph is found for z.</param>
    /// <returns>true if a graph was found, false otherwise.</returns>
    public bool TryGetGraphForZ(int z, [NotNullWhen(true)] out UnionFind2dGrid? graph)
    {
        return graphs.TryGetValue(z, out graph);
    }

    /// <summary>
    ///     Called at the start of a new tick to schedule updates to the graphs.
    /// </summary>
    public void OnTick()
    {
        if (ticksSinceChange >= MinimumScheduleDelay && dirtyLatticePointsFront.Count > 0 &&
            (graphUpdateTask is null || graphUpdateTask.IsCompleted))
        {
            (dirtyLatticePointsFront, dirtyLatticePointsBack) = (dirtyLatticePointsBack, dirtyLatticePointsFront);
            dirtyLatticePointsFront.Clear();
            graphUpdateTask = Task.Run(UpdateGraphs);
        }

        ticksSinceChange++;
    }

    /// <summary>
    ///     Updates all graphs based on the contents of the dirty lattice points back buffer.
    /// </summary>
    private void UpdateGraphs()
    {
        var stopwatch = new Stopwatch();

        stopwatch.Restart();
        ApplyAdds();
        ApplyRemoves();
        stopwatch.Stop();

        logger.LogTrace("{Count} updates ({GraphCount} graphs) took {Elapsed} ms.",
            dirtyLatticePointsBack.Count, graphs.Count,
            stopwatch.Elapsed.Nanoseconds * 1E-6);
    }

    /// <summary>
    ///     Progressively adds vertices to the graphs.
    /// </summary>
    private void ApplyAdds()
    {
        // Downward pass, building up the added vertices and adding them to each z layer.
        changeSet.Clear();
        for (var z = maxZ; z >= minZ; z--)
        {
            if (!graphs.TryGetValue(z, out var graph))
            {
                graph = new UnionFind2dGrid();
                graphs[z] = graph;
            }

            foreach (var latticePoint in dirtyLatticePointsBack)
            {
                if (changeSet.Contains(latticePoint)) continue;

                if (perspectiveLineManager.TryGetHighestZFloorForLine(latticePoint, out var lineZ) && lineZ > z)
                    changeSet.Add(latticePoint);
            }

            graph.AddVertices(changeSet);
        }
    }

    /// <summary>
    ///     Progressively removes vertices from the graphs.
    /// </summary>
    private void ApplyRemoves()
    {
        // Upward pass, building up the removed vertices and removing them from each z layer.
        changeSet.Clear();
        for (var z = minZ; z <= maxZ; z++)
        {
            if (!graphs.TryGetValue(z, out var graph)) continue;

            foreach (var latticePoint in dirtyLatticePointsBack)
            {
                if (changeSet.Contains(latticePoint)) continue;

                if (!perspectiveLineManager.TryGetHighestZFloorForLine(latticePoint, out var lineZ) || lineZ <= z)
                    changeSet.Add(latticePoint);
            }

            graph.RemoveVertices(changeSet);
        }
    }

    /// <summary>
    ///     Called when a block is added/loaded.
    /// </summary>
    /// <param name="entityId">Block entity ID.</param>
    /// <param name="blockPosition">Block position.</param>
    /// <param name="unused">Unused.</param>
    private void OnBlockAdded(ulong entityId, GridPosition blockPosition, bool unused)
    {
        dirtyLatticePointsFront.Add((blockPosition.X, blockPosition.Y + blockPosition.Z));
        if (blockPosition.Z < minZ) minZ = blockPosition.Z;
        if (blockPosition.Z > maxZ) maxZ = blockPosition.Z;
        ticksSinceChange = 0;
    }

    /// <summary>
    ///     Called when a block is moved. This should never happen, but is here for completeness.
    /// </summary>
    /// <param name="entityId">Block entity ID.</param>
    /// <param name="blockPosition">Block position.</param>
    private void OnBlockMoved(ulong entityId, GridPosition blockPosition)
    {
        OnBlockAdded(entityId, blockPosition, false);
    }

    /// <summary>
    ///     Called when a block is removed/unloaded.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="unused">Unused.</param>
    private void OnBlockRemoved(ulong entityId, bool unused)
    {
        if (!blockPositions.HasComponentForEntity(entityId, true))
        {
            logger.LogWarning("No position found for removed block {EntityId:X}.", entityId);
            return;
        }

        var blockPosition = blockPositions.GetComponentWithLookback(entityId);
        OnBlockAdded(entityId, blockPosition, false);
    }
}