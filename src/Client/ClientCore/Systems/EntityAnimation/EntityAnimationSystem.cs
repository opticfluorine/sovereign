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
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;

namespace Sovereign.ClientCore.Systems.EntityAnimation;

public class EntityAnimationSystem : ISystem
{
    private readonly AnimationPhaseStateMachine animationPhaseStateMachine;

    public EntityAnimationSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop,
        AnimationPhaseStateMachine animationPhaseStateMachine)
    {
        this.animationPhaseStateMachine = animationPhaseStateMachine;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Movement_Move
    };

    public int WorkloadEstimate => 30;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var processed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            processed++;
            switch (ev.EventId)
            {
                case EventId.Core_Movement_Move:
                {
                    if (ev.EventDetails is not MoveEventDetails details)
                    {
                        Logger.Error("Received Move event without details.");
                        break;
                    }

                    animationPhaseStateMachine.OnMovement(details);
                }
                    break;
            }
        }

        return processed;
    }
}