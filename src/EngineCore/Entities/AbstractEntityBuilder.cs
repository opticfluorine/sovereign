/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.EngineUtil.Threading;
using System;
using System.Numerics;

namespace Sovereign.EngineCore.Entities
{

    /// <summary>
    /// Base builder class for new entities.
    /// </summary>
    public abstract class AbstractEntityBuilder : IEntityBuilder, IDisposable
    {

        protected readonly ulong entityId;
        protected readonly ComponentManager componentManager;
        protected readonly PositionComponentCollection positions;
        protected readonly VelocityComponentCollection velocities;
        protected readonly MaterialComponentCollection materials;
        protected readonly MaterialModifierComponentCollection materialModifiers;
        protected readonly AboveBlockComponentCollection aboveBlocks;

        private IncrementalGuard.IncrementalGuardWeakLock weakLock;

        public AbstractEntityBuilder(ulong entityId, 
            ComponentManager componentManager, PositionComponentCollection positions,
            VelocityComponentCollection velocities, MaterialComponentCollection materials,
            MaterialModifierComponentCollection materialModifiers,
            AboveBlockComponentCollection aboveBlocks)
        {
            this.entityId = entityId;
            this.componentManager = componentManager;
            this.positions = positions;
            this.velocities = velocities;
            this.materials = materials;
            this.materialModifiers = materialModifiers;
            this.aboveBlocks = aboveBlocks;

            weakLock = componentManager.ComponentGuard.AcquireWeakLock();
        }

        public void Dispose()
        {
            weakLock?.Dispose();
        }

        public ulong Build()
        {
            weakLock.Dispose();
            weakLock = null;

            return entityId;
        }

        public IEntityBuilder Positionable(Vector3 position, Vector3 velocity)
        {
            positions.AddComponent(entityId, position);
            velocities.AddComponent(entityId, velocity);
            return this;
        }

        public IEntityBuilder Positionable(Vector3 position)
        {
            return Positionable(position, Vector3.Zero);
        }

        public IEntityBuilder Positionable()
        {
            return Positionable(Vector3.Zero, Vector3.Zero);
        }

        public IEntityBuilder Material(int materialId, int materialModifier)
        {
            materials.AddComponent(entityId, materialId);
            materialModifiers.AddComponent(entityId, materialModifier);
            return this;
        }

        public IEntityBuilder AboveBlock(ulong otherEntityId)
        {
            aboveBlocks.AddComponent(entityId, otherEntityId);
            return this;
        }

        abstract public IEntityBuilder Drawable();

        abstract public IEntityBuilder AnimatedSprite(int animatedSpriteId);

    }

}
