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

using System;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.Scripting.Lua;
using Sovereign.ServerCore.Systems.Scripting;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.ServerCore.Systems.Data;

/// <summary>
///     Lua library that provides access to DataSystem data.
/// </summary>
public class DataLuaLibrary : ILuaLibrary
{
    private const string LibraryName = "data";
    private readonly IDataController dataController;
    private readonly IDataServices dataServices;
    private readonly IEventSender eventSender;
    private readonly ILogger<DataLuaLibrary> logger;
    private readonly ScriptingServices scriptingServices;

    public DataLuaLibrary(IDataController dataController, IEventSender eventSender,
        ScriptingServices scriptingServices, ILogger<DataLuaLibrary> logger,
        IDataServices dataServices)
    {
        this.dataController = dataController;
        this.eventSender = eventSender;
        this.scriptingServices = scriptingServices;
        this.logger = logger;
        this.dataServices = dataServices;
    }

    public void Install(LuaHost luaHost)
    {
        try
        {
            luaHost.BeginLibrary(LibraryName);

            InstallGlobalKeyValues(luaHost);
        }
        finally
        {
            luaHost.EndLibrary();
        }
    }

    /// <summary>
    ///     Installs library support for the global key-value store.
    /// </summary>
    /// <param name="luaHost">Lua host.</param>
    private void InstallGlobalKeyValues(LuaHost luaHost)
    {
        // Create an opaque object to act as a proxy to the global key-values.
        luaL_checkstack(luaHost.LuaState, 4, null);
        lua_getglobal(luaHost.LuaState, LibraryName); // <-- data
        lua_createtable(luaHost.LuaState, 0, 0); // <-- data.global

        // Create metatable for the global KV proxy.
        lua_createtable(luaHost.LuaState, 0, 2);

        luaHost.PushFunction(GlobalKvGet);
        lua_setfield(luaHost.LuaState, -2, "__index");

        luaHost.PushFunction(GlobalKvSet);
        lua_setfield(luaHost.LuaState, -2, "__newindex");

        lua_setmetatable(luaHost.LuaState, -2);
        lua_setfield(luaHost.LuaState, -2, "global");
        lua_pop(luaHost.LuaState, 1);

        luaHost.AddLibraryFunction("RemoveGlobal", GlobalKvRemove);
    }

    /// <summary>
    ///     Lua metamethod on data.global that indexes into the global key-value store.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of return values (always 1).</returns>
    private int GlobalKvGet(IntPtr luaState)
    {
        // First argument: data.global
        // Second argument: key

        luaL_checkstack(luaState, 1, null);

        if (!lua_isstring(luaState, -1))
        {
            scriptingServices.GetScriptLogger(luaState, logger)
                .LogError("data.global[key] requires a string key.");
            lua_pushnil(luaState);
            return 1;
        }

        var key = lua_tostring(luaState, -1);
        if (dataServices.TryGetGlobal(key, out var value))
            lua_pushstring(luaState, value);
        else
            lua_pushnil(luaState);

        return 1;
    }

    /// <summary>
    ///     Lua metamethod on data.global that asynchronously sets a global key-value pair.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of return values (always 0).</returns>
    private int GlobalKvSet(IntPtr luaState)
    {
        // First argument: data.global
        // Second argument: key
        // Third argument: value

        if (!lua_isstring(luaState, -2))
        {
            scriptingServices.GetScriptLogger(luaState, logger)
                .LogError("data.global[key] requires a string key.");
            return 0;
        }

        var key = lua_tostring(luaState, -2);
        var valueType = lua_type(luaState, -1);
        switch (valueType)
        {
            case LuaType.String:
                dataController.SetGlobal(eventSender, key, lua_tostring(luaState, -1));
                break;

            case LuaType.Number:
                if (lua_isinteger(luaState, -1))
                    dataController.SetGlobal(eventSender, key, lua_tointeger(luaState, -1));
                else
                    dataController.SetGlobal(eventSender, key, lua_tonumber(luaState, -1));
                break;

            case LuaType.Boolean:
                dataController.SetGlobal(eventSender, key, lua_toboolean(luaState, -1));
                break;

            default:
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError("Unsupported value type for data.global[key].");
                return 0;
        }

        return 0;
    }

    /// <summary>
    ///     data.RemoveGlobal(key) Lua function. Asynchronously removes a global key-value pair
    ///     if it exists, or does nothing otherwise.
    /// </summary>
    /// <param name="luaState"></param>
    /// <returns></returns>
    private int GlobalKvRemove(IntPtr luaState)
    {
        // First argument: data.global
        // Second argument: key

        if (lua_gettop(luaState) != 1)
        {
            scriptingServices.GetScriptLogger(luaState, logger)
                .LogError("data.remove_global(key) requires 1 argument.");
            return 0;
        }

        if (!lua_isstring(luaState, -1))
        {
            scriptingServices.GetScriptLogger(luaState, logger)
                .LogError("data.remove_global(key) requires a string key.");
            return 0;
        }

        var key = lua_tostring(luaState, -1);
        dataController.RemoveGlobal(eventSender, key);

        return 0;
    }
}