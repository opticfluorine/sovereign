/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
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
using Sovereign.ClientCore.Events;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;

namespace Sovereign.ClientCore.Systems.ClientNetwork;

/// <summary>
///     System responsible for managing the client network resources.
/// </summary>
public sealed class ClientNetworkSystem : ISystem, IDisposable
{
    private readonly ClientNetworkEventHandler eventHandler;
    private readonly IEventLoop eventLoop;

    public ClientNetworkSystem(EventCommunicator eventCommunicator,
        IEventLoop eventLoop, EventDescriptions eventDescriptions,
        ClientNetworkEventHandler eventHandler)
    {
        this.eventLoop = eventLoop;
        this.eventHandler = eventHandler;
        EventCommunicator = eventCommunicator;

        eventDescriptions.RegisterNullEvent(EventId.Client_Network_Connected);
        eventDescriptions.RegisterNullEvent(EventId.Client_Network_ConnectionLost);
        eventDescriptions.RegisterEvent<BeginConnectionEventDetails>(EventId.Client_Network_BeginConnection);
        eventDescriptions.RegisterEvent<ErrorEventDetails>(EventId.Client_Network_ConnectionAttemptFailed);
        eventDescriptions.RegisterEvent<ErrorEventDetails>(EventId.Client_Network_LoginFailed);

        eventLoop.RegisterSystem(this);
    }

    public void Dispose()
    {
        eventLoop.UnregisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>
    {
        EventId.Client_Network_ConnectionLost,
        EventId.Client_Network_BeginConnection,
        EventId.Core_WorldManagement_Subscribe,
        EventId.Core_WorldManagement_Unsubscribe,
        EventId.Client_Network_EndConnection,
        EventId.Core_Network_Logout
    };

    public int WorkloadEstimate => 20;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        /* Poll for events. */
        var eventsProcessed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            eventHandler.HandleEvent(ev);
            eventsProcessed++;
        }

        return eventsProcessed;
    }
}