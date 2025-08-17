// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using Microsoft.AspNetCore.Authorization;

namespace Sovereign.ServerNetwork.Network.Rest;

/// <summary>
///     Provides support for configuring Sovereign Engine REST API authorization policies.
/// </summary>
public static class RestAuthorizationPolicyProvider
{
    /// <summary>
    ///     Adds authorization policies for the Sovereign Engine REST API.
    /// </summary>
    /// <param name="builder">Authorization builder.</param>
    /// <returns>Authorization builder.</returns>
    public static AuthorizationBuilder AddSovereignPolicies(this AuthorizationBuilder builder)
    {
        return builder
            .AddPolicy(RestAuthorization.Policies.AdminOnly,
                policy => policy.RequireRole(RestAuthorization.Roles.Admin))
            .AddPolicy(RestAuthorization.Policies.RequirePlayer,
                policy => policy.RequireRole(RestAuthorization.Roles.Player))
            .AddPolicy(RestAuthorization.Policies.RequireOutOfGame,
                policy => policy.RequireRole(RestAuthorization.Roles.OutOfGame));
    }
}