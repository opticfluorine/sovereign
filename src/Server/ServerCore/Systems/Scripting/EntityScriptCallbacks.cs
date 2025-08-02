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
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Data;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Handles entity callbacks for scripts.
/// </summary>
public sealed class EntityScriptCallbacks
{
    private const ulong ExcludeRangeStart = EntityConstants.FirstBlockEntityId;
    private const ulong ExcludeRangeEnd = EntityConstants.FirstPersistedEntityId;

    private readonly IDataServices dataServices;

    private readonly Queue<ulong> entityAddQueue = new();
    private readonly Queue<ulong> entityLoadQueue = new();
    private readonly Queue<ulong> entityRemoveQueue = new();
    private readonly Queue<ulong> entityUnloadQueue = new();
    private readonly ILogger<EntityScriptCallbacks> logger;
    private readonly ScriptManager scriptManager;

    public EntityScriptCallbacks(EntityTable entityTable, IDataServices dataServices, ScriptManager scriptManager,
        ILogger<EntityScriptCallbacks> logger)
    {
        this.dataServices = dataServices;
        this.scriptManager = scriptManager;
        this.logger = logger;
        entityTable.OnEntityAdded += OnEntityAdded;
        entityTable.OnEntityRemoved += OnEntityRemoved;
    }

    /// <summary>
    ///     Processes any pending callbacks for entity additions and removals.
    /// </summary>
    public void ProcessCallbacks()
    {
        ProcessCallbacks(entityAddQueue, EntityConstants.AddCallbackScriptKey, EntityConstants.AddCallbackFunctionKey,
            EntityConstants.AddCallbackName);
        ProcessCallbacks(entityRemoveQueue, EntityConstants.RemoveCallbackScriptKey,
            EntityConstants.RemoveCallbackFunctionKey, EntityConstants.RemoveCallbackName);
        ProcessCallbacks(entityLoadQueue, EntityConstants.LoadCallbackScriptKey,
            EntityConstants.LoadCallbackFunctionKey, EntityConstants.LoadCallbackName);
        ProcessCallbacks(entityUnloadQueue, EntityConstants.UnloadCallbackScriptKey,
            EntityConstants.UnloadCallbackFunctionKey, EntityConstants.UnloadCallbackName);
    }

    /// <summary>
    ///     Processes a set of pending callbacks.
    /// </summary>
    /// <param name="queue">Queue to process.</param>
    /// <param name="scriptKey">Script name key.</param>
    /// <param name="functionKey">Function name key.</param>
    /// <param name="callbackName">Callback name.</param>
    private void ProcessCallbacks(Queue<ulong> queue, string scriptKey, string functionKey, string callbackName)
    {
        while (queue.TryDequeue(out var entityId))
        {
            if (!dataServices.TryGetEntityKeyValue(entityId, scriptKey, out var scriptName)) continue;
            if (!dataServices.TryGetEntityKeyValue(entityId, functionKey, out var functionName)) continue;

            if (!scriptManager.TryGetHost(scriptName, out var host))
            {
                logger.LogError("Entity {EntityId:X} has unknown {CallbackName} callback script {ScriptName}.",
                    entityId, callbackName, scriptName);
                continue;
            }

            try
            {
                logger.LogTrace("Calling {CallbackName} callback {ScriptName}::{FunctionName} for entity {EntityId:X}.",
                    callbackName, scriptName, functionName, entityId);
                var currentEntityId = entityId;
                Task.Run(() =>
                {
                    try
                    {
                        host.CallNamedFunction(functionName, args =>
                        {
                            args.AddInteger((long)currentEntityId);
                            return 1;
                        });
                    }
                    catch (Exception e)
                    {
                        host.Logger.LogError(e,
                            "Error calling {CallbackName} callback {FunctionName} for entity {EntityId:X}.",
                            callbackName, functionName, currentEntityId);
                    }
                });
            }
            catch (Exception e)
            {
                host.Logger.LogError(e, "Error calling {CallbackName} callback {FunctionName} for entity {EntityId:X}.",
                    callbackName, functionName, entityId);
            }
        }
    }

    /// <summary>
    ///     Called when an entity is added.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isLoad">Whether this is a load.</param>
    private void OnEntityAdded(ulong entityId, bool isLoad)
    {
        // Adds also get processed as loads.
        if (entityId is >= ExcludeRangeStart and < ExcludeRangeEnd) return;
        entityLoadQueue.Enqueue(entityId);
        if (!isLoad) entityAddQueue.Enqueue(entityId);
    }

    /// <summary>
    ///     Called when an entity is removed.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isUnload">Whether this is an unload.</param>
    private void OnEntityRemoved(ulong entityId, bool isUnload)
    {
        // Removes also get processed as unloads.
        if (entityId is >= ExcludeRangeStart and < ExcludeRangeEnd) return;
        entityUnloadQueue.Enqueue(entityId);
        if (!isUnload) entityRemoveQueue.Enqueue(entityId);
    }
}