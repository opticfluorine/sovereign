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
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.Persistence.Systems.Persistence
{

    /// <summary>
    /// Responsible for retrieving ranges of entities.
    /// </summary>
    public sealed class PersistenceRangeRetriever
    {
        private readonly EntityProcessor entityProcessor;
        private readonly PersistenceProviderManager providerManager;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public PersistenceRangeRetriever(EntityProcessor entityProcessor,
            PersistenceProviderManager providerManager)
        {
            this.entityProcessor = entityProcessor;
            this.providerManager = providerManager;
        }

        public void RetrieveRange(Vector3 minPos, Vector3 maxPos)
        {
            Logger.DebugFormat("Retrieve entities from {0} to {1}.",
                minPos, maxPos);

            Task.Run(() => DoRetrieve(minPos, maxPos));
        }

        private void DoRetrieve(Vector3 minPos, Vector3 maxPos)
        {
            try
            {
                /* Execute query. */
                var query = providerManager.PersistenceProvider.RetrieveRangeQuery;
                using (var reader = query.RetrieveEntitiesInRange(minPos, maxPos))
                {
                    entityProcessor.ProcessFromReader(reader.Reader);
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Error retrieving entities from {0} to {1}.",
                    minPos, maxPos), e);
            }
        }

    }

}
