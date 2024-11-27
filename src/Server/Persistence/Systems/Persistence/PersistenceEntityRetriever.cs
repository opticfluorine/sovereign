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
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Entities;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Responsible for coordinating the retrieval of single entities.
/// </summary>
public sealed class PersistenceEntityRetriever
{
    private readonly EntityProcessor entityProcessor;
    private readonly IEventSender eventSender;
    private readonly PersistenceInternalController internalController;
    private readonly ILogger<PersistenceEntityRetriever> logger;
    private readonly PersistenceProviderManager providerManager;

    public PersistenceEntityRetriever(PersistenceProviderManager providerManager,
        EntityProcessor entityProcessor, PersistenceInternalController internalController,
        IEventSender eventSender, ILogger<PersistenceEntityRetriever> logger)
    {
        this.providerManager = providerManager;
        this.entityProcessor = entityProcessor;
        this.internalController = internalController;
        this.eventSender = eventSender;
        this.logger = logger;
    }

    /// <summary>
    ///     Retrieves the entity tree rooted at the given entity from the database.
    /// </summary>
    /// <param name="entityId">Entity ID to retrieve.</param>
    public void RetrieveEntity(ulong entityId)
    {
        logger.LogDebug("Retrieve entity tree for entity ID {Id}.", entityId);

        DoRetrieve(entityId);
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
            logger.LogError(e, string.Format("Error retrieving entity {Id}.", entityId));
        }
    }
}