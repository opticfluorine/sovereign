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

using System.Numerics;
using Castle.Core.Logging;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.ServerCore.Events;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Event handler for the persistence system.
/// </summary>
public sealed class PersistenceEventHandler
{
    private readonly PersistenceEntityRetriever entityRetriever;
    private readonly PersistenceRangeRetriever rangeRetriever;
    private readonly PersistenceScheduler scheduler;
    private readonly PersistenceSynchronizer synchronizer;

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

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void HandleEvent(Event ev)
    {
        switch (ev.EventId)
        {
            case EventId.Core_Quit:
                OnCoreQuit();
                break;

            case EventId.Server_Persistence_RetrieveEntity:
            {
                if (ev.EventDetails == null)
                {
                    Logger.Error("Received RetrieveEntity event with no details.");
                    break;
                }

                var details = (EntityEventDetails)ev.EventDetails;
                OnRetrieveEntity(details.EntityId);
            }
                break;

            case EventId.Server_Persistence_RetrieveEntitiesInRange:
            {
                if (ev.EventDetails == null)
                {
                    Logger.Error("Received RetrieveEntitiesInRange event with no details.");
                    break;
                }

                var details = (VectorPairEventDetails)ev.EventDetails;
                OnRetrieveEntitiesInRange(details.First, details.Second);
            }
                break;

            case EventId.Server_Persistence_RetrieveWorldSegment:
            {
                if (ev.EventDetails == null)
                {
                    Logger.Error("Received RetrieveWorldSegment event with no details.");
                    break;
                }

                var details = (WorldSegmentEventDetails)ev.EventDetails;
                OnRetrieveWorldSegment(details.SegmentIndex);
            }
                break;

            case EventId.Server_Persistence_Synchronize:
                OnSynchronize();
                break;

            case EventId.Server_Accounts_SelectPlayer:
            {
                if (ev.EventDetails == null)
                {
                    Logger.Error("Received SelectPlayer event with no details.");
                    break;
                }

                var details = (SelectPlayerEventDetails)ev.EventDetails;
                OnSelectPlayer(details);
            }
                break;
        }
    }

    /// <summary>
    ///     Called to handle a Server_Accounts_SelectPlayer event.
    /// </summary>
    /// <param name="details">Event details.</param>
    private void OnSelectPlayer(SelectPlayerEventDetails details)
    {
        // Map this request directly to an entity retrieval for the player character entity,
        // but only if the player isn't newly created (and therefore already in memory).
        if (details.NewPlayer) OnRetrieveEntity(details.PlayerCharacterEntityId);
    }

    /// <summary>
    ///     Called to handle a Core_Quit event.
    /// </summary>
    private void OnCoreQuit()
    {
        /* Synchronize the database before exit. */
        PerformSynchronization();
    }

    /// <summary>
    ///     Called to handle a Server_Persistence_RetrieveEntity event.
    /// </summary>
    /// <param name="entityId">Entity ID to retrieve.</param>
    private void OnRetrieveEntity(ulong entityId)
    {
        entityRetriever.RetrieveEntity(entityId);
    }

    /// <summary>
    ///     Called to handle a Server_Persistence_RetrieveEntitiesInRange event.
    /// </summary>
    /// <param name="minPos">Minimum position in the range.</param>
    /// <param name="maxPos">Maximum position in the range.</param>
    private void OnRetrieveEntitiesInRange(Vector3 minPos, Vector3 maxPos)
    {
        rangeRetriever.RetrieveRange(minPos, maxPos);
    }

    /// <summary>
    ///     Called to handle a Server_Persistence_RetrieveWorldSegment event.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    private void OnRetrieveWorldSegment(GridPosition segmentIndex)
    {
    }

    /// <summary>
    ///     Called to handle a Server_Persistence_Synchronize event.
    /// </summary>
    private void OnSynchronize()
    {
        /* Synchronize. */
        PerformSynchronization();

        /* Schedule the next synchronization. */
        scheduler.ScheduleSynchronize();
    }

    /// <summary>
    ///     Performs the database synchronization.
    /// </summary>
    private void PerformSynchronization()
    {
        synchronizer.Synchronize();
    }
}