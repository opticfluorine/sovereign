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
using Sovereign.ClientCore.Events.Details;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.EntityAnimation;

public class EntityAnimationSystem : ISystem
{
    private readonly ActiveAnimationManager animationManager;
    private readonly AnimationPhaseStateMachine animationPhaseStateMachine;
    private readonly ILogger<EntityAnimationSystem> logger;

    /// <summary>
    ///     Creates a new entity animation system.
    /// </summary>
    /// <param name="eventCommunicator">Event communicator.</param>
    /// <param name="eventLoop">Event loop.</param>
    /// <param name="animationPhaseStateMachine">Animation phase state machine.</param>
    /// <param name="animationManager">Active animation manager.</param>
    /// <param name="logger">Logger.</param>
    public EntityAnimationSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop,
        AnimationPhaseStateMachine animationPhaseStateMachine,
        ActiveAnimationManager animationManager,
        ILogger<EntityAnimationSystem> logger)
    {
        this.animationPhaseStateMachine = animationPhaseStateMachine;
        this.animationManager = animationManager;
        this.logger = logger;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Movement_Move,
        EventId.Client_EntityAnimation_PlayOverride,
        EventId.Server_Combat_Attack
    };

    public int WorkloadEstimate => 50;

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
                        logger.LogError("Received Move event without details.");
                        break;
                    }

                    animationPhaseStateMachine.OnMovement(details);
                }
                    break;

                case EventId.Client_EntityAnimation_PlayOverride:
                {
                    if (ev.EventDetails is not ActiveAnimationPhaseEventDetails details)
                    {
                        logger.LogError("Received PlayOverride event without details.");
                        break;
                    }

                    animationManager.OnPlayOverride(details);
                }
                    break;

                case EventId.Server_Combat_Attack:
                {
                    if (ev.EventDetails is not AttackEventDetails details)
                    {
                        logger.LogError("Received Attack event without details.");
                        break;
                    }

                    animationManager.OnPlayOverride(new ActiveAnimationPhaseEventDetails
                    {
                        EntityId = details.ActorId,
                        Phase = AnimationPhase.Attack,
                        Loops = 1
                    });
                }
                    break;
            }
        }

        animationManager.ExpireCompletedOverrides();

        return processed;
    }
}
