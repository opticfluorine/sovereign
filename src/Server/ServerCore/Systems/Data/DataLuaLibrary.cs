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
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.Scripting.Lua;
using Sovereign.ServerCore.Systems.Scripting;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.ServerCore.Systems.Data;

/// <summary>
///     Lua library that provides access to DataSystem data.
/// </summary>
public class DataLuaLibrary : ILuaLibrary, IDisposable
{
    private const string LibraryName = "data";
    private const long TableEntityIndex = 0;

    private const string ReadOnlyKeyPrefix = "__";
    private readonly IDataController dataController;
    private readonly IDataServices dataServices;

    private readonly LuaCFunction entityGetCallback;

    private readonly LuaCFunction entitySetCallback;

    private readonly ILogger<DataLuaLibrary> logger;
    private readonly ScriptingServices scriptingServices;
    private GCHandle entityGetGcHandle;
    private GCHandle entitySetGcHandle;

    public DataLuaLibrary(IDataController dataController,
        ScriptingServices scriptingServices, ILogger<DataLuaLibrary> logger,
        IDataServices dataServices)
    {
        this.dataController = dataController;
        this.scriptingServices = scriptingServices;
        this.logger = logger;
        this.dataServices = dataServices;

        entityGetCallback = EntityKvGet;
        entityGetGcHandle = GCHandle.Alloc(entityGetCallback);
        entitySetCallback = EntityKvSet;
        entitySetGcHandle = GCHandle.Alloc(entityGetCallback);
    }

    public void Dispose()
    {
        entityGetGcHandle.Free();
        entitySetGcHandle.Free();
    }

    public void Install(LuaHost luaHost)
    {
        try
        {
            luaHost.BeginLibrary(LibraryName);

            InstallGlobalKeyValues(luaHost);
            InstallEntityKeyValues(luaHost);
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

        try
        {
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
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in data.global[key].");
            luaL_checkstack(luaState, 1, null);
            lua_pushnil(luaState);
        }

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

        try
        {
            if (!lua_isstring(luaState, -2))
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError("data.global[key] requires a string key.");
                return 0;
            }

            var key = lua_tostring(luaState, -2);

            if (IsKeyReadOnly(key))
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError("data.global[key]: key {Key} is read-only.", key);
                return 0;
            }

            var valueType = lua_type(luaState, -1);
            switch (valueType)
            {
                case LuaType.String:
                    dataController.SetGlobalSync(key, lua_tostring(luaState, -1));
                    break;

                case LuaType.Number:
                    if (lua_isinteger(luaState, -1))
                        dataController.SetGlobalSync(key, lua_tointeger(luaState, -1));
                    else
                        dataController.SetGlobalSync(key, lua_tonumber(luaState, -1));
                    break;

                case LuaType.Boolean:
                    dataController.SetGlobalSync(key, lua_toboolean(luaState, -1));
                    break;

                case LuaType.Nil:
                    // Special case - nil assignment deletes the key-value pair.
                    dataController.RemoveGlobalSync(key);
                    break;

                default:
                    scriptingServices.GetScriptLogger(luaState, logger)
                        .LogError("Unsupported value type for data.global[key].");
                    return 0;
            }
        }
        catch (Exception e)
        {
            scriptingServices.GetScriptLogger(luaState, logger)
                .LogError(e, "Error setting data.global[key].");
        }

        return 0;
    }

    /// <summary>
    ///     Installs the entity key-value store API into a Lua host.
    /// </summary>
    /// <param name="luaHost">Lua host.</param>
    private void InstallEntityKeyValues(LuaHost luaHost)
    {
        luaL_checkstack(luaHost.LuaState, 2, null);
        lua_getglobal(luaHost.LuaState, LibraryName); // <-- data

        luaHost.PushFunction(GetEntityData);
        lua_setfield(luaHost.LuaState, -2, nameof(GetEntityData));

        lua_pop(luaHost.LuaState, 1);
    }

    /// <summary>
    ///     Implementation of data.GetEntityTable(entityId). Creates a Lua table for accessing
    ///     per-entity key-value data.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of values returned.</returns>
    private int GetEntityData(IntPtr luaState)
    {
        // First argument: entity ID
        if (lua_gettop(luaState) < 1 || !lua_isinteger(luaState, -1))
        {
            scriptingServices.GetScriptLogger(luaState, logger)
                .LogError("data.GetEntityData(entityId) requires entity ID as the first argument.");
            return 0;
        }

        var entityId = (ulong)lua_tointeger(luaState, -1);

        // Create an opaque object to act as a proxy to the global key-values.
        luaL_checkstack(luaState, 4, null);
        lua_createtable(luaState, 1, 0); // <-- returned table
        lua_createtable(luaState, 0, 2); // <-- metatable for returned table

        // Bind new table to entity.
        lua_pushinteger(luaState, TableEntityIndex);
        lua_pushinteger(luaState, (long)entityId);
        lua_rawset(luaState, -4);

        // Populate metatable.
        lua_pushcfunction(luaState, entityGetCallback);
        lua_setfield(luaState, -2, "__index");
        lua_pushcfunction(luaState, entitySetCallback);
        lua_setfield(luaState, -2, "__newindex");
        lua_setmetatable(luaState, -2);

        return 1;
    }

    /// <summary>
    ///     Lua metamethod on an entity KV table that indexes into the global key-value store.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of return values (always 1).</returns>
    private int EntityKvGet(IntPtr luaState)
    {
        // First argument: entity KV table
        // Second argument: key

        try
        {
            luaL_checkstack(luaState, 2, null);

            if (!lua_isstring(luaState, -1))
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError("Entity KV access requires a string key.");
                return 0;
            }

            // Retrieve the bound entity ID.
            lua_pushinteger(luaState, TableEntityIndex);
            lua_rawget(luaState, -3);
            if (!lua_isinteger(luaState, -1))
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogCritical(
                        "Possible Lua host corruption: Entity KV table has no bound entity ID. Script may have a security issue.");
                return 0;
            }

            var entityId = (ulong)lua_tointeger(luaState, -1);
            lua_pop(luaState, 1);

            // Fetch value for key if it exists.
            var key = lua_tostring(luaState, -1);
            if (dataServices.TryGetEntityKeyValue(entityId, key, out var value))
                lua_pushstring(luaState, value);
            else
                return 0;
        }
        catch (Exception e)
        {
            scriptingServices.GetScriptLogger(luaState, logger)
                .LogError(e, "Error in entity KV getter.");
            return 0;
        }

        return 1;
    }

    /// <summary>
    ///     Sets a key-value pair in the entity's key-value store.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always 0.</returns>
    private int EntityKvSet(IntPtr luaState)
    {
        // First argument: entity KV table
        // Second argument: key
        // Third argument: value

        try
        {
            luaL_checkstack(luaState, 1, null);

            var key = lua_tostring(luaState, -2);

            if (IsKeyReadOnly(key))
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogError("Entity KV set: key {Key} is read-only.", key);
                return 0;
            }

            // Retrieve the bound entity ID.
            lua_pushinteger(luaState, TableEntityIndex);
            lua_rawget(luaState, -4);
            if (!lua_isinteger(luaState, -1))
            {
                scriptingServices.GetScriptLogger(luaState, logger)
                    .LogCritical(
                        "Possible Lua host corruption: Entity KV table has no bound entity ID. Script may have a security issue.");
                lua_pushnil(luaState);
                return 1;
            }

            var entityId = (ulong)lua_tointeger(luaState, -1);
            lua_pop(luaState, 1);

            // Handle the set or delete if the type is supported.
            var valueType = lua_type(luaState, -1);
            switch (valueType)
            {
                case LuaType.String:
                    dataController.SetEntityKeyValueSync(entityId, key, lua_tostring(luaState, -1));
                    break;

                case LuaType.Number:
                    if (lua_isinteger(luaState, -1))
                        dataController.SetEntityKeyValueSync(entityId, key, lua_tointeger(luaState, -1));
                    else
                        dataController.SetEntityKeyValueSync(entityId, key, lua_tonumber(luaState, -1));
                    break;

                case LuaType.Boolean:
                    dataController.SetEntityKeyValueSync(entityId, key, lua_toboolean(luaState, -1));
                    break;

                case LuaType.Nil:
                    // Special case - nil assignment is idiomatic for deleting a key-value pair.
                    dataController.RemoveEntityKeyValueSync(entityId, key);
                    break;

                default:
                    scriptingServices.GetScriptLogger(luaState, logger)
                        .LogError("Unsupported value type for entity key-value pair.");
                    return 0;
            }
        }
        catch (Exception e)
        {
            scriptingServices.GetScriptLogger(luaState, logger)
                .LogError(e, "Error in entity KV setter.");
        }

        return 0;
    }

    /// <summary>
    ///     Determines whether a key is read-only to scripts (i.e. special internal
    ///     keys used by the engine such as the in-game clock).
    /// </summary>
    /// <param name="key">Key.</param>
    /// <returns>true if read-only to scripts, false otherwise.</returns>
    private bool IsKeyReadOnly(string key)
    {
        return key.StartsWith(ReadOnlyKeyPrefix);
    }
}