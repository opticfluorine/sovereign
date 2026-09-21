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
///     Event sender that records all sent events for inspection by tests.
/// </summary>
public sealed class TestEventSender : IEventSender
{
    /// <summary>
    ///     Events sent through this sender, in send order.
    /// </summary>
    public List<Event> SentEvents { get; } = new();

    public void SendEvent(Event ev)
    {
        SentEvents.Add(ev);
    }

    public bool TryGetOutgoingEvent(out Event? ev)
    {
        ev = null;
        return false;
    }
}
