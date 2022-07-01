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
