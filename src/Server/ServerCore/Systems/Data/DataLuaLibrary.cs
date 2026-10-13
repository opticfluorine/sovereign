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
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.EngineCore.World;
using Sovereign.Scripting.Lua;
using Sovereign.ServerCore.Systems.Persistence;
using Sovereign.ServerCore.Systems.Scripting;
using Sovereign.ServerCore.Systems.WorldManagement;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.ServerCore.Systems.Data;

/// <summary>
///     Lua library that provides access to DataSystem data.
/// </summary>
public class DataLuaLibrary : ILuaLibrary, IDisposable
{
    private const string LibraryName = "Data";
    private const long TableEntityIndex = 0;

    /// <summary>
    ///     Maximum number of world segments a single box flag write may affect.
    /// </summary>
    private const int MaximumBoxSegments = 65536;

    /// <summary>
    ///     Bitwise OR of every defined WorldSegmentFlag value other than None.
    /// </summary>
    private static readonly WorldSegmentFlag DefinedFlagMask = ComputeDefinedFlagMask();

    private readonly IDataController dataController;
    private readonly IDataServices dataServices;
    private readonly IEventSender eventSender;

    private readonly LuaCFunction entityGetCallback;

    private readonly LuaCFunction entitySetCallback;

    private readonly ILogger<DataLuaLibrary> logger;
    private readonly PersistenceController persistenceController;
    private readonly ScriptingServices scriptingServices;
    private readonly SegmentFlagManager segmentFlagManager;
    private readonly WorldSegmentResolver segmentResolver;
    private GCHandle entityGetGcHandle;
    private GCHandle entitySetGcHandle;

    public DataLuaLibrary(IDataController dataController,
        ScriptingServices scriptingServices, ILogger<DataLuaLibrary> logger,
        IDataServices dataServices, SegmentFlagManager segmentFlagManager,
        WorldSegmentResolver segmentResolver, PersistenceController persistenceController,
        IEventSender eventSender)
    {
        this.dataController = dataController;
        this.scriptingServices = scriptingServices;
        this.logger = logger;
        this.dataServices = dataServices;
        this.segmentFlagManager = segmentFlagManager;
        this.segmentResolver = segmentResolver;
        this.persistenceController = persistenceController;
        this.eventSender = eventSender;

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
            InstallSegmentFlags(luaHost);
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
        lua_setfield(luaHost.LuaState, -2, "Global");
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

        var mainState = LuaUtil.GetMainThread(luaState);

        try
        {
            luaL_checkstack(luaState, 1, null);

            if (!lua_isstring(luaState, -1))
            {
                scriptingServices.GetScriptLogger(mainState, logger)
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

        var mainState = LuaUtil.GetMainThread(luaState);

        try
        {
            if (!lua_isstring(luaState, -2))
            {
                scriptingServices.GetScriptLogger(mainState, logger)
                    .LogError("data.global[key] requires a string key.");
                return 0;
            }

            var key = lua_tostring(luaState, -2);

            if (dataServices.IsKeyReadOnly(key))
            {
                scriptingServices.GetScriptLogger(mainState, logger)
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
                    scriptingServices.GetScriptLogger(mainState, logger)
                        .LogError("Unsupported value type for data.global[key].");
                    return 0;
            }
        }
        catch (Exception e)
        {
            scriptingServices.GetScriptLogger(mainState, logger)
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
        var mainState = LuaUtil.GetMainThread(luaState);

        // First argument: entity ID
        if (lua_gettop(luaState) < 1 || !lua_islightuserdata(luaState, -1))
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("data.GetEntityData(entityId) requires entity ID as the first argument.");
            return 0;
        }

        var entityId = (ulong)lua_touserdata(luaState, -1);
        if (entityId >= EntityConstants.FirstBlockEntityId && entityId <= EntityConstants.LastBlockEntityId)
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("data.GetEntityData(entityId) cannot be used on block entities.");
            return 0;
        }

        // Create an opaque object to act as a proxy to the global key-values.
        luaL_checkstack(luaState, 4, null);
        lua_createtable(luaState, 1, 0); // <-- returned table
        lua_createtable(luaState, 0, 2); // <-- metatable for returned table

        // Bind new table to entity.
        lua_pushinteger(luaState, TableEntityIndex);
        lua_pushlightuserdata(luaState, (IntPtr)entityId);
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

        var mainState = LuaUtil.GetMainThread(luaState);

        try
        {
            luaL_checkstack(luaState, 2, null);

            if (!lua_isstring(luaState, -1))
            {
                scriptingServices.GetScriptLogger(mainState, logger)
                    .LogError("Entity KV access requires a string key.");
                return 0;
            }

            // Retrieve the bound entity ID.
            lua_pushinteger(luaState, TableEntityIndex);
            lua_rawget(luaState, -3);
            if (!lua_islightuserdata(luaState, -1))
            {
                scriptingServices.GetScriptLogger(mainState, logger)
                    .LogCritical(
                        "Possible Lua host corruption: Entity KV table has no bound entity ID. Script may have a security issue.");
                return 0;
            }

            var entityId = (ulong)lua_touserdata(luaState, -1);
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
            scriptingServices.GetScriptLogger(mainState, logger)
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

        var mainState = LuaUtil.GetMainThread(luaState);

        try
        {
            luaL_checkstack(luaState, 1, null);

            var key = lua_tostring(luaState, -2);

            if (dataServices.IsKeyReadOnly(key))
            {
                scriptingServices.GetScriptLogger(mainState, logger)
                    .LogError("Entity KV set: key {Key} is read-only.", key);
                return 0;
            }

            // Retrieve the bound entity ID.
            lua_pushinteger(luaState, TableEntityIndex);
            lua_rawget(luaState, -4);
            if (!lua_islightuserdata(luaState, -1))
            {
                scriptingServices.GetScriptLogger(mainState, logger)
                    .LogCritical(
                        "Possible Lua host corruption: Entity KV table has no bound entity ID. Script may have a security issue.");
                lua_pushnil(luaState);
                return 1;
            }

            var entityId = (ulong)lua_touserdata(luaState, -1);
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
                    scriptingServices.GetScriptLogger(mainState, logger)
                        .LogError("Unsupported value type for entity key-value pair.");
                    return 0;
            }
        }
        catch (Exception e)
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError(e, "Error in entity KV setter.");
        }

        return 0;
    }

    /// <summary>
    ///     Installs the world segment flag API into a Lua host.
    /// </summary>
    /// <param name="luaHost">Lua host.</param>
    private void InstallSegmentFlags(LuaHost luaHost)
    {
        luaL_checkstack(luaHost.LuaState, 2, null);
        lua_getglobal(luaHost.LuaState, LibraryName); // <-- data

        luaHost.PushFunction(GetSegmentFlags);
        lua_setfield(luaHost.LuaState, -2, nameof(GetSegmentFlags));

        luaHost.PushFunction(GetSegmentFlagsForPosition);
        lua_setfield(luaHost.LuaState, -2, nameof(GetSegmentFlagsForPosition));

        luaHost.PushFunction(SetSegmentFlag);
        lua_setfield(luaHost.LuaState, -2, nameof(SetSegmentFlag));

        luaHost.PushFunction(ClearSegmentFlag);
        lua_setfield(luaHost.LuaState, -2, nameof(ClearSegmentFlag));

        luaHost.PushFunction(SetSegmentFlagForPosition);
        lua_setfield(luaHost.LuaState, -2, nameof(SetSegmentFlagForPosition));

        luaHost.PushFunction(ClearSegmentFlagForPosition);
        lua_setfield(luaHost.LuaState, -2, nameof(ClearSegmentFlagForPosition));

        luaHost.PushFunction(SetSegmentFlagInBox);
        lua_setfield(luaHost.LuaState, -2, nameof(SetSegmentFlagInBox));

        luaHost.PushFunction(ClearSegmentFlagInBox);
        lua_setfield(luaHost.LuaState, -2, nameof(ClearSegmentFlagInBox));

        lua_pop(luaHost.LuaState, 1);
    }

    /// <summary>
    ///     Implementation of Data.GetSegmentFlags(segment).
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of values returned.</returns>
    private int GetSegmentFlags(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);

        if (lua_gettop(luaState) != 1)
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("Data.GetSegmentFlags(segment) requires exactly one Vector3 argument.");
            lua_pushinteger(luaState, 0);
            return 1;
        }

        if (!TryGetSegmentVector(luaState, mainState, nameof(GetSegmentFlags), "segment", out var segment))
        {
            lua_pushinteger(luaState, 0);
            return 1;
        }

        lua_pushinteger(luaState, (long)segmentFlagManager.GetFlags(segment));
        return 1;
    }

    /// <summary>
    ///     Implementation of Data.GetSegmentFlagsForPosition(position).
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of values returned.</returns>
    private int GetSegmentFlagsForPosition(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);

        if (lua_gettop(luaState) != 1)
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("Data.GetSegmentFlagsForPosition(position) requires exactly one Vector3 argument.");
            lua_pushinteger(luaState, 0);
            return 1;
        }

        if (!TryGetPositionVector(luaState, mainState, nameof(GetSegmentFlagsForPosition), out var segment))
        {
            lua_pushinteger(luaState, 0);
            return 1;
        }

        lua_pushinteger(luaState, (long)segmentFlagManager.GetFlags(segment));
        return 1;
    }

    /// <summary>
    ///     Implementation of Data.SetSegmentFlag(segment, flag).
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of values returned (always 0).</returns>
    private int SetSegmentFlag(IntPtr luaState)
    {
        return UpdateSingleSegmentFlag(luaState, true);
    }

    /// <summary>
    ///     Implementation of Data.ClearSegmentFlag(segment, flag).
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of values returned (always 0).</returns>
    private int ClearSegmentFlag(IntPtr luaState)
    {
        return UpdateSingleSegmentFlag(luaState, false);
    }

    /// <summary>
    ///     Implementation of Data.SetSegmentFlagForPosition(position, flag).
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of values returned (always 0).</returns>
    private int SetSegmentFlagForPosition(IntPtr luaState)
    {
        return UpdatePositionSegmentFlag(luaState, true);
    }

    /// <summary>
    ///     Implementation of Data.ClearSegmentFlagForPosition(position, flag).
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of values returned (always 0).</returns>
    private int ClearSegmentFlagForPosition(IntPtr luaState)
    {
        return UpdatePositionSegmentFlag(luaState, false);
    }

    /// <summary>
    ///     Implementation of Data.SetSegmentFlagInBox(corner1, corner2, flag).
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of values returned (always 0).</returns>
    private int SetSegmentFlagInBox(IntPtr luaState)
    {
        return UpdateBoxSegmentFlag(luaState, true);
    }

    /// <summary>
    ///     Implementation of Data.ClearSegmentFlagInBox(corner1, corner2, flag).
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of values returned (always 0).</returns>
    private int ClearSegmentFlagInBox(IntPtr luaState)
    {
        return UpdateBoxSegmentFlag(luaState, false);
    }

    /// <summary>
    ///     Shared implementation of the single-segment flag write functions.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="set">Whether to set the flag (true) or clear it (false).</param>
    /// <returns>Number of values returned (always 0).</returns>
    private int UpdateSingleSegmentFlag(IntPtr luaState, bool set)
    {
        var mainState = LuaUtil.GetMainThread(luaState);

        if (lua_gettop(luaState) != 2)
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("Segment flag write requires exactly two arguments: a Vector3 segment and a flag.");
            return 0;
        }

        if (!TryGetSegmentVector(luaState, mainState, "Segment flag write", "segment", out var segment)) return 0;
        if (!TryGetFlagArgument(luaState, 2, mainState, "Segment flag write", out var flag)) return 0;

        ApplySegmentFlag(segment, segment, flag, set);
        return 0;
    }

    /// <summary>
    ///     Shared implementation of the position-based flag write functions.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="set">Whether to set the flag (true) or clear it (false).</param>
    /// <returns>Number of values returned (always 0).</returns>
    private int UpdatePositionSegmentFlag(IntPtr luaState, bool set)
    {
        var mainState = LuaUtil.GetMainThread(luaState);

        if (lua_gettop(luaState) != 2)
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("Position segment flag write requires exactly two arguments: a Vector3 position and a flag.");
            return 0;
        }

        if (!TryGetPositionVector(luaState, mainState, "Position segment flag write", out var segment)) return 0;
        if (!TryGetFlagArgument(luaState, 2, mainState, "Position segment flag write", out var flag)) return 0;

        ApplySegmentFlag(segment, segment, flag, set);
        return 0;
    }

    /// <summary>
    ///     Shared implementation of the box flag write functions.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="set">Whether to set the flag (true) or clear it (false).</param>
    /// <returns>Number of values returned (always 0).</returns>
    private int UpdateBoxSegmentFlag(IntPtr luaState, bool set)
    {
        var mainState = LuaUtil.GetMainThread(luaState);

        if (lua_gettop(luaState) != 3)
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("Box segment flag write requires exactly three arguments: two Vector3 corners and a flag.");
            return 0;
        }

        if (!TryGetSegmentVector(luaState, mainState, "Box segment flag write", "corner1", 1, out var corner1)
            || !TryGetSegmentVector(luaState, mainState, "Box segment flag write", "corner2", 2, out var corner2)
            || !TryGetFlagArgument(luaState, 3, mainState, "Box segment flag write", out var flag))
            return 0;

        var min = new GridPosition(Math.Min(corner1.X, corner2.X), Math.Min(corner1.Y, corner2.Y),
            Math.Min(corner1.Z, corner2.Z));
        var max = new GridPosition(Math.Max(corner1.X, corner2.X), Math.Max(corner1.Y, corner2.Y),
            Math.Max(corner1.Z, corner2.Z));

        if (!IsBoxSizeOk(min, max))
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("Box segment flag write: requested box exceeds {Max} segments.", MaximumBoxSegments);
            return 0;
        }

        ApplySegmentFlag(min, max, flag, set);
        return 0;
    }

    /// <summary>
    ///     Enqueues a segment flag update event and logs the successful write.
    /// </summary>
    /// <param name="min">Minimum (inclusive) corner of the affected segment box.</param>
    /// <param name="max">Maximum (inclusive) corner of the affected segment box.</param>
    /// <param name="flag">Flag to apply.</param>
    /// <param name="set">Whether to set the flag (true) or clear it (false).</param>
    private void ApplySegmentFlag(GridPosition min, GridPosition max, WorldSegmentFlag flag, bool set)
    {
        persistenceController.UpdateSegmentFlags(eventSender, min, max, flag, set);

        var action = set ? "set" : "cleared";
        logger.LogInformation("Script {Action} segment flag {Flag} over {Min} to {Max}.", action, flag, min, max);
    }

    /// <summary>
    ///     Reads a Vector3 argument of segment indices and converts it to a GridPosition.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="mainState">Main Lua state.</param>
    /// <param name="functionName">Function name for error messages.</param>
    /// <param name="argumentName">Argument name for error messages.</param>
    /// <param name="segment">Parsed segment. Only valid if the method returns true.</param>
    /// <returns>true if the argument was a valid segment vector, false otherwise.</returns>
    private bool TryGetSegmentVector(IntPtr luaState, IntPtr mainState, string functionName, string argumentName,
        out GridPosition segment)
    {
        return TryGetSegmentVector(luaState, mainState, functionName, argumentName, 1, out segment);
    }

    /// <summary>
    ///     Reads a Vector3 argument at the given stack index as segment indices.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="mainState">Main Lua state.</param>
    /// <param name="functionName">Function name for error messages.</param>
    /// <param name="argumentName">Argument name for error messages.</param>
    /// <param name="index">Stack index of the argument.</param>
    /// <param name="segment">Parsed segment. Only valid if the method returns true.</param>
    /// <returns>true if the argument was a valid segment vector, false otherwise.</returns>
    private bool TryGetSegmentVector(IntPtr luaState, IntPtr mainState, string functionName, string argumentName,
        int index, out GridPosition segment)
    {
        segment = default;

        if (!TryGetVector3Argument(luaState, index, mainState, functionName, argumentName, true, out var vector))
            return false;

        segment = new GridPosition((int)vector.X, (int)vector.Y, (int)vector.Z);
        return true;
    }

    /// <summary>
    ///     Reads a Vector3 position argument and resolves the containing world segment.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="mainState">Main Lua state.</param>
    /// <param name="functionName">Function name for error messages.</param>
    /// <param name="segment">Resolved segment. Only valid if the method returns true.</param>
    /// <returns>true if the position was valid, false otherwise.</returns>
    private bool TryGetPositionVector(IntPtr luaState, IntPtr mainState, string functionName, out GridPosition segment)
    {
        segment = default;

        if (!TryGetVector3Argument(luaState, 1, mainState, functionName, "position", false, out var vector))
            return false;

        segment = segmentResolver.GetWorldSegmentForPosition(vector);
        return true;
    }

    /// <summary>
    ///     Reads and validates a Vector3 argument.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="index">Stack index of the argument.</param>
    /// <param name="mainState">Main Lua state.</param>
    /// <param name="functionName">Function name for error messages.</param>
    /// <param name="argumentName">Argument name for error messages.</param>
    /// <param name="requireIntegerComponents">Whether every component must be an integer.</param>
    /// <param name="value">Parsed vector. Only valid if the method returns true.</param>
    /// <returns>true if the argument was a valid Vector3, false otherwise.</returns>
    private bool TryGetVector3Argument(IntPtr luaState, int index, IntPtr mainState, string functionName,
        string argumentName, bool requireIntegerComponents, out Vector3 value)
    {
        value = default;

        if (!lua_istable(luaState, index))
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("{Function}: {Argument} must be a Vector3.", functionName, argumentName);
            return false;
        }

        luaL_checkstack(luaState, 1, null);

        var vector = new Vector3();
        if (!TryGetVector3Component(luaState, index, "X", mainState, functionName, argumentName,
                requireIntegerComponents, out vector.X)
            || !TryGetVector3Component(luaState, index, "Y", mainState, functionName, argumentName,
                requireIntegerComponents, out vector.Y)
            || !TryGetVector3Component(luaState, index, "Z", mainState, functionName, argumentName,
                requireIntegerComponents, out vector.Z))
            return false;

        value = vector;
        return true;
    }

    /// <summary>
    ///     Reads and validates a single numeric component of a Vector3 argument.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="index">Stack index of the Vector3 table.</param>
    /// <param name="component">Component field name.</param>
    /// <param name="mainState">Main Lua state.</param>
    /// <param name="functionName">Function name for error messages.</param>
    /// <param name="argumentName">Argument name for error messages.</param>
    /// <param name="requireInteger">Whether the component must be an integer.</param>
    /// <param name="value">Parsed component. Only valid if the method returns true.</param>
    /// <returns>true if the component was valid, false otherwise.</returns>
    private bool TryGetVector3Component(IntPtr luaState, int index, string component, IntPtr mainState,
        string functionName, string argumentName, bool requireInteger, out float value)
    {
        value = 0.0f;

        if (lua_getfield(luaState, index, component) != LuaType.Number)
        {
            lua_pop(luaState, 1);
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("{Function}: {Argument}.{Component} must be a number.", functionName, argumentName,
                    component);
            return false;
        }

        if (requireInteger && !lua_isinteger(luaState, -1))
        {
            lua_pop(luaState, 1);
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("{Function}: {Argument}.{Component} must be an integer.", functionName, argumentName,
                    component);
            return false;
        }

        var raw = lua_tonumber(luaState, -1);
        lua_pop(luaState, 1);

        if (requireInteger && (raw < int.MinValue || raw > int.MaxValue))
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("{Function}: {Argument}.{Component} is out of range.", functionName, argumentName,
                    component);
            return false;
        }

        value = (float)raw;
        return true;
    }

    /// <summary>
    ///     Reads and validates a WorldSegmentFlag argument.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="index">Stack index of the argument.</param>
    /// <param name="mainState">Main Lua state.</param>
    /// <param name="functionName">Function name for error messages.</param>
    /// <param name="flag">Parsed flag. Only valid if the method returns true.</param>
    /// <returns>true if the argument was a valid non-zero flag, false otherwise.</returns>
    private bool TryGetFlagArgument(IntPtr luaState, int index, IntPtr mainState, string functionName,
        out WorldSegmentFlag flag)
    {
        flag = WorldSegmentFlag.None;

        if (!lua_isinteger(luaState, index))
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("{Function}: flag must be a WorldSegmentFlag integer value.", functionName);
            return false;
        }

        var raw = lua_tointeger(luaState, index);
        if (raw <= 0 || (raw & ~(long)DefinedFlagMask) != 0)
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError("{Function}: flag must be a non-zero combination of defined WorldSegmentFlag values.",
                    functionName);
            return false;
        }

        flag = (WorldSegmentFlag)raw;
        return true;
    }

    /// <summary>
    ///     Checks that the given box does not exceed the maximum allowed segment count.
    /// </summary>
    /// <param name="min">Minimum (inclusive) corner of the box.</param>
    /// <param name="max">Maximum (inclusive) corner of the box.</param>
    /// <returns>true if the box is acceptable, false otherwise.</returns>
    private static bool IsBoxSizeOk(GridPosition min, GridPosition max)
    {
        long sizeX = (long)max.X - min.X + 1;
        long sizeY = (long)max.Y - min.Y + 1;
        long sizeZ = (long)max.Z - min.Z + 1;

        return sizeX <= MaximumBoxSegments && sizeY <= MaximumBoxSegments && sizeZ <= MaximumBoxSegments
               && sizeX * sizeY * sizeZ <= MaximumBoxSegments;
    }

    /// <summary>
    ///     Computes the bitwise OR of every defined WorldSegmentFlag value.
    /// </summary>
    /// <returns>Mask of defined flag bits.</returns>
    private static WorldSegmentFlag ComputeDefinedFlagMask()
    {
        var mask = (long)WorldSegmentFlag.None;
        foreach (var value in Enum.GetValues<WorldSegmentFlag>()) mask |= (long)value;
        return (WorldSegmentFlag)mask;
    }
}
