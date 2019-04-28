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
