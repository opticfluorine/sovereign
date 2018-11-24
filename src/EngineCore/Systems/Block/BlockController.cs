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
        public void AddBlock(EventSender eventSender, BlockRecord blockRecord,
            ulong eventTime = Event.Immediate)
        {
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
        public void AddBlocks(EventSender eventSender, Action<IList<BlockRecord>> recordProvider,
            ulong eventTime = Event.Immediate)
        {
            /* Populate the add records. */
            var recordList = batchAddPool.TakeObject();
            recordProvider(recordList);

            /* Send the event. */
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
        public void RemoveBlock(EventSender eventSender, ulong blockEntityId,
            ulong eventTime = Event.Immediate)
        {
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
        public void RemoveBlocks(EventSender eventSender, Action<IList<ulong>> blockEntityIdProvider,
            ulong eventTime = Event.Immediate)
        {
            /* Populate the buffer. */
            var removeList = batchRemovePool.TakeObject();
            blockEntityIdProvider(removeList);

            /* Send the event. */
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
