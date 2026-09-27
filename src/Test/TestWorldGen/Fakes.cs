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
using Sovereign.EngineCore.Systems;

namespace TestWorldGen;

/// <summary>
///     Event loop fake that records registered systems without dispatching events.
/// </summary>
internal sealed class FakeEventLoop : IEventLoop
{
    public void UpdateSystemTime(ulong systemTime)
    {
    }

    public int PumpEventLoop()
    {
        return 0;
    }

    public void RegisterEventSender(IEventSender eventSender)
    {
    }

    public void UnregisterEventSender(IEventSender eventSender)
    {
    }

    public void RegisterSystem(ISystem system)
    {
    }

    public void UnregisterSystem(ISystem system)
    {
    }
}

/// <summary>
///     Event sender fake that records sent events.
/// </summary>
internal sealed class FakeEventSender : IEventSender
{
    /// <summary>
    ///     Events that have been sent through this fake.
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
