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
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Systems;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Entities;
using Sovereign.Persistence.State.Trackers;
using System;
using System.Collections.Generic;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
/// System responsible for managing server state persistence.
/// </summary>
public sealed class PersistenceSystem : ISystem
{
    private readonly PersistenceProviderManager providerManager;
    private readonly DatabaseValidator databaseValidator;
    private readonly PersistenceEventHandler eventHandler;
    private readonly PersistenceScheduler scheduler;
    private readonly IEventLoop eventLoop;
    private readonly EntityMapper entityMapper;
    private readonly TrackerManager trackerManager;

    private ILogger Logger { get; set; } = NullLogger.Instance;

    private readonly ISet<EventId> eventIdsOfInterest = new HashSet<EventId>()
        {
            EventId.Core_Quit,
            EventId.Server_Persistence_RetrieveEntity,
            EventId.Server_Persistence_RetrieveEntitiesInRange,
            EventId.Server_Persistence_Synchronize
        };

    public EventCommunicator EventCommunicator { get; private set; }

    public ISet<EventId> EventIdsOfInterest { get => eventIdsOfInterest; }

    public int WorkloadEstimate => 20;

    public PersistenceSystem(PersistenceProviderManager providerManager,
        DatabaseValidator databaseValidator,
        PersistenceEventHandler eventHandler,
        PersistenceScheduler scheduler,
        EventCommunicator eventCommunicator,
        IEventLoop eventLoop,
        EntityMapper entityMapper,
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
        this.trackerManager = trackerManager;
        EventCommunicator = eventCommunicator;
        Logger = logger;

        /* Register events. */
        eventDescriptions.RegisterEvent<EntityEventDetails>(EventId.Server_Persistence_RetrieveEntity);
        eventDescriptions.RegisterEvent<VectorPairEventDetails>(EventId.Server_Persistence_RetrieveEntitiesInRange);
        eventDescriptions.RegisterEvent<WorldSegmentEventDetails>(EventId.Server_Persistence_RetrieveWorldSegment);
        eventDescriptions.RegisterNullEvent(EventId.Server_Persistence_Synchronize);

        /* Register system. */
        eventLoop.RegisterSystem(this);

        /* Initialize provider and connect to database. */
        var provider = providerManager.PersistenceProvider;
        provider.Initialize();

        /* Configure the persistence engine against the database. */
        ConfigurePersistence();
    }

    public void Initialize()
    {
        Logger.Info("Starting Persistence system.");

        /* Schedule the first synchronization. */
        scheduler.ScheduleSynchronize();

        Logger.Info("Persistence system started.");
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
        Logger.Info("Stopping Persistence system.");

        var provider = providerManager.PersistenceProvider;
        provider.Cleanup();

        Logger.Info("Persistence system stopped.");
    }

    /// <summary>
    /// Configures the persistence engine based on the current state of the database.
    /// </summary>
    private void ConfigurePersistence()
    {
        /* Validate the database. */
        if (!databaseValidator.ValidateDatabase(providerManager.PersistenceProvider))
        {
            throw new FatalErrorException();
        }

        /* Set up the entity mapper. */
        try
        {
            entityMapper.InitializeMapper(providerManager.PersistenceProvider.NextPersistedIdQuery);

            Logger.DebugFormat("First available persisted entity ID is {0:X}.",
                entityMapper.NextPersistedId);
        }
        catch (Exception e)
        {
            Logger.Fatal("Error creating entity mapper.", e);
            throw new FatalErrorException();
        }
    }

}
