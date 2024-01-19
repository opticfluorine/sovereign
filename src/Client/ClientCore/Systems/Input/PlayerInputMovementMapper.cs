/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Maps player input to movement events.
/// </summary>
public class PlayerInputMovementMapper
{
    /// <summary>
    ///     Event communicator.
    /// </summary>
    private readonly IEventSender eventSender;

    public PlayerInputMovementMapper(IEventSender eventSender)
    {
        this.eventSender = eventSender;
    }

    /// <summary>
    ///     Updates player movement based on player input.
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
            var dx = (right ? 1.0f : 0.0f) - (left ? 1.0f : 0.0f);
            var dy = (down ? 1.0f : 0.0f) - (up ? 1.0f : 0.0f);
            var norm = (float)Math.Sqrt(dx * dx + dy * dy);

            // TODO Set player movement
            // TODO Schedule repeat event if keyboard state unchanged
        }
        // TODO Stop player movement if no keys are pressed
    }
}