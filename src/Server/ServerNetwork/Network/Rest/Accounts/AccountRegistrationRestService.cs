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
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ServerNetwork.Network.Rest.Accounts;

/// <summary>
///     REST service for account registration.
/// </summary>
public sealed class AccountRegistrationRestService(
    AccountServices accountServices,
    ILogger<AccountRegistrationRestService> logger)
{
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

    /// <summary>
    ///     POST endpoint for account registration.
    /// </summary>
    /// <param name="request">Registration request.</param>
    /// <param name="context">Context.</param>
    /// <returns>Result.</returns>
    public Task<IResult> PostRegister([FromBody] RegistrationRequest request, HttpContext context)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                logger.LogError("Received incomplete registration request. (Request ID: {ID}", context.TraceIdentifier);
                return Task.FromResult(Results.BadRequest("Missing username and/or password."));
            }

            // Handle registration.
            var result = accountServices.Register(request.Username, request.Password);

            logger.LogInformation("Registration for user '{Username}' returned status {Result}. (Request ID: {ID})",
                request.Username, result, context.TraceIdentifier);
            var response = new RegistrationResponse
            {
                Result = resultToString[result]
            };
            return Task.FromResult(result switch
            {
                RegistrationResult.Successful => Results.Ok(response),
                RegistrationResult.InvalidInput => Results.BadRequest(response),
                RegistrationResult.UsernameTaken => Results.BadRequest(response),
                _ => Results.InternalServerError(response)
            });
        }
        catch (JsonException)
        {
            logger.LogError("Received malformed registration request. (Request ID: {ID})",
                context.TraceIdentifier);
            return Task.FromResult(Results.BadRequest("Malformed request."));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing registration request. (Request ID: {ID})",
                context.TraceIdentifier);
            return Task.FromResult(Results.InternalServerError("Error processing request."));
        }
    }
}