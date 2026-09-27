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
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Systems.Combat;
using Sovereign.EngineCore.Systems.Vitals;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineCore.World;

namespace Sovereign.ServerCore.Systems.Combat;

/// <summary>
///     Responsible for validating and applying attacks.
/// </summary>
internal sealed class AttackHandler
{
    /// <summary>
    ///     Time of the last processed attack for each actor, in microseconds.
    /// </summary>
    private readonly Dictionary<ulong, ulong> lastAttackTimes = new();

    private readonly AttackDetailsComponentCollection attackDetails;
    private readonly CombatDamageCalculator damageCalculator;
    private readonly CombatInternalController internalController;
    private readonly EntityTypeComponentCollection entityTypes;
    private readonly IEventSender eventSender;
    private readonly KinematicsComponentCollection kinematics;
    private readonly ILogger<AttackHandler> logger;
    private readonly LoggingUtil loggingUtil;
    private readonly NonBlockWorldSegmentIndexer nonBlockIndexer;
    private readonly OrientationComponentCollection orientations;
    private readonly ParentComponentCollection parents;
    private readonly ISystemTimer timer;
    private readonly VitalsController vitalsController;
    private readonly WorldSegmentResolver segmentResolver;
    private readonly CombatOptions options;

    public AttackHandler(AttackDetailsComponentCollection attackDetails,
        KinematicsComponentCollection kinematics,
        ParentComponentCollection parents,
        EntityTypeComponentCollection entityTypes,
        OrientationComponentCollection orientations,
        NonBlockWorldSegmentIndexer nonBlockIndexer,
        WorldSegmentResolver segmentResolver,
        ISystemTimer timer,
        VitalsController vitalsController,
        CombatDamageCalculator damageCalculator,
        CombatInternalController internalController,
        IEventSender eventSender,
        LoggingUtil loggingUtil,
        IOptions<CombatOptions> options,
        ILogger<AttackHandler> logger)
    {
        this.attackDetails = attackDetails;
        this.kinematics = kinematics;
        this.parents = parents;
        this.entityTypes = entityTypes;
        this.orientations = orientations;
        this.nonBlockIndexer = nonBlockIndexer;
        this.segmentResolver = segmentResolver;
        this.timer = timer;
        this.vitalsController = vitalsController;
        this.damageCalculator = damageCalculator;
        this.internalController = internalController;
        this.eventSender = eventSender;
        this.loggingUtil = loggingUtil;
        this.options = options.Value;
        this.logger = logger;
    }

    /// <summary>
    ///     Handles an attack performed by the given actor.
    /// </summary>
    /// <param name="actorId">Entity ID of the attacking actor.</param>
    public void HandleAttack(ulong actorId)
    {
        if (!IsRateLimitOk(actorId))
        {
            logger.LogDebug("Attack by {Actor} was rate limited.", loggingUtil.FormatEntity(actorId));
            return;
        }

        if (!kinematics.TryGetValue(actorId, out var actorKinematics))
        {
            logger.LogError("Attack by unpositioned actor {Actor} was rejected.",
                loggingUtil.FormatEntity(actorId));
            return;
        }

        var attackDetailsValue = GetEffectiveAttackDetails(actorId);
        var facing = OrientationUtil.GetUnitVector(orientations.TryGetValue(actorId, out var orientation)
            ? orientation
            : Orientation.South);

        foreach (var targetId in FindTargets(actorId, actorKinematics.Position, attackDetailsValue.AttackRange,
                     facing))
        {
            var damage = damageCalculator.CalculateDamage(actorId, targetId);
            vitalsController.ChangeVitals(eventSender, targetId, VitalType.Health, -damage);
            internalController.DamagedBy(eventSender, targetId, actorId);
        }
    }

    /// <summary>
    ///     Gets the effective attack details for the given actor, falling back to the
    ///     configured defaults if the actor has no AttackDetails component.
    /// </summary>
    /// <param name="actorId">Entity ID of the actor.</param>
    /// <returns>Effective attack details.</returns>
    private AttackDetails GetEffectiveAttackDetails(ulong actorId)
    {
        if (attackDetails.TryGetValue(actorId, out var details)) return details;

        return new AttackDetails
        {
            AttackRange = options.DefaultAttackRange,
            AttackDelayUs = options.DefaultAttackDelayUs
        };
    }

    /// <summary>
    ///     Finds all attackable entities within range of the actor in the direction the actor faces.
    /// </summary>
    /// <param name="actorId">Entity ID of the actor.</param>
    /// <param name="actorPosition">Position of the actor.</param>
    /// <param name="range">Attack range in world units.</param>
    /// <param name="facing">Unit vector facing of the actor.</param>
    /// <returns>Entity IDs of all valid targets.</returns>
    private IEnumerable<ulong> FindTargets(ulong actorId, Vector3 actorPosition, float range, Vector3 facing)
    {
        var targets = new List<ulong>();
        var rangeSquared = range * range;

        foreach (var candidateId in EnumerateCandidates(actorPosition, range))
        {
            if (candidateId == actorId) continue;
            if (!entityTypes.TryGetValue(candidateId, out var entityType)) continue;
            if (entityType != EntityType.Player && entityType != EntityType.Npc) continue;
            if (parents.HasComponentForEntity(candidateId)) continue;
            if (!kinematics.TryGetValue(candidateId, out var candidateKinematics)) continue;

            var delta = candidateKinematics.Position - actorPosition;
            if (Vector2.Dot(new Vector2(delta.X, delta.Y), new Vector2(facing.X, facing.Y)) <= 0f) continue;

            var distanceSquared = delta.X * delta.X + delta.Y * delta.Y;
            if (distanceSquared > rangeSquared) continue;

            targets.Add(candidateId);
        }

        return targets;
    }

    /// <summary>
    ///     Enumerates all non-block entities in the world segments overlapping the attack range.
    /// </summary>
    /// <param name="actorPosition">Position of the actor.</param>
    /// <param name="range">Attack range in world units.</param>
    /// <returns>Entity IDs of candidates.</returns>
    private IEnumerable<ulong> EnumerateCandidates(Vector3 actorPosition, float range)
    {
        var minSegment = segmentResolver.GetWorldSegmentForPosition(
            new Vector3(actorPosition.X - range, actorPosition.Y - range, actorPosition.Z));
        var maxSegment = segmentResolver.GetWorldSegmentForPosition(
            new Vector3(actorPosition.X + range, actorPosition.Y + range, actorPosition.Z));

        for (var x = minSegment.X; x <= maxSegment.X; ++x)
        for (var y = minSegment.Y; y <= maxSegment.Y; ++y)
        for (var z = minSegment.Z; z <= maxSegment.Z; ++z)
        foreach (var entityId in nonBlockIndexer.GetEntitiesInWorldSegment(new GridPosition { X = x, Y = y, Z = z }))
            yield return entityId;
    }

    /// <summary>
    ///     Checks whether the attack rate limit is temporarily exceeded for the actor.
    /// </summary>
    /// <param name="actorId">Entity ID of the actor.</param>
    /// <returns>true if the rate limit is not exceeded, false otherwise.</returns>
    private bool IsRateLimitOk(ulong actorId)
    {
        var delayUs = GetEffectiveAttackDetails(actorId).AttackDelayUs;
        var now = timer.GetTime();
        if (lastAttackTimes.TryGetValue(actorId, out var lastTime))
            if (now - lastTime < delayUs)
                return false;

        lastAttackTimes[actorId] = now;
        return true;
    }
}
