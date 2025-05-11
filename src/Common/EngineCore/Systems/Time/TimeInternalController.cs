// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

namespace Sovereign.EngineCore.Systems.Time;

/// <summary>
///     Internal controller for TimeSystem.
/// </summary>
internal sealed class TimeInternalController
{
    private readonly IEventSender eventSender;

    public TimeInternalController(IEventSender eventSender)
    {
        this.eventSender = eventSender;
    }

    /// <summary>
    ///     Sends a clock event with the specified time.
    /// </summary>
    /// <param name="time">In-game absolute time in seconds.</param>
    public void SendClock(uint time)
    {
        var details = new IntEventDetails { Value = time };
        var ev = new Event(EventId.Core_Time_Clock, details);
        eventSender.SendEvent(ev);
    }
}