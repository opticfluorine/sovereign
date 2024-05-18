// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

namespace Sovereign.ServerNetwork.Network.Rest.TemplateEntities;

/// <summary>
///     REST service that provides a list of all template entities.
/// </summary>
public class TemplateEntitiesRestService : AuthenticatedRestService
{
    private readonly TemplateEntityDataGenerator generator;

    public TemplateEntitiesRestService(RestAuthenticator authenticator, TemplateEntityDataGenerator generator)
        : base(authenticator)
    {
        this.generator = generator;
    }

    public override string Path => RestEndpoints.TemplateEntities;
    public override RestPathType PathType => RestPathType.Static;
    public override HttpMethod RequestType => HttpMethod.GET;

    protected override async Task OnAuthenticatedRequest(HttpContext ctx, Guid accountId)
    {
        await ctx.Response.Send();
    }
}