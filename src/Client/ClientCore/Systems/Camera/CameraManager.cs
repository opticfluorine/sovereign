/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Movement.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Systems.Camera
{

    /// <summary>
    /// Responsible for managing the camera.
    /// </summary>
    public sealed class CameraManager
    {
        private readonly PositionComponentCollection positions;
        private readonly VelocityComponentCollection velocities;

        /// <summary>
        /// Camera position.
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Camera velocity.
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// Entity ID of the entity tracked by the camera.
        /// </summary>
        public ulong TrackingEntityId { get; private set; }

        /// <summary>
        /// Whether the camera is tracking an entity.
        /// </summary>
        public bool IsTracking { get; private set; }

        public CameraManager(PositionComponentCollection positions,
            VelocityComponentCollection velocities)
        {
            this.positions = positions;
            this.velocities = velocities;
        }

        public void Initialize()
        {
        }

        /// <summary>
        /// Updates the camera if needed.
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
        /// Sets the camera state.
        /// </summary>
        /// <param name="attached">Whether the camera is attached to an entity.</param>
        /// <param name="entityId">ID of the entity to which the camera is attached.</param>
        internal void SetCameraState(bool attached, ulong entityId)
        {
            IsTracking = attached;
            TrackingEntityId = entityId;
        }

    }

}
