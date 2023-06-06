﻿/*
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

using Castle.DynamicProxy.Generators;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Rest.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sovereign.ClientNetwork.Network
{

    /// <summary>
    /// Responsible for authenticating the client with the server.
    /// </summary>
    public sealed class AuthenticationClient
    {
        private readonly RestClient restClient;

        public AuthenticationClient(RestClient restClient)
        {
            this.restClient = restClient;
        }

        public async Maybe<string> LoginAsync(string username, string password)
        {
            // Prepare request.
            var request = new LoginRequest()
            {
                Username = username,
                Password = password
            };

            // Send request, handle response.
            var response = await restClient.PostJson(RestEndpoints.Authentication, request);
        }

    }

}

