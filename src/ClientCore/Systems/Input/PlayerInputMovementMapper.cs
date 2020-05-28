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
using Sovereign.EngineCore.Systems.Movement.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Systems.Input
{

    /// <summary>
    /// Maps player input to movement events.
    /// </summary>
    public class PlayerInputMovementMapper
    {

        /// <summary>
        /// Event communicator.
        /// </summary>
        private readonly IEventSender eventSender;

        /// <summary>
        /// Input controller.
        /// </summary>
        private InputController inputController;

        public PlayerInputMovementMapper(IEventSender eventSender, InputController inputController)
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
