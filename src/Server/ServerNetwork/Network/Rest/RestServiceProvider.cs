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

using Microsoft.AspNetCore.Builder;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.ServerNetwork.Network.Rest.Accounts;

namespace Sovereign.ServerNetwork.Network.Rest;

/// <summary>
///     Supports installing Sovereign's REST endpoints into an ASP.NET Core WebApplication.
/// </summary>
public sealed class RestServiceProvider(AuthenticationRestService authentication)
{
    /// <summary>
    ///     Adds Sovereign endpoints to a WebApplication.
    /// </summary>
    /// <param name="app">Web application.</param>
    public void AddEndpoints(WebApplication app)
    {
        app.MapPost(RestEndpoints.Authentication, authentication.PostLogin);
    }
}