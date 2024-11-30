/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
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
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Network.Infrastructure;

/// <summary>
///     Responsible for making registration requests to the REST server.
/// </summary>
public sealed class RegistrationClient
{
    private const long MaxResponseLength = 1024;
    private readonly ILogger<RegistrationClient> logger;
    private readonly RestClient restClient;

    public RegistrationClient(RestClient restClient, ILogger<RegistrationClient> logger)
    {
        this.restClient = restClient;
        this.logger = logger;
    }

    /// <summary>
    ///     Sends a request to the server to register a new account.
    /// </summary>
    /// <param name="connectionParameters">Connection parameters for server.</param>
    /// <param name="request">Registration request.</param>
    /// <returns>
    ///     Async task that will yield either the response on success,
    ///     or an error message string on failure.
    /// </returns>
    public async Task<Option<RegistrationResponse, string>> RegisterAsync(
        ClientConnectionParameters connectionParameters,
        RegistrationRequest request)
    {
        var result = new Option<RegistrationResponse, string>("Unexpected error occurred.");

        try
        {
            // REST client must not already be connected.
            if (restClient.Connected)
            {
                logger.LogError("Cannot register while already connected.");
                result = new Option<RegistrationResponse, string>("Cannot register while already connected.");
                return result;
            }

            logger.LogInformation("Attempting to register new account {0} with server at {1}:{2}.",
                request.Username, connectionParameters.RestHost, connectionParameters.RestPort);

            // Temporarily connect to the target REST server.
            restClient.SelectServer(connectionParameters);

            // Send the registration request.
            var httpResponse = await restClient.PostJson(RestEndpoints.AccountRegistration, request);
            if (httpResponse.Content.Headers.ContentLength > MaxResponseLength)
            {
                logger.LogError("Registration response length {0} is too long.",
                    httpResponse.Content.Headers.ContentLength);
                result = new Option<RegistrationResponse, string>("Response too long.");
                return result;
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<RegistrationResponse>();
            if (response == null)
            {
                var msg = "Received null response from server.";
                logger.LogError(msg);
                return new Option<RegistrationResponse, string>(msg);
            }

            if (httpResponse.StatusCode == HttpStatusCode.Created)
            {
                logger.LogInformation("Registration successful.");
                result = new Option<RegistrationResponse, string>(response);
            }
            else
            {
                if (response.Result != null)
                {
                    logger.LogError("Registration failed: {0}", response.Result);
                    result = new Option<RegistrationResponse, string>(response.Result);
                }
                else
                {
                    logger.LogError("Registration failed: Unknown error.");
                    result = new Option<RegistrationResponse, string>("Unknown error.");
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception thrown while attempting to register.");
        }
        finally
        {
            // Make sure the REST client goes back to the disconnected state.
            restClient.Disconnect();
        }

        return result;
    }
}