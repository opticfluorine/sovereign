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
    private readonly Queue<ulong> entityReloadQueue = new();
    private readonly Queue<ulong> entityRemoveQueue = new();
    private readonly Dictionary<ulong, Task> entityTasks = new();
    private readonly Queue<(ulong, ulong)> entityTemplateUnloadQueue = new();
    private readonly Queue<ulong> entityUnloadQueue = new();
    private readonly ILogger<EntityScriptCallbacks> logger;
    private readonly ScriptManager scriptManager;
    private readonly EntityTable entityTable;

    public EntityScriptCallbacks(EntityTable entityTable, IDataServices dataServices, ScriptManager scriptManager,
        ILogger<EntityScriptCallbacks> logger)
    {
        this.entityTable = entityTable;
        this.dataServices = dataServices;
        this.scriptManager = scriptManager;
        this.logger = logger;
        entityTable.OnEntityAdded += OnEntityAdded;
        entityTable.OnEntityRemoved += OnEntityRemoved;
        entityTable.OnTemplateSet += OnTemplateSet;
    }

    /// <summary>
    ///     Requests a soft reload of the given entity by enqueueing its unload and load callbacks.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if the entity was accepted for reload, false otherwise.</returns>
    public bool RequestEntityReload(ulong entityId)
    {
        if (entityId is >= ExcludeRangeStart and < ExcludeRangeEnd) return false;
        if (!entityTable.Exists(entityId)) return false;
        entityReloadQueue.Enqueue(entityId);
        return true;
    }

    /// <summary>
    ///     Requests a soft reload of all loaded instances of the given template.
    /// </summary>
    /// <param name="templateId">Template entity ID.</param>
    /// <returns>Number of instances enqueued for reload.</returns>
    public int RequestTemplateReload(ulong templateId)
    {
        // Snapshot the live instance set so that changes during processing do not affect this request.
        var count = 0;
        foreach (var entityId in entityTable.GetInstancesOfTemplate(templateId))
        {
            entityReloadQueue.Enqueue(entityId);
            count++;
        }

        return count;
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
        ProcessReloadCallbacks();
    }

    /// <summary>
    ///     Invokes the interact callback of the target entity with the using entity ID, tool entity ID,
    ///     and target entity ID as arguments, in that order.
    /// </summary>
    /// <param name="usingEntityId">Entity ID of the entity using the tool.</param>
    /// <param name="toolEntityId">Tool entity ID.</param>
    /// <param name="targetEntityId">Target entity ID.</param>
    public void InvokeInteractCallback(ulong usingEntityId, ulong toolEntityId, ulong targetEntityId)
    {
        if (!dataServices.TryGetEntityKeyValue(targetEntityId, EntityConstants.InteractScriptKey,
                out var scriptName) ||
            !dataServices.TryGetEntityKeyValue(targetEntityId, EntityConstants.InteractFunctionKey,
                out var functionName)) return;

        InvokeCallback(targetEntityId, EntityConstants.InteractCallbackName, scriptName, functionName,
            usingEntityId, toolEntityId, targetEntityId);
    }

    /// <summary>
    ///     Processes pending soft reload callbacks by firing the unload hook followed by the load hook.
    /// </summary>
    private void ProcessReloadCallbacks()
    {
        while (entityReloadQueue.TryDequeue(out var entityId))
        {
            // Skip entities that were removed or unloaded since the reload was requested.
            if (!entityTable.Exists(entityId)) continue;

            // Fire the unload hook first, then the load hook. InvokeCallback chains tasks per entity ID,
            // so the load hook will not run until the unload hook completes.
            if (dataServices.TryGetEntityKeyValue(entityId, EntityConstants.UnloadCallbackScriptKey,
                    out var unloadScript) &&
                dataServices.TryGetEntityKeyValue(entityId, EntityConstants.UnloadCallbackFunctionKey,
                    out var unloadFunction))
                InvokeCallback(entityId, EntityConstants.UnloadCallbackName, unloadScript, unloadFunction, entityId);

            if (dataServices.TryGetEntityKeyValue(entityId, EntityConstants.LoadCallbackScriptKey,
                    out var loadScript) &&
                dataServices.TryGetEntityKeyValue(entityId, EntityConstants.LoadCallbackFunctionKey,
                    out var loadFunction))
                InvokeCallback(entityId, EntityConstants.LoadCallbackName, loadScript, loadFunction, entityId);
        }
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

            InvokeCallback(entityId, callbackName, scriptName, functionName, entityId);
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

            InvokeCallback(entityId, EntityConstants.UnloadCallbackName, scriptName, functionName, entityId);
        }
    }

    /// <summary>
    ///     Invokes a lifecycle callback for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="callbackName">Callback name.</param>
    /// <param name="scriptName">Script name.</param>
    /// <param name="functionName">Function name.</param>
    /// <param name="callbackArgs">Arguments passed to the callback.</param>
    private void InvokeCallback(ulong entityId, string callbackName, string scriptName, string functionName,
        params ulong[] callbackArgs)
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
                entityTasks[entityId] = Task.Run(() =>
                    RunCallback(callbackName, functionName, host, entityId, callbackArgs));
            else
                entityTasks[entityId] =
                    prevTask.ContinueWith(_ => RunCallback(callbackName, functionName, host, entityId, callbackArgs));
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
    /// <param name="callbackArgs">Arguments passed to the callback.</param>
    private static void RunCallback(string callbackName, string functionName, LuaHost host, ulong entityId,
        ulong[] callbackArgs)
    {
        try
        {
            host.CallNamedFunction(functionName, args =>
            {
                foreach (var arg in callbackArgs) args.AddLightUserData(arg);
                return callbackArgs.Length;
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
        if (isNew || entityId is >= ExcludeRangeStart and < ExcludeRangeEnd) return;

        entityLoadQueue.Enqueue(entityId);
        if (oldTemplateId > 0) entityTemplateUnloadQueue.Enqueue((entityId, oldTemplateId));
    }
}
