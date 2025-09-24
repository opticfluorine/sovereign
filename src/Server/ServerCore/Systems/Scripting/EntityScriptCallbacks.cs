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
using Sovereign.Scripting.Lua;

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
    private readonly Dictionary<ulong, Task> entityTasks = new();
    private readonly Queue<(ulong, ulong)> entityTemplateUnloadQueue = new();
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
        entityTable.OnTemplateSet += OnTemplateSet;
    }

    /// <summary>
    ///     Processes any pending callbacks for entity additions and removals.
    /// </summary>
    public void ProcessCallbacks()
    {
        // Note that ProcessPriorTemplateUnloadCallbacks() must be called prior to the load callbacks, because
        // the new template entity could have callbacks into the same script but with different parameters - the
        // script needs a chance to clean up the prior template state before attaching the new template.
        entityTasks.Clear();
        ProcessCallbacks(entityAddQueue, EntityConstants.AddCallbackScriptKey, EntityConstants.AddCallbackFunctionKey,
            EntityConstants.AddCallbackName);
        ProcessCallbacks(entityRemoveQueue, EntityConstants.RemoveCallbackScriptKey,
            EntityConstants.RemoveCallbackFunctionKey, EntityConstants.RemoveCallbackName);
        ProcessPriorTemplateUnloadCallbacks();
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

            InvokeCallback(entityId, callbackName, scriptName, functionName);
        }
    }

    /// <summary>
    ///     Processes pending unload callbacks from template changes.
    /// </summary>
    private void ProcessPriorTemplateUnloadCallbacks()
    {
        while (entityTemplateUnloadQueue.TryDequeue(out var info))
        {
            var (entityId, templateId) = info;

            // Look up the unload callback for the previous template entity directly since the entity
            // now points to its new template.
            if (!dataServices.TryGetEntityKeyValue(templateId, EntityConstants.UnloadCallbackScriptKey,
                    out var scriptName)) continue;
            if (!dataServices.TryGetEntityKeyValue(templateId, EntityConstants.UnloadCallbackFunctionKey,
                    out var functionName)) continue;

            InvokeCallback(entityId, EntityConstants.UnloadCallbackName, scriptName, functionName);
        }
    }

    /// <summary>
    ///     Invokes a lifecycle callback for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="callbackName">Callback name.</param>
    /// <param name="scriptName">Script name.</param>
    /// <param name="functionName">Function name.</param>
    private void InvokeCallback(ulong entityId, string callbackName, string scriptName, string functionName)
    {
        if (!scriptManager.TryGetHost(scriptName, out var host))
        {
            logger.LogError("Entity {EntityId:X} has unknown {CallbackName} callback script {ScriptName}.",
                entityId, callbackName, scriptName);
            return;
        }

        try
        {
            logger.LogTrace("Calling {CallbackName} callback {ScriptName}::{FunctionName} for entity {EntityId:X}.",
                callbackName, scriptName, functionName, entityId);
            if (!entityTasks.TryGetValue(entityId, out var prevTask))
                entityTasks[entityId] = Task.Run(() => RunCallback(callbackName, functionName, host, entityId));
            else
                entityTasks[entityId] =
                    prevTask.ContinueWith(_ => RunCallback(callbackName, functionName, host, entityId));
        }
        catch (Exception e)
        {
            host.Logger.LogError(e, "Error calling {CallbackName} callback {FunctionName} for entity {EntityId:X}.",
                callbackName, functionName, entityId);
        }
    }

    /// <summary>
    ///     Runs a callback.
    /// </summary>
    /// <param name="callbackName">Callback name.</param>
    /// <param name="functionName">Function name.</param>
    /// <param name="host">Script host.</param>
    /// <param name="entityId">Entity ID.</param>
    private static void RunCallback(string callbackName, string functionName, LuaHost host, ulong entityId)
    {
        try
        {
            host.CallNamedFunction(functionName, args =>
            {
                args.AddInteger((long)entityId);
                return 1;
            });
        }
        catch (Exception e)
        {
            host.Logger.LogError(e,
                "Error calling {CallbackName} callback {FunctionName} for entity {EntityId:X}.",
                callbackName, functionName, entityId);
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

    /// <summary>
    ///     Called when an entity's template is changed.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="templateId">Template ID.</param>
    /// <param name="oldTemplateId">Old template ID, or 0 if there was no previous template.</param>
    /// <param name="isUnload">Unused.</param>
    /// <param name="isNew">If true, entity is newly added; false otherwise.</param>
    private void OnTemplateSet(ulong entityId, ulong templateId, ulong oldTemplateId, bool isUnload, bool isNew)
    {
        // If the entity is newly added, its callbacks will be enqueued through OnEntityAdded. Don't do anything
        // here in order to avoid double-invoking the lifecycle callbacks.
        if (isNew) return;

        entityLoadQueue.Enqueue(entityId);
        if (oldTemplateId > 0) entityTemplateUnloadQueue.Enqueue((entityId, oldTemplateId));
    }
}