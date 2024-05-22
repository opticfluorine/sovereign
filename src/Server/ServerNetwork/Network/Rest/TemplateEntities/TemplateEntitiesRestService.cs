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
using Castle.Core.Logging;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.ServerCore.Systems.TemplateEntity;
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Rest.TemplateEntities;

/// <summary>
///     REST service that provides a list of all template entities.
/// </summary>
public class TemplateEntitiesRestService : AuthenticatedRestService
{
    private readonly TemplateEntityServices templateEntityServices;

    public TemplateEntitiesRestService(RestAuthenticator authenticator, TemplateEntityServices templateEntityServices)
        : base(authenticator)
    {
        this.templateEntityServices = templateEntityServices;
    }

    public new ILogger Logger { private get; set; } = NullLogger.Instance;

    public override string Path => RestEndpoints.TemplateEntities;
    public override RestPathType PathType => RestPathType.Static;
    public override HttpMethod RequestType => HttpMethod.GET;

    protected override async Task OnAuthenticatedRequest(HttpContext ctx, Guid accountId)
    {
        Logger.InfoFormat("Received template entity data request from account {0}.", accountId);

        try
        {
            var data = await templateEntityServices.GetLatestTemplateEntityData();
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "application/octet-stream";
            ctx.Response.ContentLength = data.Length;
            await ctx.Response.Send(data);
        }
        catch (Exception e)
        {
            Logger.Error("Error when sending template entity data.", e);
            try
            {
                ctx.Response.StatusCode = 500;
                await ctx.Response.Send();
            }
            catch (Exception e2)
            {
                Logger.Error("Error when sending error response.", e2);
            }
        }
    }
}