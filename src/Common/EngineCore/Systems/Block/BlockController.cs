/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineUtil.Collections;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.EngineCore.Systems.Block;

/// <summary>
///     Provides a public API to the block system.
/// </summary>
public class BlockController
{
    /// <summary>
    ///     Default size of add and remove batch buffers.
    /// </summary>
    public const int DefaultBufferSize = 128;

    /// <summary>
    ///     Reusable object pool to manage batch add buffers.
    /// </summary>
    private readonly ObjectPool<List<BlockRecord>> batchAddPool = new(() => new List<BlockRecord>(DefaultBufferSize));

    /// <summary>
    ///     Reusable object pool to manage batch remove buffers.
    /// </summary>
    private readonly ObjectPool<List<ulong>> batchRemovePool = new(() => new List<ulong>(DefaultBufferSize));

    private readonly ILogger<BlockController> logger;

    public BlockController(ILogger<BlockController> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    ///     Adds a single block and sends a subsequent block change notification.
    /// </summary>
    /// <param name="eventSender">Event sender for the calling thread.</param>
    /// <param name="blockRecord">Block to add.</param>
    /// <param name="eventTime">System time to dispatch event, in microseconds.</param>
    public void AddBlock(IEventSender eventSender, BlockRecord blockRecord,
        ulong eventTime = Event.Immediate)
    {
        logger.LogDebug("Requesting to add block {Block}.", blockRecord.ToString());

        var details = new BlockAddEventDetails
        {
            BlockRecord = blockRecord
        };
        var ev = new Event(EventId.Core_Block_Add, details, eventTime);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Loads a set of blocks as the contents of a world segment, then announces the segment load
    ///     upon completion.
    /// </summary>
    /// <param name="eventSender">Event sender for the calling thread.</param>
    /// <param name="recordProvider">Function that populates the list of blocks to create.</param>
    /// <param name="segmentIndex">World segment index.</param>
    public void AddBlocksForWorldSegment(IEventSender eventSender, Action<IList<BlockRecord>> recordProvider,
        GridPosition segmentIndex)
    {
        /* Populate the add records. */
        var recordList = batchAddPool.TakeObject();
        recordList.Clear();
        recordProvider(recordList);

        logger.LogDebug("Requesting to add {Count} blocks.", recordList.Count);
        var details = new BlockAddBatchEventDetails
        {
            BlockRecords = recordList,
            IsLoad = true,
            IsWorldSegment = true,
            SegmentIndex = segmentIndex
        };
        var ev = new Event(EventId.Core_Block_AddBatch, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Removes the block with the given entity ID, sending a block removal notification.
    /// </summary>
    /// <param name="eventSender">Event sender for the calling thread.</param>
    /// <param name="blockEntityId">Entity ID of the block to remove.</param>
    /// <param name="eventTime">System time to dispatch event, in microseconds.</param>
    public void RemoveBlock(IEventSender eventSender, ulong blockEntityId,
        ulong eventTime = Event.Immediate)
    {
        logger.LogDebug("Requesting to remove block {Id:X}.", blockEntityId);
        var details = new EntityEventDetails { EntityId = blockEntityId };
        var ev = new Event(EventId.Core_Block_Remove, details, eventTime);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Removes multiple blocks at once without sending block removal notifications.
    /// </summary>
    /// <param name="eventSender">Event sender for the calling thread.</param>
    /// <param name="blockEntityIdProvider">Function that populates the list of entity IDs to remove.</param>
    /// <param name="eventTime">System time to dispatch event, in microseconds.</param>
    public void RemoveBlocks(IEventSender eventSender, Action<IList<ulong>> blockEntityIdProvider,
        ulong eventTime = Event.Immediate)
    {
        /* Populate the buffer. */
        var removeList = batchRemovePool.TakeObject();
        blockEntityIdProvider(removeList);

        /* Send the event. */
        logger.LogDebug("Requesting to remove {Count} blocks.", removeList.Count);
        var details = new BlockRemoveBatchEventDetails { EntityIds = removeList };
        var ev = new Event(EventId.Core_Block_RemoveBatch, details, eventTime);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Removes the block at the given position, then sends a subsequent block removal notification.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="position">Position.</param>
    public void RemoveBlockAtPosition(IEventSender eventSender, GridPosition position)
    {
        var details = new GridPositionEventDetails
        {
            GridPosition = position
        };
        var ev = new Event(EventId.Core_Block_RemoveAt, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sends a notification of a block change (add or modify, but not load/unload).
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="blockPosition">Block position.</param>
    /// <param name="templateEntityId">Block template entity ID.</param>
    public void NotifyBlockChanged(IEventSender eventSender, GridPosition blockPosition, ulong templateEntityId)
    {
        var details = new BlockAddEventDetails
        {
            BlockRecord = new BlockRecord
            {
                Position = blockPosition,
                TemplateEntityId = templateEntityId
            }
        };
        var ev = new Event(EventId.Core_Block_ModifyNotice, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sends a notification of a block removal (not unload).
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="blockPosition">Block position.</param>
    public void NotifyBlockRemoved(IEventSender eventSender, GridPosition blockPosition)
    {
        var details = new GridPositionEventDetails { GridPosition = blockPosition };
        var ev = new Event(EventId.Core_Block_RemoveNotice, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Returns an add buffer to the object pool. This should only be called internally.
    /// </summary>
    /// <param name="addBuffer">Buffer to return.</param>
    internal void ReturnAddBuffer(List<BlockRecord> addBuffer)
    {
        batchAddPool.ReturnObject(addBuffer);
    }

    /// <summary>
    ///     Returns a remove buffer to the object pool. This should only be called internally.
    /// </summary>
    /// <param name="removeBuffer">Buffer to return.</param>
    internal void ReturnRemoveBuffer(List<ulong> removeBuffer)
    {
        batchRemovePool.ReturnObject(removeBuffer);
    }
}