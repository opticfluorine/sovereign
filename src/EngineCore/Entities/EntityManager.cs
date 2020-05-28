﻿/*
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
