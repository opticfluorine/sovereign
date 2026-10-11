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
using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Systems.Combat;
using Sovereign.EngineCore.Systems.Movement;
using Sovereign.EngineCore.Timing;

namespace Sovereign.EngineCore.Systems.Vitals;

/// <summary>
///     Handles the death of entities for the vitals system.
/// </summary>
public class EntityDeathHandler
{
    /// <summary>
    ///     Guards access to the last-attacker tracking dictionary.
    /// </summary>
    private readonly object lockObject = new();

    /// <summary>
    ///     Tracks the most recent attacker of each damaged entity with the time of the last damage,
    ///     keyed by victim entity ID.
    /// </summary>
    private readonly Dictionary<ulong, (ulong AttackerId, ulong TimestampUs)> lastAttackers = new();

    private readonly EntityTypeComponentCollection entityTypes;
    private readonly HealthComponentCollection healths;
    private readonly EntityManager entityManager;
    private readonly EntityTable entityTable;
    private readonly VitalsController vitalsController;
    private readonly MovementController movementController;
    private readonly CombatInternalController internalController;
    private readonly EntityKilledByPlayerHandler killedByPlayerHandler;
    private readonly LoggingUtil loggingUtil;
    private readonly IEventSender eventSender;
    private readonly ISystemTimer timer;
    private readonly CombatOptions options;
    private readonly ILogger<EntityDeathHandler> logger;

    public EntityDeathHandler(EntityTypeComponentCollection entityTypes,
        HealthComponentCollection healths,
        EntityManager entityManager,
        EntityTable entityTable,
        VitalsController vitalsController,
        MovementController movementController,
        CombatInternalController internalController,
        EntityKilledByPlayerHandler killedByPlayerHandler,
        LoggingUtil loggingUtil,
        IEventSender eventSender,
        ISystemTimer timer,
        IOptions<CombatOptions> options,
        ILogger<EntityDeathHandler> logger)
    {
        this.entityTypes = entityTypes;
        this.healths = healths;
        this.entityManager = entityManager;
        this.entityTable = entityTable;
        this.vitalsController = vitalsController;
        this.movementController = movementController;
        this.internalController = internalController;
        this.killedByPlayerHandler = killedByPlayerHandler;
        this.loggingUtil = loggingUtil;
        this.eventSender = eventSender;
        this.timer = timer;
        this.options = options.Value;
        this.logger = logger;

        entityTable.OnEntityRemoved += HandleEntityRemoved;
    }

    /// <summary>
    ///     Records the most recent attacker of a damaged entity.
    /// </summary>
    /// <param name="details">DamagedBy event details.</param>
    public void HandleDamagedBy(DamagedByEventDetails details)
    {
        if (!healths.HasComponentForEntity(details.VictimEntityId)) return;
        if (!entityTypes.TryGetValue(details.AttackerEntityId, out _)) return;

        lock (lockObject)
        {
            lastAttackers[details.VictimEntityId] = (details.AttackerEntityId, timer.GetTime());
        }
    }

    /// <summary>
    ///     Handles the death of the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID of the dead entity.</param>
    public void HandleDeath(ulong entityId)
    {
        var isPlayer = entityTypes.TryGetValue(entityId, out var entityType) && entityType == EntityType.Player;
        if (isPlayer)
            HandlePlayerDeath(entityId);
        else
            entityManager.RemoveEntity(entityId);
    }

    /// <summary>
    ///     Handles the death of a player entity by respawning the player at the spawn point.
    /// </summary>
    /// <param name="entityId">Entity ID of the dead player.</param>
    private void HandlePlayerDeath(ulong entityId)
    {
        ulong killerId = 0;
        if (TryTakeUnexpiredAttacker(entityId, out var attackerId)
            && entityTypes.HasComponentForEntity(attackerId))
        {
            killerId = attackerId;
        }

        if (killerId > 0)
        {
            logger.LogInformation("Player {Player} was killed by {Killer}.",
                loggingUtil.FormatEntity(entityId), loggingUtil.FormatEntity(killerId));
            internalController.PlayerKilled(eventSender, entityId, killerId);
        }
        else
        {
            logger.LogInformation("Player {Player} died; respawning at spawn point.",
                loggingUtil.FormatEntity(entityId));
        }

        // TODO Select per-player spawn point.
        var spawnPoint = new Vector3(0, 0, 1);

        if (healths.TryGetValue(entityId, out var health))
            vitalsController.ChangeVitals(eventSender, entityId, VitalType.Health,
                health.MaxValue - health.Value);

        movementController.Teleport(eventSender, entityId, spawnPoint);
    }

    /// <summary>
    ///     Handles the removal of an entity by crediting a player kill if applicable.
    /// </summary>
    /// <param name="entityId">Entity ID of the removed entity.</param>
    /// <param name="isUnload">true if this is an unload rather than a removal.</param>
    private void HandleEntityRemoved(ulong entityId, bool isUnload)
    {
        var attackerIsPlayer = TryTakeUnexpiredAttacker(entityId, out var attackerId)
            && entityTypes.TryGetValue(attackerId, out var attackerType)
            && attackerType == EntityType.Player;

        if (!isUnload && attackerIsPlayer) killedByPlayerHandler.HandleEntityKilledByPlayer(entityId, attackerId);
    }

    /// <summary>
    ///     Tries to take the last-attacker entry for the given victim if it exists and has not expired.
    /// </summary>
    /// <param name="victimEntityId">Entity ID of the victim.</param>
    /// <param name="attackerId">Entity ID of the attacker, if an unexpired entry was taken.</param>
    /// <returns>true if an unexpired entry was taken, false if there was no entry or it had expired.</returns>
    private bool TryTakeUnexpiredAttacker(ulong victimEntityId, out ulong attackerId)
    {
        lock (lockObject)
        {
            if (!lastAttackers.Remove(victimEntityId, out var entry)
                || timer.GetTime() - entry.TimestampUs > options.KillCreditTimeoutUs)
            {
                attackerId = 0;
                return false;
            }

            attackerId = entry.AttackerId;
            return true;
        }
    }
}
