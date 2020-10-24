/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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
