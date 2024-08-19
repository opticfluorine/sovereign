// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Text;
using Sovereign.Accounts.Accounts.Authentication;
using Sovereign.EngineUtil.Monads;
using WatsonWebserver;
using WatsonWebserver.Core;

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
    public RestAuthenticationResult Authenticate(HttpContextBase ctx, out Maybe<Guid> accountId)
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
        catch (Exception)
        {
            // If we get here, there was something malformed with the header. Deny access.
            return RestAuthenticationResult.Denied;
        }
    }

    /// <summary>
    ///     Configures an HTTP response for an unauthorized request.
    /// </summary>
    /// <param name="response">Response to configure.</param>
    public void SetupUnauthorizedResponse(HttpResponseBase response)
    {
        response.StatusCode = 401;
        response.Headers.Add("WWW-Authenticate", "Basic realm=\"Sovereign REST API\", charset=\"UTF-8\"");
    }
}