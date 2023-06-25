/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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
using System.Threading;
using Castle.Core.Logging;
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
    private readonly NetLogger netLogger;
    private readonly INetworkManager networkManager;
    private readonly OutboundNetworkPipeline outboundPipeline;

    /// <summary>
    ///     Thread that the service is running on.
    /// </summary>
    private Thread serviceThread;

    /// <summary>
    ///     Flag indicating whether the service has been requested to stop.
    /// </summary>
    private bool stopRequested;

    public NetworkingService(InboundNetworkPipeline inboundPipeline,
        OutboundNetworkPipeline outboundPipeline,
        NetLogger netLogger,
        INetworkManager networkManager,
        FatalErrorHandler fatalErrorHandler)
    {
        // Dependency injection.
        this.inboundPipeline = inboundPipeline;
        this.outboundPipeline = outboundPipeline;
        this.netLogger = netLogger;
        this.networkManager = networkManager;
        this.fatalErrorHandler = fatalErrorHandler;

        // Wire up pipelines.
        networkManager.OnNetworkReceive += NetworkManager_OnNetworkReceive;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Starts the networking service.
    /// </summary>
    public void Start()
    {
        stopRequested = false;
        serviceThread = new Thread(Run)
        {
            Name = "Networking"
        };
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
        Logger.Info("Networking service is started.");
        OutputStartupDiagnostics();

        /* Start the network manager. */
        try
        {
            networkManager.Initialize();
        }
        catch (Exception e)
        {
            Logger.Fatal("Failed to start the network manager.", e);
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
                Logger.Error("Error in networking service.", e);
            }

        /* Clean up networking resources. */
        try
        {
            networkManager.Dispose();
        }
        catch (Exception e)
        {
            Logger.Fatal("Error stopping network manager.", e);
        }

        Logger.Info("Networking service is stopped.");
    }

    /// <summary>
    ///     Outputs startup diagnonstic messages.
    /// </summary>
    private void OutputStartupDiagnostics()
    {
        inboundPipeline.OutputStartupDiagnostics();
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