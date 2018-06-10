/*
 * Engine8 Dynamic World MMORPG Engine
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

using Engine8.EngineCore.Events;
using Engine8.EngineCore.Events.Details;
using Engine8.EngineCore.Systems.Movement.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Systems.Input
{

    /// <summary>
    /// Maps player input to movement events.
    /// </summary>
    public class PlayerInputMovementMapper
    {

        /// <summary>
        /// Event communicator.
        /// </summary>
        private EventSender eventSender;

        public PlayerInputMovementMapper(EventSender eventSender)
        {
            this.eventSender = eventSender;
        }

        /// <summary>
        /// Updates player movement based on player input.
        /// </summary>
        /// <param name="up">Input state for upward movement.</param>
        /// <param name="down">Input state for downward movement.</param>
        /// <param name="left">Input state for leftward movement.</param>
        /// <param name="right">Input state for rightward movement.</param>
        public void UpdateMovement(bool up, bool down, bool left, bool right)
        {
            /* Check whether movement has started or stopped. */
            Event ev;
            if (up || down || left || right)
            {
                /* Compute direction of the movement. */
                float dx = (right ? 1.0f : 0.0f) - (left ? 1.0f : 0.0f);
                float dy = (down ? 1.0f : 0.0f) - (up ? 1.0f : 0.0f);

                /* Fire a movement event. */
                var details = new SetVelocityEventDetails()
                {
                    EntityId = 0,
                    RateX = dx,
                    RateY = dy,
                };
                ev = new Event(EventId.Core_Set_Velocity, details);
            }
            else
            {
                var details = new EntityEventDetails()
                {
                    EntityId = 0,
                };
                ev = new Event(EventId.Core_End_Movement, details);
            }

            /* Fire the event. */
            eventSender.SendEvent(ev);
        }

    }

}
