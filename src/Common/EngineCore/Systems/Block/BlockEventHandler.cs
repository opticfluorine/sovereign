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

using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems.WorldManagement;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.EngineCore.Systems.Block;

/// <summary>
///     Handles events for the block system.
/// </summary>
public sealed class BlockEventHandler
{
    private readonly BlockGridPositionIndexer blockGridPositionIndexer;
    private readonly BlockController controller;
    private readonly CoreWorldManagementController coreWorldManagementController;
    private readonly IEventSender eventSender;
    private readonly ILogger<BlockEventHandler> logger;
    private readonly BlockManager manager;
    private readonly BlockNoticeProcessor noticeProcessor;

    public BlockEventHandler(BlockController controller, BlockManager manager,
        BlockGridPositionIndexer blockGridPositionIndexer, BlockNoticeProcessor noticeProcessor,
        IEventSender eventSender, CoreWorldManagementController coreWorldManagementController,
        ILogger<BlockEventHandler> logger)
    {
        this.controller = controller;
        this.manager = manager;
        this.blockGridPositionIndexer = blockGridPositionIndexer;
        this.noticeProcessor = noticeProcessor;
        this.eventSender = eventSender;
        this.coreWorldManagementController = coreWorldManagementController;
        this.logger = logger;
    }

    /// <summary>
    ///     Handles a block-related event.
    /// </summary>
    /// <param name="ev">Event to handle.</param>
    public void HandleEvent(Event ev)
    {
        switch (ev.EventId)
        {
            case EventId.Core_Block_Add:
                var addDetails = (BlockAddEventDetails)ev.EventDetails!;
                logger.LogDebug("Adding block {Block}.", addDetails.BlockRecord);
                HandleAdd(addDetails);
                break;

            case EventId.Core_Block_AddBatch:
                var addBatchDetails = (BlockAddBatchEventDetails)ev.EventDetails!;
                logger.LogDebug("Adding {Block} blocks.", addBatchDetails.BlockRecords.Count);
                HandleAddBatch(addBatchDetails);
                break;

            case EventId.Core_Block_Remove:
                var removeDetails = (EntityEventDetails)ev.EventDetails!;
                logger.LogDebug("Removing block {Id}.", removeDetails.EntityId);
                HandleRemove(removeDetails);
                break;

            case EventId.Core_Block_RemoveBatch:
                var removeBatchDetails = (BlockRemoveBatchEventDetails)ev.EventDetails!;
                logger.LogDebug("Removing {Count} blocks.", removeBatchDetails.EntityIds.Count);
                HandleRemoveBatch(removeBatchDetails);
                break;

            case EventId.Core_Block_RemoveAt:
            {
                if (ev.EventDetails is not GridPositionEventDetails details)
                {
                    logger.LogWarning("Received RemoveAt event without details.");
                    break;
                }

                HandleRemoveAt(details.GridPosition);
                break;
            }

            case EventId.Core_Block_ModifyNotice:
            {
                // Ignore local notices since they reflect current local state.
                if (ev.Local) break;

                if (ev.EventDetails is not BlockAddEventDetails details)
                {
                    logger.LogWarning("Received ModifyNotice without details.");
                    break;
                }

                HandleModifyNotice(details);
                break;
            }

            case EventId.Core_Block_RemoveNotice:
            {
                // Ignore local notices since they reflect current local state.
                if (ev.Local) break;

                if (ev.EventDetails is not GridPositionEventDetails details)
                {
                    logger.LogWarning("Received RemoveNotice without details.");
                    break;
                }

                HandleRemoveNotice(details);
                break;
            }

            default:
                logger.LogWarning("Unexpected event ID {Id}", ev.EventId);
                break;
        }
    }

    /// <summary>
    ///     Handles a remove-at-position event.
    /// </summary>
    /// <param name="position"></param>
    private void HandleRemoveAt(GridPosition position)
    {
        var entities = blockGridPositionIndexer.GetEntitiesAtPosition(position);
        if (entities == null) return;

        foreach (var entityId in entities.Keys) manager.RemoveBlock(entityId);
    }

    /// <summary>
    ///     Handles an add block event.
    /// </summary>
    /// <param name="eventDetails">Event details.</param>
    private void HandleAdd(BlockAddEventDetails eventDetails)
    {
        manager.AddBlock(eventDetails.BlockRecord, false);
    }

    /// <summary>
    ///     Handles an add batch event.
    /// </summary>
    /// <param name="eventDetails">Event details.</param>
    private void HandleAddBatch(BlockAddBatchEventDetails eventDetails)
    {
        // Process blocks.
        foreach (var block in eventDetails.BlockRecords) manager.AddBlock(block, eventDetails.IsLoad);
        controller.ReturnAddBuffer(eventDetails.BlockRecords);

        // Announce world segment load if needed.
        if (eventDetails.IsWorldSegment)
            coreWorldManagementController.AnnounceWorldSegmentLoaded(eventSender, eventDetails.SegmentIndex);
    }

    /// <summary>
    ///     Handles a remove block event.
    /// </summary>
    /// <param name="eventDetails">Event details.</param>
    private void HandleRemove(EntityEventDetails eventDetails)
    {
        manager.RemoveBlock(eventDetails.EntityId);
    }

    /// <summary>
    ///     Handles a remove batch event.
    /// </summary>
    /// <param name="eventDetails">Event details.</param>
    private void HandleRemoveBatch(BlockRemoveBatchEventDetails eventDetails)
    {
        foreach (var blockEntityId in eventDetails.EntityIds) manager.RemoveBlock(blockEntityId);
        controller.ReturnRemoveBuffer(eventDetails.EntityIds);
    }

    /// <summary>
    ///     Handles a block modification notice received over the network.
    /// </summary>
    /// <param name="details">Details.</param>
    private void HandleModifyNotice(BlockAddEventDetails details)
    {
        noticeProcessor.ProcessModifyNotice(details.BlockRecord);
    }

    /// <summary>
    ///     Handles a block removal notice received over the network.
    /// </summary>
    /// <param name="details">Details.</param>
    private void HandleRemoveNotice(GridPositionEventDetails details)
    {
        noticeProcessor.ProcessRemoveNotice(details.GridPosition);
    }
}