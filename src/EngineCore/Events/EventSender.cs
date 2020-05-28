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
