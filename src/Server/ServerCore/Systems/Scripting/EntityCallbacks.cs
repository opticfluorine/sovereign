// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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
using System.Threading;
using Microsoft.Extensions.Logging;
using Sovereign.Scripting.Lua;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Manages a set of entity-specific scripting callbacks.
/// </summary>
public sealed class EntityCallbackManager
{
    private readonly Lock accessLock = new();
    private readonly Dictionary<ulong, List<(LuaHost, int)>> callbacks = new();
    private readonly Dictionary<IntPtr, List<ulong>> entitiesByHost = new();
    private readonly HashSet<(LuaHost, ulong, int)> pendingRemoves = new();

    /// <summary>
    ///     Adds an entity callback.
    /// </summary>
    /// <param name="luaHost">Lua host.</param>
    /// <param name="entityId">Entity ID to subscribe to updates for.</param>
    /// <param name="callbackRef">Callback function reference index.</param>
    public void AddCallback(LuaHost luaHost, ulong entityId, int callbackRef)
    {
        lock (accessLock)
        {
            if (!callbacks.TryGetValue(entityId, out var entityCallbacks))
            {
                entityCallbacks = new List<(LuaHost, int)>();
                callbacks[entityId] = entityCallbacks;
            }

            if (!entitiesByHost.TryGetValue(luaHost.LuaState, out var entityList))
            {
                entityList = new List<ulong>();
                entitiesByHost[luaHost.LuaState] = entityList;
            }

            entityCallbacks.Add((luaHost, callbackRef));
            entityList.Add(entityId);
        }
    }

    /// <summary>
    ///     Removes an entity callback.
    /// </summary>
    /// <param name="luaHost">Lua host.</param>
    /// <param name="callingLuaState">Calling Lua state.</param>
    /// <param name="entityId">Entity ID to unsubscribe from.</param>
    /// <param name="callbackRef">Callback function reference index.</param>
    public void RemoveCallback(LuaHost luaHost, IntPtr callingLuaState, ulong entityId, int callbackRef)
    {
        lock (accessLock)
        {
            pendingRemoves.Add((luaHost, entityId, callbackRef));
        }
    }

    /// <summary>
    ///     Applies pending removes.
    /// </summary>
    public void DoRemoveCallback()
    {
        lock (accessLock)
        {
            foreach (var (luaHost, entityId, callbackRef) in pendingRemoves)
            {
                if (!callbacks.TryGetValue(entityId, out var entityCallbacks))
                {
                    luaHost.Logger.LogWarning("Tried to remove nonexistent callback for entity ID {EntityId:X}.",
                        entityId);
                    return;
                }

                if (!entitiesByHost.TryGetValue(luaHost.LuaState, out var entityList))
                {
                    luaHost.Logger.LogError("No entity list found for script.");
                    entityList = [];
                }

                for (var i = 0; i < entityCallbacks.Count; ++i)
                {
                    var (curHost, curCbRef) = entityCallbacks[i];
                    if (curHost.LuaState == luaHost.LuaState && curCbRef == callbackRef)
                    {
                        entityCallbacks.RemoveAt(i);
                        break;
                    }
                }

                for (var i = 0; i < entityList.Count; ++i)
                {
                    if (entityList[i] == entityId)
                    {
                        entityList.RemoveAt(i);
                        break;
                    }
                }
            }

            pendingRemoves.Clear();
        }
    }

    /// <summary>
    ///     Removes all entity callbacks for the given Lua host.
    /// </summary>
    /// <param name="luaHost">Lua host.</param>
    public void RemoveHost(LuaHost luaHost)
    {
        lock (accessLock)
        {
            if (!entitiesByHost.TryGetValue(luaHost.LuaState, out var entityList))
            {
                luaHost.Logger.LogError(luaHost.LuaState, "No entity list found for script.");
                entityList = [];
            }

            foreach (var entityId in entityList)
            {
                if (!callbacks.TryGetValue(entityId, out var entityCallbacks))
                {
                    luaHost.Logger.LogError(luaHost.LuaState,
                        "No callbacks found when removing for entity ID {EntityId:X}.", entityId);
                    continue;
                }

                for (var i = entityCallbacks.Count - 1; i >= 0; --i)
                {
                    var (curHost, _) = entityCallbacks[i];
                    if (curHost.LuaState == luaHost.LuaState) entityCallbacks.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    ///     Called when an entity-specific event occurs.
    /// </summary>
    /// <param name="entityId"></param>
    public void OnEvent(ulong entityId)
    {
        lock (accessLock)
        {
            if (!callbacks.TryGetValue(entityId, out var matchingCallbacks)) return;

            foreach (var (host, cref) in matchingCallbacks)
            {
                try
                {
                    if (pendingRemoves.Contains((host, entityId, cref))) continue;
                    host.Logger.LogTrace("Invoking callback for entity ID {EntityId:X}.", entityId);
                    host.CallRefFunction(cref, (long)entityId);
                }
                catch (Exception e)
                {
                    host.Logger.LogError(e, "Error calling callback for entity ID {EntityId:X}.",
                        entityId);
                }
            }
        }
    }
}

/// <summary>
///     Collection of entity callbacks for scripting.
/// </summary>
public sealed class EntityCallbacks
{
    /// <summary>
    ///     Collision callbacks.
    /// </summary>
    public EntityCallbackManager Collisions { get; } = new();

    /// <summary>
    ///     Scheduled stop callbacks.
    /// </summary>
    public EntityCallbackManager ScheduledStops { get; } = new();

    /// <summary>
    ///     Handles any per-tick processing for the various callbacks.
    /// </summary>
    public void DoPerTickProcessing()
    {
        Collisions.DoRemoveCallback();
        ScheduledStops.DoRemoveCallback();
    }
}