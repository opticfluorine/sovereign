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
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Authentication
{

    /// <summary>
    /// Internal exception type thrown when an authentication is rejected.
    /// </summary>
    internal sealed class AuthenticationFailureException : Exception
    {
        public AuthenticationFailureException()
        {
        }

        public AuthenticationFailureException(string message) : base(message)
        {
        }

        public AuthenticationFailureException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// REST service for client authentication.
    /// </summary>
    public sealed class AuthenticationRestService : IRestService
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public string Path => "/login/";

        public HttpMethod RequestType => HttpMethod.POST;

        public HttpResponse OnRequest(HttpRequest req)
        {
            try
            {
                return HandleRequest(req);
            }
            catch (AuthenticationFailureException e)
            {
                var sb = new StringBuilder();
                sb.Append("Authentication request rejected from ")
                    .Append(req.SourceIp).Append(".");
                Logger.Info(sb.ToString(), e);

                return new HttpResponse(req, 401, null);
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();
                sb.Append("Error handling authentication request from ")
                    .Append(req.SourceIp).Append(".");
                Logger.Error(sb.ToString(), e);

                return new HttpResponse(req, 500, null);
            }
        }

        /// <summary>
        /// Handles the request.
        /// </summary>
        /// <param name="req">Authentication request.</param>
        /// <returns>Returns a 200 message containing the HMAC key.</returns>
        /// <exception cref="AuthenticationFailureException">
        /// Thrown if the authentication request is rejected.
        /// </exception>
        private HttpResponse HandleRequest(HttpRequest req)
        {
            throw new NotImplementedException();
        }

    }

}
