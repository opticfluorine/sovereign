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
///     Describes an adapter that connects an external event source to
///     the event loop on the main thread.
/// </summary>
public interface IEventAdapter
{
    /// <summary>
    ///     Called immediately before the event loop will poll for events.
    /// </summary>
    void PrepareEvents();

    /// <summary>
    ///     Non-blocking method that polls for the next available event.
    /// </summary>
    /// <param name="ev">Event.</param>
    /// If no event was available, ev is undefined.
    /// <returns>
    ///     true if an event was available, false otherwise.
    /// </returns>
    bool PollEvent(out Event ev);
}