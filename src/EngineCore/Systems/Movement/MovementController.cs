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
using Sovereign.EngineCore.Systems.Movement.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Systems.Movement
{

    /// <summary>
    /// Provides a public API for controlling the movement of entities.
    /// </summary>
    public class MovementController
    {

        /// <summary>
        /// Event sender used for communication with the event loop.
        /// </summary>
        private readonly EventSender eventSender;

        public MovementController(EventSender eventSender)
        {
            this.eventSender = eventSender;
        }

        /// <summary>
        /// Sets the movement for the given entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rateX"></param>
        /// <param name="rateY"></param>
        /// <param name="eventTime"></param>
        public void SetMovement(ulong entity, float rateX, float rateY,
            ulong eventTime=Event.Immediate)
        {
            var details = new SetVelocityEventDetails()
            {
                EntityId = entity,
                RateX = rateX,
                RateY = rateY,
            };
            var ev = new Event(EventId.Core_Set_Velocity, details, eventTime);
        }

    }

}
