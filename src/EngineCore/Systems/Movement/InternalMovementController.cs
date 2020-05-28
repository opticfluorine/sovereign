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
using Sovereign.EngineCore.Systems.Movement.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Systems.Movement
{

    /// <summary>
    /// Defines a private API for use within the MovementSystem to
    /// control the movement of entities.
    /// </summary>
    public class InternalMovementController
    {

        /// <summary>
        /// Event sender used for communication with the event loop.
        /// </summary>
        private readonly IEventSender eventSender;

        public InternalMovementController(IEventSender eventSender)
        {
            this.eventSender = eventSender;
        }

        /// <summary>
        /// Schedules a movement of an entity to occur at the given time.
        /// 
        /// This method is intended to be used within the MovementSystem only.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="movementPhase">Unique movement phase identifier.</param>
        /// <param name="movementTime">System time of the movement, in us.</param>
        internal void ScheduleMovement(ulong entityId, uint movementPhase,
            ulong movementTime)
        {
            var details = new MoveOnceEventDetails()
            {
                EntityId = entityId,
                MovementPhase = movementPhase,
            };
            var ev = new Event(EventId.Core_Move_Once, details, movementTime);

            eventSender.SendEvent(ev);
        }

    }
}
