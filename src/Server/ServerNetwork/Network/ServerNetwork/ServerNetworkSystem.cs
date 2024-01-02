// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.ServerCore.Events;

namespace Sovereign.ServerNetwork.Network.ServerNetwork;

/// <summary>
///     Responsible for managing network resources at the server.
/// </summary>
public class ServerNetworkSystem : ISystem
{
    private readonly INetworkManager networkManager;

    public ServerNetworkSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop,
        INetworkManager networkManager)
    {
        this.networkManager = networkManager;
        EventCommunicator = eventCommunicator;
        eventLoop.RegisterSystem(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>
    {
        EventId.Server_Network_DisconnectClient
    };

    public int WorkloadEstimate => 5;

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
            switch (ev.EventId)
            {
                case EventId.Server_Network_DisconnectClient:
                    if (ev.EventDetails == null)
                    {
                        Logger.Error("Received DisconnectClient event without details.");
                        break;
                    }

                    OnDisconnectClient((ConnectionIdEventDetails)ev.EventDetails);
                    break;
            }

            eventsProcessed++;
        }

        return eventsProcessed;
    }

    /// <summary>
    ///     Handles a request to disconnect the client associated with the given connection.
    /// </summary>
    /// <param name="details">Request.</param>
    private void OnDisconnectClient(ConnectionIdEventDetails details)
    {
        networkManager.Disconnect(details.ConnectionId);
    }
}