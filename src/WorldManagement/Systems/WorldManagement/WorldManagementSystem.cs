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
