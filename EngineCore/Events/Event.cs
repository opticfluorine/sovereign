/*
 * Engine8 Dynamic World MMORPG Engine
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

using System.Collections.Generic;

namespace Engine8.EngineCore.Events
{

    /// <summary>
    /// Interface implemented by event classes.
    /// </summary>
    public class Event
    {

        /// <summary>
        /// Time used to indicate that the event should be dispatched immediately.
        /// </summary>
        public const int TIME_IMMEDIATE = 0;

        /// <summary>
        /// Unique identifier for the event type.
        /// </summary>
        public EventId EventId { get; private set; }

        /// <summary>
        /// Time at which the event becomes valid, in microseconds.
        /// </summary>
        public ulong EventTime { get; private set; }

        /// <summary>
        /// Details associated with the event.
        /// </summary>
        public IEventDetails EventDetails { get; private set; }

        public Event(EventId eventId, ulong eventTime = TIME_IMMEDIATE)
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
            ulong eventTime = TIME_IMMEDIATE)
        {
            EventId = eventId;
            EventDetails = details;
            EventTime = eventTime;
        }

    }

}
