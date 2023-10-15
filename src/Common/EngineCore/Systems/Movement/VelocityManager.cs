/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Castle.Core.Logging;
using Sovereign.EngineCore.Configuration;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Manages the velocities of all moving entities.
/// </summary>
public class VelocityManager
{
    /// <summary>
    ///     Active entity velocities that are generating movement events.
    /// </summary>
    private readonly Dictionary<ulong, VelocityState> activeVelocities = new();

    /// <summary>
    ///     Engine configuration.
    /// </summary>
    private readonly IEngineConfiguration engineConfiguration;

    /// <summary>
    ///     Movement controller.
    /// </summary>
    private readonly InternalMovementController internalMovementController;

    /// <summary>
    ///     Used to assign unique movement phases.
    /// </summary>
    private uint PhaseCounter;

    public VelocityManager(IEngineConfiguration engineConfiguration,
        InternalMovementController internalMovementController)
    {
        this.engineConfiguration = engineConfiguration;
        this.internalMovementController = internalMovementController;
    }

    public ILogger Log { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Sets the velocity of the given entity.
    /// </summary>
    /// <param name="entity">Entity ID.</param>
    /// <param name="dx">Relative velocity along x as a multiple of the entity base speed.</param>
    /// <param name="dy">Relative velocity along y as a multiple of the entity base speed.</param>
    /// <param name="currentTime">Current system time, in us.</param>
    public void SetVelocity(ulong entity, float dx, float dy, ulong currentTime)
    {
        /* Declare a new velocity state. */
        var state = new VelocityState
        {
            MovementPhase = PhaseCounter++,
            StepX = dx,
            StepY = dy
        };
        activeVelocities[entity] = state;

        /* Schedule the first movement. */
        ScheduleNextMovement(entity, currentTime, state);
    }

    /// <summary>
    ///     Stops movement of the given entity.
    /// </summary>
    /// <param name="entity">Entity ID.</param>
    public void StopMovement(ulong entity)
    {
    }

    /// <summary>
    ///     Immediately moves the given entity.
    /// </summary>
    /// <param name="entity">Entity ID.</param>
    /// <param name="phase">Unique movement phase.</param>
    /// <param name="currentTime">Current system time, in us.</param>
    public void MoveOnce(ulong entity, uint phase, ulong currentTime)
    {
        /* Ensure that the movement has not been cancelled. */
        if (!activeVelocities.ContainsKey(entity)) return;
        var velocityState = activeVelocities[entity];
        if (velocityState.MovementPhase != phase) return;

        /* Move the entity. */
        ExecuteMovement(entity, velocityState);

        /* Schedule the next movement. */
        ScheduleNextMovement(entity, currentTime, velocityState);
    }

    /// <summary>
    ///     Schedules the next movement for the given entity.
    /// </summary>
    /// <param name="entity">Entity ID.</param>
    /// <param name="currentTime">Current system time, in us.</param>
    /// <param name="velocityState">Velocity state.</param>
    private void ScheduleNextMovement(ulong entity, ulong currentTime,
        VelocityState velocityState)
    {
        var nextTime = currentTime + engineConfiguration.EventTickInterval;
        internalMovementController.ScheduleMovement(entity, velocityState.MovementPhase,
            nextTime);
    }

    /// <summary>
    ///     Executes a scheduled movement for the given entity.
    /// </summary>
    /// <param name="entity">Entity ID.</param>
    /// <param name="velocityState">Velocity state.</param>
    private void ExecuteMovement(ulong entity, VelocityState velocityState)
    {
        // TODO: Implement
    }

    /// <summary>
    ///     Internal state of an entity's velocity.
    /// </summary>
    private struct VelocityState
    {
        /// <summary>
        ///     Unique identifier for each phase of motion.
        ///     Used to filter out cancelled events.
        /// </summary>
        public uint MovementPhase;

        /// <summary>
        ///     Distance to move along x during each tick.
        /// </summary>
        public float StepX;

        /// <summary>
        ///     Distance to move along y during each tick.
        /// </summary>
        public float StepY;
    }
}