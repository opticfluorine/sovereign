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
using System.Linq;
using Sovereign.ClientCore.Components;
using Sovereign.ClientCore.Events.Details;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.ClientCore.Systems.Player;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using TestClientCore.TestSupport;
using Xunit;

namespace TestClientCore.Systems.Player;

/// <summary>
///     Unit tests for the client-side player attack request, cooldown, and prediction.
/// </summary>
public class TestPlayerAttack : IDisposable
{
    private const ulong PlayerId = 0x7fff000000000042;
    private const uint DefaultDelayUs = 250_000;

    private readonly ClientTestHarness harness = new(o => o.DefaultAttackDelayUs = DefaultDelayUs);
    private readonly PlayerAttackHandler handler;
    private readonly PlayerSystem playerSystem;
    private readonly PlayerStateManager playerStateManager;
    private readonly AttackDetailsComponentCollection attackDetails;

    public TestPlayerAttack()
    {
        handler = harness.Get<PlayerAttackHandler>();
        playerSystem = harness.Get<PlayerSystem>();
        playerStateManager = harness.Get<PlayerStateManager>();
        attackDetails = harness.Get<AttackDetailsComponentCollection>();

        playerStateManager.PlayerSelected(PlayerId);
    }

    public void Dispose()
    {
        harness.Dispose();
    }

    [Fact]
    public void PlayerSystem_ClientAttack_SendsServerAttackForSelectedPlayer()
    {
        playerSystem.EventCommunicator.SendEventToSystem(new Event(EventId.Client_Player_Attack));
        Assert.Equal(1, playerSystem.ExecuteOnce());

        var attack = Assert.Single(harness.EventSender.SentEvents
            .Where(ev => ev.EventId == EventId.Server_Combat_Attack));
        var details = Assert.IsType<AttackEventDetails>(attack.EventDetails);
        Assert.Equal(PlayerId, details.ActorId);
    }

    [Fact]
    public void Cooldown_UsesDefaultDelayWhenNoAttackDetails()
    {
        harness.SystemTimer.Time = 0;
        handler.Attack();
        Assert.Equal(1, AttackCount());

        harness.SystemTimer.Time = DefaultDelayUs - 1;
        handler.Attack();
        Assert.Equal(1, AttackCount());

        harness.SystemTimer.Time = DefaultDelayUs;
        handler.Attack();
        Assert.Equal(2, AttackCount());
    }

    [Fact]
    public void Cooldown_UsesAttackDetailsDelayWhenPresent()
    {
        const uint detailsDelayUs = 100_000;
        attackDetails.AddOrUpdateComponent(PlayerId, new AttackDetails { AttackDelayUs = detailsDelayUs });
        attackDetails.ApplyComponentUpdates();

        harness.SystemTimer.Time = 0;
        handler.Attack();
        Assert.Equal(1, AttackCount());

        // Below the component delay nothing is sent; at the component delay (which is shorter than
        // the default) the attack goes through, proving the component window is used.
        harness.SystemTimer.Time = detailsDelayUs - 1;
        handler.Attack();
        Assert.Equal(1, AttackCount());

        harness.SystemTimer.Time = detailsDelayUs;
        handler.Attack();
        Assert.Equal(2, AttackCount());
    }

    [Fact]
    public void NoSelectedPlayer_SendsNothing()
    {
        playerStateManager.PlayerLogout();

        harness.SystemTimer.Time = 0;
        handler.Attack();

        Assert.Equal(0, AttackCount());
        Assert.Empty(harness.EventSender.SentEvents);
    }

    [Fact]
    public void AcceptedAttack_RequestsAttackOverride()
    {
        harness.SystemTimer.Time = 0;
        handler.Attack();

        var overrideEvent = Assert.Single(harness.EventSender.SentEvents
            .Where(ev => ev.EventId == EventId.Client_EntityAnimation_PlayOverride));
        var details = Assert.IsType<ActiveAnimationPhaseEventDetails>(overrideEvent.EventDetails);
        Assert.Equal(PlayerId, details.EntityId);
        Assert.Equal(AnimationPhase.Attack, details.Phase);
        Assert.Equal(1, details.Loops);
    }

    /// <summary>
    ///     Counts the Server_Combat_Attack events sent through the harness event sender.
    /// </summary>
    /// <returns>Number of server attack events sent.</returns>
    private int AttackCount()
    {
        return harness.EventSender.SentEvents.Count(ev => ev.EventId == EventId.Server_Combat_Attack);
    }
}
