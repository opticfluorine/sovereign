// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.ServerCore.Events;

namespace Sovereign.ServerCore.Systems.Debug;

public sealed class DebugSystem : ISystem, IDisposable
{
    private readonly DebugEventHandler eventHandler;
    private readonly IEventLoop eventLoop;

    public DebugSystem(DebugEventHandler eventHandler, EventCommunicator eventCommunicator,
        IEventLoop eventLoop, EventDescriptions eventDescriptions)
    {
        // Dependency injection.
        this.eventLoop = eventLoop;
        this.eventHandler = eventHandler;
        EventCommunicator = eventCommunicator;

        // Register events.
        eventDescriptions.RegisterEvent<DebugCommandEventDetails>(EventId.Server_Debug_Command);

        // Register system.
        eventLoop.RegisterSystem(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void Dispose()
    {
        eventLoop.UnregisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>
    {
        EventId.Server_Debug_Command
    };

    public int WorkloadEstimate => 20;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        // Poll for events.
        var eventsProcessed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            if (ev.EventDetails == null)
            {
                Logger.Error("Received debug command without details.");
                continue;
            }

            eventHandler.HandleDebugCommand((DebugCommandEventDetails)ev.EventDetails);
            eventsProcessed++;
        }

        return eventsProcessed;
    }
}