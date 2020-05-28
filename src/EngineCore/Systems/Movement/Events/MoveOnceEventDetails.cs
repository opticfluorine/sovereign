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

using ProtoBuf;
using Sovereign.EngineCore.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Systems.Movement.Events
{

    /// <summary>
    /// Event details associated with the Core_Movement event, which
    /// describes an attempt to move an entity relative to its current
    /// position.
    /// 
    /// If the entity is able to move to the new position (i.e. the entity
    /// is not fixed and is not somehow forbidden from the new position),
    /// the movement is successful. Otherwise no movement occurs.
    /// </summary>
    [ProtoContract]
    public class MoveOnceEventDetails : IEventDetails
    {

        /// <summary>
        /// Identifier of the entity that is moving.
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public ulong EntityId { get; set; }

        /// <summary>
        /// Unique movement phase used to filter expired events.
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public uint MovementPhase { get; set; }

    }

}
