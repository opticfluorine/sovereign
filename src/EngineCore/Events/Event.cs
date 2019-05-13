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
        /// <remarks>
        /// This constructor always produces a local event. Nonlocal
        /// (remote) events should be created with the copy constructor.
        /// </remarks>
        public Event(EventId eventId, IEventDetails details, 
            ulong eventTime = Immediate)
        {
            EventId = eventId;
            EventDetails = details;
            EventTime = eventTime;
            Local = true;
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
