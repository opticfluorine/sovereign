/*
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
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.Accounts.Accounts.Authentication;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.ServerNetwork.Network.Rest.Accounts;

/// <summary>
///     REST service for client authentication.
/// </summary>
public sealed class AuthenticationRestService
{
    /// <summary>
    ///     Maximum request length, in bytes.
    /// </summary>
    public const int MaxRequestLength = 1024;

    private readonly AccountServices accountServices;
    private readonly NetworkConnectionManager connectionManager;
    private readonly ILogger<AuthenticationRestService> logger;

    private readonly AccountLoginTracker loginTracker;

    private readonly NetworkOptions networkOptions;

    /// <summary>
    ///     Map from result to HTTP status code.
    /// </summary>
    private readonly IDictionary<AuthenticationResult, int> resultToStatus
        = new Dictionary<AuthenticationResult, int>
        {
            { AuthenticationResult.Successful, 201 },
            { AuthenticationResult.AlreadyLoggedIn, 409 },
            { AuthenticationResult.TooManyAttempts, 429 },
            { AuthenticationResult.Failed, 403 }
        };

    /// <summary>
    ///     Map from result to human-readable result string.
    /// </summary>
    private readonly IDictionary<AuthenticationResult, string> resultToString
        = new Dictionary<AuthenticationResult, string>
        {
            { AuthenticationResult.Successful, "Successful." },
            { AuthenticationResult.AlreadyLoggedIn, "The account is already logged in." },
            {
                AuthenticationResult.TooManyAttempts,
                "The account is temporarily locked due to too many failed login attempts."
            },
            { AuthenticationResult.Failed, "Failed." }
        };

    public AuthenticationRestService(AccountServices accountServices,
        IOptions<NetworkOptions> networkOptions,
        AccountLoginTracker loginTracker,
        NetworkConnectionManager connectionManager,
        ILogger<AuthenticationRestService> logger)
    {
        this.accountServices = accountServices;
        this.networkOptions = networkOptions.Value;
        this.loginTracker = loginTracker;
        this.connectionManager = connectionManager;
        this.logger = logger;
    }

    /// <summary>
    ///     Login POST endpoint.
    /// </summary>
    /// <param name="requestData">Request.</param>
    /// <returns>Result.</returns>
    public IResult PostLogin([FromBody] LoginRequest requestData)
    {
        try
        {
            // Enforce connection limit if present.
            if (networkOptions.MaxConnections > 0 && connectionManager.ConnectionCount >= networkOptions.MaxConnections)
            {
                return Results.Text("Server is full.", null, Encoding.UTF8, 503);
            }

            // Decode and validate input.
            if (requestData.Username == null || requestData.Password == null)
            {
                return Results.BadRequest("Incomplete input.");
            }

            // Attempt login.
            var result = accountServices.Authenticate(requestData.Username,
                requestData.Password,
                out var guid, out var secret);
            if (result != AuthenticationResult.Successful)
            {
                // Report error.
                return Results.Text(resultToString[result], null, Encoding.UTF8, resultToStatus[result]);
            }

            // Register handoff and return connection information.
            return Results.Ok(new LoginResponse
            {
                Result = resultToString[result],
                UserId = guid.ToString(),
                RestApiKey = loginTracker.GetApiKey(guid),
                SharedSecret = secret,
                ServerHost = networkOptions.Host,
                ServerPort = networkOptions.Port
            });
        }
        catch (JsonException)
        {
            return Results.BadRequest("Malformed input.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling login.");
            return Results.InternalServerError("Error processing login request.");
        }
    }
}