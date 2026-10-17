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
using Sovereign.EngineCore.Timing;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.EntityAnimation;

public class EntityAnimationSystem : ISystem
{
    /// <summary>
    ///     Window in microseconds within which a server attack echo is treated as a duplicate of a
    ///     locally predicted attack and suppressed. Exceeds typical round-trip time so echoes land
    ///     inside it, while staying below the server's minimum re-attack delay (250 ms default) so
    ///     genuine rapid swings are not swallowed.
    /// </summary>
    private const ulong DedupeWindowUs = 150_000;

    private readonly ActiveAnimationManager animationManager;
    private readonly AnimationPhaseStateMachine animationPhaseStateMachine;
    private readonly ILogger<EntityAnimationSystem> logger;

    /// <summary>
    ///     System time in microseconds of the most recent predicted attack override per entity,
    ///     used to suppress the duplicate override from the server's rebroadcast of the attack.
    /// </summary>
    private readonly Dictionary<ulong, ulong> lastPredictedAttackUs = new();

    /// <summary>
    ///     Scratch buffer of predicted-attack entries to prune, reused to avoid per-scan allocation.
    /// </summary>
    private readonly List<ulong> stalePredictedAttacks = new();

    private readonly ISystemTimer systemTimer;

    /// <summary>
    ///     Creates a new entity animation system.
    /// </summary>
    /// <param name="eventCommunicator">Event communicator.</param>
    /// <param name="eventLoop">Event loop.</param>
    /// <param name="animationPhaseStateMachine">Animation phase state machine.</param>
    /// <param name="animationManager">Active animation manager.</param>
    /// <param name="systemTimer">System timer.</param>
    /// <param name="logger">Logger.</param>
    public EntityAnimationSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop,
        AnimationPhaseStateMachine animationPhaseStateMachine,
        ActiveAnimationManager animationManager,
        ISystemTimer systemTimer,
        ILogger<EntityAnimationSystem> logger)
    {
        this.animationPhaseStateMachine = animationPhaseStateMachine;
        this.animationManager = animationManager;
        this.systemTimer = systemTimer;
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

                    if (details.Phase == AnimationPhase.Attack)
                        lastPredictedAttackUs[details.EntityId] = systemTimer.GetTime();

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

                    var nowUs = systemTimer.GetTime();
                    PrunePredictedAttacks(nowUs);

                    // If the local client already predicted this attack, the override is already
                    // playing; restarting it here would show as a stutter one RTT after the swing.
                    if (lastPredictedAttackUs.TryGetValue(details.ActorId, out var predictedUs) &&
                        nowUs - predictedUs < DedupeWindowUs)
                        break;

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

    /// <summary>
    ///     Removes predicted-attack entries older than the dedupe window so the map stays bounded
    ///     by the set of actors attacked within the window.
    /// </summary>
    /// <param name="nowUs">Current system time in microseconds.</param>
    private void PrunePredictedAttacks(ulong nowUs)
    {
        if (lastPredictedAttackUs.Count == 0) return;

        stalePredictedAttacks.Clear();
        foreach (var (entityId, predictedUs) in lastPredictedAttackUs)
            if (nowUs - predictedUs >= DedupeWindowUs)
                stalePredictedAttacks.Add(entityId);

        foreach (var entityId in stalePredictedAttacks)
            lastPredictedAttackUs.Remove(entityId);
    }
}
