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
using Sovereign.Persistence.Entities;
using System;

namespace Sovereign.Persistence.State
{

    /// <summary>
    /// Base class for state trackers.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    public abstract class BaseStateTracker<T> : IDisposable
        where T : unmanaged
    {
        private readonly BaseComponentCollection<T> components;
        private readonly T defaultElement;
        protected readonly EntityMapper entityMapper;
        protected readonly StateManager stateManager;

        /// <summary>
        /// Creates the base state tracker.
        /// </summary>
        /// <param name="components">Component collection.</param>
        /// <param name="defaultElement">Default value for elements.</param>
        /// <param name="entityMapper">Entity mapper.</param>
        /// <param name="stateManager">State manager.</param>
        public BaseStateTracker(BaseComponentCollection<T> components,
            T defaultElement, EntityMapper entityMapper, 
            StateManager stateManager)
        {
            this.components = components;
            this.defaultElement = defaultElement;
            this.entityMapper = entityMapper;
            this.stateManager = stateManager;

            RegisterCallbacks();
        }

        public void Dispose()
        {
            DeregisterCallbacks();
        }

        /// <summary>
        /// Called when a state update is available.
        /// </summary>
        /// <param name="update">State update.</param>
        protected abstract void OnStateUpdate(ref StateUpdate<T> update);

        /// <summary>
        /// Called when a component is added.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="componentValue">Component value.</param>
        private void OnComponentAdded(ulong entityId, T componentValue)
        {
            /* Map the entity ID. */
            ulong persistedId = GetPersistedId(entityId);

            /* Create the update. */
            var update = new StateUpdate<T>
            {
                EntityId = persistedId,
                StateUpdateType = StateUpdateType.Add,
                Value = componentValue
            };
            OnStateUpdate(ref update);
        }

        /// <summary>
        /// Called when a component is modified.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="componentValue">Component value.</param>
        private void OnComponentModified(ulong entityId, T componentValue)
        {
            /* Map the entity ID. */
            ulong persistedId = GetPersistedId(entityId);

            /* Create the update. */
            var update = new StateUpdate<T>
            {
                EntityId = persistedId,
                StateUpdateType = StateUpdateType.Modify,
                Value = componentValue
            };
            OnStateUpdate(ref update);
        }

        /// <summary>
        /// Called when a component is removed.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        private void OnComponentRemoved(ulong entityId)
        {
            /* Map the entity ID. */
            ulong persistedId = GetPersistedId(entityId);

            /* Create the update. */
            var update = new StateUpdate<T>
            {
                EntityId = persistedId,
                StateUpdateType = StateUpdateType.Remove,
                Value = defaultElement
            };
            OnStateUpdate(ref update);
        }

        /// <summary>
        /// Registers the component callbacks.
        /// </summary>
        private void RegisterCallbacks()
        {
            components.OnComponentAdded += OnComponentAdded;
            components.OnComponentModified += OnComponentModified;
            components.OnComponentRemoved += OnComponentRemoved;
        }

        /// <summary>
        /// Deregisters the component callbacks.
        /// </summary>
        private void DeregisterCallbacks()
        {
            components.OnComponentRemoved -= OnComponentRemoved;
            components.OnComponentModified -= OnComponentModified;
            components.OnComponentAdded -= OnComponentAdded;
        }

        /// <summary>
        /// Gets the persisted ID for the given entity ID, queueing an entity
        /// creation in the database if needed.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <returns>Persisted entity ID.</returns>
        private ulong GetPersistedId(ulong entityId)
        {
            ulong persistedId = entityMapper.GetPersistedId(entityId,
                out bool needToCreate);
            if (needToCreate)
            {
                stateManager.FrontBuffer.AddEntity(persistedId);
            }
            return persistedId;
        }

    }

}
