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

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.Scripting.Lua;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Public controller class for the Scripting system.
/// </summary>
public class ScriptingController(ScriptManager scriptManager, ILogger<ScriptingController> logger)
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

    /// <summary>
    ///     Asynchronously invokes a function from a loaded script.
    /// </summary>
    /// <param name="scriptName">Script name.</param>
    /// <param name="functionName">Function name.</param>
    /// <param name="args">Arguments.</param>
    public void CallFunctionAsync(string scriptName, string functionName, params ulong[] args)
    {
        Task.Run(() =>
        {
            try
            {
                if (!scriptManager.TryGetHost(scriptName, out var host))
                    throw new LuaException($"Script '{scriptName}' not found.");

                host.CallNamedFunction(functionName, builder =>
                {
                    foreach (var arg in args) builder.AddInteger((long)arg);
                    return args.Length;
                });
            }
            catch (LuaException e)
            {
                logger.LogError(e, "Error calling {ScriptName}.{FunctionName}.", scriptName, functionName);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unhandled exception calling {ScriptName}.{FunctionName}.", scriptName,
                    functionName);
            }
        });
    }
}