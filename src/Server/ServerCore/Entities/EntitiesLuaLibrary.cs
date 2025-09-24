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
using System.Threading;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Lua;
using Sovereign.Scripting.Lua;
using Sovereign.ServerCore.Systems.Scripting;
using Sovereign.ServerCore.Systems.WorldManagement;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.ServerCore.Entities;

/// <summary>
///     Lua binding for ServerEntityBuilder.
/// </summary>
public class EntitiesLuaLibrary : ILuaLibrary
{
    private const string IsTemplate =
        @"function entities.IsTemplate(entityId) 
              return entities.FirstTemplateEntityId <= entityId and entityId <= entities.LastTemplateEntityId 
          end";

    private readonly IEntityFactory entityFactory;
    private readonly EntityManager entityManager;
    private readonly EntityTable entityTable;
    private readonly Lock eventLock = new();
    private readonly IEventSender eventSender;
    private readonly ILogger<EntitiesLuaLibrary> logger;
    private readonly ScriptingServices scriptingServices;
    private readonly WorldManagementController worldManagementController;

    public EntitiesLuaLibrary(IEntityFactory entityFactory, ScriptingServices scriptingServices,
        ILogger<EntitiesLuaLibrary> logger, EntityManager entityManager, EntityTable entityTable,
        WorldManagementController worldManagementController, IEventSender eventSender)
    {
        this.entityFactory = entityFactory;
        this.scriptingServices = scriptingServices;
        this.logger = logger;
        this.entityManager = entityManager;
        this.entityTable = entityTable;
        this.worldManagementController = worldManagementController;
        this.eventSender = eventSender;
    }

    public void Install(LuaHost luaHost)
    {
        luaHost.BeginLibrary("entities");
        try
        {
            luaHost.AddLibraryFunction(nameof(Create), Create);
            luaHost.AddLibraryFunction(nameof(Remove), Remove);
            luaHost.AddLibraryFunction(nameof(GetTemplate), GetTemplate);
            luaHost.AddLibraryFunction(nameof(SetTemplate), SetTemplate);
            luaHost.AddLibraryFunction(nameof(Sync), Sync);
            luaHost.AddLibraryFunction(nameof(SyncTree), SyncTree);
            luaHost.AddLibraryConstant(nameof(EntityConstants.FirstTemplateEntityId),
                (long)EntityConstants.FirstTemplateEntityId);
            luaHost.AddLibraryConstant(nameof(EntityConstants.LastTemplateEntityId),
                (long)EntityConstants.LastTemplateEntityId);
            luaHost.AddLibraryConstant(nameof(EntityConstants.FirstBlockEntityId),
                (long)EntityConstants.FirstBlockEntityId);
            luaHost.AddLibraryConstant(nameof(EntityConstants.LastBlockEntityId),
                (long)EntityConstants.LastBlockEntityId);
            luaHost.AddLibraryConstant(nameof(EntityConstants.FirstPersistedEntityId),
                (long)EntityConstants.FirstPersistedEntityId);
            luaHost.LoadAndExecuteString(IsTemplate);
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
    private int Remove(IntPtr luaState)
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
    ///     Implementation of Lua function entities.Create.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of results pushed to the Lua stack.</returns>
    private int Create(IntPtr luaState)
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

    /// <summary>
    ///     Implementation of Lua function entities.GetTemplate(entityId).
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of return values.</returns>
    private int GetTemplate(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        var localLogger = scriptingServices.GetScriptLogger(mainState, logger);

        try
        {
            if (lua_gettop(luaState) != 1)
            {
                localLogger.LogError($"entities.{nameof(GetTemplate)}() requires one argument.");
                return 0;
            }

            if (!lua_isinteger(luaState, -1))
            {
                localLogger.LogError($"entities.{nameof(GetTemplate)}(): argument must be integer.");
                return 0;
            }

            var entityId = (ulong)lua_tointeger(luaState, -1);
            lua_pop(luaState, 1);

            if (!entityTable.TryGetTemplate(entityId, out var templateId)) return 0;

            lua_pushinteger(luaState, (long)templateId);
            return 1;
        }
        catch (Exception e)
        {
            localLogger.LogError(e, $"Error in entities.{nameof(GetTemplate)}().");
            return 0;
        }
    }

    /// <summary>
    ///     Implementation of Lua function entities.SetTemplate(entityId, templateId).
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always 0.</returns>
    private int SetTemplate(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        var localLogger = scriptingServices.GetScriptLogger(mainState, logger);

        try
        {
            if (lua_gettop(luaState) != 2)
            {
                localLogger.LogError($"entities.{nameof(SetTemplate)} requires two arguments.");
                return 0;
            }

            if (!lua_isinteger(luaState, -2) || !lua_isinteger(luaState, -1))
            {
                localLogger.LogError($"entities.{nameof(SetTemplate)}: arguments must be integers.");
                return 0;
            }

            var entityId = (ulong)lua_tointeger(luaState, -2);
            var templateId = (ulong)lua_tointeger(luaState, -1);

            if (EntityUtil.IsTemplateEntity(entityId))
            {
                localLogger.LogError(
                    $"entities.{nameof(SetTemplate)}: {{EntityId:X}} is a template and may not have its own template.",
                    entityId);
                return 0;
            }

            if (!EntityUtil.IsTemplateEntity(templateId))
            {
                localLogger.LogError($"entities.{nameof(SetTemplate)}: {{TemplateId:X}} is not a valid template ID.",
                    templateId);
                return 0;
            }

            entityTable.SetTemplate(entityId, templateId);
            return 0;
        }
        catch (Exception e)
        {
            localLogger.LogError(e, $"Error in entities.{nameof(SetTemplate)}.");
            return 0;
        }
    }

    /// <summary>
    ///     Implementation of Lua function entities.Sync(entities).
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always 0.</returns>
    private int Sync(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        var localLogger = scriptingServices.GetScriptLogger(mainState, logger);

        try
        {
            if (lua_gettop(luaState) != 1)
            {
                localLogger.LogError($"entities.{nameof(Sync)} requires one argument.");
                return 0;
            }

            if (lua_isinteger(luaState, -1))
            {
                // Request single entity sync.
                var entityId = (ulong)lua_tointeger(luaState, -1);
                DoSyncSingle(entityId);
            }
            else if (lua_istable(luaState, -1))
            {
                // Request sync of list of entities.
                luaL_checkstack(luaState, 2, null);
                lua_pushnil(luaState);
                while (lua_next(luaState, 1) != 0)
                {
                    if (!lua_isinteger(luaState, -1))
                    {
                        localLogger.LogWarning(
                            $"found non-integer item in table passed to entities.{nameof(Sync)}; skipping.");
                        lua_pop(luaState, 1);
                        continue;
                    }

                    var entityId = (ulong)lua_tointeger(luaState, -1);
                    DoSyncSingle(entityId);
                    lua_pop(luaState, 1);
                }
            }
            else
            {
                localLogger.LogError($"entities.{nameof(Sync)} requires integer or table argument.");
            }
        }
        catch (Exception e)
        {
            localLogger.LogError(e, $"Error in entities.{nameof(Sync)}.");
        }

        return 0;
    }

    /// <summary>
    ///     Synchronizes a single entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void DoSyncSingle(ulong entityId)
    {
        lock (eventLock)
        {
            worldManagementController.ResyncEntity(eventSender, entityId);
        }
    }

    /// <summary>
    ///     Implementation of Lua function entities.SyncTree(entities).
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always 0.</returns>
    private int SyncTree(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        var localLogger = scriptingServices.GetScriptLogger(mainState, logger);

        try
        {
            if (lua_gettop(luaState) != 1)
            {
                localLogger.LogError($"entities.{nameof(SyncTree)} requires one argument.");
                return 0;
            }

            if (lua_isinteger(luaState, -1))
            {
                // Request single entity sync.
                var entityId = (ulong)lua_tointeger(luaState, -1);
                DoSyncTreeSingle(entityId);
            }
            else if (lua_istable(luaState, -1))
            {
                // Request sync of list of entities.
                luaL_checkstack(luaState, 2, null);
                lua_pushnil(luaState);
                while (lua_next(luaState, 1) != 0)
                {
                    if (!lua_isinteger(luaState, -1))
                    {
                        localLogger.LogWarning(
                            $"found non-integer item in table passed to entities.{nameof(SyncTree)}; skipping.");
                        lua_pop(luaState, 1);
                        continue;
                    }

                    var entityId = (ulong)lua_tointeger(luaState, -1);
                    DoSyncTreeSingle(entityId);
                    lua_pop(luaState, 1);
                }
            }
            else
            {
                localLogger.LogError($"entities.{nameof(SyncTree)} requires integer or table argument.");
            }
        }
        catch (Exception e)
        {
            localLogger.LogError(e, $"Error in entities.{nameof(SyncTree)}.");
        }

        return 0;
    }

    /// <summary>
    ///     Synchronizes a single entity tree.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void DoSyncTreeSingle(ulong entityId)
    {
        lock (eventLock)
        {
            worldManagementController.ResyncEntityTree(eventSender, entityId);
        }
    }
}