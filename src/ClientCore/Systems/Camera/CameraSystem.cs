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

        public void ExecuteOnce()
        {
            /* Poll for events. */
            while (EventCommunicator.GetIncomingEvent(out var ev))
            {
                eventHandler.HandleEvent(ev);
            }
        }

   }

}
