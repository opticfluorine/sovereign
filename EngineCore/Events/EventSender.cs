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

using System.Collections.Concurrent;

namespace Engine8.EngineCore.Events
{

    /// <summary>
    /// Sends events from one or more systems to the event loop.
    /// </summary>
    public class EventSender
    {

        /// <summary>
        /// Events being sent from the system thread to the event loop.
        /// </summary>
        private readonly ConcurrentQueue<Event> outgoingEvents
            = new ConcurrentQueue<Event>();

        /// <summary>
        /// Sends an event to the event loop.
        /// </summary>
        /// <param name="ev">Event to be sent.</param>
        public void SendEvent(Event ev)
        {
            outgoingEvents.Enqueue(ev);
        }

        /// <summary>
        /// Gets the next event being sent to the event loop.
        /// This method does not block.
        /// </summary>
        /// <returns>Next event being sent to the event loop, or null if none is available.</returns>
        public Event GetOutgoingEvent()
        {
            Event ev;
            bool hasEvent = outgoingEvents.TryDequeue(out ev);
            return hasEvent ? ev : null;
        }

    }

}
