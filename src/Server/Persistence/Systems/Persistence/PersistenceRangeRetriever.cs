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
using System.Numerics;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
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
    private readonly IEventSender eventSender;
    private readonly PersistenceProviderManager providerManager;
    private readonly WorldSegmentResolver worldSegmentResolver;

    public PersistenceRangeRetriever(EntityProcessor entityProcessor,
        PersistenceProviderManager providerManager,
        WorldSegmentResolver worldSegmentResolver,
        IEventSender eventSender)
    {
        this.entityProcessor = entityProcessor;
        this.providerManager = providerManager;
        this.worldSegmentResolver = worldSegmentResolver;
        this.eventSender = eventSender;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Retrieves all entities in the given range.
    /// </summary>
    /// <param name="minPos">Minimum position.</param>
    /// <param name="maxPos">Maximum position.</param>
    public void RetrieveRange(Vector3 minPos, Vector3 maxPos)
    {
        Logger.DebugFormat("Retrieve entities from {0} to {1}.",
            minPos, maxPos);

        Task.Factory.StartNew(() => DoRetrieve(minPos, maxPos));
    }

    /// <summary>
    ///     Retrieves all entities in the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void RetrieveWorldSegment(GridPosition segmentIndex)
    {
        Logger.DebugFormat("Retrieve world segment {0}.", segmentIndex);

        // Retrieve, then send a completion event.
        var (rangeMin, rangeMax) = worldSegmentResolver.GetRangeForWorldSegment(segmentIndex);
        Task.Factory.StartNew(() =>
        {
            // Retrieve world segment.
            DoRetrieve(rangeMin, rangeMax);

            // Signal completion.
            // Sync the event to the next tick to ensure that the loaded entities
            // and components have been fully processed.
            var details = new WorldSegmentEventDetails { SegmentIndex = segmentIndex };
            var ev = new Event(EventId.Server_WorldManagement_WorldSegmentLoaded, details);
            ev.SyncToTick = true;
            eventSender.SendEvent(ev);
        });
    }

    /// <summary>
    ///     Blocking call that retrieves all entities within a given range.
    /// </summary>
    /// <param name="minPos">Minimum position.</param>
    /// <param name="maxPos">Maximum position.</param>
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