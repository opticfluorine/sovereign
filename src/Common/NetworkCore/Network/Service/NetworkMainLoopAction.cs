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
public sealed class NetworkMainLoopAction : IMainLoopAction
{
    private readonly InboundNetworkPipeline inboundPipeline;
    private readonly ILogger<NetworkMainLoopAction> logger;
    private readonly NetLogger netLogger;
    private readonly INetworkManager networkManager;
    private readonly OutboundNetworkPipeline outboundPipeline;

    public NetworkMainLoopAction(InboundNetworkPipeline inboundPipeline,
        OutboundNetworkPipeline outboundPipeline,
        NetLogger netLogger,
        INetworkManager networkManager,
        FatalErrorHandler fatalErrorHandler,
        ILogger<NetworkMainLoopAction> logger)
    {
        // Dependency injection.
        this.inboundPipeline = inboundPipeline;
        this.outboundPipeline = outboundPipeline;
        this.netLogger = netLogger;
        this.networkManager = networkManager;
        this.logger = logger;

        // Wire up pipelines.
        networkManager.OnNetworkReceive += NetworkManager_OnNetworkReceive;

        logger.LogInformation("Starting network manager.");

        /* Start the network manager. */
        try
        {
            networkManager.Initialize();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Failed to start the network manager.");
            fatalErrorHandler.FatalError();
        }
    }
    
    public ulong CycleInterval => 1;
    
    public void Execute()
    {
        try
        {
            networkManager.Poll();
        }
        catch (Exception e)
        {
            /* Unhandled exception escaped the network thread. */
            logger.LogError(e, "Error in network manager.");
        }
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