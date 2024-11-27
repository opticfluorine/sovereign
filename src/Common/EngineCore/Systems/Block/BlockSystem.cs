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
using Sovereign.EngineCore.Events;

namespace Sovereign.EngineCore.Systems.Block;

/// <summary>
///     System responsible for managing the block entities.
/// </summary>
public sealed class BlockSystem : ISystem, IDisposable
{
    private readonly BlockEventHandler eventHandler;
    private readonly IEventLoop eventLoop;

    public BlockSystem(BlockEventHandler eventHandler, IEventLoop eventLoop,
        EventCommunicator eventCommunicator)
    {
        /* Dependency injection. */
        this.eventHandler = eventHandler;
        this.eventLoop = eventLoop;
        EventCommunicator = eventCommunicator;

        /* Register system. */
        eventLoop.RegisterSystem(this);
    }

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
        EventId.Core_Block_RemoveBatch,
        EventId.Core_Block_RemoveAt,
        EventId.Core_Block_ModifyNotice,
        EventId.Core_Block_RemoveNotice
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
            eventHandler.HandleEvent(ev);
            eventsProcessed++;
        }

        return eventsProcessed;
    }
}