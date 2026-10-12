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
using Microsoft.Extensions.Logging.Abstractions;
using Sovereign.ClientCore.Components;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Systems.EntityAnimation;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using TestClientCore.TestSupport;
using Xunit;

namespace TestClientCore.Systems.EntityAnimation;

/// <summary>
///     Unit tests for the one-shot animation override component and its countdown.
/// </summary>
public class TestActiveAnimationPhase
{
    private const ulong EntityId = 0x7fff000000000001;

    private readonly ActiveAnimationPhaseComponentCollection activePhases;
    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly AnimationPhaseComponentCollection basePhases;
    private readonly OrientationComponentCollection orientations;
    private readonly AnimationPhaseSelector selector;
    private readonly EntityAnimationController controller;
    private readonly EntityAnimationSystem system;
    private readonly ForwardingEventSender eventSender;
    private readonly FakeSystemTimer systemTimer = new();

    public TestActiveAnimationPhase()
    {
        var entityTable = new EntityTable();
        var componentManager = new ComponentManager(new EntityNotifier());
        basePhases = new AnimationPhaseComponentCollection(entityTable, componentManager);
        activePhases = new ActiveAnimationPhaseComponentCollection(entityTable, componentManager);
        animatedSprites = new AnimatedSpriteComponentCollection(entityTable, componentManager);
        orientations = new OrientationComponentCollection(entityTable, componentManager);
        animatedSpriteManager =
            new AnimatedSpriteManager(null!, null!, null!, NullLogger<AnimatedSpriteManager>.Instance);

        selector = new AnimationPhaseSelector(basePhases, activePhases);
        system = new EntityAnimationSystem(new EventCommunicator(), new FakeEventLoop(),
            new AnimationPhaseStateMachine(basePhases), new ActiveAnimationManager(activePhases,
                animatedSprites, orientations, animatedSpriteManager, systemTimer),
            systemTimer, NullLogger<EntityAnimationSystem>.Instance);
        eventSender = new ForwardingEventSender(system);
        controller = new EntityAnimationController();
    }

    [Fact]
    public void Override_PlaysFullLoopsFromFrameZero()
    {
        var sprite = AddSprite(CreateSprite(
            (AnimationPhase.Moving, Orientation.South, 4)));

        const ulong startTime = 12345;
        basePhases.AddOrUpdateComponent(EntityId, AnimationPhase.Default);
        systemTimer.Time = startTime;
        controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Moving, 2);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();

        // Immediately after setting the override the effective time is zero (frame zero).
        selector.Select(EntityId, startTime, out var phase, out var effectiveTime);
        Assert.Equal(AnimationPhase.Moving, phase);
        Assert.Equal(0UL, effectiveTime);
        Assert.Equal(0, sprite.GetPhaseData(phase).GetSpriteForTime(effectiveTime, Orientation.South).Id);

        // First scan resolves and pins the loop duration of 400.
        Scan(startTime);
        activePhases.ApplyComponentUpdates();
        Assert.Equal(400UL, activePhases[EntityId].LoopDurationUs);

        // Still present immediately before the expiry time.
        Scan(startTime + 800 - 1);
        activePhases.ApplyComponentUpdates();
        Assert.True(activePhases.HasComponentForEntity(EntityId));

        // Expired at exactly two loop durations.
        Scan(startTime + 800);
        activePhases.ApplyComponentUpdates();
        Assert.False(activePhases.HasComponentForEntity(EntityId));

        // The same scan renders the base phase.
        selector.Select(EntityId, startTime + 800, out var revertedPhase, out var revertedTime);
        Assert.Equal(AnimationPhase.Default, revertedPhase);
        Assert.Equal(startTime + 800, revertedTime);
    }

    [Fact]
    public void Override_StartsAtFrameZeroRegardlessOfAbsoluteTime()
    {
        var sprite = AddSprite(CreateSprite(
            (AnimationPhase.Default, Orientation.South, 3),
            (AnimationPhase.Moving, Orientation.South, 4)));

        // 1150 is mid-way through the underlying phase's absolute loop of 300.
        const ulong startTime = 1150;
        systemTimer.Time = startTime;
        controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Moving, 1);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();

        selector.Select(EntityId, startTime, out var phase, out var effectiveTime);
        Assert.Equal(AnimationPhase.Moving, phase);
        Assert.Equal(0UL, effectiveTime);

        var selected = sprite.GetPhaseData(phase).GetSpriteForTime(effectiveTime, Orientation.South);
        var absolute = sprite.GetPhaseData(phase).GetSpriteForTime(startTime, Orientation.South);
        Assert.Equal(0, selected.Id);
        Assert.NotEqual(selected.Id, absolute.Id);
    }

    [Fact]
    public void Override_PinsLoopDurationAcrossOrientationChange()
    {
        AddSprite(CreateSprite(
            (AnimationPhase.Default, Orientation.East, 4),
            (AnimationPhase.Default, Orientation.North, 2)));

        const ulong startTime = 5000;
        orientations.AddOrUpdateComponent(EntityId, Orientation.East);
        orientations.ApplyComponentUpdates();

        systemTimer.Time = startTime;
        controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Default, 2);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();

        // First scan pins the loop duration from the initial orientation (4 frames x 100 = 400).
        Scan(startTime);
        activePhases.ApplyComponentUpdates();
        Assert.Equal(400UL, activePhases[EntityId].LoopDurationUs);

        // Turn to an orientation whose frame count differs; the pinned duration must not change.
        orientations.AddOrUpdateComponent(EntityId, Orientation.North);
        orientations.ApplyComponentUpdates();

        Scan(startTime + 800 - 1);
        activePhases.ApplyComponentUpdates();
        Assert.True(activePhases.HasComponentForEntity(EntityId));

        Scan(startTime + 800);
        activePhases.ApplyComponentUpdates();
        Assert.False(activePhases.HasComponentForEntity(EntityId));
    }

    [Fact]
    public void Override_RemovedWhenSpriteComponentMissing()
    {
        systemTimer.Time = 100;
        controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Moving, 1);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();

        Scan(100);
        activePhases.ApplyComponentUpdates();

        Assert.False(activePhases.HasComponentForEntity(EntityId));
    }

    [Fact]
    public void Override_RemovedWhenSpriteIdUnknown()
    {
        animatedSprites.AddOrUpdateComponent(EntityId, 999);
        systemTimer.Time = 100;
        controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Moving, 1);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();

        Scan(100);
        activePhases.ApplyComponentUpdates();

        Assert.False(activePhases.HasComponentForEntity(EntityId));
    }

    [Fact]
    public void Override_UsesDefaultPhaseDurationWhenPhaseUndefined()
    {
        AddSprite(CreateSprite(
            (AnimationPhase.Default, Orientation.South, 3)));

        const ulong startTime = 700;
        systemTimer.Time = startTime;
        controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Moving, 1);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();

        Scan(startTime);
        activePhases.ApplyComponentUpdates();
        Assert.Equal(300UL, activePhases[EntityId].LoopDurationUs);

        Scan(startTime + 300 - 1);
        activePhases.ApplyComponentUpdates();
        Assert.True(activePhases.HasComponentForEntity(EntityId));

        Scan(startTime + 300);
        activePhases.ApplyComponentUpdates();
        Assert.False(activePhases.HasComponentForEntity(EntityId));
    }

    [Fact]
    public void AttackEvent_PlaysOneLoopAttackOverrideOnActor()
    {
        const ulong actorId = 0x7fff00000000abcd;
        const ulong startTime = 2000;

        AddSprite(CreateSprite(
            (AnimationPhase.Default, Orientation.South, 4),
            (AnimationPhase.Attack, Orientation.South, 2)), actorId);

        systemTimer.Time = startTime;
        system.EventCommunicator.SendEventToSystem(new Event(
            EventId.Server_Combat_Attack,
            new AttackEventDetails { ActorId = actorId }));
        Assert.Equal(1, system.ExecuteOnce());
        activePhases.ApplyComponentUpdates();

        // The attack override targets the attacking actor with the Attack phase, one loop.
        Assert.True(activePhases.HasComponentForEntity(actorId));
        Assert.Equal(AnimationPhase.Attack, activePhases[actorId].Phase);
        Assert.Equal(1, activePhases[actorId].Loops);

        // The first scan pins the loop duration from the sprite (2 frames x 100).
        Scan(startTime);
        activePhases.ApplyComponentUpdates();
        Assert.Equal(200UL, activePhases[actorId].LoopDurationUs);

        // Still present immediately before the expiry time.
        Scan(startTime + 200 - 1);
        activePhases.ApplyComponentUpdates();
        Assert.True(activePhases.HasComponentForEntity(actorId));

        // Removed by ExpireCompletedOverrides after one loop.
        Scan(startTime + 200);
        activePhases.ApplyComponentUpdates();
        Assert.False(activePhases.HasComponentForEntity(actorId));
    }

    [Fact]
    public void PlayAnimation_ThrowsForNonPositiveLoopCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Moving, 0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Moving, -1));
    }

    [Fact]
    public void PlayAnimation_DoesNotSendEventForNonPositiveLoopCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Moving, 0));

        // No event may have been sent: the system's communicator drains empty.
        Assert.Equal(0, system.ExecuteOnce());
    }

    [Fact]
    public void PlayAnimation_RestartsOverride()
    {
        AddSprite(CreateSprite(
            (AnimationPhase.Default, Orientation.South, 4),
            (AnimationPhase.Moving, Orientation.South, 4)));

        systemTimer.Time = 1000;
        controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Moving, 3);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();
        Scan(1000);
        activePhases.ApplyComponentUpdates();

        // Restart with a new phase, loop count, and start time.
        systemTimer.Time = 1500;
        controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Default, 1);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();

        Scan(1500 + 400 - 1);
        activePhases.ApplyComponentUpdates();
        Assert.True(activePhases.HasComponentForEntity(EntityId));
        selector.Select(EntityId, 1500 + 400 - 1, out var phase, out _);
        Assert.Equal(AnimationPhase.Default, phase);

        Scan(1500 + 400);
        activePhases.ApplyComponentUpdates();
        Assert.False(activePhases.HasComponentForEntity(EntityId));
    }

    [Fact]
    public void PlayAnimation_RestartBeforePendingRemoveApplies_SurvivesStaleRemoval()
    {
        AddSprite(CreateSprite(
            (AnimationPhase.Default, Orientation.South, 4),
            (AnimationPhase.Moving, Orientation.South, 4)));

        const ulong startTime = 1000;
        const ulong loopDuration = 400;
        systemTimer.Time = startTime;
        controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Moving, 1);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();

        // Expire the override, but do NOT apply the component updates afterwards:
        // the removal is enqueued but still pending in the collection.
        Scan(startTime + loopDuration);

        // Re-trigger the override before the pending removal is applied. The restart
        // must cancel the stale removal rather than let it eat the restarted override.
        const ulong restartTime = startTime + loopDuration;
        systemTimer.Time = restartTime;
        controller.PlayAnimation(eventSender, EntityId, AnimationPhase.Moving, 1);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();
        Assert.True(activePhases.HasComponentForEntity(EntityId));

        // The stale removal must not arrive late: another full update cycle must
        // leave the restarted override in place.
        Scan(restartTime + 1);
        activePhases.ApplyComponentUpdates();
        Assert.True(activePhases.HasComponentForEntity(EntityId));

        // The restarted override must still expire naturally, one loop after its restart.
        Scan(restartTime + loopDuration);
        activePhases.ApplyComponentUpdates();
        Assert.False(activePhases.HasComponentForEntity(EntityId));
    }

    /// <summary>
    ///     Runs the countdown scan at the given system time.
    /// </summary>
    /// <param name="time">System time.</param>
    private void Scan(ulong time)
    {
        systemTimer.Time = time;
        system.ExecuteOnce();
    }

    /// <summary>
    ///     Event sender that forwards sent events into the system's event communicator so
    ///     that the events are drained by the next system.ExecuteOnce() call.
    /// </summary>
    private sealed class ForwardingEventSender(EntityAnimationSystem system) : IEventSender
    {
        public void SendEvent(Event ev)
        {
            system.EventCommunicator.SendEventToSystem(ev);
        }

        public bool TryGetOutgoingEvent(out Event? ev)
        {
            ev = null;
            return false;
        }
    }

    /// <summary>
    ///     Adds an animated sprite to the manager and assigns it to the test entity.
    /// </summary>
    /// <param name="sprite">Animated sprite.</param>
    /// <param name="entityId">Entity to which the sprite is assigned.</param>
    /// <returns>Animated sprite.</returns>
    private AnimatedSprite AddSprite(AnimatedSprite sprite, ulong entityId = EntityId)
    {
        var spriteId = animatedSpriteManager.AnimatedSprites.Count;
        animatedSpriteManager.AnimatedSprites.Add(sprite);
        animatedSprites.AddOrUpdateComponent(entityId, spriteId);
        return sprite;
    }

    /// <summary>
    ///     Creates an animated sprite whose default phase is one frame long.
    /// </summary>
    /// <param name="faces">Phase, orientation, and frame count triples.</param>
    /// <returns>Animated sprite.</returns>
    private static AnimatedSprite CreateSprite(
        params (AnimationPhase Phase, Orientation Orientation, int FrameCount)[] faces)
    {
        var sprite = new AnimatedSprite();
        var phaseDatas = new Dictionary<AnimationPhase, AnimatedSprite.AnimationPhaseData>();
        foreach (var (phase, orientation, frameCount) in faces)
        {
            if (!phaseDatas.TryGetValue(phase, out var phaseData))
            {
                phaseData = new AnimatedSprite.AnimationPhaseData { FrameTime = 100 };
                phaseDatas[phase] = phaseData;
                sprite.Phases[phase] = phaseData;
            }

            var frames = new List<Sprite>();
            for (var i = 0; i < frameCount; ++i) frames.Add(new Sprite { Id = i });
            phaseData.Frames[orientation] = frames;
        }

        return sprite;
    }
}
