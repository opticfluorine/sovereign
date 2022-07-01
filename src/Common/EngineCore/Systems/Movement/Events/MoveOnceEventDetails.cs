/*
 * Sovereign Engine
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
