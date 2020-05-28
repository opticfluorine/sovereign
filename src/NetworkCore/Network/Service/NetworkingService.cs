/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sovereign.NetworkCore.Network.Service
{

    /// <summary>
    /// Responsible for managing network operations on one or more
    /// separate threads.
    /// </summary>
    public sealed class NetworkingService
    {
        private readonly InboundNetworkPipeline inboundPipeline;
        private readonly OutboundNetworkPipeline outboundPipeline;
        private readonly NetLogger netLogger;
        private readonly INetworkManager networkManager;
        private readonly FatalErrorHandler fatalErrorHandler;

        /// <summary>
        /// Thread that the service is running on.
        /// </summary>
        private Thread serviceThread;

        /// <summary>
        /// Flag indicating whether the service has been requested to stop.
        /// </summary>
        private bool stopRequested;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Queue of outgoing events to be processed and sent.
        /// </summary>
        public ConcurrentQueue<Event> EventsToSend { get; }
            = new ConcurrentQueue<Event>();

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

        /// <summary>
        /// Starts the networking service.
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
        /// Stops the networking service.
        /// </summary>
        public void Stop()
        {
            stopRequested = true;
            serviceThread.Join();
        }

        /// <summary>
        /// Runs the networking service.
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
            {
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
        /// Outputs startup diagnonstic messages.
        /// </summary>
        private void OutputStartupDiagnostics()
        {
            inboundPipeline.OutputStartupDiagnostics();
            outboundPipeline.OutputStartupDiagnostics();
        }

        /// <summary>
        /// Called when an event is received from the network.
        /// </summary>
        /// <param name="ev">Received event.</param>
        /// <param name="connection">Associated connection.</param>
        private void NetworkManager_OnNetworkReceive(Event ev, NetworkConnection connection)
        {
            inboundPipeline.ProcessEvent(ev, connection);
        }

    }

}
