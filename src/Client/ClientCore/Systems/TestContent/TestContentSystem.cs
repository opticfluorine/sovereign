/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

using System;
using System.Collections.Generic;
using Castle.Core.Logging;
using Sovereign.ClientCore.Network;
using Sovereign.ClientCore.Systems.ClientNetwork;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Systems.TestContent;

/// <summary>
///     System responsible for supplying content for early testing.
/// </summary>
/// <remarks>
///     This will be removed in the future.
/// </remarks>
public sealed class TestContentSystem : ISystem, IDisposable
{
    /// <summary>
    ///     Hardcoded username for debug. Remove once connection is configurable.
    /// </summary>
    private const string username = "debug";

    /// <summary>
    ///     Hardcoded password for debug. Remove once connection is configurable.
    /// </summary>
    private const string password = "debug12345";

    /// <summary>
    ///     Hardcoded connection parameters for debug. Remove once connection is configurable.
    /// </summary>
    private readonly ClientConnectionParameters connectionParameters =
        new("127.0.0.1", 12820, "127.0.0.1", 8080, false);

    private readonly IEventLoop eventLoop;

    private readonly IEventSender eventSender;
    private readonly ClientNetworkController networkController;
    private readonly WorldManagementController worldManagementController;

    public TestContentSystem(IEventLoop eventLoop,
        EventCommunicator eventCommunicator,
        IEventSender eventSender,
        WorldManagementController worldManagementController,
        ClientNetworkController networkController)
    {
        this.eventLoop = eventLoop;
        EventCommunicator = eventCommunicator;
        this.eventSender = eventSender;
        this.worldManagementController = worldManagementController;
        this.networkController = networkController;

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
        EventId.Client_Network_Connected,
        EventId.Client_Network_RegisterSuccess,
        EventId.Client_Network_RegisterFailed
    };

    public int WorkloadEstimate => 0;

    public void Initialize()
    {
        AutomateRegister();
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
            switch (ev.EventId)
            {
                case EventId.Client_Network_RegisterSuccess:
                case EventId.Client_Network_RegisterFailed:
                    AutomateConnect();
                    break;

                case EventId.Client_Network_Connected:
                    //LoadSegmentsNearOrigin();
                    break;
            }

            eventsProcessed++;
        }

        return eventsProcessed;
    }

    /// <summary>
    ///     Automatically attempts to register the debug account with the local server.
    /// </summary>
    private void AutomateRegister()
    {
        Logger.Info("Automatically registering debug account with local server.");
        networkController.RegisterAccount(eventSender,
            new RegistrationRequest
            {
                Username = username,
                Password = password
            },
            connectionParameters);
    }

    /// <summary>
    ///     Automatically starts the connection process.
    /// </summary>
    private void AutomateConnect()
    {
        /* Automatically connect to a local server with debug credentials. */
        Logger.Info("Automatically connecting to local server.");
        networkController.BeginConnection(eventSender,
            connectionParameters,
            new LoginParameters(username, password));
    }

    /// <summary>
    ///     Automatically loads some test world segments near the global origin.
    /// </summary>
    private void LoadSegmentsNearOrigin()
    {
        /* Load some test world segments. */
        Logger.Info("Automatically loading test world segments.");
        for (var i = -1; i < 2; ++i)
        for (var j = -1; j < 2; ++j)
            worldManagementController.LoadSegment(eventSender,
                new GridPosition(i, j, 0));
    }
}