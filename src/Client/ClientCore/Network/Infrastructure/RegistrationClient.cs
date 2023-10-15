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
using Castle.Core.Logging;
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
    private readonly RestClient restClient;

    public RegistrationClient(RestClient restClient)
    {
        this.restClient = restClient;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

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
                Logger.Error("Cannot register while already connected.");
                result = new Option<RegistrationResponse, string>("Cannot register while already connected.");
                return result;
            }

            Logger.InfoFormat("Attempting to register new account {0} with server at {1}:{2}.",
                request.Username, connectionParameters.RestHost, connectionParameters.RestPort);

            // Temporarily connect to the target REST server.
            restClient.SelectServer(connectionParameters);

            // Send the registration request.
            var httpResponse = await restClient.PostJson(RestEndpoints.AccountRegistration, request);
            var response = await httpResponse.Content.ReadFromJsonAsync<RegistrationResponse>();
            if (httpResponse.StatusCode == HttpStatusCode.Created)
            {
                Logger.Info("Registration successful.");
                result = new Option<RegistrationResponse, string>(response);
            }
            else
            {
                Logger.ErrorFormat("Registration failed: {0}", response.Result);
                result = new Option<RegistrationResponse, string>(response.Result);
            }
        }
        catch (Exception e)
        {
            Logger.Error("Exception thrown while attempting to register.", e);
        }
        finally
        {
            // Make sure the REST client goes back to the disconnected state.
            restClient.Disconnect();
        }

        return result;
    }
}