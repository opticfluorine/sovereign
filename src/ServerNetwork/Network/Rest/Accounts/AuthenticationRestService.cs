/*
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
                    requestData.Password, out var guid, out var secret);
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
