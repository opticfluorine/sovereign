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
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.World;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Entities;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Responsible for retrieving ranges of entities.
/// </summary>
public sealed class PersistenceRangeRetriever
{
    private readonly EntityProcessor entityProcessor;
    private readonly ILogger<PersistenceRangeRetriever> logger;
    private readonly PersistenceProviderManager providerManager;
    private readonly WorldSegmentPersister worldSegmentPersister;
    private readonly WorldSegmentResolver worldSegmentResolver;

    public PersistenceRangeRetriever(EntityProcessor entityProcessor,
        PersistenceProviderManager providerManager,
        WorldSegmentResolver worldSegmentResolver,
        WorldSegmentPersister worldSegmentPersister,
        ILogger<PersistenceRangeRetriever> logger)
    {
        this.entityProcessor = entityProcessor;
        this.providerManager = providerManager;
        this.worldSegmentResolver = worldSegmentResolver;
        this.worldSegmentPersister = worldSegmentPersister;
        this.logger = logger;
    }

    /// <summary>
    ///     Retrieves all entities in the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void RetrieveWorldSegment(GridPosition segmentIndex)
    {
        logger.LogTrace("Retrieve world segment {Index}.", segmentIndex);

        // Retrieve world segment.
        DoRetrieve(segmentIndex);
    }

    /// <summary>
    ///     Blocking call that retrieves all entities within a given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    private void DoRetrieve(GridPosition segmentIndex)
    {
        var (minPos, maxPos) = worldSegmentResolver.GetRangeForWorldSegment(segmentIndex);
        try
        {
            // Retrieve and load the block data for the segment.
            worldSegmentPersister.LoadWorldSegmentBlockData(providerManager.PersistenceProvider, segmentIndex);

            // Retrieve the non-block entities in the segment.
            var query = providerManager.PersistenceProvider.RetrieveRangeQuery;
            using (var reader = query.RetrieveEntitiesInRange(minPos, maxPos))
            {
                var count = entityProcessor.ProcessFromReader(reader.Reader);
                logger.LogTrace("Retrieved {Count} entities for segment {Index}.", count, segmentIndex);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving entities from {Min} to {Max}.", minPos, maxPos);
        }
    }
}