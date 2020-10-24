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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Entities
{

    /// <summary>
    /// Top-level manager of the entity infrastructure.
    /// </summary>
    /// 
    /// The methods exposed by this class are thread-safe.
    public class EntityManager
    {
        private readonly ComponentManager componentManager;
        private readonly EntityNotifier entityNotifier;

        /// <summary>
        /// First block reserved for use by automatic assignment.
        /// </summary>
        public const uint FirstReservedBlock = 0;

        /// <summary>
        /// First block not reserved for use by automatic assignment.
        /// </summary>
        public const uint FirstUnreservedBlock = 16777216;

        /// <summary>
        /// Next block identifier.
        /// </summary>
        private int nextBlock = (int)FirstReservedBlock;

        public EntityManager(ComponentManager componentManager,
            EntityNotifier entityNotifier)
        {
            this.componentManager = componentManager;
            this.entityNotifier = entityNotifier;
        }

        /// <summary>
        /// Gets a new EntityAssigner.
        /// </summary>
        /// 
        /// This method is thread-safe.
        /// 
        /// <returns>New EntityAssigner over the next available block.</returns>
        public EntityAssigner GetNewAssigner()
        {
            var block = Interlocked.Increment(ref nextBlock);
            return GetNewAssigner((uint) block);
        }

        /// <summary>
        /// Gets a new EntityAssigner for the given block.
        /// </summary>
        /// 
        /// No checking of whether the block is already in use is performed. If
        /// the block is already in use, this may lead to entity ID collisions.
        /// 
        /// This method is thread-safe.
        /// 
        /// <param name="block">Block identifier.</param>
        /// <returns>New EntityAssigner over the given block.</returns>
        public EntityAssigner GetNewAssigner(uint block)
        {
            return new EntityAssigner(block);
        }

        /// <summary>
        /// Removes the given entity.
        /// </summary>
        /// <param name="entityId">Entity ID to be removed.</param>
        public void RemoveEntity(ulong entityId)
        {
            componentManager.RemoveAllComponentsForEntity(entityId);
            entityNotifier.EnqueueRemove(entityId);
        }

        /// <summary>
        /// Unloads the given entity, but does not formally delete it.
        /// </summary>
        /// <param name="entityId">Entity ID to be unloaded.</param>
        public void UnloadEntity(ulong entityId)
        {
            componentManager.UnloadAllComponentsForEntity(entityId);
            entityNotifier.EnqueueUnload(entityId);
        }

    }

}
