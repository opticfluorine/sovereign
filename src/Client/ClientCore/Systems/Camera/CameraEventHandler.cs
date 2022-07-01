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

namespace Sovereign.ClientCore.Systems.Camera
{

    /// <summary>
    /// Responsible for handling camera-related events.
    /// </summary>
    public sealed class CameraEventHandler
    {
        private readonly CameraManager manager;

        public CameraEventHandler(CameraManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Handles a camera-related event.
        /// </summary>
        /// <param name="ev">Event.</param>
        public void HandleEvent(Event ev)
        {
            switch (ev.EventId)
            {
                case EventId.Client_Camera_Attach:
                    HandleAttachEvent((EntityEventDetails)ev.EventDetails);
                    break;

                case EventId.Client_Camera_Detach:
                    HandleDetachEvent();
                    break;

                case EventId.Core_Tick:
                    manager.UpdateCamera();
                    break;
            }
        }

        /// <summary>
        /// Attaches the camera to an entity.
        /// </summary>
        /// <param name="details">Event details.</param>
        private void HandleAttachEvent(EntityEventDetails details)
        {
            manager.SetCameraState(true, details.EntityId);
            manager.UpdateCamera();
        }

        /// <summary>
        /// Detaches the camera from an entity.
        /// </summary>
        private void HandleDetachEvent()
        {
            manager.SetCameraState(false, 0);
            manager.UpdateCamera();
        }

    }

}
