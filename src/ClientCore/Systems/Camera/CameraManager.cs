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

    public sealed class CameraManager
    {
        private readonly IEntityFactory entityFactory;
        private readonly PositionComponentCollection positions;
        private readonly VelocityComponentCollection velocities;

        /// <summary>
        /// Entity ID of the camera.
        /// </summary>
        public ulong CameraEntityId { get; private set; }

        /// <summary>
        /// Entity ID of the entity tracked by the camera.
        /// </summary>
        public ulong TrackingEntityId { get; private set; }

        /// <summary>
        /// Whether the camera is tracking an entity.
        /// </summary>
        public bool IsTracking { get; private set; }

        public CameraManager(IEntityFactory entityFactory, 
            PositionComponentCollection positions,
            VelocityComponentCollection velocities)
        {
            this.entityFactory = entityFactory;
            this.positions = positions;
            this.velocities = velocities;
        }

        public void Initialize()
        {
            CreateCameraEntity();
        }

        /// <summary>
        /// Updates the camera if needed.
        /// </summary>
        internal void UpdateCamera()
        {
            if (!IsTracking)
            {
                /* Zero the velocity so the camera doesn't drift from interpolation. */
                velocities.ModifyComponent(CameraEntityId, ComponentOperation.Set, Vector3.Zero);
            }
            else
            {
                /* Match position and velocity with the target entity. */
                var targetPos = positions.GetComponentForEntity(CameraEntityId);
                var targetVel = velocities.GetComponentForEntity(CameraEntityId);
                if (targetPos.HasValue)
                    positions.ModifyComponent(CameraEntityId, ComponentOperation.Set, targetPos.Value);
                if (targetVel.HasValue)
                    velocities.ModifyComponent(CameraEntityId, ComponentOperation.Set, targetVel.Value);
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

        /// <summary>
        /// Creates the camera entity.
        /// </summary>
        private void CreateCameraEntity()
        {
            CameraEntityId = entityFactory.GetBuilder()
                .Positionable()
                .Build();
        }

    }

}
