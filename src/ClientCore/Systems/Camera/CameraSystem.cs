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

using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Systems.Camera
{

    /// <summary>
    /// System responsible for managing the camera.
    /// </summary>
    public sealed class CameraSystem : ISystem, IDisposable
    {
        private readonly CameraManager cameraManager;
        private readonly CameraEventHandler eventHandler;
        private readonly IEventLoop eventLoop;

        public int WorkloadEstimate => 20;

        public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>
        {
            EventId.Client_Camera_Attach,
            EventId.Client_Camera_Detach,
            EventId.Core_Tick
        };

        public CameraSystem(CameraManager cameraManager, CameraEventHandler eventHandler,
            IEventLoop eventLoop, EventCommunicator eventCommunicator, 
            EventDescriptions eventDescriptions)
        {
            /* Dependency injection. */
            this.cameraManager = cameraManager;
            this.eventHandler = eventHandler;
            this.eventLoop = eventLoop;
            EventCommunicator = eventCommunicator;

            /* Register events. */
            eventDescriptions.RegisterEvent<EntityEventDetails>(EventId.Client_Camera_Attach);
            eventDescriptions.RegisterNullEvent(EventId.Client_Camera_Detach);

            /* Register system. */
            eventLoop.RegisterSystem(this);
        }

        public void Dispose()
        {
            eventLoop.UnregisterSystem(this);
        }

        public EventCommunicator EventCommunicator { get; private set; }

        public void Initialize()
        {
            cameraManager.Initialize();
        }
 

        public void Cleanup()
        {
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
