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

using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Sovereign.Persistence.Systems.Persistence
{

    /// <summary>
    /// Event handler for the persistence system.
    /// </summary>
    public sealed class PersistenceEventHandler
    {
        private readonly PersistenceScheduler scheduler;
        private readonly PersistenceSynchronizer synchronizer;
        private readonly PersistenceEntityRetriever entityRetriever;
        private readonly PersistenceRangeRetriever rangeRetriever;

        public PersistenceEventHandler(PersistenceScheduler scheduler,
            PersistenceSynchronizer synchronizer,
            PersistenceEntityRetriever entityRetriever,
            PersistenceRangeRetriever rangeRetriever)
        {
            this.scheduler = scheduler;
            this.synchronizer = synchronizer;
            this.entityRetriever = entityRetriever;
            this.rangeRetriever = rangeRetriever;
        }

        public void HandleEvent(Event ev)
        {
            switch (ev.EventId)
            {
                case EventId.Core_Quit:
                    OnCoreQuit();
                    break;

                case EventId.Server_Persistence_RetrieveEntity:
                    {
                        var details = (EntityEventDetails)ev.EventDetails;
                        OnRetrieveEntity(details.EntityId);
                    }
                    break;

                case EventId.Server_Persistence_RetrieveEntitiesInRange:
                    {
                        var details = (VectorPairEventDetails)ev.EventDetails;
                        OnRetrieveEntitiesInRange(details.First, details.Second);
                    }
                    break;

                case EventId.Server_Persistence_Synchronize:
                    OnSynchronize();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Called to handle a Core_Quit event.
        /// </summary>
        private void OnCoreQuit()
        {
            /* Synchronize the database before exit. */
            PerformSynchronization();
        }

        /// <summary>
        /// Called to handle a Server_Persistence_RetrieveEntity event.
        /// </summary>
        /// <param name="entityId">Entity ID to retrieve.</param>
        private void OnRetrieveEntity(ulong entityId)
        {
            entityRetriever.RetrieveEntity(entityId);
        }

        /// <summary>
        /// Called to handle a Server_Persistence_RetrieveEntitiesInRange event.
        /// </summary>
        /// <param name="minPos">Minimum position in the range.</param>
        /// <param name="maxPos">Maximum position in the range.</param>
        private void OnRetrieveEntitiesInRange(Vector3 minPos, Vector3 maxPos)
        {
            rangeRetriever.RetrieveRange(minPos, maxPos);
        }

        /// <summary>
        /// Called to handle a Server_Persistence_Synchronize event.
        /// </summary>
        private void OnSynchronize()
        {
            /* Synchronize. */
            PerformSynchronization();

            /* Schedule the next synchronization. */
            scheduler.ScheduleSynchronize();
        }

        /// <summary>
        /// Performs the database synchronization.
        /// </summary>
        private void PerformSynchronization()
        {
            synchronizer.Synchronize();
        }
    }

}
