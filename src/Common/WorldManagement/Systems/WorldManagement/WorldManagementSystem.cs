/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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
using Sovereign.EngineCore.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldManagement.Systems.WorldManagement
{

    /// <summary>
    /// System responsible for managing the world state lifecycle.
    /// </summary>
    public sealed class WorldManagementSystem : ISystem
    {
        public EventCommunicator EventCommunicator { get; private set; }

        private readonly IEventLoop eventLoop;
        private readonly WorldManagementEventHandler eventHandler;

        public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>()
        {
            EventId.Core_WorldManagement_LoadSegment,
            EventId.Core_WorldManagement_UnloadSegment
        };

        public int WorkloadEstimate => 80;

        public WorldManagementSystem(EventCommunicator eventCommunicator,
            IEventLoop eventLoop, WorldManagementEventHandler eventHandler,
            EventDescriptions eventDescriptions)
        {
            /* Dependency injection. */
            EventCommunicator = eventCommunicator;
            this.eventLoop = eventLoop;
            this.eventHandler = eventHandler;

            /* Register events. */
            eventDescriptions.RegisterEvent<WorldSegmentEventDetails>(EventId.Core_WorldManagement_LoadSegment);
            eventDescriptions.RegisterEvent<WorldSegmentEventDetails>(EventId.Core_WorldManagement_UnloadSegment);
            eventDescriptions.RegisterEvent<WorldSegmentEventDetails>(EventId.Core_WorldManagement_WorldSegmentLoaded);

            /* Register system. */
            eventLoop.RegisterSystem(this);
        }

        public void Cleanup()
        {

        }

        public int ExecuteOnce()
        {
            var eventsProcessed = 0;
            while (EventCommunicator.GetIncomingEvent(out var ev))
            {
                eventHandler.HandleEvent(ev);
                eventsProcessed++;
            }

            return eventsProcessed;
        }

        public void Initialize()
        {

        }
    }
}
