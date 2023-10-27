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
using System.Threading.Tasks;
using Sovereign.EngineCore.Network.Rest;
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     REST service for selecting a player character for use.
/// </summary>
public class SelectPlayerRestService : AuthenticatedRestService
{
    public SelectPlayerRestService(RestAuthenticator authenticator) : base(authenticator)
    {
    }

    public override string Path => RestEndpoints.Player + "/{id}";
    public override RestPathType PathType => RestPathType.Parameter;
    public override HttpMethod RequestType => HttpMethod.POST;

    protected override Task OnAuthenticatedRequest(HttpContext ctx, Guid accountId)
    {
        throw new NotImplementedException();
    }
}