/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems.Block.Events;
using System;

namespace Sovereign.EngineCore.Systems.Block
{

    /// <summary>
    /// Handles events for the block system.
    /// </summary>
    public sealed class BlockEventHandler
    {
        private readonly BlockController controller;
        private readonly BlockManager manager;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public BlockEventHandler(BlockController controller, BlockManager manager)
        {
            this.controller = controller;
            this.manager = manager;
        }

        /// <summary>
        /// Handles a block-related event.
        /// </summary>
        /// <param name="ev">Event to handle.</param>
        public void HandleEvent(Event ev)
        {
            switch (ev.EventId)
            {
                case EventId.Core_Block_Add:
                    var addDetails = (BlockAddEventDetails)ev.EventDetails;
                    Logger.DebugFormat("Adding block {0}.", addDetails.BlockRecord);
                    HandleAdd(addDetails);
                    break;

                case EventId.Core_Block_AddBatch:
                    var addBatchDetails = (BlockAddBatchEventDetails)ev.EventDetails;
                    Logger.DebugFormat("Adding {0} blocks.", addBatchDetails.BlockRecords.Count);
                    HandleAddBatch(addBatchDetails);
                    break;

                case EventId.Core_Block_Remove:
                    var removeDetails = (EntityEventDetails)ev.EventDetails;
                    Logger.DebugFormat("Removing block {0}.", removeDetails.EntityId);
                    HandleRemove(removeDetails);
                    break;

                case EventId.Core_Block_RemoveBatch:
                    var removeBatchDetails = (BlockRemoveBatchEventDetails)ev.EventDetails;
                    Logger.DebugFormat("Removing {0} blocks.", removeBatchDetails.EntityIds.Count);
                    HandleRemoveBatch(removeBatchDetails);
                    break;

                default:
                    Logger.WarnFormat("Unexpected event ID {0}", ev.EventId);
                    break;
            }
        }

        /// <summary>
        /// Handles an add block event.
        /// </summary>
        /// <param name="eventDetails">Event details.</param>
        private void HandleAdd(BlockAddEventDetails eventDetails)
        {
            manager.AddBlock(eventDetails.BlockRecord);
        }

        /// <summary>
        /// Handles an add batch event.
        /// </summary>
        /// <param name="eventDetails">Event details.</param>
        private void HandleAddBatch(BlockAddBatchEventDetails eventDetails)
        {
            foreach (var block in eventDetails.BlockRecords)
            {
                manager.AddBlock(block);
            }
            controller.ReturnAddBuffer(eventDetails.BlockRecords);
        }

        /// <summary>
        /// Handles a remove block event.
        /// </summary>
        /// <param name="eventDetails">Event details.</param>
        private void HandleRemove(EntityEventDetails eventDetails)
        {
            manager.RemoveBlock(eventDetails.EntityId);
        }

        /// <summary>
        /// Handles a remove batch event.
        /// </summary>
        /// <param name="eventDetails">Event details.</param>
        private void HandleRemoveBatch(BlockRemoveBatchEventDetails eventDetails)
        {
            foreach (var blockEntityId in eventDetails.EntityIds)
            {
                manager.RemoveBlock(blockEntityId);
            }
            controller.ReturnRemoveBuffer(eventDetails.EntityIds);
        }
    }
}
