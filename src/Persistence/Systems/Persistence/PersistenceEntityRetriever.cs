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
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.Persistence.Systems.Persistence
{

    /// <summary>
    /// Responsible for coordinating the retrieval of single entities.
    /// </summary>
    public sealed class PersistenceEntityRetriever
    {
        private readonly PersistenceProviderManager providerManager;
        private readonly EntityProcessor entityProcessor;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public PersistenceEntityRetriever(PersistenceProviderManager providerManager,
            EntityProcessor entityProcessor)
        {
            this.providerManager = providerManager;
            this.entityProcessor = entityProcessor;
        }

        /// <summary>
        /// Retrieves the given entity from the database.
        /// </summary>
        /// <param name="entityId">Entity ID to retrieve.</param>
        public void RetrieveEntity(ulong entityId)
        {
            Logger.DebugFormat("Retrieve entity ID {0}.", entityId);

            /* Asynchronously retrieve the entity using a worker thread. */
            Task.Run(() => DoRetrieve(entityId));
        }

        /// <summary>
        /// Does the work of retrieving the given entity.
        /// </summary>
        /// <param name="entityId">Entity ID to retrieve.</param>
        private void DoRetrieve(ulong entityId)
        {
            try
            {
                /* Execute query. */
                var query = providerManager.PersistenceProvider.RetrieveEntityQuery;
                using (var reader = query.RetrieveEntity(entityId))
                {
                    /* Process results. */
                    entityProcessor.ProcessFromReader(reader.Reader);
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Error retrieving entity {0,16:X}.", entityId), e);
            }
        }

    }

}
