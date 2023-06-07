/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
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

using Sovereign.ClientCore.Events;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;
using System;
using System.Collections.Generic;

namespace Sovereign.ClientCore.Systems.ClientNetwork
{

    /// <summary>
    /// System responsible for managing the client network resources.
    /// </summary>
    public sealed class ClientNetworkSystem : ISystem, IDisposable
    {
        private readonly IEventLoop eventLoop;
        private readonly ClientNetworkEventHandler eventHandler;

        public EventCommunicator EventCommunicator { get; private set; }

        public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>
        {
            EventId.Client_Network_ConnectionLost,
            EventId.Client_Network_BeginConnection,
        };

        public int WorkloadEstimate => 20;

        public ClientNetworkSystem(EventCommunicator eventCommunicator,
            IEventLoop eventLoop, EventDescriptions eventDescriptions,
            ClientNetworkEventHandler eventHandler)
        {
            this.eventLoop = eventLoop;
            this.eventHandler = eventHandler;
            EventCommunicator = eventCommunicator;

            eventDescriptions.RegisterNullEvent(EventId.Client_Network_ConnectionLost);
            eventDescriptions.RegisterEvent<BeginConnectionEventDetails>(EventId.Client_Network_BeginConnection);
            eventDescriptions.RegisterEvent<ErrorEventDetails>(EventId.Client_Network_ConnectionAttemptFailed);
            eventDescriptions.RegisterEvent<ErrorEventDetails>(EventId.Client_Network_LoginFailed);

            eventLoop.RegisterSystem(this);
        }
        public void Initialize()
        {
        }

        public void Cleanup()
        {
        }

        public void Dispose()
        {
            eventLoop.UnregisterSystem(this);
        }

        public int ExecuteOnce()
        {
            /* Poll for events. */
            var eventsProcessed = 0;
            while (EventCommunicator.GetIncomingEvent(out var ev))
            {
                eventHandler.HandleEvent(ev);
                eventsProcessed++;
            }

            return eventsProcessed;
        }

    }
}
