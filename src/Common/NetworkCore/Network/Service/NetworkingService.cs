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
using System.Threading;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Inbound;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;

namespace Sovereign.NetworkCore.Network.Service;

/// <summary>
///     Responsible for managing network operations on one or more
///     separate threads.
/// </summary>
public sealed class NetworkingService
{
    private readonly FatalErrorHandler fatalErrorHandler;
    private readonly InboundNetworkPipeline inboundPipeline;
    private readonly ILogger<NetworkingService> logger;
    private readonly NetLogger netLogger;
    private readonly INetworkManager networkManager;
    private readonly OutboundNetworkPipeline outboundPipeline;

    /// <summary>
    ///     Thread that the service is running on.
    /// </summary>
    private readonly Thread serviceThread;

    /// <summary>
    ///     Flag indicating whether the service has been requested to stop.
    /// </summary>
    private bool stopRequested;

    public NetworkingService(InboundNetworkPipeline inboundPipeline,
        OutboundNetworkPipeline outboundPipeline,
        NetLogger netLogger,
        INetworkManager networkManager,
        FatalErrorHandler fatalErrorHandler,
        ILogger<NetworkingService> logger)
    {
        // Dependency injection.
        this.inboundPipeline = inboundPipeline;
        this.outboundPipeline = outboundPipeline;
        this.netLogger = netLogger;
        this.networkManager = networkManager;
        this.fatalErrorHandler = fatalErrorHandler;
        this.logger = logger;

        // Wire up pipelines.
        networkManager.OnNetworkReceive += NetworkManager_OnNetworkReceive;

        // Set up thread, but do not start.
        serviceThread = new Thread(Run)
        {
            Name = "Networking"
        };
    }

    /// <summary>
    ///     Starts the networking service.
    /// </summary>
    public void Start()
    {
        stopRequested = false;
        serviceThread.Start();
    }

    /// <summary>
    ///     Stops the networking service.
    /// </summary>
    public void Stop()
    {
        stopRequested = true;
        serviceThread.Join();
    }

    /// <summary>
    ///     Runs the networking service.
    /// </summary>
    private void Run()
    {
        logger.LogInformation("Networking service is started.");

        /* Start the network manager. */
        try
        {
            networkManager.Initialize();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Failed to start the network manager.");
            fatalErrorHandler.FatalError();
            return;
        }

        /* Loop until shutdown. */
        while (!stopRequested)
            try
            {
                networkManager.Poll();
                Thread.Sleep(1);
            }
            catch (Exception e)
            {
                /* Unhandled exception escaped the network thread. */
                logger.LogError(e, "Error in networking service.");
            }

        /* Clean up networking resources. */
        try
        {
            networkManager.Dispose();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Error stopping network manager.");
        }

        logger.LogInformation("Networking service is stopped.");
    }

    /// <summary>
    ///     Called when an event is received from the network.
    /// </summary>
    /// <param name="ev">Received event.</param>
    /// <param name="connection">Associated connection.</param>
    private void NetworkManager_OnNetworkReceive(Event ev, NetworkConnection connection)
    {
        inboundPipeline.ProcessEvent(ev, connection);
    }
}