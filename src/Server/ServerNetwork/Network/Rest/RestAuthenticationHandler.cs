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
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.Accounts.Accounts.Authentication;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Player;

namespace Sovereign.ServerNetwork.Network.Rest;

/// <summary>
///     Provides reusable authentication services for REST requests.
/// </summary>
/// <remarks>
///     This will be deprecated in the future and replaced with JWT bearer tokens from OAuth 2.0.
/// </remarks>
public sealed class RestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    ///     Authentication scheme name.
    /// </summary>
    public const string SchemeName = "Sovereign";

    private readonly AccountServices accountServices;

    private readonly AccountLoginTracker loginTracker;
    private readonly PlayerRoleCheck roleCheck;

    public RestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory, UrlEncoder urlEncoder, AccountLoginTracker loginTracker,
        PlayerRoleCheck roleCheck, AccountServices accountServices)
        : base(options, loggerFactory, urlEncoder)
    {
        this.loginTracker = loginTracker;
        this.roleCheck = roleCheck;
        this.accountServices = accountServices;
    }

    /// <summary>
    ///     Authenticates a REST request.
    /// </summary>
    /// <returns>Authentication result.</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Request must have Authorization header, otherwise credentials are missing.
        if (Request.Headers.Authorization.Count < 1)
            return Task.FromResult(AuthenticateResult.NoResult());
        if (Request.Headers.Authorization.Count > 1)
            return Task.FromResult(AuthenticateResult.Fail("Too many Authorization headers."));

        try
        {
            // Extract the credentials. If the header is corrupt or is not Basic authentication, deny access.
            var tokens = Request.Headers.Authorization[0]!.Split(' ', 2);
            if (tokens.Length != 2) return Task.FromResult(AuthenticateResult.Fail("Malformed Authorization header."));
            if (!tokens[0].Equals("Basic"))
                return Task.FromResult(AuthenticateResult.Fail($"Expected Basic authorization, got {tokens[0]}."));

            var rawCreds = Encoding.UTF8.GetString(Convert.FromBase64String(tokens[1]));
            var creds = rawCreds.Split(':', 2);
            if (creds.Length != 2) return Task.FromResult(AuthenticateResult.Fail("Malformed credentials."));

            // Username is the account ID, password is the API key for the account. Verify.
            var claimedAccountId = Guid.Parse(creds[0]);
            var apiKey = creds[1];
            var expectedApiKey = loginTracker.GetApiKey(claimedAccountId);
            if (!AuthenticationUtil.CompareUtf8Strings(apiKey, expectedApiKey))
                return Task.FromResult(AuthenticateResult.Fail("Unrecognized access token."));

            // If we get here, the request has been successfully authenticated.
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(RestAuthentication.ClaimTypes.AccountId, claimedAccountId.ToString()));

            if (accountServices.TryGetPlayerForAccount(claimedAccountId, out var playerId))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, RestAuthorization.Roles.Player));
                if (roleCheck.IsPlayerAdmin(playerId))
                    identity.AddClaim(new Claim(ClaimTypes.Role, RestAuthorization.Roles.Admin));
            }
            else
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, RestAuthorization.Roles.OutOfGame));
            }

            return Task.FromResult(
                AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
        }
        catch (Exception e)
        {
            // If we get here, there was something malformed with the header. Deny access.
            return Task.FromResult(AuthenticateResult.Fail(e));
        }
    }
}