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
using System.Globalization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Lua;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.Scripting.Lua;
using Sovereign.ServerCore.Systems.Scripting;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.ServerCore.Entities;

/// <summary>
///     Lua binding for ServerEntityBuilder.
/// </summary>
public class EntitiesLuaLibrary : ILuaLibrary
{
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
        luaHost.BeginLibrary("Entities");
        try
        {
            luaHost.AddLibraryFunction(nameof(Create), Create);
            luaHost.AddLibraryFunction(nameof(Remove), Remove);
            luaHost.AddLibraryFunction(nameof(GetTemplate), GetTemplate);
            luaHost.AddLibraryFunction(nameof(SetTemplate), SetTemplate);
            luaHost.AddLibraryFunction(nameof(Sync), Sync);
            luaHost.AddLibraryFunction(nameof(SyncTree), SyncTree);
            luaHost.AddLibraryFunction(nameof(AbsoluteTemplateId), AbsoluteTemplateId);
            luaHost.AddLibraryFunction(nameof(FormatEntityId), FormatEntityId);
            luaHost.AddLibraryFunction(nameof(ToEntityId), ToEntityId);
            luaHost.AddLibraryFunction(nameof(ToTemplateEntityId), ToTemplateEntityId);
            luaHost.AddLibraryFunction(nameof(IsTemplate), IsTemplate);
            luaHost.AddLibraryEntityIdConstant(nameof(EntityConstants.FirstTemplateEntityId),
                EntityConstants.FirstTemplateEntityId);
            luaHost.AddLibraryEntityIdConstant(nameof(EntityConstants.LastTemplateEntityId),
                EntityConstants.LastTemplateEntityId);
            luaHost.AddLibraryEntityIdConstant(nameof(EntityConstants.FirstBlockEntityId),
                EntityConstants.FirstBlockEntityId);
            luaHost.AddLibraryEntityIdConstant(nameof(EntityConstants.LastBlockEntityId),
                EntityConstants.LastBlockEntityId);
            luaHost.AddLibraryEntityIdConstant(nameof(EntityConstants.FirstPersistedEntityId),
                EntityConstants.FirstPersistedEntityId);
            luaHost.AddLibraryEntityIdConstant("None", EntityConstants.NoEntity);
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
    ///     Reads an entity ID from the given position of the Lua stack.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="idx">Stack index.</param>
    /// <returns>Entity ID.</returns>
    /// <exception cref="LuaException">Thrown if the value is not an entity ID.</exception>
    private static ulong UnmarshalEntityId(IntPtr luaState, int idx)
    {
        if (!lua_islightuserdata(luaState, idx))
            throw new LuaException("Value is not an entity ID.");
        return (ulong)lua_touserdata(luaState, idx);
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
                if (!lua_islightuserdata(luaState, -1))
                {
                    localLogger.LogError(
                        "entities.Build: EntityId must be an entity ID (lightuserdata) if specified.");
                    lua_pop(luaState, 1);
                    lua_pushnil(luaState);
                    return 1;
                }

                var entityId = (ulong)lua_touserdata(luaState, -1);
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
            lua_pushlightuserdata(luaState, (IntPtr)builtEntityId);
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

            try
            {
                var entityId = UnmarshalEntityId(luaState, -1);
                lua_pop(luaState, 1);

                if (!entityTable.TryGetTemplate(entityId, out var templateId)) return 0;

                lua_pushlightuserdata(luaState, (IntPtr)templateId);
                return 1;
            }
            catch (LuaException)
            {
                localLogger.LogError($"entities.{nameof(GetTemplate)}(): argument must be an entity ID.");
                return 0;
            }
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

            try
            {
                var entityId = UnmarshalEntityId(luaState, -2);
                var templateId = UnmarshalEntityId(luaState, -1);

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
            catch (LuaException)
            {
                localLogger.LogError($"entities.{nameof(SetTemplate)}: arguments must be entity IDs.");
                return 0;
            }
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

            try
            {
                if (lua_islightuserdata(luaState, -1))
                {
                    // Request single entity sync.
                    var entityId = UnmarshalEntityId(luaState, -1);
                    DoSyncSingle(entityId);
                }
                else if (lua_istable(luaState, -1))
                {
                    // Request sync of list of entities.
                    luaL_checkstack(luaState, 2, null);
                    lua_pushnil(luaState);
                    while (lua_next(luaState, 1) != 0)
                    {
                        if (!lua_islightuserdata(luaState, -1))
                        {
                            localLogger.LogWarning(
                                $"found non-entity-ID item in table passed to entities.{nameof(Sync)}; skipping.");
                            lua_pop(luaState, 1);
                            continue;
                        }

                        var entityId = UnmarshalEntityId(luaState, -1);
                        DoSyncSingle(entityId);
                        lua_pop(luaState, 1);
                    }
                }
                else
                {
                    localLogger.LogError($"entities.{nameof(Sync)} requires entity ID or table argument.");
                }
            }
            catch (LuaException)
            {
                localLogger.LogError($"entities.{nameof(Sync)} requires entity ID or table argument.");
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

            try
            {
                if (lua_islightuserdata(luaState, -1))
                {
                    // Request single entity sync.
                    var entityId = UnmarshalEntityId(luaState, -1);
                    DoSyncTreeSingle(entityId);
                }
                else if (lua_istable(luaState, -1))
                {
                    // Request sync of list of entities.
                    luaL_checkstack(luaState, 2, null);
                    lua_pushnil(luaState);
                    while (lua_next(luaState, 1) != 0)
                    {
                        if (!lua_islightuserdata(luaState, -1))
                        {
                            localLogger.LogWarning(
                                $"found non-entity-ID item in table passed to entities.{nameof(SyncTree)}; skipping.");
                            lua_pop(luaState, 1);
                            continue;
                        }

                        var entityId = UnmarshalEntityId(luaState, -1);
                        DoSyncTreeSingle(entityId);
                        lua_pop(luaState, 1);
                    }
                }
                else
                {
                    localLogger.LogError($"entities.{nameof(SyncTree)} requires entity ID or table argument.");
                }
            }
            catch (LuaException)
            {
                localLogger.LogError($"entities.{nameof(SyncTree)} requires entity ID or table argument.");
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

    /// <summary>
    ///     Converts a relative template ID to absolute template ID.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of return values.</returns>
    private int AbsoluteTemplateId(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        var localLogger = scriptingServices.GetScriptLogger(mainState, logger);

        try
        {
            if (lua_gettop(luaState) != 1)
            {
                localLogger.LogError("AbsoluteTemplateId requires one parameter.");
                return 0;
            }

            if (!lua_isinteger(luaState, -1))
            {
                localLogger.LogError("AbsoluteTemplateId parameter must be an integer.");
                return 0;
            }

            var relativeId = (ulong)lua_tointeger(luaState, -1);
            lua_pop(luaState, 1);

            lua_pushlightuserdata(luaState, (IntPtr)(relativeId + EntityConstants.FirstTemplateEntityId));
            return 1;
        }
        catch (Exception e)
        {
            localLogger.LogError(e, "Error in AbsoluteTemplateId.");
            return 0;
        }
    }

    /// <summary>
    ///     Formats an entity ID as an uppercase hexadecimal string.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of return values.</returns>
    private int FormatEntityId(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        var localLogger = scriptingServices.GetScriptLogger(mainState, logger);

        try
        {
            if (lua_gettop(luaState) != 1)
            {
                localLogger.LogError("FormatEntityId requires one parameter.");
                return 0;
            }

            var entityId = UnmarshalEntityId(luaState, -1);
            lua_pushstring(luaState, entityId.ToString("X", CultureInfo.InvariantCulture));
            return 1;
        }
        catch (LuaException)
        {
            localLogger.LogError("FormatEntityId parameter must be an entity ID.");
            return 0;
        }
        catch (Exception e)
        {
            localLogger.LogError(e, "Error in FormatEntityId.");
            return 0;
        }
    }

    /// <summary>
    ///     Converts an integer to an entity ID.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of return values.</returns>
    private int ToEntityId(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        var localLogger = scriptingServices.GetScriptLogger(mainState, logger);

        try
        {
            if (lua_gettop(luaState) != 1)
            {
                localLogger.LogError("ToEntityId requires one parameter.");
                return 0;
            }

            if (!lua_isinteger(luaState, -1))
            {
                localLogger.LogError("ToEntityId parameter must be an integer.");
                return 0;
            }

            var entityId = (ulong)lua_tointeger(luaState, -1);
            lua_pushlightuserdata(luaState, (IntPtr)entityId);
            return 1;
        }
        catch (Exception e)
        {
            localLogger.LogError(e, "Error in ToEntityId.");
            return 0;
        }
    }

    /// <summary>
    ///     Converts a relative template ID to an absolute template entity ID.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of return values.</returns>
    private int ToTemplateEntityId(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        var localLogger = scriptingServices.GetScriptLogger(mainState, logger);

        try
        {
            if (lua_gettop(luaState) != 1)
            {
                localLogger.LogError("ToTemplateEntityId requires one parameter.");
                return 0;
            }

            if (!lua_isinteger(luaState, -1))
            {
                localLogger.LogError("ToTemplateEntityId parameter must be an integer.");
                return 0;
            }

            var relativeId = (ulong)lua_tointeger(luaState, -1);
            lua_pushlightuserdata(luaState, (IntPtr)(EntityConstants.FirstTemplateEntityId + relativeId));
            return 1;
        }
        catch (Exception e)
        {
            localLogger.LogError(e, "Error in ToTemplateEntityId.");
            return 0;
        }
    }

    /// <summary>
    ///     Determines whether an entity ID is a template entity ID.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of return values.</returns>
    private int IsTemplate(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        var localLogger = scriptingServices.GetScriptLogger(mainState, logger);

        try
        {
            if (lua_gettop(luaState) != 1)
            {
                localLogger.LogError("IsTemplate requires one parameter.");
                return 0;
            }

            var entityId = UnmarshalEntityId(luaState, -1);
            lua_pushboolean(luaState, EntityUtil.IsTemplateEntity(entityId));
            return 1;
        }
        catch (LuaException)
        {
            localLogger.LogError("IsTemplate parameter must be an entity ID.");
            return 0;
        }
        catch (Exception e)
        {
            localLogger.LogError(e, "Error in IsTemplate.");
            return 0;
        }
    }
}
