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
using Sovereign.EngineCore.Systems.Movement.Events;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Provides a public API for controlling the movement of entities.
/// </summary>
public class MovementController
{
    /// <summary>
    ///     Event sender used for communication with the event loop.
    /// </summary>
    private readonly IEventSender eventSender;

    public MovementController(IEventSender eventSender)
    {
        this.eventSender = eventSender;
    }

    /// <summary>
    ///     Sets the movement for the given entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="rateX"></param>
    /// <param name="rateY"></param>
    /// <param name="eventTime"></param>
    public void SetMovement(ulong entity, float rateX, float rateY,
        ulong eventTime = Event.Immediate)
    {
        var details = new SetVelocityEventDetails
        {
            EntityId = entity,
            RateX = rateX,
            RateY = rateY
        };
        var ev = new Event(EventId.Core_Set_Velocity, details, eventTime);
    }
}