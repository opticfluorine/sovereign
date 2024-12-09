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

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Lua;
using Sovereign.EngineCore.Systems;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     System responsible for managing server-side scripts.
/// </summary>
public class ScriptingSystem : ISystem
{
    private readonly ScriptManager manager;
    private readonly ScriptingCallbackManager callbackManager;
    private readonly ScriptLoader scriptLoader;
    private readonly ILogger<ScriptingSystem> logger;

    public ScriptingSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop, ScriptManager manager,
        ScriptingCallbackManager callbackManager, ScriptLoader scriptLoader, ILogger<ScriptingSystem> logger)
    {
        this.manager = manager;
        this.callbackManager = callbackManager;
        this.scriptLoader = scriptLoader;
        this.logger = logger;
        EventCommunicator = eventCommunicator;

        EventIdsOfInterest = new HashSet<EventId>(ScriptableEventSet.Events);
        EventIdsOfInterest.UnionWith([EventId.Server_Scripting_ReloadAll]);
        
        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }
    public ISet<EventId> EventIdsOfInterest { get; private set; }

    public int WorkloadEstimate => 1000;

    public void Initialize()
    {
        OnReloadAll();
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var handled = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            handled++;

            switch (ev.EventId)
            {
                case EventId.Server_Scripting_ReloadAll:
                    OnReloadAll();
                    break;
                
                default:
                    ForwardEventToScripts(ev);
                    break;
            }
        }
        
        return handled;
    }

    /// <summary>
    ///     Forwards an event to any interested scripts with registered callbacks.
    /// </summary>
    /// <param name="ev">Event.</param>
    private void ForwardEventToScripts(Event ev)
    {
        callbackManager.DispatchEvent(ev.EventId, ev.EventDetails);
    }

    /// <summary>
    ///     Called when a ReloadAll event is received.
    /// </summary>
    private void OnReloadAll()
    {
        logger.LogInformation("Reloading all scripts.");
        
        manager.UnloadAll();
        var scripts = scriptLoader.LoadAll();
        manager.Load(scripts);

        logger.LogInformation($"Loaded {scripts.Count} scripts.");
    }
}