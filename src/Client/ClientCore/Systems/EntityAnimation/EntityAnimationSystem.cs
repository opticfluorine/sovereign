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
using Sovereign.ClientCore.Components;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineCore.Timing;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.EntityAnimation;

public class EntityAnimationSystem : ISystem
{
    private readonly ActiveAnimationPhaseComponentCollection activePhases;
    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly AnimationPhaseStateMachine animationPhaseStateMachine;
    private readonly ILogger<EntityAnimationSystem> logger;
    private readonly OrientationComponentCollection orientations;

    /// <summary>
    ///     Entities whose override removal has been enqueued but not yet applied. Prevents the
    ///     removal from being enqueued again on every scan between component update ticks.
    /// </summary>
    private readonly HashSet<ulong> pendingRemovals = new();

    private readonly ISystemTimer systemTimer;

    /// <summary>
    ///     Creates a new entity animation system.
    /// </summary>
    /// <param name="eventCommunicator">Event communicator.</param>
    /// <param name="eventLoop">Event loop.</param>
    /// <param name="animationPhaseStateMachine">Animation phase state machine.</param>
    /// <param name="activePhases">Active animation phase override collection.</param>
    /// <param name="animatedSprites">Animated sprite component collection.</param>
    /// <param name="orientations">Orientation component collection.</param>
    /// <param name="animatedSpriteManager">Animated sprite manager.</param>
    /// <param name="systemTimer">System timer.</param>
    /// <param name="logger">Logger.</param>
    public EntityAnimationSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop,
        AnimationPhaseStateMachine animationPhaseStateMachine,
        ActiveAnimationPhaseComponentCollection activePhases,
        AnimatedSpriteComponentCollection animatedSprites,
        OrientationComponentCollection orientations,
        AnimatedSpriteManager animatedSpriteManager,
        ISystemTimer systemTimer,
        ILogger<EntityAnimationSystem> logger)
    {
        this.animationPhaseStateMachine = animationPhaseStateMachine;
        this.activePhases = activePhases;
        this.animatedSprites = animatedSprites;
        this.orientations = orientations;
        this.animatedSpriteManager = animatedSpriteManager;
        this.systemTimer = systemTimer;
        this.logger = logger;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Movement_Move
    };

    public int WorkloadEstimate => 40;

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
            }
        }

        ExpireActiveAnimationPhases();

        return processed;
    }

    /// <summary>
    ///     Scans for animation overrides that have played their loop count and removes them so the
    ///     entity reverts to its movement-driven animation phase.
    /// </summary>
    private void ExpireActiveAnimationPhases()
    {
        // Forget scheduled removals once they are no longer buffered in the component
        // collection, i.e. once they have been applied or cancelled by a restart. Entries
        // are kept while the removal remains pending so it is not enqueued twice between
        // component update ticks; once forgotten, the entity is re-examined by later scans
        // and expires naturally based on its current override.
        if (pendingRemovals.Count > 0)
            pendingRemovals.RemoveWhere(entityId => !activePhases.HasPendingRemoveForEntity(entityId));

        if (activePhases.ComponentCount == 0) return;

        var systemTime = systemTimer.GetTime();
        for (var i = 0; i < activePhases.ComponentCount; ++i)
        {
            if (!activePhases.TryGetEntityForIndex(i, out var entityId)) continue;
            if (pendingRemovals.Contains(entityId)) continue;
            if (!activePhases.TryGetValue(entityId, out var active)) continue;

            var loopDuration = active.LoopDurationUs;
            if (loopDuration == 0)
            {
                if (!TryResolveLoopDuration(entityId, active, out loopDuration))
                {
                    // The entity cannot render the override, so remove it immediately.
                    RemoveOverride(entityId);
                    continue;
                }

                // Pin the resolved duration so playback time stays stable across orientation and
                // sprite-definition changes mid-override.
                active.LoopDurationUs = loopDuration;
                activePhases.AddOrUpdateComponent(entityId, active);
            }

            if (systemTime - active.StartTimeUs >= (ulong)active.Loops * loopDuration)
                RemoveOverride(entityId);
        }
    }

    /// <summary>
    ///     Enqueues the removal of the given entity's override exactly once.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void RemoveOverride(ulong entityId)
    {
        activePhases.RemoveComponent(entityId);
        pendingRemovals.Add(entityId);
    }

    /// <summary>
    ///     Attempts to resolve the loop duration of the given override from the entity's animated
    ///     sprite and orientation.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="active">Active override.</param>
    /// <param name="loopDuration">Resolved loop duration. Only meaningful if this returns true.</param>
    /// <returns>true if a nonzero loop duration was resolved, false otherwise.</returns>
    private bool TryResolveLoopDuration(ulong entityId, ActiveAnimationPhase active, out ulong loopDuration)
    {
        loopDuration = 0;

        if (!animatedSprites.TryGetValue(entityId, out var spriteId)) return false;
        if (spriteId < 0 || spriteId >= animatedSpriteManager.AnimatedSprites.Count) return false;

        var orientation = orientations.TryGetValue(entityId, out var entityOrientation)
            ? entityOrientation
            : Orientation.South;

        var animatedSprite = animatedSpriteManager.AnimatedSprites[spriteId];
        loopDuration = animatedSprite.GetPhaseData(active.Phase).GetLoopDurationUs(orientation);
        return loopDuration > 0;
    }
}
