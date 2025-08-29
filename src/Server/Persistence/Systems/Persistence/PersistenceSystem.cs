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
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Systems;
using Sovereign.Persistence.Data;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Entities;
using Sovereign.Persistence.State.Trackers;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     System responsible for managing server state persistence.
/// </summary>
public sealed class PersistenceSystem : ISystem
{
    private readonly DatabaseValidator databaseValidator;
    private readonly EntityAssigner entityAssigner;
    private readonly EntityProcessor entityProcessor;
    private readonly PersistenceEventHandler eventHandler;
    private readonly GlobalKeyValueProcessor globalKeyValueProcessor;

    private readonly ILogger<PersistenceSystem> logger;
    private readonly PersistenceProviderManager providerManager;
    private readonly PersistenceScheduler scheduler;
    private readonly TrackerManager trackerManager;

    public PersistenceSystem(PersistenceProviderManager providerManager,
        DatabaseValidator databaseValidator,
        PersistenceEventHandler eventHandler,
        PersistenceScheduler scheduler,
        EventCommunicator eventCommunicator,
        IEventLoop eventLoop,
        EntityProcessor entityProcessor,
        TrackerManager trackerManager,
        ILogger<PersistenceSystem> logger,
        GlobalKeyValueProcessor globalKeyValueProcessor,
        EntityAssigner entityAssigner)
    {
        /* Dependency injection. */
        this.providerManager = providerManager;
        this.databaseValidator = databaseValidator;
        this.eventHandler = eventHandler;
        this.scheduler = scheduler;
        this.entityProcessor = entityProcessor;
        this.trackerManager = trackerManager;
        this.logger = logger;
        this.globalKeyValueProcessor = globalKeyValueProcessor;
        this.entityAssigner = entityAssigner;
        EventCommunicator = eventCommunicator;

        /* Register system. */
        eventLoop.RegisterSystem(this);

        /* Configure the persistence engine against the database. */
        ConfigurePersistence();
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Server_Persistence_RetrieveEntity,
        EventId.Server_Persistence_RetrieveWorldSegment,
        EventId.Server_Persistence_Synchronize,
        EventId.Server_Accounts_SelectPlayer,
        EventId.Core_Data_GlobalSet,
        EventId.Core_Data_GlobalRemoved,
        EventId.Core_Data_EntityKeyValueSet,
        EventId.Core_Data_EntityKeyValueRemoved
    };

    public int WorkloadEstimate => 20;

    public void Initialize()
    {
        logger.LogInformation("Starting Persistence system.");

        /* Schedule the first synchronization. */
        scheduler.ScheduleSynchronize();

        logger.LogInformation("Persistence system started.");
    }

    public int ExecuteOnce()
    {
        var eventsProcessed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            eventHandler.HandleEvent(ev);
            eventsProcessed++;
        }

        return eventsProcessed;
    }

    public void Cleanup()
    {
        logger.LogInformation("Stopping Persistence system.");

        eventHandler.Cleanup();

        var provider = providerManager.PersistenceProvider;
        provider.Dispose();

        logger.LogInformation("Persistence system stopped.");
    }

    /// <summary>
    ///     Configures the persistence engine based on the current state of the database.
    /// </summary>
    private void ConfigurePersistence()
    {
        /* Validate the database. */
        if (!databaseValidator.ValidateDatabase(providerManager.PersistenceProvider)) throw new FatalErrorException();

        /* Set up the entity mapper. */
        try
        {
            var nextId = providerManager.PersistenceProvider.NextPersistedIdQuery.GetNextPersistedEntityId();
            entityAssigner.NextEntityId = nextId;
            logger.LogDebug("First available persisted entity ID is {Id:X}.", nextId);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Error creating entity mapper.");
            throw new FatalErrorException();
        }

        LoadTemplates();
        LoadGlobalKeyValuePairs();
    }

    /// <summary>
    ///     Loads all template entities from the database.
    /// </summary>
    private void LoadTemplates()
    {
        try
        {
            using var reader =
                providerManager.PersistenceProvider.RetrieveAllTemplatesQuery.RetrieveAllTemplates();
            var count = entityProcessor.ProcessFromReader(reader.Reader);
            logger.LogInformation("Loaded {Count} template entities.", count);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Error loading template entities.");
            throw new FatalErrorException();
        }
    }

    /// <summary>
    ///     Loads all global key-value pairs from the database.
    /// </summary>
    private void LoadGlobalKeyValuePairs()
    {
        try
        {
            using var reader = providerManager.PersistenceProvider.GetGlobalKeyValuePairsQuery.GetGlobalKeyValuePairs();
            var count = globalKeyValueProcessor.Process(reader);
            logger.LogInformation("Loaded {Count} global key-value pairs.", count);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Error loading global key-value pairs.");
            throw new FatalErrorException();
        }
    }
}