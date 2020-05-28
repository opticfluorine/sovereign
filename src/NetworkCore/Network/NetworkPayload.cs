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
using System.Text;

namespace Sovereign.NetworkCore.Network
{

    /// <summary>
    /// Payload of a network packet.
    /// </summary>
    [ProtoContract]
    public sealed class NetworkPayload
    {

        /// <summary>
        /// Nonce for this payload.
        /// </summary>
        /// <remarks>
        /// The nonce only needs to be unique for the connection.
        /// </remarks>
        [ProtoMember(0, IsRequired = true)]
        public uint Nonce { get; set; }

        /// <summary>
        /// Type of event in this packet.
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public EventId EventId { get; set; }

        /// <summary>
        /// Serialized packet details.
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public byte[] SerializedDetails { get; set; }

    }

}
