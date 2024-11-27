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
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Systems;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Entities;
using Sovereign.Persistence.State.Trackers;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     System responsible for managing server state persistence.
/// </summary>
public sealed class PersistenceSystem : ISystem
{
    private readonly DatabaseValidator databaseValidator;
    private readonly EntityMapper entityMapper;
    private readonly EntityProcessor entityProcessor;
    private readonly PersistenceEventHandler eventHandler;

    private readonly IEventLoop eventLoop;
    private readonly PersistenceProviderManager providerManager;
    private readonly PersistenceScheduler scheduler;
    private readonly TrackerManager trackerManager;

    public PersistenceSystem(PersistenceProviderManager providerManager,
        DatabaseValidator databaseValidator,
        PersistenceEventHandler eventHandler,
        PersistenceScheduler scheduler,
        EventCommunicator eventCommunicator,
        IEventLoop eventLoop,
        EntityMapper entityMapper,
        EntityProcessor entityProcessor,
        TrackerManager trackerManager,
        ILogger logger,
        EventDescriptions eventDescriptions)
    {
        /* Dependency injection. */
        this.providerManager = providerManager;
        this.databaseValidator = databaseValidator;
        this.eventHandler = eventHandler;
        this.scheduler = scheduler;
        this.eventLoop = eventLoop;
        this.entityMapper = entityMapper;
        this.entityProcessor = entityProcessor;
        this.trackerManager = trackerManager;
        EventCommunicator = eventCommunicator;
        Logger = logger;

        /* Register system. */
        eventLoop.RegisterSystem(this);

        /* Configure the persistence engine against the database. */
        ConfigurePersistence();
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Quit,
        EventId.Server_Persistence_RetrieveEntity,
        EventId.Server_Persistence_RetrieveWorldSegment,
        EventId.Server_Persistence_Synchronize,
        EventId.Server_Accounts_SelectPlayer
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
            entityMapper.InitializeMapper(providerManager.PersistenceProvider.NextPersistedIdQuery!);

            logger.LogDebug("First available persisted entity ID is {0:X}.",
                entityMapper.NextPersistedId);
        }
        catch (Exception e)
        {
            logger.LogCritical("Error creating entity mapper.", e);
            throw new FatalErrorException();
        }

        LoadTemplates();
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
            logger.LogInformation("Loaded {0} template entities.", count);
        }
        catch (Exception e)
        {
            logger.LogCritical("Error loading template entities.", e);
            throw new FatalErrorException();
        }
    }
}