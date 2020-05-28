﻿/*
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
            EventCommunicator eventCommunicator,
            IEventLoop eventLoop,
            NetworkEventAdapter eventAdapter)
        {
            this.networkingService = networkingService;
            EventCommunicator = eventCommunicator;

            eventLoop.RegisterSystem(this);
        }

        public void Cleanup()
        {
            networkingService.Stop();
        }

        public int ExecuteOnce()
        {
            /* Process outgoing local events. */
            var eventsProcessed = 0;
            while (EventCommunicator.GetIncomingEvent(out var ev))
            {
                /* Discard nonlocal events. */
                if (!ev.Local) continue;

                /* Enqueue to send. */
                networkingService.EventsToSend.Enqueue(ev);
                eventsProcessed++;
            }

            return eventsProcessed;
        }

        public void Initialize()
        {
            networkingService.Start();
        }
    }

}
