/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems.Block.Events;
using Sovereign.EngineUtil.Collections;
using System;
using System.Collections.Generic;

namespace Sovereign.EngineCore.Systems.Block
{

    /// <summary>
    /// Provides a public API to the block system.
    /// </summary>
    public class BlockController
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Default size of add and remove batch buffers.
        /// </summary>
        public const int DefaultBufferSize = 128;

        /// <summary>
        /// Reusable object pool to manage batch add buffers.
        /// </summary>
        private ObjectPool<IList<BlockRecord>> batchAddPool
            = new ObjectPool<IList<BlockRecord>>(() => new List<BlockRecord>(DefaultBufferSize));

        /// <summary>
        /// Reusable object pool to manage batch remove buffers.
        /// </summary>
        private ObjectPool<IList<ulong>> batchRemovePool
            = new ObjectPool<IList<ulong>>(() => new List<ulong>(DefaultBufferSize));

        /// <summary>
        /// Adds a single block.
        /// </summary>
        /// <param name="eventSender">Event sender for the calling thread.</param>
        /// <param name="blockRecord">Block to add.</param>
        /// <param name="eventTime">System time to dispatch event, in microseconds.</param>
        public void AddBlock(IEventSender eventSender, BlockRecord blockRecord,
            ulong eventTime = Event.Immediate)
        {
            Logger.DebugFormat("Requesting to add block {0}.", blockRecord.ToString());

            var details = new BlockAddEventDetails()
            {
                BlockRecord = blockRecord
            };
            var ev = new Event(EventId.Core_Block_Add, details, eventTime);
            eventSender.SendEvent(ev);
        }

        /// <summary>
        /// Adds multiple blocks at once.
        /// </summary>
        /// <param name="eventSender">Event sender for the calling thread.</param>
        /// <param name="recordProvider">Function that populates the list of blocks to create.</param>
        /// <param name="eventTime">System time to dispatch event, in microseconds.</param>
        public void AddBlocks(IEventSender eventSender, Action<IList<BlockRecord>> recordProvider,
            ulong eventTime = Event.Immediate)
        {
            /* Populate the add records. */
            var recordList = batchAddPool.TakeObject();
            recordProvider(recordList);

            /* Send the event. */
            Logger.DebugFormat("Requesting to add {0} blocks.", recordList.Count);
            var details = new BlockAddBatchEventDetails()
            {
                BlockRecords = recordList
            };
            var ev = new Event(EventId.Core_Block_AddBatch, details, eventTime);
            eventSender.SendEvent(ev);
        }

        /// <summary>
        /// Removes the block with the given entity ID.
        /// </summary>
        /// <param name="eventSender">Event sender for the calling thread.</param>
        /// <param name="blockEntityId">Entity ID of the block to remove.</param>
        /// <param name="eventTime">System time to dispatch event, in microseconds.</param>
        public void RemoveBlock(IEventSender eventSender, ulong blockEntityId,
            ulong eventTime = Event.Immediate)
        {
            Logger.DebugFormat("Requesting to remove block {0}.", blockEntityId);
            var details = new EntityEventDetails() { EntityId = blockEntityId };
            var ev = new Event(EventId.Core_Block_Remove, details, eventTime);
            eventSender.SendEvent(ev);
        }

        /// <summary>
        /// Removes multiple blocks at once.
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
            Logger.DebugFormat("Requesting to remove {0} blocks.", removeList.Count);
            var details = new BlockRemoveBatchEventDetails() { EntityIds = removeList };
            var ev = new Event(EventId.Core_Block_RemoveBatch, details, eventTime);
            eventSender.SendEvent(ev);
        }

        /// <summary>
        /// Returns an add buffer to the object pool. This should only be called internally.
        /// </summary>
        /// <param name="addBuffer">Buffer to return.</param>
        internal void ReturnAddBuffer(IList<BlockRecord> addBuffer)
        {
            batchAddPool.ReturnObject(addBuffer);
        }

        /// <summary>
        /// Returns a remove buffer to the object pool. This should only be called internally.
        /// </summary>
        /// <param name="removeBuffer">Buffer to return.</param>
        internal void ReturnRemoveBuffer(IList<ulong> removeBuffer)
        {
            batchRemovePool.ReturnObject(removeBuffer);
        }

    }

}
