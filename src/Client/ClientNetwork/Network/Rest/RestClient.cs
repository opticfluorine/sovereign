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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientNetwork.Network.Rest
{

    /// <summary>
    /// Provides a common interface for interacting with the REST server from the client.
    /// </summary>
    public sealed class RestClient
    {

        /// <summary>
        /// HTTP client instance.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// Base URI for the REST server.
        /// </summary>
        private Uri baseUri;

        /// <summary>
        /// Selects the REST server to use for all future requests.
        /// </summary>
        /// <param name="connectionParameters">Updated connection parameters to use.</param>
        public void SelectServer(ClientConnectionParameters connectionParameters)
        {
            var builder = new UriBuilder();
            builder.Scheme = connectionParameters.RestTls ? "https" : "http";
            builder.Host = connectionParameters.RestHost;
            builder.Port = connectionParameters.RestPort;
            baseUri = builder.Uri;
        }

    } 

}
