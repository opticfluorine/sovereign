/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems.Block.Events;

namespace Sovereign.EngineCore.Systems.Block;

/// <summary>
///     System responsible for managing the block entities.
/// </summary>
public sealed class BlockSystem : ISystem, IDisposable
{
    private readonly BlockEventHandler eventHandler;
    private readonly IEventLoop eventLoop;

    public BlockSystem(BlockEventHandler eventHandler, IEventLoop eventLoop,
        EventCommunicator eventCommunicator, EventDescriptions eventDescriptions)
    {
        /* Dependency injection. */
        this.eventHandler = eventHandler;
        this.eventLoop = eventLoop;
        EventCommunicator = eventCommunicator;

        /* Register events. */
        eventDescriptions.RegisterEvent<BlockAddEventDetails>(EventId.Core_Block_Add);
        eventDescriptions.RegisterEvent<BlockAddBatchEventDetails>(EventId.Core_Block_AddBatch);
        eventDescriptions.RegisterEvent<EntityEventDetails>(EventId.Core_Block_Remove);
        eventDescriptions.RegisterEvent<BlockRemoveBatchEventDetails>(EventId.Core_Block_RemoveBatch);

        /* Register system. */
        eventLoop.RegisterSystem(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void Dispose()
    {
        eventLoop.UnregisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>
    {
        EventId.Core_Block_Add,
        EventId.Core_Block_AddBatch,
        EventId.Core_Block_Remove,
        EventId.Core_Block_RemoveBatch
    };

    public int WorkloadEstimate => 50;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var eventsProcessed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            Logger.DebugFormat("Received event with type {0}.", ev.EventId);
            eventHandler.HandleEvent(ev);
            eventsProcessed++;
        }

        return eventsProcessed;
    }
}