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
        private readonly EventSender eventSender;

        /// <summary>
        /// Input controller.
        /// </summary>
        private InputController inputController;

        public PlayerInputMovementMapper(EventSender eventSender, InputController inputController)
        {
            this.eventSender = eventSender;
            this.inputController = inputController;
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
            if (up || down || left || right)
            {
                /* Compute direction of the movement. */
                float dx = (right ? 1.0f : 0.0f) - (left ? 1.0f : 0.0f);
                float dy = (down ? 1.0f : 0.0f) - (up ? 1.0f : 0.0f);
                float norm = (float)Math.Sqrt(dx * dx + dy * dy);

                /* Set the player movement. */
                if (dx == 0.0 && dy == 0.0)
                {
                    inputController.StopPlayerMovement();
                }
                else
                {
                    inputController.SetPlayerMovement(dx / norm, dy / norm);
                }
            }
            else
            {
                /* Stop the player movement. */
                inputController.StopPlayerMovement();
            }
        }

    }

}
