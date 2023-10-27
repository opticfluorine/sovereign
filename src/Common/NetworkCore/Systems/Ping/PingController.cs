// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.NetworkCore.Systems.Ping;

/// <summary>
///     Provides public API to the ping system.
/// </summary>
public class PingController
{
    /// <summary>
    ///     Commands the ping system to initiate a new ping.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="eventTime">System time at which the ping should be sent.</param>
    public void StartPing(IEventSender eventSender, ulong eventTime = Event.Immediate)
    {
        var ev = new Event(EventId.Core_Ping_Start, eventTime);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Configures auto-ping behavior.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="enabled">If true, enable auto-ping; if false, disable.</param>
    /// <param name="intervalMs">Milliseconds between pings. Ignored if auto-ping is disabled.</param>
    public void SetAutoPing(IEventSender eventSender, bool enabled, uint intervalMs)
    {
        var details = new AutoPingEventDetails(enabled, intervalMs);
        var ev = new Event(EventId.Core_Ping_SetAuto, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Internal API to immediately publishes a ping.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    internal void Ping(IEventSender eventSender)
    {
        var ev = new Event(EventId.Core_Ping_Ping);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Internal API to immediately publish a pong.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    internal void Pong(IEventSender eventSender)
    {
        var ev = new Event(EventId.Core_Ping_Pong);
        eventSender.SendEvent(ev);
    }
}