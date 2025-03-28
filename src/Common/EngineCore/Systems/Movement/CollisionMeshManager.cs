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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Responsible for managing the set of block entity collision meshes.
/// </summary>
public class CollisionMeshManager
{
    private readonly Dictionary<Tuple<GridPosition, int>, Task> generationTasks = new();
    private readonly ILogger<CollisionMeshManager> logger;
    private readonly ConcurrentDictionary<Tuple<GridPosition, int>, List<BoundingBox>> meshes = new();
    private readonly CollisionMeshFactory meshFactory;
    private readonly HashSet<Tuple<GridPosition, int>> scheduledUpdates = new();

    public CollisionMeshManager(CollisionMeshFactory meshFactory, ILogger<CollisionMeshManager> logger)
    {
        this.meshFactory = meshFactory;
        this.logger = logger;
    }

    /// <summary>
    ///     Gets any collision meshes associated with the given world segment and Z plane.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="z">Z index.</param>
    /// <param name="result">Collision meshes.</param>
    /// <returns>true if one or more meshes were found, false otherwise.</returns>
    public bool TryGetMeshes(GridPosition segmentIndex, int z, [NotNullWhen(true)] out List<BoundingBox>? result)
    {
        return meshes.TryGetValue(Tuple.Create(segmentIndex, z), out result) && result.Count > 0;
    }

    /// <summary>
    ///     Schedules regeneration of meshes for the given world segment and Z plane.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="z">Z coordinate of Z plane.</param>
    public void ScheduleUpdate(GridPosition segmentIndex, int z)
    {
        scheduledUpdates.Add(Tuple.Create(segmentIndex, z));
    }

    /// <summary>
    ///     Called at the start of a new tick.
    /// </summary>
    public void OnTick()
    {
        foreach (var info in scheduledUpdates)
            if (generationTasks.TryGetValue(info, out var task))
            {
                // If regeneration is already in progress, schedule another regeneration for when it is done.
                // Otherwise, just use the normal processing path.
                if (task.IsCompleted) DoFastGeneration(info);
                else generationTasks[info] = task.ContinueWith(_ => DoFullGeneration(info));
            }
            else
            {
                DoFastGeneration(info);
            }

        scheduledUpdates.Clear();
    }

    /// <summary>
    ///     Performs fast mesh generation which contains a separate collision slab for every block.
    /// </summary>
    /// <param name="key">World segment index and Z coordinate.</param>
    private void DoFastGeneration(Tuple<GridPosition, int> key)
    {
        meshes[key] = meshFactory.MakeBlockwiseSlabs(key.Item1, key.Item2, out var shouldSimplify);
        if (shouldSimplify) generationTasks[key] = Task.Run(() => DoFullGeneration(key));
    }

    /// <summary>
    ///     Performs full mesh generation which merges collision slabs where possible to reduce
    ///     the number of collision tests that need to be performed against the mesh.
    /// </summary>
    /// <param name="key">World segment index and Z coordinate.</param>
    private void DoFullGeneration(Tuple<GridPosition, int> key)
    {
        try
        {
            meshes[key] = meshFactory.MakeMergedSlabs(key.Item1, key.Item2);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in full mesh generation for {SegmentIndex} (Z={Z}).", key.Item1, key.Item2);
        }
    }
}