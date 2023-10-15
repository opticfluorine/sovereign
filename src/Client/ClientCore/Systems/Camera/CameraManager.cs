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

using System.Numerics;
using Sovereign.EngineCore.Systems.Movement.Components;

namespace Sovereign.ClientCore.Systems.Camera;

/// <summary>
///     Responsible for managing the camera.
/// </summary>
public sealed class CameraManager
{
    private readonly PositionComponentCollection positions;
    private readonly VelocityComponentCollection velocities;

    public CameraManager(PositionComponentCollection positions,
        VelocityComponentCollection velocities)
    {
        this.positions = positions;
        this.velocities = velocities;
    }

    /// <summary>
    ///     Camera position.
    /// </summary>
    public Vector3 Position { get; private set; }

    /// <summary>
    ///     Camera velocity.
    /// </summary>
    public Vector3 Velocity { get; private set; }

    /// <summary>
    ///     Entity ID of the entity tracked by the camera.
    /// </summary>
    public ulong TrackingEntityId { get; private set; }

    /// <summary>
    ///     Whether the camera is tracking an entity.
    /// </summary>
    public bool IsTracking { get; private set; }

    public void Initialize()
    {
    }

    /// <summary>
    ///     Updates the camera if needed.
    /// </summary>
    internal void UpdateCamera()
    {
        if (!IsTracking)
        {
            /* Zero the velocity so the camera doesn't drift from interpolation. */
            Velocity = Vector3.Zero;
        }
        else
        {
            /* Match position and velocity with the target entity. */
            var targetPos = positions.GetComponentForEntity(TrackingEntityId);
            var targetVel = velocities.GetComponentForEntity(TrackingEntityId);
            if (targetPos.HasValue)
                Position = targetPos.Value;
            if (targetVel.HasValue)
                Velocity = targetVel.Value;
        }
    }

    /// <summary>
    ///     Sets the camera state.
    /// </summary>
    /// <param name="attached">Whether the camera is attached to an entity.</param>
    /// <param name="entityId">ID of the entity to which the camera is attached.</param>
    internal void SetCameraState(bool attached, ulong entityId)
    {
        IsTracking = attached;
        TrackingEntityId = entityId;
    }
}