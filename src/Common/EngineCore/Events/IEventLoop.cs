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

using Sovereign.EngineCore.Systems;

namespace Sovereign.EngineCore.Events;

/// <summary>
///     Interface for event loops, which are implemented as top-level dispatchers.
/// </summary>
public interface IEventLoop
{
    /// <summary>
    ///     Whether the event loop has terminated.
    /// </summary>
    bool Terminated { get; }

    /// <summary>
    ///     Advances the event loop time to the given system time value.
    ///     This value should be a multiple of the tick interval.
    /// </summary>
    /// <param name="systemTime">System time of the current tick.</param>
    void UpdateSystemTime(ulong systemTime);

    /// <summary>
    ///     Pumps the event loop.
    /// </summary>
    /// <returns>
    ///     Number of events processed by this call.
    /// </returns>
    int PumpEventLoop();

    /// <summary>
    ///     Register an event sender.
    /// </summary>
    /// <param name="eventSender">Event sender to register.</param>
    void RegisterEventSender(IEventSender eventSender);

    /// <summary>
    ///     Unregisters an event sender.
    /// </summary>
    /// <param name="eventSender">Event sender to unregister.</param>
    void UnregisterEventSender(IEventSender eventSender);

    /// <summary>
    ///     Registers a system.
    /// </summary>
    /// <param name="system">System to register.</param>
    void RegisterSystem(ISystem system);

    /// <summary>
    ///     Unregisters a system.
    /// </summary>
    /// <param name="system">System to unregister.</param>
    void UnregisterSystem(ISystem system);
}