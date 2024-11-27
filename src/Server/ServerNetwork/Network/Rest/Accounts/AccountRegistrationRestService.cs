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
using System.Threading.Tasks;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.NetworkCore.Network.Rest.Data;
using WatsonWebserver.Core;

namespace Sovereign.ServerNetwork.Network.Rest.Accounts;

/// <summary>
///     REST service for account registration.
/// </summary>
public sealed class AccountRegistrationRestService : IRestService
{
    /// <summary>
    ///     Maximum request length, in bytes.
    /// </summary>
    private const int MaxRequestLength = 1024;

    private readonly AccountServices accountServices;

    private readonly IDictionary<RegistrationResult, int> resultToStatus
        = new Dictionary<RegistrationResult, int>
        {
            { RegistrationResult.Successful, 201 },
            { RegistrationResult.InvalidInput, 400 },
            { RegistrationResult.UsernameTaken, 400 },
            { RegistrationResult.UnknownFailure, 500 }
        };

    /// <summary>
    ///     Map from internal registration results to external messages.
    /// </summary>
    private readonly IDictionary<RegistrationResult, string> resultToString
        = new Dictionary<RegistrationResult, string>
        {
            { RegistrationResult.Successful, "Successful." },
            { RegistrationResult.InvalidInput, "The username and/or password did not meet minimum requirements." },
            { RegistrationResult.UsernameTaken, "The username is already in use." },
            { RegistrationResult.UnknownFailure, "An unknown error occurred." }
        };

    public AccountRegistrationRestService(AccountServices accountServices)
    {
        this.accountServices = accountServices;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public string Path => RestEndpoints.AccountRegistration;

    public RestPathType PathType => RestPathType.Static;

    public HttpMethod RequestType => HttpMethod.POST;

    public async Task OnRequest(HttpContextBase ctx)
    {
        try
        {
            // Safety check.
            if (ctx.Request.ContentLength > MaxRequestLength)
            {
                logger.LogError("Received registration request from {0} that was too large.",
                    ctx.Request.Source.IpAddress);
                await SendResponse(ctx, 413, "Request too large.");
                return;
            }

            // Parse input.
            var requestBody = ctx.Request.DataAsString;
            var registrationRequest = JsonSerializer.Deserialize<RegistrationRequest>(requestBody,
                MessageConfig.JsonOptions);

            if (registrationRequest == null || registrationRequest.Username == null ||
                registrationRequest.Password == null)
            {
                logger.LogError("Received incomplete registration request from {0}.",
                    ctx.Request.Source.IpAddress);
                await SendResponse(ctx, 400, "Incomplete request.");
                return;
            }

            // Handle registration.
            var result = accountServices.Register(registrationRequest.Username,
                registrationRequest.Password);

            logger.LogInformation("Registration for user '{0}' from {1} returned status {2}.",
                registrationRequest.Username, ctx.Request.Source.IpAddress, result);
            await SendResponse(ctx,
                resultToStatus[result],
                resultToString[result]);
        }
        catch (JsonException)
        {
            try
            {
                logger.LogError("Received malformed registration request from {0}.",
                    ctx.Request.Source.IpAddress);
                await SendResponse(ctx, 400, "Malformed request.");
            }
            catch (Exception e)
            {
                logger.LogError("Error sending malformed request response.", e);
            }
        }
        catch (Exception e)
        {
            logger.LogError("Error processing registration request.", e);
            try
            {
                await SendResponse(ctx, 500, "Error processing registration request.");
            }
            catch (Exception e2)
            {
                logger.LogError("Error sending error response.", e2);
            }
        }
    }

    /// <summary>
    ///     Creates the HTTP response.
    /// </summary>
    /// <param name="ctx">HTTP context.</param>
    /// <param name="status">HTTP status code.</param>
    /// <param name="result">Result string.</param>
    /// <returns>HTTP response.</returns>
    private async Task SendResponse(HttpContextBase ctx, int status, string result)
    {
        var responseData = new RegistrationResponse
        {
            Result = result
        };
        var responseJson = JsonSerializer.Serialize(responseData);
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.Send(Encoding.UTF8.GetBytes(responseJson));
    }
}