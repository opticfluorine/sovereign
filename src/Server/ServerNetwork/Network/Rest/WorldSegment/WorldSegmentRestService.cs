/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Threading.Tasks;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.ServerCore.Systems.WorldManagement;
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Rest.WorldSegment;

/// <summary>
///     REST service providing batch transfer of world segment block data from
///     server to client.
/// </summary>
public sealed class WorldSegmentRestService : AuthenticatedRestService
{
    private readonly WorldSegmentBlockDataManager blockDataManager;

    public WorldSegmentRestService(RestAuthenticator authenticator, WorldSegmentBlockDataManager blockDataManager)
        : base(authenticator)
    {
        this.blockDataManager = blockDataManager;
    }

    public override string Path => RestEndpoints.WorldSegment + "/{x}/{y}/{z}";

    public override RestPathType PathType => RestPathType.Parameter;

    public override HttpMethod RequestType => HttpMethod.GET;

    protected override async Task OnAuthenticatedRequest(HttpContext ctx, Guid accountId)
    {
        // Parse parameters.
        int x, y, z;
        if (int.TryParse(ctx.Request.Url.Parameters["x"], out x)
            && int.TryParse(ctx.Request.Url.Parameters["y"], out y)
            && int.TryParse(ctx.Request.Url.Parameters["z"], out z))
        {
            // Successful parse, try to fulfill the request.
            // At some point we should also check that the segment is in range for the
            // user that requested it, we don't want to be leaking data for the other
            // side of the world.
            var dataTask = blockDataManager.GetWorldSegmentBlockData(new GridPosition(x, y, z));
            if (dataTask != null)
            {
                // Get the latest version of the block data and encode it for transfer.
                var blockData = await dataTask;

                ctx.Response.ContentType = "application/octet-stream";
                ctx.Response.ContentLength = blockData.Length;
                await ctx.Response.Send(blockData);
            }
            else
            {
                // Segment is not loaded by server at this time.
                ctx.Response.StatusCode = 503;
                await ctx.Response.Send();
            }
        }
        else
        {
            // Invalid coordinates.
            ctx.Response.StatusCode = 404;
            await ctx.Response.Send();
        }
    }
}