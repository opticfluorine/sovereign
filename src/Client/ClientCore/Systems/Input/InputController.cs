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

using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems.Movement.Events;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Exposes an API for reacting to user input.
/// </summary>
public class InputController
{
    /// <summary>
    ///     Event sender back to the event loop.
    /// </summary>
    private readonly IEventSender eventSender;

    public InputController(IEventSender eventSender)
    {
        this.eventSender = eventSender;
    }

    /// <summary>
    ///     Sets the movement of the player.
    /// </summary>
    /// <param name="dx">
    ///     Relative velocity along x as a multiple of the player base speed.
    /// </param>
    /// <param name="dy">
    ///     Relative velocity along y as a multiple of the player base speed.
    /// </param>
    public void SetPlayerMovement(float dx, float dy)
    {
        var details = new SetVelocityEventDetails
        {
            EntityId = 0,
            RateX = dx,
            RateY = dy
        };
        var ev = new Event(EventId.Core_Set_Velocity, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Stops all player movement.
    /// </summary>
    public void StopPlayerMovement()
    {
        var details = new EntityEventDetails
        {
            EntityId = 0
        };
        var ev = new Event(EventId.Core_End_Movement, details);
        eventSender.SendEvent(ev);
    }
}