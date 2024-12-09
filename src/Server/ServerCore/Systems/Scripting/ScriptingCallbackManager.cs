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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Lua;
using Sovereign.Scripting.Lua;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Manages the set of registered script callback functions.
/// </summary>
public class ScriptingCallbackManager
{
    private readonly ILogger<ScriptingCallbackManager> logger;
    private readonly ConcurrentDictionary<EventId, List<Callback>> callbacks = new();
    private readonly ConcurrentDictionary<IntPtr, HashSet<EventId>> eventIdsByHost = new();

    public ScriptingCallbackManager(ILogger<ScriptingCallbackManager> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    ///     Registers an event callback function.
    /// </summary>
    /// <param name="luaHost">Lua host.</param>
    /// <param name="eventId">Event ID.</param>
    /// <param name="quickLookupIndex">Quick lookup function index to be called.</param>
    public void AddEventCallback(LuaHost luaHost, EventId eventId, uint quickLookupIndex)
    {
        if (!callbacks.TryGetValue(eventId, out var cbList))
        {
            cbList = new List<Callback>();
            callbacks[eventId] = cbList;
        }

        if (!eventIdsByHost.TryGetValue(luaHost.LuaState, out var evList))
        {
            evList = new HashSet<EventId>();
            eventIdsByHost[luaHost.LuaState] = evList;
        }

        cbList.Add(new Callback(luaHost, quickLookupIndex));
        evList.Add(eventId);
    }

    /// <summary>
    ///     Dispatches an event to all registered callbacks on that event.
    /// </summary>
    /// <remarks>
    ///     Each callback is invoked in a separate task to avoid blocking the calling thread
    ///     in the event that the callback contains malformed code (e.g. infinite loops).
    /// </remarks>
    /// <param name="eventId">Event ID.</param>
    /// <param name="details">Event details.</param>
    public void DispatchEvent(EventId eventId, IEventDetails? details)
    {
        if (!callbacks.TryGetValue(eventId, out var cbList)) return;
        foreach (var callback in cbList) DispatchEventAsync(eventId, callback, details);
    }

    /// <summary>
    ///     Invokes the given event callback asynchronously in a background task.
    /// </summary>
    /// <param name="eventId">Event ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="details">Details.</param>
    private void DispatchEventAsync(EventId eventId, Callback callback, IEventDetails? details)
    {
        Task.Run(() =>
        {
            if (callback.LuaHost.IsDisposed) return;
            try
            {
                callback.LuaHost.CallQuickLookupFunction(callback.QuickLookupIndex,
                    () => ScriptableEventSet.MarshalEventDetails(callback.LuaHost, eventId, details));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error dispatching event {Event} to script {Script}.", eventId,
                    callback.LuaHost.Name);
            }
        });
    }

    private record struct Callback(LuaHost LuaHost, uint QuickLookupIndex);
}