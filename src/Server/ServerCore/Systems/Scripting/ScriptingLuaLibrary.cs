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
using Sovereign.EngineCore.Events.Details.Validators;
using Sovereign.EngineCore.Lua;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineUtil.Numerics;
using Sovereign.Scripting.Lua;
using static Sovereign.Scripting.Lua.LuaBindings;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Interface for running timed callbacks in the scripting engine.
/// </summary>
internal interface ITimedCallbackRunner
{
    /// <summary>
    ///     Runs a timed callback in a background task.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="callbackRef">Lua callback reference.</param>
    /// <param name="argRef">Lua argument reference.</param>
    void RunTimedCallback(IntPtr luaState, int callbackRef, int argRef);
}

/// <summary>
///     Lua library that provides support for registering callbacks with the scripting system.
/// </summary>
public class ScriptingLuaLibrary : ILuaLibrary, ITimedCallbackRunner
{
    private readonly ScriptingCallbackManager callbackManager;
    private readonly IEventSender eventSender;
    private readonly ILogger<ScriptingLuaLibrary> logger;
    private readonly ScriptingServices scriptingServices;
    private readonly ScriptManager scriptManager;
    private readonly ISystemTimer systemTimer;

    public ScriptingLuaLibrary(ScriptingCallbackManager callbackManager, ILogger<ScriptingLuaLibrary> logger,
        ScriptManager scriptManager, IEventSender eventSender, ScriptingServices scriptingServices,
        ISystemTimer systemTimer)
    {
        this.callbackManager = callbackManager;
        this.logger = logger;
        this.scriptManager = scriptManager;
        this.eventSender = eventSender;
        this.scriptingServices = scriptingServices;
        this.systemTimer = systemTimer;
    }

    public void Install(LuaHost luaHost)
    {
        try
        {
            luaHost.BeginLibrary("scripting");
            luaHost.AddLibraryFunction(nameof(AddEventCallback), AddEventCallback);
            luaHost.AddLibraryFunction(nameof(AddTimedCallback), AddTimedCallback);
            luaHost.AddLibraryFunction(nameof(AddEntityParameterHint), AddEntityParameterHint);
        }
        finally
        {
            luaHost.EndLibrary();
        }
    }

    public void RunTimedCallback(IntPtr luaState, int callbackRef, int argRef)
    {
        if (!scriptManager.TryGetHost(luaState, out var host))
        {
            logger.LogError("Unrecognized Lua state.");
            return;
        }

        Task.Run(() => host.ExecuteOther(controls =>
        {
            // Restore the execution context from the callback table.
            luaL_checkstack(luaState, 2, null);
            lua_rawgeti(luaState, LUA_REGISTRYINDEX, callbackRef);
            lua_rawgeti(luaState, LUA_REGISTRYINDEX, argRef);
            luaL_unref(luaState, LUA_REGISTRYINDEX, argRef);
            luaL_unref(luaState, LUA_REGISTRYINDEX, callbackRef);

            // Invoke the callback. This also restores the stack to its original state.
            try
            {
                controls.PCall(1, 0);
            }
            catch (Exception e)
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError(e, "Error executing timed callback.");
            }
        }));
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
                luaHost.Logger.LogError("AddEventCallback requires 2 arguments.");
                return 0;
            }

            if (!lua_isinteger(luaState, -2))
            {
                luaHost.Logger.LogError("AddEventCallback first argument must be an integer.");
                return 0;
            }

            var eventId = (EventId)lua_tointeger(luaState, -2);
            if (!ScriptableEventSet.Events.Contains(eventId))
            {
                luaHost.Logger.LogError("AddEventCallback: unsupported event id {Id}.", eventId);
                return 0;
            }

            var index = luaHost.AddQuickLookupFunction();
            callbackManager.AddEventCallback(luaHost, eventId, index);

            luaHost.Logger.LogDebug("Added callback for event {Id}.", eventId);
        }
        catch (Exception e)
        {
            scriptingServices.GetScriptLogger(luaState, logger)
                .LogError(e, "Error in AddEventCallback.");
        }

        return 0;
    }

    /// <summary>
    ///     Registers a timed callback with the scripting engine.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always zero.</returns>
    private int AddTimedCallback(IntPtr luaState)
    {
        // arg 0 = delay in seconds
        // arg 1 = callback function
        // arg 2 = argument to callback function (optional)

        try
        {
            if (lua_gettop(luaState) < 2 || lua_gettop(luaState) > 3)
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError("AddTimedCallback requires 2 or 3 arguments.");
                return 0;
            }

            // Fill in optional argument with nil if not provided.
            if (lua_gettop(luaState) == 2)
            {
                luaL_checkstack(luaState, 1, null);
                lua_pushnil(luaState);
            }

            if (!lua_isnumber(luaState, -3))
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError("AddTimedCallback first argument must be a number.");
                return 0;
            }

            if (!lua_isfunction(luaState, -2))
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError("AddTimedCallback second argument must be a function.");
                return 0;
            }

            var delay = lua_tonumber(luaState, -3);
            if (!double.IsNormal(delay) || delay < 0.0)
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError("AddTimedCallback delay must be a positive number.");
                return 0;
            }

            var eventTime = systemTimer.GetTime() + (ulong)(delay * UnitConversions.SToUs);

            var argumentRef = luaL_ref(luaState, LUA_REGISTRYINDEX);
            var callbackRef = luaL_ref(luaState, LUA_REGISTRYINDEX);

            var details = new ScriptingCallbackEventDetails
            {
                LuaState = luaState,
                CallbackReference = callbackRef,
                ArgumentReference = argumentRef
            };
            var ev = new Event(EventId.Server_Scripting_TimedCallback, details, eventTime);
            lock (eventSender)
            {
                eventSender.SendEvent(ev);
            }
        }
        catch (Exception e)
        {
            scriptingServices.GetScriptLogger(luaState, logger)
                .LogError(e, "Error in AddTimedCallback.");
        }

        return 0;
    }

    /// <summary>
    ///     Lua function that adds an entity parameter hint for a function.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always 0.</returns>
    private int AddEntityParameterHint(IntPtr luaState)
    {
        try
        {
            if (lua_gettop(luaState) != 2)
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError("AddEntityParameterHint requires 2 arguments.");
                return 0;
            }

            if (!lua_isstring(luaState, -2) || !lua_isstring(luaState, -1))
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError("AddEntityParameterHint requires two string arguments.");
                return 0;
            }

            if (!scriptManager.TryGetHost(luaState, out var luaHost))
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError("Unrecognized Lua host.");
                return 0;
            }

            var functionName = lua_tostring(luaState, -2);
            var parameterName = lua_tostring(luaState, -1);

            luaHost.AddEntityParameterHint(functionName, parameterName);
        }
        catch (Exception e)
        {
            scriptingServices.GetScriptLogger(luaState, logger)
                .LogError(e, "Error in AddEntityParameterHint.");
        }

        return 0;
    }
}