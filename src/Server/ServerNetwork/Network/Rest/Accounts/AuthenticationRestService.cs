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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.Accounts.Accounts.Authentication;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.ServerCore.Configuration;
using WatsonWebserver.Core;

namespace Sovereign.ServerNetwork.Network.Rest.Accounts;

/// <summary>
///     REST service for client authentication.
/// </summary>
public sealed class AuthenticationRestService : IRestService
{
    /// <summary>
    ///     Maximum request length, in bytes.
    /// </summary>
    private const int MaxRequestLength = 1024;

    private readonly AccountServices accountServices;
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
        ILogger<AuthenticationRestService> logger)
    {
        this.accountServices = accountServices;
        this.networkOptions = networkOptions.Value;
        this.loginTracker = loginTracker;
        this.logger = logger;
    }

    public string Path => RestEndpoints.Authentication;

    public RestPathType PathType => RestPathType.Static;

    public HttpMethod RequestType => HttpMethod.POST;

    public async Task OnRequest(HttpContextBase ctx)
    {
        try
        {
            // Safety check.
            if (ctx.Request.ContentLength > MaxRequestLength)
            {
                await SendResponse(ctx, 413, "Request too large.");
                return;
            }

            // Decode and validate input.
            var requestJson = ctx.Request.DataAsString;
            var requestData = JsonSerializer.Deserialize<LoginRequest>(requestJson,
                MessageConfig.JsonOptions);
            if (requestData == null || requestData.Username == null || requestData.Password == null)
            {
                await SendResponse(ctx, 400, "Incomplete input.");
                return;
            }

            // Attempt login.
            var result = accountServices.Authenticate(requestData.Username,
                requestData.Password,
                out var guid, out var secret);
            if (result != AuthenticationResult.Successful)
            {
                // Report error.
                await SendResponse(ctx, resultToStatus[result], resultToString[result]);
                return;
            }

            // Register handoff and return connection information.
            await SendResponse(ctx,
                resultToStatus[result],
                resultToString[result],
                guid.ToString(),
                loginTracker.GetApiKey(guid),
                secret,
                networkOptions.Host,
                networkOptions.Port);
        }
        catch (JsonException)
        {
            try
            {
                await SendResponse(ctx, 400, "Malformed input.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error sending malformed input response.");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling login.");
            try
            {
                await SendResponse(ctx, 500, "Error processing login request.");
            }
            catch (Exception e2)
            {
                logger.LogError(e2, "Error sending error response.");
            }
        }
    }

    private async Task SendResponse(HttpContextBase ctx,
        int status, string result, string id = "", string apiKey = "",
        string secret = "", string host = "",
        ushort port = 0)
    {
        ctx.Response.StatusCode = status;

        var responseData = new LoginResponse
        {
            Result = result,
            UserId = id,
            RestApiKey = apiKey,
            SharedSecret = secret,
            ServerHost = host,
            ServerPort = port
        };
        var responseJson = JsonSerializer.Serialize(responseData);

        await ctx.Response.Send(Encoding.UTF8.GetBytes(responseJson));
    }
}