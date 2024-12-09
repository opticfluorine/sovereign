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
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Lua;
using Sovereign.Scripting.Lua;
using static Sovereign.Scripting.Lua.LuaBindings;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Lua library that provides support for registering callbacks with the scripting system.
/// </summary>
public class ScriptingLuaLibrary : ILuaLibrary
{
    private readonly ScriptingCallbackManager callbackManager;
    private readonly ILogger<ScriptingLuaLibrary> logger;
    private readonly ScriptManager scriptManager;

    public ScriptingLuaLibrary(ScriptingCallbackManager callbackManager, ILogger<ScriptingLuaLibrary> logger,
        ScriptManager scriptManager)
    {
        this.callbackManager = callbackManager;
        this.logger = logger;
        this.scriptManager = scriptManager;
    }

    public void Install(LuaHost luaHost)
    {
        try
        {
            luaHost.BeginLibrary("scripting");
            luaHost.AddLibraryFunction("add_event_callback", AddEventCallback);
        }
        finally
        {
            luaHost.EndLibrary();
        }
    }

    /// <summary>
    ///     Registers an event callback with the scripting engine.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always 0.</returns>
    private int AddEventCallback(IntPtr luaState)
    {
        // arg 0 = event ID
        // arg 1 (top of stack) = callback function
        try
        {
            if (!scriptManager.TryGetHost(luaState, out var luaHost))
            {
                logger.LogError("Unrecognized Lua host.");
                return 0;
            }

            if (lua_gettop(luaState) != 2)
            {
                logger.LogError("add_event_callback requires 2 arguments.");
                return 0;
            }

            if (!lua_isinteger(luaState, -2))
            {
                logger.LogError("add_event_callback first argument must be an integer.");
                return 0;
            }

            var eventId = (EventId)lua_tointeger(luaState, -2);
            if (!ScriptableEventSet.Events.Contains(eventId))
            {
                logger.LogError("add_event_callback: unsupported event id {Id}.", eventId);
                return 0;
            }

            var index = luaHost.AddQuickLookupFunction();
            callbackManager.AddEventCallback(luaHost, eventId, index);

            logger.LogDebug("Script {Name} added callback for event {Id}.", luaHost.Name, eventId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in add_event_callback.");
        }

        return 0;
    }
}