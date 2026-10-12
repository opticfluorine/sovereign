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

using System.Collections.Generic;
using System.Reflection;
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
///     Unit tests for deduplication of the server attack echo against a locally predicted attack
///     animation override.
/// </summary>
public class TestAttackEchoDedupe
{
    private const ulong ActorA = 0x7fff0000000000a1;
    private const ulong ActorB = 0x7fff0000000000b2;

    private readonly ActiveAnimationPhaseComponentCollection activePhases;
    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly EntityAnimationController controller = new();
    private readonly ForwardingEventSender eventSender;
    private readonly EntityAnimationSystem system;
    private readonly FakeSystemTimer systemTimer = new();

    public TestAttackEchoDedupe()
    {
        var entityTable = new EntityTable();
        var componentManager = new ComponentManager(new EntityNotifier());
        var basePhases = new AnimationPhaseComponentCollection(entityTable, componentManager);
        activePhases = new ActiveAnimationPhaseComponentCollection(entityTable, componentManager);
        animatedSprites = new AnimatedSpriteComponentCollection(entityTable, componentManager);
        var orientations = new OrientationComponentCollection(entityTable, componentManager);
        animatedSpriteManager =
            new AnimatedSpriteManager(null!, null!, null!, NullLogger<AnimatedSpriteManager>.Instance);

        system = new EntityAnimationSystem(new EventCommunicator(), new FakeEventLoop(),
            new AnimationPhaseStateMachine(basePhases),
            new ActiveAnimationManager(activePhases, animatedSprites, orientations, animatedSpriteManager,
                systemTimer),
            systemTimer, NullLogger<EntityAnimationSystem>.Instance);
        eventSender = new ForwardingEventSender(system);
    }

    [Fact]
    public void ServerEcho_WithinWindow_DoesNotRestartPrediction()
    {
        AddSprite(ActorA, CreateAttackSprite(10));

        const ulong startTime = 1_000_000;
        systemTimer.Time = startTime;
        controller.PlayAnimation(eventSender, ActorA, AnimationPhase.Attack, 1);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();
        Assert.Equal(startTime, activePhases[ActorA].StartTimeUs);

        // An echo inside the dedupe window must leave the prediction playing.
        systemTimer.Time = startTime + 100_000;
        SendAttack(ActorA);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();
        Assert.Equal(startTime, activePhases[ActorA].StartTimeUs);

        // An echo outside the window restarts the override at the echo time.
        systemTimer.Time = startTime + 200_000;
        SendAttack(ActorA);
        system.ExecuteOnce();
        activePhases.ApplyComponentUpdates();
        Assert.Equal(startTime + 200_000, activePhases[ActorA].StartTimeUs);
    }

    [Fact]
    public void PredictedAttackMap_PrunesStaleEntries()
    {
        const ulong startTime = 1_000_000;
        ulong[] earlierActors = { 0xa1, 0xa2, 0xa3, 0xa4, 0xa5 };

        systemTimer.Time = startTime;
        foreach (var actor in earlierActors)
            controller.PlayAnimation(eventSender, actor, AnimationPhase.Attack, 1);
        system.ExecuteOnce();
        Assert.Equal(earlierActors.Length, PredictedAttacks().Count);

        // A prediction made after the window has passed prunes the stale entries on the next
        // attack handling, so the map is bounded by the actors predicted within the window.
        const ulong laterTime = startTime + 200_000;
        systemTimer.Time = laterTime;
        controller.PlayAnimation(eventSender, ActorB, AnimationPhase.Attack, 1);
        system.ExecuteOnce();

        SendAttack(ActorB);
        system.ExecuteOnce();

        Assert.Single(PredictedAttacks());
    }

    /// <summary>
    ///     Sends a server attack event for the given actor to the animation system.
    /// </summary>
    /// <param name="actorId">Attacking actor.</param>
    private void SendAttack(ulong actorId)
    {
        system.EventCommunicator.SendEventToSystem(new Event(EventId.Server_Combat_Attack,
            new AttackEventDetails { ActorId = actorId }));
    }

    /// <summary>
    ///     Reads the animation system's private predicted-attack timestamp map.
    /// </summary>
    /// <returns>Predicted-attack timestamps keyed by entity ID.</returns>
    private Dictionary<ulong, ulong> PredictedAttacks()
    {
        var field = typeof(EntityAnimationSystem).GetField("lastPredictedAttackUs",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        return (Dictionary<ulong, ulong>)field!.GetValue(system)!;
    }

    /// <summary>
    ///     Registers an animated sprite and assigns it to the given entity.
    /// </summary>
    /// <param name="entityId">Entity to assign the sprite to.</param>
    /// <param name="sprite">Animated sprite.</param>
    private void AddSprite(ulong entityId, AnimatedSprite sprite)
    {
        var spriteId = animatedSpriteManager.AnimatedSprites.Count;
        animatedSpriteManager.AnimatedSprites.Add(sprite);
        animatedSprites.AddOrUpdateComponent(entityId, spriteId);
    }

    /// <summary>
    ///     Creates an animated sprite with a long one-second-per-frame attack phase.
    /// </summary>
    /// <param name="frameCount">Number of attack frames.</param>
    /// <returns>Animated sprite.</returns>
    private static AnimatedSprite CreateAttackSprite(int frameCount)
    {
        var sprite = new AnimatedSprite();
        var phaseData = new AnimatedSprite.AnimationPhaseData { FrameTime = 1_000_000 };
        sprite.Phases[AnimationPhase.Attack] = phaseData;

        var frames = new List<Sprite>();
        for (var i = 0; i < frameCount; ++i) frames.Add(new Sprite { Id = i });
        phaseData.Frames[Orientation.South] = frames;

        return sprite;
    }

    /// <summary>
    ///     Event sender that forwards sent events into the system's event communicator so they are
    ///     drained by the next ExecuteOnce call.
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
}
