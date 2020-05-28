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
using System.Collections.Generic;

namespace Sovereign.EngineCore.Events
{

    /// <summary>
    /// Interface implemented by event classes.
    /// </summary>
    [ProtoContract]
    public class Event
    {

        /// <summary>
        /// Time used to indicate that the event should be dispatched immediately.
        /// </summary>
        public const ulong Immediate = 0;

        /// <summary>
        /// Unique identifier for the event type.
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public EventId EventId { get; private set; }

        /// <summary>
        /// Time at which the event becomes valid, in microseconds.
        /// </summary>
        /// <remarks>
        /// This field is not sent over the network as timed events will not
        /// be relayed until their trigger time is reached.
        /// </remarks>
        public ulong EventTime { get; set; }

        /// <summary>
        /// Flag indicating that the event originated locally
        /// (i.e. not from a remote client or server).
        /// </summary>
        /// <remarks>
        /// This field is not sent over the network as the value of the
        /// locality flag depends on the execution context.
        /// </remarks>
        public bool Local { get; set; }

        /// <summary>
        /// Details associated with the event.
        /// </summary>
        [ProtoMember(2, IsRequired = false)]
        public IEventDetails EventDetails { get; private set; }

        /// <summary>
        /// Default constructor, needed for Protobuf compatibility.
        /// </summary>
        public Event()
        {

        }

        public Event(EventId eventId, ulong eventTime = Immediate)
            : this(eventId, null, eventTime)
        {
        }

        /// <summary>
        /// Creates a new Event object.
        /// </summary>
        /// <param name="eventId">Event type ID.</param>
        /// <param name="details">Associated event details.</param>
        /// <param name="eventTime">
        /// System time (us) when this event should be dispatched.
        /// </param>
        public Event(EventId eventId, IEventDetails details, 
            ulong eventTime = Immediate, bool local = true)
        {
            EventId = eventId;
            EventDetails = details;
            EventTime = eventTime;
            Local = local;
        }

        /// <summary>
        /// Shallow copy constructor that updates the locality flag.
        /// </summary>
        /// <param name="other">Event to be copied.</param>
        /// <param name="local">New value for Local.</param>
        /// <remarks>
        /// This constructor performs a shallow copy only; the EventDetails
        /// object, if any, is not copied.
        /// </remarks>
        public Event(Event other, bool local = false)
        {
            EventId = other.EventId;
            EventDetails = other.EventDetails;
            EventTime = other.EventTime;
            Local = local;
        }

    }

}
