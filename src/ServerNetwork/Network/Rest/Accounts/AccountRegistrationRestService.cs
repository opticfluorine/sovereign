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

using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core.Logging;
using Newtonsoft.Json;
using Sovereign.Accounts.Accounts.Services;
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

        public string Path => "/register/";

        public HttpMethod RequestType => HttpMethod.POST;

        public AccountRegistrationRestService(AccountServices accountServices)
        {
            this.accountServices = accountServices;
        }

        public HttpResponse OnRequest(HttpRequest req)
        {
            try
            {
                // Parse input.
                var requestBody = Encoding.UTF8.GetString(req.Data);
                var registrationRequest = JsonConvert
                    .DeserializeObject<RegistrationRequest>(requestBody);
                if (registrationRequest.Username == null ||
                    registrationRequest.Password == null)
                {
                    return CreateResponse(req, 400, "Incomplete request.");
                }

                // Handle registration.
                var result = accountServices.Register(registrationRequest.Username,
                    registrationRequest.Password);
                return CreateResponse(req,
                    resultToStatus[result],
                    resultToString[result]);
            }
            catch (JsonReaderException)
            {
                return CreateResponse(req, 400, "Malformed request.");
            }
            catch (Exception e)
            {
                Logger.Error("Error processing registration request.", e);
                return CreateResponse(req, 500, "Error processing registration request.");
            }
        }

        /// <summary>
        /// Creates the HTTP response.
        /// </summary>
        /// <param name="req">HTTP request.</param>
        /// <param name="status">HTTP status code.</param>
        /// <param name="result">Result string.</param>
        /// <returns>HTTP response.</returns>
        private HttpResponse CreateResponse(HttpRequest req, int status, string result)
        {
            var responseData = new RegistrationResponse()
            {
                Result = result
            };
            var responseJson = JsonConvert.SerializeObject(responseData);
            return new HttpResponse(req, status, null, "application/json",
                Encoding.UTF8.GetBytes(responseJson));
        }

    }

}
