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
