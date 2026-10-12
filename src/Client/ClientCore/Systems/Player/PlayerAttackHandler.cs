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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.ClientCore.Systems.EntityAnimation;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Combat;
using Sovereign.EngineCore.Timing;

namespace Sovereign.ClientCore.Systems.Player;

/// <summary>
///     Handles client-side attack requests from PlayerSystem, applying a local cooldown and
///     predicting the attack animation.
/// </summary>
public sealed class PlayerAttackHandler(
    ClientStateServices stateServices,
    CombatController combatController,
    EntityAnimationController animationController,
    AttackDetailsComponentCollection attackDetails,
    IOptions<CombatOptions> options,
    ISystemTimer systemTimer,
    IEventSender eventSender,
    ILogger<PlayerAttackHandler> logger)
{
    /// <summary>
    ///     System time in microseconds of the last accepted attack for each locally controlled
    ///     player, used to reject spammed requests before they reach the server.
    /// </summary>
    private readonly Dictionary<ulong, ulong> lastAttackSentUs = new();

    /// <summary>
    ///     Requests an attack by the selected player if the local cooldown permits, predicting the
    ///     attack animation immediately.
    /// </summary>
    public void Attack()
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerId))
        {
            logger.LogError("Tried attack without player selected.");
            return;
        }

        var delayUs = attackDetails.TryGetValue(playerId, out var details)
            ? details.AttackDelayUs
            : options.Value.DefaultAttackDelayUs;

        var nowUs = systemTimer.GetTime();
        if (lastAttackSentUs.TryGetValue(playerId, out var lastSentUs) && nowUs - lastSentUs < delayUs)
        {
            logger.LogDebug("Attack request for player {PlayerId:X} rejected by local cooldown.", playerId);
            return;
        }

        lastAttackSentUs[playerId] = nowUs;
        combatController.Attack(eventSender, playerId);
        animationController.PlayAnimation(eventSender, playerId, AnimationPhase.Attack);
    }
}
