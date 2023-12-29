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

namespace Sovereign.EngineCore.Events;

/// <summary>
///     Interface for event senders.
/// </summary>
public interface IEventSender
{
    /// <summary>
    ///     Sends an event to the event loop.
    /// </summary>
    /// <param name="ev">Event to be sent.</param>
    void SendEvent(Event ev);

    /// <summary>
    ///     Gets the next event being sent to the event loop.
    ///     This method does not block.
    /// </summary>
    /// <param name="ev">Next event being sent to the event loop, or null if none is available.</param>
    /// <returns>true if an event was available, false otherwise.</returns>
    bool TryGetOutgoingEvent(out Event? ev);
}