/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Core.Logging;
using MessagePack;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.EngineCore.World;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Manages world segment summary block data for transmission to clients.
/// </summary>
public sealed class WorldSegmentBlockDataManager
{
    /// <summary>
    ///     Set of changed blocks whose segments need to be scheduled for regeneration.
    /// </summary>
    private readonly Queue<ulong> changedBlocks = new();

    /// <summary>
    ///     Summary block data for loaded world segments via their producer tasks.
    ///     A producer task is created when segment load is completed.
    ///     Updates are made via continuations of the tasks.
    ///     This allows the REST endpoint to await any segment data.
    /// </summary>
    private readonly ConcurrentDictionary<GridPosition, Task<byte[]?>> dataProducers = new();

    /// <summary>
    ///     Deletion tasks. The presence of a deletion task in this map indicates
    ///     that the deletion has been scheduled but is not yet complete.
    /// </summary>
    private readonly ConcurrentDictionary<GridPosition, Task> deletionTasks = new();

    private readonly WorldSegmentBlockDataGenerator generator;

    private readonly PositionComponentCollection positions;
    private readonly WorldSegmentResolver resolver;

    /// <summary>
    ///     Set of world segments to schedule for regeneration.
    /// </summary>
    private readonly HashSet<GridPosition> segmentsToRegenerate = new();

    public WorldSegmentBlockDataManager(
        WorldSegmentBlockDataGenerator generator,
        MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        PositionComponentCollection positions,
        WorldSegmentResolver resolver,
        EntityManager entityManager)
    {
        this.generator = generator;
        this.positions = positions;
        this.resolver = resolver;

        materials.OnComponentAdded += OnBlockChanged;
        materialModifiers.OnComponentAdded += OnBlockChanged;
        materials.OnComponentModified += OnBlockChanged;
        materialModifiers.OnComponentModified += OnBlockChanged;
        materials.OnComponentRemoved += ScheduleFromBlock;
        materialModifiers.OnComponentRemoved += ScheduleFromBlock;

        entityManager.OnUpdatesComplete += OnEndUpdates;
    }


    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Gets summary block data for the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Summary block data, or null if no summary block data is available.</returns>
    public Task<byte[]?>? GetWorldSegmentBlockData(GridPosition segmentIndex)
    {
        // If the segment is scheduled for regeneration, kick off the lazy load now that it's been requested.
        if (segmentsToRegenerate.Contains(segmentIndex))
        {
            AddWorldSegment(segmentIndex);
            segmentsToRegenerate.Remove(segmentIndex);
        }

        return dataProducers.TryGetValue(segmentIndex, out var data) ? data : null;
    }

    /// <summary>
    ///     Asynchronously adds a world segment to the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void AddWorldSegment(GridPosition segmentIndex)
    {
        if (deletionTasks.TryGetValue(segmentIndex, out var deletionTask))
            // Segment was rapidly unloaded and reloaded.
            // Schedule the reload for after the unload is complete.
            dataProducers[segmentIndex] = deletionTask.ContinueWith(task => DoAddWorldSegment(segmentIndex));
        else
            // Segment has not yet been loaded or has been fully unloaded.
            // Start a new processing chain from scratch for this segment.
            dataProducers[segmentIndex] = Task.Factory.StartNew(() => DoAddWorldSegment(segmentIndex));
    }

    /// <summary>
    ///     Asynchronously removes a world segment from the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void RemoveWorldSegment(GridPosition segmentIndex)
    {
        // If the world segment is already being removed, don't attempt to remove it again.
        if (deletionTasks.ContainsKey(segmentIndex)) return;

        // Immediately remove the segment from the data set, then schedule it for disposal.
        if (dataProducers.TryRemove(segmentIndex, out var currentTask))
            deletionTasks[segmentIndex] = currentTask.ContinueWith(
                task => DoRemoveWorldSegment(segmentIndex));
        else
            Logger.ErrorFormat("Tried to remove world segemnt data for {0} before it was added.", segmentIndex);
    }

    /// <summary>
    ///     Blocking call that adds a world segment to the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    private byte[]? DoAddWorldSegment(GridPosition segmentIndex)
    {
        try
        {
            Logger.DebugFormat("Adding summary block data for world segment {0}.", segmentIndex);
            var blockData = generator.Create(segmentIndex);
            return MessagePackSerializer.Serialize(blockData, MessageConfig.CompressedUntrustedMessagePackOptions);
        }
        catch (Exception e)
        {
            Logger.ErrorFormat(e, "Error adding summary block data for world segment {0}.", segmentIndex);
            return null;
        }
    }

    /// <summary>
    ///     Blocking call that removes a world segment from the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    private void DoRemoveWorldSegment(GridPosition segmentIndex)
    {
        // Clear deletion task.
        deletionTasks.TryRemove(segmentIndex, out var unused);
    }

    /// <summary>
    ///     Called when a block is added or modified.
    /// </summary>
    /// <param name="entityId">Block entity ID.</param>
    /// <param name="newValue">Unused.</param>
    private void OnBlockChanged(ulong entityId, int newValue)
    {
        // The block position may not be committed yet, so enqueue the block for processing after updates finish.
        changedBlocks.Enqueue(entityId);
    }

    /// <summary>
    ///     Called when a block is removed.
    /// </summary>
    /// <param name="entityId">Block entity ID.</param>
    private void ScheduleFromBlock(ulong entityId)
    {
        var lastPosition = positions.GetComponentForEntity(entityId, true);
        if (lastPosition.HasValue)
        {
            var segmentIndex = resolver.GetWorldSegmentForPosition(lastPosition.Value);
            segmentsToRegenerate.Add(segmentIndex);
        }
        else
        {
            Logger.ErrorFormat("Could not resolve position of removed block {0}.", entityId);
        }
    }

    /// <summary>
    ///     Called when a round of entity/component updates are complete and data generation can proceed.
    /// </summary>
    private void OnEndUpdates()
    {
        while (changedBlocks.TryDequeue(out var entityId)) ScheduleFromBlock(entityId);
    }
}