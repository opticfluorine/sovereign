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
using System.Collections.Generic;

namespace Sovereign.EngineCore.Systems.Block
{

    /// <summary>
    /// System responsible for managing the block entities.
    /// </summary>
    public sealed class BlockSystem : ISystem, IDisposable
    {
        private readonly BlockEventHandler eventHandler;
        private readonly IEventLoop eventLoop;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public EventCommunicator EventCommunicator { get; private set; }

        public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>()
        {
            EventId.Core_Block_Add,
            EventId.Core_Block_AddBatch,
            EventId.Core_Block_Remove,
            EventId.Core_Block_RemoveBatch,
        };

        public int WorkloadEstimate => 50;

        public BlockSystem(BlockEventHandler eventHandler, IEventLoop eventLoop,
            EventCommunicator eventCommunicator, EventDescriptions eventDescriptions)
        {
            /* Dependency injection. */
            this.eventHandler = eventHandler;
            this.eventLoop = eventLoop;
            EventCommunicator = eventCommunicator;

            /* Register events. */
            eventDescriptions.RegisterEvent<BlockAddEventDetails>(EventId.Core_Block_Add);
            eventDescriptions.RegisterEvent<BlockAddBatchEventDetails>(EventId.Core_Block_AddBatch);
            eventDescriptions.RegisterEvent<EntityEventDetails>(EventId.Core_Block_Remove);
            eventDescriptions.RegisterEvent<BlockRemoveBatchEventDetails>(EventId.Core_Block_RemoveBatch);

            /* Register system. */
            eventLoop.RegisterSystem(this);
        }

        public void Dispose()
        {
            eventLoop.UnregisterSystem(this);
        }

        public void Initialize()
        {
        }

        public void Cleanup()
        {
        }

        public int ExecuteOnce()
        {
            var eventsProcessed = 0;
            while (EventCommunicator.GetIncomingEvent(out var ev))
            {
                Logger.DebugFormat("Received event with type {0}.", ev.EventId);
                eventHandler.HandleEvent(ev);
                eventsProcessed++;
            }

            return eventsProcessed;
        }

    }

}
