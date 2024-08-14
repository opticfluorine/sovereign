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
using System.Numerics;
using System.Threading.Tasks;
using Castle.Core.Logging;
using MessagePack;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.EngineCore.World;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Manages world segment summary block data for transmission to clients.
/// </summary>
public sealed class WorldSegmentBlockDataManager
{
    private readonly BlockController blockController;
    private readonly BlockPositionComponentCollection blockPositions;

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
    private readonly ConcurrentDictionary<GridPosition, Task<Tuple<WorldSegmentBlockData, byte[]>>>
        compressedDataProducers = new();

    /// <summary>
    ///     Deletion tasks. The presence of a deletion task in this map indicates
    ///     that the deletion has been scheduled but is not yet complete.
    /// </summary>
    private readonly ConcurrentDictionary<GridPosition, Task> deletionTasks = new();

    private readonly IEventSender eventSender;

    private readonly WorldSegmentBlockDataGenerator generator;

    private readonly WorldSegmentResolver resolver;

    /// <summary>
    ///     Set of world segments that need to be updated in the database.
    /// </summary>
    private readonly HashSet<GridPosition> segmentsToPersist = new();

    /// <summary>
    ///     Set of world segments to schedule for regeneration.
    /// </summary>
    private readonly HashSet<GridPosition> segmentsToRegenerate = new();

    public WorldSegmentBlockDataManager(
        WorldSegmentBlockDataGenerator generator,
        BlockPositionComponentCollection blockPositions,
        WorldSegmentResolver resolver,
        EntityManager entityManager,
        EntityTable entityTable,
        BlockController blockController,
        IEventSender eventSender)
    {
        this.generator = generator;
        this.blockPositions = blockPositions;
        this.resolver = resolver;
        this.blockController = blockController;
        this.eventSender = eventSender;

        blockPositions.OnComponentRemoved += OnBlockRemoved;
        entityManager.OnUpdatesComplete += OnEndUpdates;
        entityTable.OnTemplateSet += OnTemplateSet;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Gets summary block data for the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Summary block data, or null if no summary block data is available.</returns>
    public Task<Tuple<WorldSegmentBlockData, byte[]>> GetWorldSegmentBlockData(GridPosition segmentIndex)
    {
        // If the segment is scheduled for regeneration, kick off the lazy load now that it's been requested.
        lock (segmentsToRegenerate)
        {
            if (segmentsToRegenerate.Contains(segmentIndex))
            {
                AddWorldSegment(segmentIndex);
                segmentsToRegenerate.Remove(segmentIndex);
            }
        }

        if (!compressedDataProducers.TryGetValue(segmentIndex, out var data))
        {
            // If nothing is present, initialize a new record.
            AddWorldSegment(segmentIndex);
            data = compressedDataProducers[segmentIndex];
        }

        return data;
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
            compressedDataProducers[segmentIndex] = deletionTask.ContinueWith(_ => DoAddWorldSegment(segmentIndex));
        else
            // Segment has not yet been loaded or has been fully unloaded.
            // Start a new processing chain from scratch for this segment.
            compressedDataProducers[segmentIndex] = Task.Factory.StartNew(() => DoAddWorldSegment(segmentIndex));
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
        if (compressedDataProducers.TryRemove(segmentIndex, out var currentTask))
            deletionTasks[segmentIndex] = currentTask.ContinueWith(
                _ => DoRemoveWorldSegment(segmentIndex));
        else
            Logger.ErrorFormat("Tried to remove world segemnt data for {0} before it was added.", segmentIndex);
    }

    /// <summary>
    ///     Gets the list of world segments that need to be persisted to the database, then
    ///     clears the set so that the next call only returns the segments modified after the
    ///     current call.
    /// </summary>
    /// <returns>List of world segment indices that need to be persisted.</returns>
    public List<GridPosition> GetAndClearSegmentsToPersist()
    {
        lock (segmentsToPersist)
        {
            var list = new List<GridPosition>(segmentsToPersist);
            segmentsToPersist.Clear();
            return list;
        }
    }

    /// <summary>
    ///     Blocking call that adds a world segment to the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    private Tuple<WorldSegmentBlockData, byte[]> DoAddWorldSegment(GridPosition segmentIndex)
    {
        try
        {
            Logger.DebugFormat("Adding summary block data for world segment {0}.", segmentIndex);
            var blockData = generator.Create(segmentIndex);
            var bytes = MessagePackSerializer.Serialize(blockData, MessageConfig.CompressedUntrustedMessagePackOptions);
            return Tuple.Create(blockData, bytes);
        }
        catch (Exception e)
        {
            Logger.ErrorFormat(e, "Error adding summary block data for world segment {0}.", segmentIndex);
            return Tuple.Create(new WorldSegmentBlockData(), Array.Empty<byte>());
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
    ///     Called when the template of an entity is changed (excluding entity loads).
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="templateEntityId">New template entity ID.</param>
    private void OnTemplateSet(ulong entityId, ulong templateEntityId)
    {
        // Ignore if not a block entity.
        if (!blockPositions.HasComponentForEntity(entityId)) return;

        // Ensure that the bulk data for the affected world segment is updated on next request.
        changedBlocks.Enqueue(entityId);

        // This is only called if a block is added, not loaded. Therefore, the new block needs to
        // be advertised to any subscribed clients. Also, given the timing of the OnTemplateSet
        // event, the block position must already be committed to the ECS.
        blockController.NotifyBlockChanged(eventSender, blockPositions[entityId], templateEntityId);
    }

    /// <summary>
    ///     Called when a block is removed.
    /// </summary>
    /// <param name="entityId">Block entity ID.</param>
    /// <param name="isUnload">Unused.</param>
    private void OnBlockRemoved(ulong entityId, bool isUnload)
    {
        ScheduleFromBlock(entityId, isUnload);

        if (!isUnload)
        {
            // This is a true removal of a block, not just unloading it from memory.
            // Notify any subscribed clients of this update.
            var blockPosition = blockPositions.GetComponentWithLookback(entityId);
            blockController.NotifyBlockRemoved(eventSender, blockPosition);
        }
    }

    /// <summary>
    ///     Schedules a regeneration of block data for transfer if needed.
    /// </summary>
    /// <param name="entityId">Block entity ID.</param>
    /// <param name="isUnload">Unused.</param>
    private void ScheduleFromBlock(ulong entityId, bool isUnload)
    {
        // Ignore template entities.
        if (entityId is >= EntityConstants.FirstTemplateEntityId and <= EntityConstants.LastTemplateEntityId) return;

        try
        {
            var lastPosition = blockPositions.GetComponentWithLookback(entityId);
            var segmentIndex = resolver.GetWorldSegmentForPosition((Vector3)lastPosition);
            lock (segmentsToRegenerate)
            {
                segmentsToRegenerate.Add(segmentIndex);
            }

            lock (segmentsToPersist)
            {
                segmentsToPersist.Add(segmentIndex);
            }
        }
        catch (Exception e)
        {
            Logger.ErrorFormat(e, "Could not schedule block for entity {0}.", entityId);
        }
    }

    /// <summary>
    ///     Called when a round of entity/component updates are complete and data generation can proceed.
    /// </summary>
    private void OnEndUpdates()
    {
        while (changedBlocks.TryDequeue(out var entityId)) ScheduleFromBlock(entityId, false);
    }
}