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

using System;
using System.Collections.Concurrent;

namespace Sovereign.EngineCore.Events
{

    /// <summary>
    /// Sends events from one or more systems to the event loop.
    /// </summary>
    public class EventSender : IEventSender, IDisposable
    {
        private readonly IEventLoop eventLoop;

        /// <summary>
        /// Events being sent from the system thread to the event loop.
        /// </summary>
        private readonly ConcurrentQueue<Event> outgoingEvents
            = new ConcurrentQueue<Event>();

        public EventSender(IEventLoop eventLoop)
        {
            this.eventLoop = eventLoop;
            eventLoop.RegisterEventSender(this);
        }

        public void Dispose()
        {
            eventLoop.UnregisterEventSender(this);
        }

        public void SendEvent(Event ev)
        {
            outgoingEvents.Enqueue(ev);
        }

        public bool TryGetOutgoingEvent(out Event ev)
        {
            return outgoingEvents.TryDequeue(out ev);
        }

    }

}
