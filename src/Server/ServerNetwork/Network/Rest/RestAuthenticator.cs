// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Text;
using Sovereign.Accounts.Accounts.Authentication;
using Sovereign.EngineUtil.Monads;
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Rest;

/// <summary>
///     Provides reusable authentication services for REST requests.
/// </summary>
public class RestAuthenticator
{
    private readonly AccountLoginTracker loginTracker;

    public RestAuthenticator(AccountLoginTracker loginTracker)
    {
        this.loginTracker = loginTracker;
    }

    /// <summary>
    ///     Authenticates a REST request.
    /// </summary>
    /// <param name="ctx">Request context.</param>
    /// <param name="accountId">Authenticated account ID, if authentication was successful.</param>
    /// <returns>Authentication result.</returns>
    public RestAuthenticationResult Authenticate(HttpContext ctx, out Maybe<Guid> accountId)
    {
        accountId = new Maybe<Guid>();

        // Request must have Authorization header, otherwise credentials are missing.
        if (!ctx.Request.HeaderExists("Authorization")) return RestAuthenticationResult.MissingCredentials;

        try
        {
            // Extract the credentials. If the header is corrupt or is not Basic authentication, deny access.
            var tokens = ctx.Request.RetrieveHeaderValue("Authorization").Split(' ', 2);
            if (tokens.Length != 2) return RestAuthenticationResult.Denied;
            if (!tokens[0].Equals("Basic")) return RestAuthenticationResult.Denied;
            var rawCreds = Encoding.UTF8.GetString(Convert.FromBase64String(tokens[1]));
            var creds = rawCreds.Split(':', 2);
            if (creds.Length != 2) return RestAuthenticationResult.Denied;

            // Username is the account ID, password is the API key for the account. Verify.
            var claimedAccountId = Guid.Parse(creds[0]);
            var apiKey = creds[1];
            var expectedApiKey = loginTracker.GetApiKey(claimedAccountId);
            if (!AuthenticationUtil.CompareUtf8Strings(apiKey, expectedApiKey))
                return RestAuthenticationResult.Denied;

            // If we get here, the request has been successfully authenticated.
            accountId = new Maybe<Guid>(claimedAccountId);
            return RestAuthenticationResult.Success;
        }
        catch (Exception e)
        {
            // If we get here, there was something malformed with the header. Deny access.
            return RestAuthenticationResult.Denied;
        }
    }

    /// <summary>
    ///     Configures an HTTP response for an unauthorized request.
    /// </summary>
    /// <param name="response">Response to configure.</param>
    public void SetupUnauthorizedResponse(HttpResponse response)
    {
        response.StatusCode = 401;
        response.Headers.Add("WWW-Authenticate", "Basic realm=\"Sovereign REST API\", charset=\"UTF-8\"");
    }
}