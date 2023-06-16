// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.ServerCore.Events;

namespace Sovereign.ServerCore.Systems.Debug;

public sealed class DebugSystem : ISystem, IDisposable
{
    private readonly DebugEventHandler eventHandler;
    private readonly IEventLoop eventLoop;

    public EventCommunicator EventCommunicator { get; private set; }

    public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>()
    {
        EventId.Server_Debug_Command,
    };

    public int WorkloadEstimate => 20;
    
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
            eventHandler.HandleDebugCommand((DebugCommandEventDetails)ev.EventDetails);
            eventsProcessed++;
        }

        return eventsProcessed;
    }

    public void Dispose()
    {
        eventLoop.UnregisterSystem(this);
    }
}