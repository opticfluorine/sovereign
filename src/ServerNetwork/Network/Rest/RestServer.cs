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
using Sovereign.EngineCore.Main;
using Sovereign.ServerNetwork.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Rest
{

    /// <summary>
    /// Integrated REST server.
    /// </summary>
    public sealed class RestServer : IDisposable
    {
        private readonly IServerNetworkConfiguration networkConfiguration;
        private readonly ICollection<IRestService> restServices;
        private readonly FatalErrorHandler fatalErrorHandler;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Response headers for the default request handler.
        /// </summary>
        private readonly Dictionary<string, string> badResponseHeaders
            = new Dictionary<string, string>()
            {
                { "Cache-Control", "public" }
            };

        /// <summary>
        /// Embedded web server.
        /// </summary>
        private Server restServer;

        public RestServer(IServerNetworkConfiguration networkConfiguration,
            ICollection<IRestService> restServices,
            FatalErrorHandler fatalErrorHandler)
        {
            this.networkConfiguration = networkConfiguration;
            this.restServices = restServices;
            this.fatalErrorHandler = fatalErrorHandler;
        }

        /// <summary>
        /// Initializes the REST server.
        /// </summary>
        public void Initialize()
        {
            try
            {
                // Create REST server.
                restServer = new Server(networkConfiguration.RestHostname,
                    networkConfiguration.RestPort, false, OnUnmappedRequest);

                // Configure access.
                restServer.AccessControl.Mode = AccessControlMode.DefaultPermit;

                // Add routes for each service.
                foreach (var service in restServices)
                {
                    restServer.StaticRoutes.Add(service.RequestType,
                        service.Path, service.OnRequest);
                }

                Logger.InfoFormat("Started REST server on {0}:{1}.",
                    networkConfiguration.RestHostname,
                    networkConfiguration.RestPort);
            }
            catch (Exception e)
            {
                Logger.Fatal("Failed to create REST server.", e);
                fatalErrorHandler.FatalError();
            }
        }

        public void Dispose()
        {
            Logger.Info("Stopping REST server.");
            restServer.Dispose();
        }

        /// <summary>
        /// Returns a 404 for bad requests.
        /// </summary>
        /// <param name="arg">Request.</param>
        /// <returns>404 response.</returns>
        private HttpResponse OnUnmappedRequest(HttpRequest arg)
        {
            Logger.DebugFormat("REST server returned 404 for {0} request at {1} from {2}.",
                arg.Method.ToString(), arg.FullUrl, arg.SourceIp);

            return new HttpResponse(arg, 404, badResponseHeaders);
        }

    }

}
