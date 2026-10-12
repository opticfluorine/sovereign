// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using System.Collections.Generic;
using Sovereign.EngineCore.Events;

namespace TestClientCore.TestSupport;

/// <summary>
///     Event sender that records sent events and optionally forwards them to an event communicator.
/// </summary>
public sealed class RecordingEventSender : IEventSender
{
    /// <summary>
    ///     Events sent through this sender, in send order.
    /// </summary>
    public List<Event> SentEvents { get; } = new();

    /// <summary>
    ///     Optional communicator to which sent events are also forwarded.
    /// </summary>
    public EventCommunicator? ForwardTarget { get; set; }

    public void SendEvent(Event ev)
    {
        SentEvents.Add(ev);
        ForwardTarget?.SendEventToSystem(ev);
    }

    public bool TryGetOutgoingEvent(out Event? ev)
    {
        ev = null;
        return false;
    }
}
