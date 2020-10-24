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

using System.Collections.Concurrent;

namespace Sovereign.EngineCore.Events
{

    /// <summary>
    /// Provides interthread communication from the event loop to the system
    /// threads.
    /// </summary>
    public class EventCommunicator
    {

        /// <summary>
        /// Events being sent from the event loop to the system thread.
        /// </summary>
        private readonly ConcurrentQueue<Event> incomingEvents
            = new ConcurrentQueue<Event>();

        /// <summary>
        /// Tries to get the next event being sent to the system thread.
        /// If no event is available, this returns false and the value of
        /// ev is undefined.
        /// </summary>
        /// <param name="ev">Reference to hold the next event.</param>
        /// <returns>true if an event was retrieved, false if not.</returns>
        public bool GetIncomingEvent(out Event ev)
        {
            return incomingEvents.TryDequeue(out ev);
        }

        /// <summary>
        /// Sends an event to the system thread.
        /// </summary>
        /// <param name="ev">Event to be sent.</param>
        public void SendEventToSystem(Event ev)
        {
            incomingEvents.Enqueue(ev);
        }

    }

}
