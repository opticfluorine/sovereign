﻿/*
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Main;
using Sovereign.ServerCore.Configuration;
using WatsonWebserver;
using WatsonWebserver.Core;

namespace Sovereign.ServerNetwork.Network.Rest;

/// <summary>
///     Integrated REST server.
/// </summary>
public sealed class RestServer : IDisposable
{
    /// <summary>
    ///     Response headers for the default request handler.
    /// </summary>
    private readonly NameValueCollection badResponseHeaders
        = new()
        {
            { "Cache-Control", "public" }
        };

    private readonly FatalErrorHandler fatalErrorHandler;
    private readonly ILogger<RestServer> logger;
    private readonly NetworkOptions networkOptions;

    /// <summary>
    ///     Embedded web server.
    /// </summary>
    private readonly Lazy<Webserver> restServer;

    private readonly IEnumerable<IRestService> restServices;

    public RestServer(IOptions<NetworkOptions> networkOptions,
        IEnumerable<IRestService> restServices,
        FatalErrorHandler fatalErrorHandler,
        ILogger<RestServer> logger)
    {
        this.networkOptions = networkOptions.Value;
        this.restServices = restServices;
        this.fatalErrorHandler = fatalErrorHandler;
        this.logger = logger;

        restServer = new Lazy<Webserver>(() =>
        {
            return new Webserver(new WebserverSettings(this.networkOptions.RestHostname,
                this.networkOptions.RestPort), OnUnmappedRequest);
        });
    }

    public void Dispose()
    {
        logger.LogInformation("Stopping REST server.");
        if (restServer.Value.IsListening) restServer.Value.Stop();
    }

    /// <summary>
    ///     Initializes the REST server.
    /// </summary>
    public void Initialize()
    {
        try
        {
            // Create REST server.
            restServer.Value.Start();

            // Add routes for each service.
            foreach (var service in restServices)
                switch (service.PathType)
                {
                    case RestPathType.Static:
                        restServer.Value.Routes.PreAuthentication.Static.Add(service.RequestType,
                            service.Path, service.OnRequest);
                        break;

                    case RestPathType.Parameter:
                        restServer.Value.Routes.PreAuthentication.Parameter.Add(service.RequestType,
                            service.Path, service.OnRequest);
                        break;

                    default:
                        logger.LogError("Unrecognized path type in REST service. The service will not be available.");
                        break;
                }

            logger.LogInformation("Started REST server on {Host}:{Port}.",
                networkOptions.RestHostname,
                networkOptions.RestPort);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Failed to create REST server.");
            fatalErrorHandler.FatalError();
        }
    }

    /// <summary>
    ///     Sends a 404 for bad requests.
    /// </summary>
    /// <param name="ctx">HTTP context.</param>
    /// <returns>Task for sending response.</returns>
    private async Task OnUnmappedRequest(HttpContextBase ctx)
    {
        logger.LogDebug("REST server returned 404 for {Method} request at {Url} from {Ip}.",
            ctx.Request.Method.ToString(), ctx.Request.Url.Full, ctx.Request.Source.IpAddress);

        ctx.Response.StatusCode = 404;
        ctx.Response.Headers = badResponseHeaders;

        await ctx.Response.Send();
    }
}