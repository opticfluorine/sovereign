/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Concurrent;

namespace Sovereign.EngineCore.Events;

/// <summary>
///     Provides interthread communication from the event loop to the system
///     threads.
/// </summary>
public class EventCommunicator
{
    /// <summary>
    ///     Events being sent from the event loop to the system thread.
    /// </summary>
    private readonly ConcurrentQueue<Event> incomingEvents = new();

    /// <summary>
    ///     Tries to get the next event being sent to the system thread.
    ///     If no event is available, this returns false and the value of
    ///     ev is undefined.
    /// </summary>
    /// <param name="ev">Reference to hold the next event.</param>
    /// <returns>true if an event was retrieved, false if not.</returns>
    public bool GetIncomingEvent(out Event ev)
    {
        return incomingEvents.TryDequeue(out ev);
    }

    /// <summary>
    ///     Sends an event to the system thread.
    /// </summary>
    /// <param name="ev">Event to be sent.</param>
    public void SendEventToSystem(Event ev)
    {
        incomingEvents.Enqueue(ev);
    }
}