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
using Sovereign.EngineCore.Events;

namespace Sovereign.EngineCore.Systems.Movement;

public class MovementSystem : ISystem
{
    private readonly MovementEventHandler eventHandler;
    private readonly IEventLoop eventLoop;

    public MovementSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop, MovementEventHandler eventHandler)
    {
        this.eventLoop = eventLoop;
        this.eventHandler = eventHandler;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Movement_Move,
        EventId.Core_Movement_RequestMove,
        EventId.Core_Movement_Jump,
        EventId.Core_Movement_Teleport,
        EventId.Core_Tick,
        EventId.Core_Block_GridUpdated,
        EventId.Core_WorldManagement_WorldSegmentLoaded,
        EventId.Core_WorldManagement_WorldSegmentUnloaded
    };

    public int WorkloadEstimate => 300;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var eventsProcessed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            eventHandler.HandleEvent(ev);
            eventsProcessed++;
        }

        return eventsProcessed;
    }
}