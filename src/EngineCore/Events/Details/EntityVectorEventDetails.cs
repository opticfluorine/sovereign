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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Events.Details
{

    /// <summary>
    /// Reusable event details for events with an entity ID and a Vector3.
    /// </summary>
    [ProtoContract]
    public sealed class EntityVectorEventDetails : IEventDetails
    {

        /// <summary>
        /// Entity ID.
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public ulong EntityId { get; set; }

        /// <summary>
        /// Position.
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public Vector3 Vector { get; set; }

    }

}
