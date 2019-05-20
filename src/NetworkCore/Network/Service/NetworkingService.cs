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

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
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
        /// Queue of processed events received and accepted from the network.
        /// </summary>
        public ConcurrentQueue<Event> ReceivedEvents { get; } 
            = new ConcurrentQueue<Event>();

        /// <summary>
        /// Queue of outgoing events to be processed and sent.
        /// </summary>
        public ConcurrentQueue<Event> EventsToSend { get; }
            = new ConcurrentQueue<Event>();

        public NetworkingService(InboundNetworkPipeline inboundPipeline,
            OutboundNetworkPipeline outboundPipeline)
        {
            this.inboundPipeline = inboundPipeline;
            this.outboundPipeline = outboundPipeline;
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

        private void Run()
        {
            Logger.Info("Networking service is started.");
            OutputStartupDiagnostics();

            while (!stopRequested)
            {
                Thread.Sleep(1);
            }

            Logger.Info("Networking service is stopped.");
        }

        private void OutputStartupDiagnostics()
        {
            inboundPipeline.OutputStartupDiagnostics();
            outboundPipeline.OutputStartupDiagnostics();
        }

    }

}
