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

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems.Block.Events;

namespace Sovereign.EngineCore.Systems.Block;

/// <summary>
///     Handles events for the block system.
/// </summary>
public sealed class BlockEventHandler
{
    private readonly BlockController controller;
    private readonly BlockManager manager;

    public BlockEventHandler(BlockController controller, BlockManager manager)
    {
        this.controller = controller;
        this.manager = manager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

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
                Logger.DebugFormat("Adding block {0}.", addDetails.BlockRecord);
                HandleAdd(addDetails);
                break;

            case EventId.Core_Block_AddBatch:
                var addBatchDetails = (BlockAddBatchEventDetails)ev.EventDetails!;
                Logger.DebugFormat("Adding {0} blocks.", addBatchDetails.BlockRecords.Count);
                HandleAddBatch(addBatchDetails);
                break;

            case EventId.Core_Block_Remove:
                var removeDetails = (EntityEventDetails)ev.EventDetails!;
                Logger.DebugFormat("Removing block {0}.", removeDetails.EntityId);
                HandleRemove(removeDetails);
                break;

            case EventId.Core_Block_RemoveBatch:
                var removeBatchDetails = (BlockRemoveBatchEventDetails)ev.EventDetails!;
                Logger.DebugFormat("Removing {0} blocks.", removeBatchDetails.EntityIds.Count);
                HandleRemoveBatch(removeBatchDetails);
                break;

            default:
                Logger.WarnFormat("Unexpected event ID {0}", ev.EventId);
                break;
        }
    }

    /// <summary>
    ///     Handles an add block event.
    /// </summary>
    /// <param name="eventDetails">Event details.</param>
    private void HandleAdd(BlockAddEventDetails eventDetails)
    {
        manager.AddBlock(eventDetails.BlockRecord);
    }

    /// <summary>
    ///     Handles an add batch event.
    /// </summary>
    /// <param name="eventDetails">Event details.</param>
    private void HandleAddBatch(BlockAddBatchEventDetails eventDetails)
    {
        foreach (var block in eventDetails.BlockRecords) manager.AddBlock(block);
        controller.ReturnAddBuffer(eventDetails.BlockRecords);
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
}