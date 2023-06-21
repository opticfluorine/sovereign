/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
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
using Sovereign.ClientCore.Network.Rest;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Rest.Data;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Network.Infrastructure
{

    /// <summary>
    /// Responsible for making registration requests to the REST server.
    /// </summary>
    public sealed class RegistrationClient
    {
        private readonly RestClient restClient;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public RegistrationClient(RestClient restClient)
        {
            this.restClient = restClient;
        }

        public async Task<Option<RegistrationResponse, string>> RegisterAsync(ClientConnectionParameters connectionParameters,
            RegistrationRequest request)
        {
            var result = new Option<RegistrationResponse, string>("Unexpected error occurred.");

            try
            {
                // REST client must not already be connected.
                if (restClient.Connected)
                {
                    Logger.Error("Cannot register while already connected.");
                    result = new Option<RegistrationResponse, string>("Cannot register while already connected.");
                    return result;
                }

                Logger.InfoFormat("Attempting to register new account {0} with server at {1}:{2}.",
                    request.Username, connectionParameters.RestHost, connectionParameters.RestPort);

                // Temporarily connect to the target REST server.
                restClient.SelectServer(connectionParameters);

                // Send the registration request.
                var httpResponse = await restClient.PostJson(RestEndpoints.AccountRegistration, request);
                var response = await httpResponse.Content.ReadFromJsonAsync<RegistrationResponse>();
                if (httpResponse.StatusCode == HttpStatusCode.Created)
                {
                    Logger.Info("Registration successful.");
                    result = new Option<RegistrationResponse, string>(response);
                }
                else
                {
                    Logger.ErrorFormat("Registration failed: {0}", response.Result);
                    result = new Option<RegistrationResponse, string>(response.Result);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Exception thrown while attempting to register.", e);
            }
            finally
            {
                // Make sure the REST client goes back to the disconnected state.
                restClient.Disconnect();
            }

            return result;
        }

    }

}
