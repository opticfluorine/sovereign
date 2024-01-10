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

using System.Collections.Generic;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     System responsible for managing the world state lifecycle.
/// </summary>
public sealed class WorldManagementSystem : ISystem
{
    private readonly WorldManagementEventHandler eventHandler;

    private readonly IEventLoop eventLoop;
    private readonly WorldSegmentSubscriptionManager subscriptionManager;

    public WorldManagementSystem(EventCommunicator eventCommunicator,
        IEventLoop eventLoop, WorldManagementEventHandler eventHandler,
        EventDescriptions eventDescriptions, WorldSegmentSubscriptionManager subscriptionManager)
    {
        /* Dependency injection. */
        EventCommunicator = eventCommunicator;
        this.eventLoop = eventLoop;
        this.eventHandler = eventHandler;
        this.subscriptionManager = subscriptionManager;

        /* Register system. */
        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Server_WorldManagement_WorldSegmentLoaded
    };

    public int WorkloadEstimate => 80;

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

    public void Initialize()
    {
    }
}