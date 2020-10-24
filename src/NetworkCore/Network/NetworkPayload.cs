/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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
