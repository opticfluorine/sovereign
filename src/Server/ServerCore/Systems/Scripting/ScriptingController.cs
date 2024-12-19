// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Public controller class for the Scripting system.
/// </summary>
public class ScriptingController
{
    /// <summary>
    ///     Requests that the Scripting system reloads all scripts.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void ReloadAllScripts(IEventSender eventSender)
    {
        var ev = new Event(EventId.Server_Scripting_ReloadAll);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Requests that a specific script be reloaded.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="scriptName">Script name.</param>
    public void ReloadScript(IEventSender eventSender, string scriptName)
    {
        var details = new StringEventDetails { Value = scriptName };
        var ev = new Event(EventId.Server_Scripting_Reload, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Requests that any new scripts be loaded.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void LoadNewScripts(IEventSender eventSender)
    {
        var ev = new Event(EventId.Server_Scripting_LoadNew);
        eventSender.SendEvent(ev);
    }
}