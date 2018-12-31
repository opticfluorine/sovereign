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

        public IEntityBuilder Material(int materialId)
        {
            materials.AddComponent(entityId, materialId);
            return this;
        }

        public IEntityBuilder MaterialModifier(int materialModifier)
        {
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
