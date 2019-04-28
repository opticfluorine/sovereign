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

using Castle.Core.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineUtil.Collections;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.Persistence.State
{

    /// <summary>
    /// State update buffer.
    /// </summary>
    public sealed class StateBuffer
    {

        /// <summary>
        /// Initial size of each state update buffer.
        /// </summary>
        const int BufferSize = 8192;

        /// <summary>
        /// New entity IDs.
        /// </summary>
        private readonly StructBuffer<ulong> newEntities
            = new StructBuffer<ulong>(BufferSize);

        /// <summary>
        /// Position state updates.
        /// </summary>
        private readonly StructBuffer<StateUpdate<Vector3>> positionUpdates
            = new StructBuffer<StateUpdate<Vector3>>(BufferSize);

        /// <summary>
        /// Material state updates.
        /// </summary>
        private readonly StructBuffer<StateUpdate<int>> materialUpdates
            = new StructBuffer<StateUpdate<int>>(BufferSize);

        /// <summary>
        /// Material modifier state updates.
        /// </summary>
        private readonly StructBuffer<StateUpdate<int>> materialModifierUpdates
            = new StructBuffer<StateUpdate<int>>(BufferSize);

        private readonly ILogger Logger;
        private readonly FatalErrorHandler fatalErrorHandler;

        public StateBuffer(ILogger logger, FatalErrorHandler fatalErrorHandler)
        {
            Logger = logger;
            this.fatalErrorHandler = fatalErrorHandler;
        }

        /// <summary>
        /// Queues an entity for database insert.
        /// </summary>
        /// <param name="entityId">Persisted entity ID.</param>
        public void AddEntity(ulong entityId)
        {
            newEntities.Add(ref entityId);
        }

        /// <summary>
        /// Queues a position update.
        /// </summary>
        /// <param name="update">State update.</param>
        public void UpdatePosition(ref StateUpdate<Vector3> update)
        {
            positionUpdates.Add(ref update);
        }

        /// <summary>
        /// Queues a material update.
        /// </summary>
        /// <param name="update">State update.</param>
        public void UpdateMaterial(ref StateUpdate<int> update)
        {
            materialUpdates.Add(ref update);
        }

        /// <summary>
        /// Queues a material modifier update.
        /// </summary>
        /// <param name="update">State update.</param>
        public void UpdateMaterialModifier(ref StateUpdate<int> update)
        {
            materialModifierUpdates.Add(ref update);
        }

        /// <summary>
        /// Resets the buffer.
        /// </summary>
        public void Reset()
        {
            newEntities.Clear();
            positionUpdates.Clear();
            materialUpdates.Clear();
            materialModifierUpdates.Clear();
        }

        /// <summary>
        /// Synchronizes the buffer with the database.
        /// </summary>
        /// <param name="persistenceProvider">Persistence provider.</param>
        public void Synchronize(IPersistenceProvider persistenceProvider)
        {
            Task.Run(() => DoSynchronize(persistenceProvider));
        }

        /// <summary>
        /// Actually performs the synchronization.
        /// </summary>
        /// <param name="persistenceProvider">Persistence provider.</param>
        private void DoSynchronize(IPersistenceProvider persistenceProvider)
        {
            try
            {
                using (var transaction = persistenceProvider.Connection.BeginTransaction())
                {
                    SynchronizeEntities(persistenceProvider, transaction);

                    /* Position. */
                    SynchronizeComponent(positionUpdates,
                        persistenceProvider.AddPositionQuery,
                        persistenceProvider.ModifyPositionQuery,
                        persistenceProvider.RemovePositionQuery,
                        transaction);

                    /* Material. */
                    SynchronizeComponent(materialUpdates,
                        persistenceProvider.AddMaterialQuery,
                        persistenceProvider.ModifyMaterialQuery,
                        persistenceProvider.RemoveMaterialQuery,
                        transaction);

                    /* MaterialModifier. */
                    SynchronizeComponent(materialModifierUpdates,
                        persistenceProvider.AddMaterialModifierQuery,
                        persistenceProvider.ModifyMaterialModifierQuery,
                        persistenceProvider.RemoveMaterialModifierQuery,
                        transaction);

                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                Logger.Fatal("Error while synchronizing database.", e);
                fatalErrorHandler.FatalError();
            }
        }

        /// <summary>
        /// Adds new entities to the database.
        /// </summary>
        /// <param name="persistenceProvider">Persistence provider.</param>
        /// <param name="transaction">Transaction.</param>
        private void SynchronizeEntities(IPersistenceProvider persistenceProvider,
            IDbTransaction transaction)
        {
            var query = persistenceProvider.AddEntityQuery;
            foreach (var entityId in newEntities)
            {
                query.AddEntity(entityId, transaction);
            }
        }

        /// <summary>
        /// Synchronizes a component buffer.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="buffer">Update buffer.</param>
        /// <param name="addQuery">Add query.</param>
        /// <param name="modQuery">Modify query.</param>
        /// <param name="remQuery">Remove query.</param>
        private void SynchronizeComponent<T>(StructBuffer<StateUpdate<T>> buffer,
            IAddComponentQuery<T> addQuery, IModifyComponentQuery<T> modQuery,
            IRemoveComponentQuery remQuery, IDbTransaction transaction) 
            where T : unmanaged
        {
            foreach (var update in buffer)
            {
                switch (update.StateUpdateType)
                {
                    case StateUpdateType.Add:
                        addQuery.Add(update.EntityId, update.Value, transaction);
                        break;

                    case StateUpdateType.Modify:
                        modQuery.Modify(update.EntityId, update.Value, transaction);
                        break;

                    case StateUpdateType.Remove:
                        remQuery.Remove(update.EntityId, transaction);
                        break;
                }
            }
        }

    }

}
