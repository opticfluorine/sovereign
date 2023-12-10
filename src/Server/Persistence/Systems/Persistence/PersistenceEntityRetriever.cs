/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Entities;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Responsible for coordinating the retrieval of single entities.
/// </summary>
public sealed class PersistenceEntityRetriever
{
    private readonly EntityProcessor entityProcessor;
    private readonly PersistenceProviderManager providerManager;

    public PersistenceEntityRetriever(PersistenceProviderManager providerManager,
        EntityProcessor entityProcessor)
    {
        this.providerManager = providerManager;
        this.entityProcessor = entityProcessor;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Retrieves the entity tree rooted at the given entity from the database.
    /// </summary>
    /// <param name="entityId">Entity ID to retrieve.</param>
    public void RetrieveEntity(ulong entityId)
    {
        Logger.DebugFormat("Retrieve entity tree for entity ID {0}.", entityId);

        /* Asynchronously retrieve the entity using a worker thread. */
        Task.Run(() => DoRetrieve(entityId));
    }

    /// <summary>
    ///     Does the work of retrieving the entity tree rooted at the given entity.
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