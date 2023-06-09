﻿/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.NetworkCore.Network.Rest.Data;
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Rest.Accounts
{

    /// <summary>
    /// REST service for account registration.
    /// </summary>
    public sealed class AccountRegistrationRestService : IRestService
    {
        private readonly AccountServices accountServices;
        
        private readonly IDictionary<RegistrationResult, int> resultToStatus
            = new Dictionary<RegistrationResult, int>()
            {
                {RegistrationResult.Successful, 201},
                {RegistrationResult.InvalidInput, 400},
                {RegistrationResult.UsernameTaken, 400},
                {RegistrationResult.UnknownFailure, 500}
            };
        
        /// <summary>
        /// Maximum request length, in bytes.
        /// </summary>
        private const int MaxRequestLength = 1024;

        /// <summary>
        /// Map from internal registration results to external messages.
        /// </summary>
        private readonly IDictionary<RegistrationResult, string> resultToString
            = new Dictionary<RegistrationResult, string>()
            {
                {RegistrationResult.Successful, "Successful."},
                {RegistrationResult.InvalidInput, "The username and/or password did not meet minimum requirements."},
                {RegistrationResult.UsernameTaken, "The username is already in use."},
                {RegistrationResult.UnknownFailure, "An unknown error occurred."}
            };

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public string Path => RestEndpoints.AccountRegistration;

        public RestPathType PathType => RestPathType.Static;

        public HttpMethod RequestType => HttpMethod.POST;

        public AccountRegistrationRestService(AccountServices accountServices)
        {
            this.accountServices = accountServices;
        }

        public async Task OnRequest(HttpContext ctx)
        {
            try
            {
                // Safety check.
                if (ctx.Request.ContentLength > MaxRequestLength)
                {
                    Logger.ErrorFormat("Received registration request from {0} that was too large.",
                        ctx.Request.Source.IpAddress);
                    await SendResponse(ctx, 413, "Request too large.");
                    return;
                }
                
                // Parse input.
                var requestBody = ctx.Request.DataAsString;
                var registrationRequest = JsonSerializer.Deserialize<RegistrationRequest>(requestBody,
                    MessageConfig.JsonOptions);
                
                if (registrationRequest.Username == null ||
                    registrationRequest.Password == null)
                {
                    Logger.ErrorFormat("Received incomplete registration request from {0}.", 
                        ctx.Request.Source.IpAddress);
                    await SendResponse(ctx, 400, "Incomplete request.");
                    return;
                }

                // Handle registration.
                var result = accountServices.Register(registrationRequest.Username,
                    registrationRequest.Password);

                Logger.InfoFormat("Registration for user '{0}' from {1} returned status {2}.",
                    registrationRequest.Username, ctx.Request.Source.IpAddress, result);
                await SendResponse(ctx,
                    resultToStatus[result],
                    resultToString[result]);
            }
            catch (JsonException)
            {
                try
                {
                    Logger.ErrorFormat("Received malformed registration request from {0}.",
                        ctx.Request.Source.IpAddress);
                    await SendResponse(ctx, 400, "Malformed request.");
                }
                catch (Exception e)
                {
                    Logger.Error("Error sending malformed request response.", e);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error processing registration request.", e);
                try
                {
                    await SendResponse(ctx, 500, "Error processing registration request.");
                }
                catch (Exception e2)
                {
                    Logger.Error("Error sending error response.", e2);
                }
            }
        }

        /// <summary>
        /// Creates the HTTP response.
        /// </summary>
        /// <param name="ctx">HTTP context.<jparam>
        /// <param name="status">HTTP status code.</param>
        /// <param name="result">Result string.</param>
        /// <returns>HTTP response.</returns>
        private async Task SendResponse(HttpContext ctx, int status, string result)
        {
            var responseData = new RegistrationResponse()
            {
                Result = result
            };
            var responseJson = JsonSerializer.Serialize(responseData);
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.Send(Encoding.UTF8.GetBytes(responseJson));
        }

    }

}
