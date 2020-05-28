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
