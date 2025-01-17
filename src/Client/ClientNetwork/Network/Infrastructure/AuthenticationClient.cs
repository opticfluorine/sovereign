﻿/*
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

using Castle.Core.Logging;
using Sovereign.ClientCore.Network;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Rest.Data;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Sovereign.ClientNetwork.Network
{

    /// <summary>
    /// Responsible for authenticating the client with the server.
    /// </summary>
    public sealed class AuthenticationClient
    {
        private readonly RestClient restClient;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public AuthenticationClient(RestClient restClient)
        {
            this.restClient = restClient;
        }

        /// <summary>
        /// Attempts to login to the server with the given username and password.
        /// </summary>
        /// <param name="loginParameters">Login parameters.</param>
        /// <returns>Login response on successful login, or an empty Maybe on failure.</returns>
        public async Task<Maybe<LoginResponse>> LoginAsync(LoginParameters loginParameters)
        {
            try
            {
                // Prepare request.
                logger.LogInformation("Attempting login as {0}.", loginParameters.Username);
                var request = new LoginRequest()
                {
                    Username = loginParameters.Username,
                    Password = loginParameters.Password
                };

                // Send request, handle response.
                var response = await restClient.PostJson(RestEndpoints.Authentication, request);
                var result = new Maybe<LoginResponse>();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Created:
                        // Successful login.
                        logger.LogInformation("Login successful.");
                        result = await ProcessSuccessfulLogin(response);
                        break;

                    case HttpStatusCode.TooManyRequests:
                        // Failed: Too many login attempts, try again later.
                        logger.LogError("Login failed: too many attempts, try again later.");
                        break;

                    case HttpStatusCode.Conflict:
                        // Failed: Account is already logged in.
                        logger.LogError("Login failed: account is already logged in.");
                        break;

                    case HttpStatusCode.Forbidden:
                        // Failed: Invalid username or password.
                        logger.LogError("Login failed: invalid username or password.");
                        break;

                    case HttpStatusCode.BadRequest:
                        // Failed: Invalid request.
                        logger.LogError("Login failed: invalid request.");
                        break;

                    case HttpStatusCode.InternalServerError:
                        // Failed: Server error.
                        logger.LogError("Login failed: server error.");
                        break;

                    default:
                        // Failed: Unknown error.
                        logger.LogError("Login failed: unknown error.");
                        break;
                }

                return result;
            }
            catch (Exception e)
            {
                logger.LogError("Unhandled exception during login.", e);
                return new Maybe<LoginResponse>();
            }
        }

        /// <summary>
        /// Processes the response for a successful login.
        /// </summary>
        /// <param name="response">Response from REST server.</param>
        /// <returns>Login response, or an empty Maybe if an error occurs.</returns>
        private async Task<Maybe<LoginResponse>> ProcessSuccessfulLogin(HttpResponseMessage response)
        {
            try
            {
                return new Maybe<LoginResponse>(await response.Content.ReadFromJsonAsync<LoginResponse>());
            }
            catch (Exception e)
            {
                logger.LogError("Error while processing successful login.", e);
                return new Maybe<LoginResponse>();
            }
        }

    }

}

