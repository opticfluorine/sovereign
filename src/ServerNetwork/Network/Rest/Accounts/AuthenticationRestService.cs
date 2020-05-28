/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Castle.Core.Logging;
using Sovereign.ServerNetwork.Network.Rest;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.ServerNetwork.Configuration;
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Authentication
{

    /// <summary>
    /// REST service for client authentication.
    /// </summary>
    public sealed class AuthenticationRestService : IRestService
    {
        private readonly AccountServices accountServices;
        private readonly IServerNetworkConfiguration configuration;

        /// <summary>
        /// Map from result to HTTP status code.
        /// </summary>
        private readonly IDictionary<AuthenticationResult, int> ResultToStatus
            = new Dictionary<AuthenticationResult, int>()
            {
                {AuthenticationResult.Successful, 201},
                {AuthenticationResult.AlreadyLoggedIn, 409},
                {AuthenticationResult.TooManyAttempts, 429},
                {AuthenticationResult.Failed, 403}
            };

        /// <summary>
        /// Map from result to human-readable result string.
        /// </summary>
        private readonly IDictionary<AuthenticationResult, string> ResultToString
            = new Dictionary<AuthenticationResult, string>()
            {
                {AuthenticationResult.Successful, "Successful."},
                {AuthenticationResult.AlreadyLoggedIn, "The account is already logged in."},
                {AuthenticationResult.TooManyAttempts,
                    "The account is temporarily locked due to too many failed login attempts."},
                {AuthenticationResult.Failed, "Failed."}
            };


        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public string Path => "/login/";

        public HttpMethod RequestType => HttpMethod.POST;

        public AuthenticationRestService(AccountServices accountServices,
            IServerNetworkConfiguration configuration)
        {
            this.accountServices = accountServices;
            this.configuration = configuration;
        }

        public HttpResponse OnRequest(HttpRequest req)
        {
            try
            {
                // Decode and validate input.
                var requestJson = Encoding.UTF8.GetString(req.Data);
                var requestData = JsonConvert.DeserializeObject<LoginRequest>(requestJson);
                if (requestData.Username == null || requestData.Password == null)
                {
                    return CreateResponse(req, 400, "Incomplete input.");
                }

                // Attempt login.
                var result = accountServices.Authenticate(requestData.Username,
                    requestData.Password, req.SourceIp,
                    out var guid, out var secret);
                if (result != AuthenticationResult.Successful)
                {
                    // Report error.
                    return CreateResponse(req, ResultToStatus[result], ResultToString[result]);
                }

                // Register handoff and return connection information.
                return CreateResponse(req,
                    ResultToStatus[result],
                    ResultToString[result],
                    guid.ToString(),
                    secret,
                    configuration.Host,
                    configuration.Port);
            }
            catch (JsonReaderException)
            {
                return CreateResponse(req, 400, "Malformed input.");
            }
            catch (Exception e)
            {
                Logger.Error("Error handling login.", e);
                return CreateResponse(req, 500, "Error processing login request.");
            }
        }

        private HttpResponse CreateResponse(HttpRequest req,
            int status, string result, string id = null,
            string secret = null, string host = null,
            ushort port = 0)
        {
            var responseData = new LoginResponse()
            {
                Result = result,
                UserId = id,
                SharedSecret = secret,
                ServerHost = host,
                ServerPort = port
            };
            var responseJson = JsonConvert.SerializeObject(responseData);
            return new HttpResponse(req, status, null, "application/json",
                Encoding.UTF8.GetBytes(responseJson));
        }

    }

}
