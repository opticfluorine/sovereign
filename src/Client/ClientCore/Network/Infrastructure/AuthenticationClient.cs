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
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Network.Infrastructure;

/// <summary>
///     Responsible for authenticating the client with the server.
/// </summary>
public sealed class AuthenticationClient
{
    /// <summary>
    ///     Maximum length in bytes of a response.
    /// </summary>
    private const long MaxResponseLength = 1024;

    private readonly ILogger<AuthenticationClient> logger;

    private readonly RestClient restClient;

    public AuthenticationClient(RestClient restClient, ILogger<AuthenticationClient> logger)
    {
        this.restClient = restClient;
        this.logger = logger;
    }

    /// <summary>
    ///     Attempts to login to the server with the given username and password.
    /// </summary>
    /// <param name="loginParameters">Login parameters.</param>
    /// <returns>Login response on successful login, or an error message on failure.</returns>
    public async Task<Option<LoginResponse, string>> LoginAsync(LoginParameters loginParameters)
    {
        try
        {
            // Prepare request.
            logger.LogInformation("Attempting login as {Username}.", loginParameters.Username);
            var request = new LoginRequest
            {
                Username = loginParameters.Username,
                Password = loginParameters.Password
            };

            // Send request, handle response.
            var response = await restClient.PostJson(RestEndpoints.Authentication, request);
            var contentLen = response.Content.Headers.ContentLength;
            var result = new Option<LoginResponse, string>("Unknown error.");
            if (contentLen > MaxResponseLength)
            {
                logger.LogError("Response length {Length} too long.", contentLen);
                return result;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                    // Successful login.
                    logger.LogInformation("Login successful.");
                    result = new Option<LoginResponse, string>((await ProcessSuccessfulLogin(response)).Value);
                    break;

                case HttpStatusCode.TooManyRequests:
                    // Failed: Too many login attempts, try again later.
                    const string tooManyAttempts = "Login failed: too many attempts, try again later.";
                    logger.LogError(tooManyAttempts);
                    result = new Option<LoginResponse, string>(tooManyAttempts);
                    break;

                case HttpStatusCode.Conflict:
                    // Failed: Account is already logged in.
                    const string alreadyLoggedIn = "Login failed: account is already logged in.";
                    logger.LogError(alreadyLoggedIn);
                    result = new Option<LoginResponse, string>(alreadyLoggedIn);
                    break;

                case HttpStatusCode.Forbidden:
                    // Failed: Invalid username or password.
                    const string invalidInfo = "Login failed: invalid username or password.";
                    logger.LogError(invalidInfo);
                    result = new Option<LoginResponse, string>(invalidInfo);
                    break;

                case HttpStatusCode.BadRequest:
                    // Failed: Invalid request.
                    const string invalidRequest = "Login failed: invalid request.";
                    logger.LogError(invalidRequest);
                    result = new Option<LoginResponse, string>(invalidRequest);
                    break;

                case HttpStatusCode.InternalServerError:
                    // Failed: Server error.
                    const string serverError = "Login failed: server error.";
                    logger.LogError(serverError);
                    result = new Option<LoginResponse, string>(serverError);
                    break;

                default:
                    // Failed: Unknown error.
                    const string unknownError = "Login failed: unknown error.";
                    logger.LogError(unknownError);
                    result = new Option<LoginResponse, string>(unknownError);
                    break;
            }

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled exception during login.");
            return new Option<LoginResponse, string>(e.Message);
        }
    }

    /// <summary>
    ///     Processes the response for a successful login.
    /// </summary>
    /// <param name="response">Response from REST server.</param>
    /// <returns>Login response, or an empty Maybe if an error occurs.</returns>
    private async Task<Maybe<LoginResponse>> ProcessSuccessfulLogin(HttpResponseMessage response)
    {
        try
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return result != null ? new Maybe<LoginResponse>(result) : new Maybe<LoginResponse>();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while processing successful login.");
            return new Maybe<LoginResponse>();
        }
    }
}