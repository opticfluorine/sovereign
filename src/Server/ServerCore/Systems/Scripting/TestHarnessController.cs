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

using System;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Public controller class for the script test harness.
/// </summary>
public class TestHarnessController(CoreController coreController)
{
    /// <summary>
    ///     Requests that the Scripting system run the test suite.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void RequestRun(IEventSender eventSender)
    {
        var ev = new Event(EventId.Server_Scripting_RunTests);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Requests a graceful server shutdown with an exit code reflecting the test results.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="failed">Whether any test failed or timed out.</param>
    public void RequestShutdown(IEventSender eventSender, bool failed)
    {
        Environment.ExitCode = failed ? 1 : 0;
        coreController.Quit(eventSender);
    }
}
