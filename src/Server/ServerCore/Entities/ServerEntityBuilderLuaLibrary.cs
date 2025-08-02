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
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Lua;
using Sovereign.Scripting.Lua;
using Sovereign.ServerCore.Systems.Scripting;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.ServerCore.Entities;

/// <summary>
///     Lua binding for ServerEntityBuilder.
/// </summary>
public class ServerEntityBuilderLuaLibrary : ILuaLibrary
{
    private readonly IEntityFactory entityFactory;
    private readonly EntityManager entityManager;
    private readonly ILogger<ServerEntityBuilderLuaLibrary> logger;
    private readonly ScriptingServices scriptingServices;

    public ServerEntityBuilderLuaLibrary(IEntityFactory entityFactory, ScriptingServices scriptingServices,
        ILogger<ServerEntityBuilderLuaLibrary> logger, EntityManager entityManager)
    {
        this.entityFactory = entityFactory;
        this.scriptingServices = scriptingServices;
        this.logger = logger;
        this.entityManager = entityManager;
    }

    public void Install(LuaHost luaHost)
    {
        luaHost.BeginLibrary("entities");
        try
        {
            luaHost.AddLibraryFunction("Create", BuildEntity);
            luaHost.AddLibraryFunction("Remove", RemoveEntity);
        }
        finally
        {
            luaHost.EndLibrary();
        }
    }

    /// <summary>
    ///     Implementation of Lua function entities.Remove.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of results pushed to the Lua stack.</returns>
    private int RemoveEntity(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        var localLogger = scriptingServices.GetScriptLogger(mainState, logger);
        try
        {
            if (lua_gettop(luaState) != 1)
            {
                localLogger.LogError("entities.Remove requires one argument.");
                return 0;
            }

            LuaMarshaller.Unmarshal(luaState, out ulong entityId);
            entityManager.RemoveEntity(entityId);
        }
        catch (Exception e)
        {
            localLogger.LogError(e, "Error in entities.Remove.");
        }

        return 0;
    }

    /// <summary>
    ///     Implementation of Lua function entities.Build.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of results pushed to the Lua stack.</returns>
    private int BuildEntity(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        IEntityBuilder? builder = null;
        try
        {
            var localLogger = scriptingServices.GetScriptLogger(mainState, logger);

            luaL_checkstack(luaState, 2, null);

            if (lua_gettop(luaState) != 1)
            {
                localLogger.LogError("entities.Build requires one argument.");
                lua_pushnil(luaState);
                return 1;
            }

            if (!lua_istable(luaState, -1))
            {
                localLogger.LogError("First argument to entities.Build must be table of entity data.");
                lua_pushnil(luaState);
                return 1;
            }

            // Check if an entity ID was specified.
            var luaType = lua_getfield(luaState, -1, "EntityId");
            if (luaType != LuaType.Nil)
            {
                if (!lua_isinteger(luaState, -1))
                {
                    localLogger.LogError("entities.Build: EntityId must be an integer if specified.");
                    lua_pop(luaState, 1);
                    lua_pushnil(luaState);
                    return 1;
                }

                var entityId = (ulong)lua_tointeger(luaState, -1);
                builder = entityFactory.GetBuilder(entityId);
            }
            else
            {
                // No entity ID specified, pick a new one.
                builder = entityFactory.GetBuilder();
            }

            lua_pop(luaState, 1);

            // Now that the builder has been created, iterate the remaining fields and set components.
            var tablePos = lua_gettop(luaState);
            luaL_checkstack(luaState, 3, null);
            lua_pushnil(luaState);
            while (lua_next(luaState, tablePos) != 0)
                if (!HandleKeyValuePair(luaState, localLogger, builder))
                {
                    lua_pop(luaState, 1);
                    lua_pushnil(luaState);
                    return 1;
                }

            // No need to pop the value - the handler does this for us in the success case.
            var builtEntityId = builder.Build();
            lua_pushinteger(luaState, (long)builtEntityId);
            return 1;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled exception in BuildEntity.");
            luaL_checkstack(luaState, 1, null);
            lua_pushnil(luaState);
            return 1;
        }
        finally
        {
            builder?.Dispose();
        }
    }

    /// <summary>
    ///     Parses a single key-value pair from the specification table.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="localLogger">Local logger.</param>
    /// <param name="builder">Builder.</param>
    /// <returns>true on success, false on error.</returns>
    private bool HandleKeyValuePair(IntPtr luaState, ILogger localLogger, IEntityBuilder builder)
    {
        const int keyIdx = -2;
        var keyType = lua_type(luaState, keyIdx);
        if (keyType != LuaType.String)
        {
            localLogger.LogError("entities.Build: argument keys must be strings, found type {Type} ({Id:X}).",
                keyType, (int)keyType);
            return false;
        }

        var key = lua_tostring(luaState, keyIdx);
        if (key == "EntityId") return true; // Already handled when builder was selected.

        return LuaEntityBuilderSupport.HandleKeyValuePair(luaState, builder, localLogger, key);
    }
}