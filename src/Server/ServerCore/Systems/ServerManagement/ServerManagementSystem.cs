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
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.NetworkCore.Systems.Ping;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.ServerCore.Systems.ServerManagement;

/// <summary>
///     System responsible for managing the engine in its server configuration.
/// </summary>
public class ServerManagementSystem : ISystem
{
    private readonly IServerConfigurationManager configManager;
    private readonly IEventSender eventSender;
    private readonly PingController pingController;

    public ServerManagementSystem(IEventLoop eventLoop, EventCommunicator eventCommunicator, IEventSender eventSender,
        PingController pingController, IServerConfigurationManager configManager)
    {
        EventCommunicator = eventCommunicator;
        this.eventSender = eventSender;
        this.pingController = pingController;
        this.configManager = configManager;
        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>();
    public int WorkloadEstimate => 1;

    public void Initialize()
    {
        // Enable outbound auto-ping from the server to client.
        pingController.SetAutoPing(eventSender, true, configManager.ServerConfiguration.Network.PingIntervalMs);
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        // System currently doesn't handle any events.
        return 0;
    }
}