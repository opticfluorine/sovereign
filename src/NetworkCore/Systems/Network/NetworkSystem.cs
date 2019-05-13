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
using Sovereign.EngineCore.Systems;
using Sovereign.NetworkCore.Network.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.NetworkCore.Systems.Network
{

    /// <summary>
    /// System that connects the event loop to the networking service.
    /// </summary>
    public sealed class NetworkSystem : ISystem
    {
        private readonly NetworkingService networkingService;
        private readonly IEventSender eventSender;

        public EventCommunicator EventCommunicator { get; private set; }

        /// <summary>
        /// Event IDs that are candidates to be replicated onto the network.
        /// </summary>
        public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>()
        {
            EventId.Core_Set_Velocity,
            EventId.Core_Move_Once,
            EventId.Core_End_Movement
        };

        public int WorkloadEstimate => 50;

        public NetworkSystem(NetworkingService networkingService,
            IEventSender eventSender)
        {
            this.networkingService = networkingService;
            this.eventSender = eventSender;
        }

        public void Cleanup()
        {
            networkingService.Stop();
        }

        public void ExecuteOnce()
        {
            /* Process outgoing local events. */
            while (EventCommunicator.GetIncomingEvent(out var ev))
            {
                /* Discard nonlocal events. */
                if (!ev.Local) continue;

                /* Enqueue to send. */
                networkingService.EventsToSend.Enqueue(ev);
            }

            /* Inject incoming events. */
            while (networkingService.ReceivedEvents.TryDequeue(out var ev))
            {
                eventSender.SendEvent(ev);
            }
        }

        public void Initialize()
        {
            networkingService.Start();
        }
    }

}
