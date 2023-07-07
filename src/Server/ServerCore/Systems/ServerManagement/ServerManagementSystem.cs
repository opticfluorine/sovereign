// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

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