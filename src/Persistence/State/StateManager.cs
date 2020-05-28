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

using Castle.Core.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Main;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Sovereign.Persistence.State
{

    /// <summary>
    /// Manages changes to state of all persisted entities.
    /// </summary>
    public sealed class StateManager : IDisposable
    {
        private readonly ComponentManager componentManager;
        private readonly PersistenceProviderManager persistenceProviderManager;
        private readonly EntityNotifier entityNotifier;
        private readonly EntityMapper entityMapper;

        /// <summary>
        /// Front state buffer, currently accepting data.
        /// </summary>
        public StateBuffer FrontBuffer { get; private set; }

        /// <summary>
        /// Back state buffer, currently being processed or pending flip.
        /// </summary>
        private StateBuffer backBuffer;

        public StateManager(ComponentManager componentManager,
            PersistenceProviderManager persistenceProviderManager,
            ILogger logger, FatalErrorHandler fatalErrorHandler,
            EntityNotifier entityNotifier, EntityMapper entityMapper)
        {
            FrontBuffer = new StateBuffer(logger, fatalErrorHandler);
            backBuffer = new StateBuffer(logger, fatalErrorHandler);

            this.componentManager = componentManager;
            this.persistenceProviderManager = persistenceProviderManager;
            this.entityNotifier = entityNotifier;
            this.entityMapper = entityMapper;

            entityNotifier.OnRemoveEntity += OnRemoveEntity;
            entityNotifier.OnUnloadEntity += OnUnloadEntity;
        }

        public void Dispose()
        {
            entityNotifier.OnUnloadEntity -= OnUnloadEntity;
            entityNotifier.OnRemoveEntity -= OnRemoveEntity;
        }

        /// <summary>
        /// Commits the changes in the front buffer, swapping the buffers
        /// in the process.
        /// </summary>
        public void CommitChanges()
        {
            /* Swap the buffers. */
            SwapBuffers();

            /* Back buffer now contains the pending updates; synchronize. */
            backBuffer.Synchronize(persistenceProviderManager.PersistenceProvider);
        }

        /// <summary>
        /// Atomically swaps the component state buffers.
        /// </summary>
        private void SwapBuffers()
        {
            /* Acquire strong lock to prevent component updates during swap. */
            using (var strongLock = componentManager.ComponentGuard.AcquireStrongLock())
            {
                /* Swap the buffers. */
                var swap = backBuffer;
                backBuffer = FrontBuffer;
                FrontBuffer = swap;

                /* Clear front buffer so that it can be reused. */
                FrontBuffer.Reset();
            }
        }

        /// <summary>
        /// Called when an entity is removed.
        /// </summary>
        /// <param name="entityId">Removed entity ID.</param>
        private void OnRemoveEntity(ulong entityId)
        {
            var persistedId = entityMapper.GetPersistedId(entityId, out bool needToCreate);
            if (!needToCreate)
            {
                FrontBuffer.RemoveEntity(persistedId);
            }
            entityMapper.UnloadId(entityId);
        }

        /// <summary>
        /// Called when an entity is unloaded.
        /// </summary>
        /// <param name="entityId">Unloaded entity ID.</param>
        private void OnUnloadEntity(ulong entityId)
        {
            entityMapper.UnloadId(entityId);
        }

    }

}
