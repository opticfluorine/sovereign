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
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.ServerCore.Systems.WorldManagement;
using WatsonWebserver.Core;

namespace Sovereign.ServerNetwork.Network.Rest.WorldSegment;

/// <summary>
///     REST service providing batch transfer of world segment block data from
///     server to client.
/// </summary>
public sealed class WorldSegmentRestService : AuthenticatedRestService
{
    private readonly AccountServices accountServices;
    private readonly WorldManagementServices worldManagementServices;

    public WorldSegmentRestService(RestAuthenticator authenticator,
        WorldManagementServices worldManagementServices, AccountServices accountServices)
        : base(authenticator)
    {
        this.worldManagementServices = worldManagementServices;
        this.accountServices = accountServices;
    }

    public override string Path => RestEndpoints.WorldSegment + "/{x}/{y}/{z}";

    public override RestPathType PathType => RestPathType.Parameter;

    public override HttpMethod RequestType => HttpMethod.GET;

    protected override async Task OnAuthenticatedRequest(HttpContextBase ctx, Guid accountId)
    {
        // Parse parameters.
        int x, y, z;
        if (int.TryParse(ctx.Request.Url.Parameters["x"], out x)
            && int.TryParse(ctx.Request.Url.Parameters["y"], out y)
            && int.TryParse(ctx.Request.Url.Parameters["z"], out z))
        {
            // Successful parse, try to fulfill the request if the player is subscribed.
            var segmentIndex = new GridPosition(x, y, z);
            var canAccess = accountServices.TryGetPlayerForAccount(accountId, out var playerEntityId)
                            && worldManagementServices.IsPlayerSubscribedToWorldSegment(segmentIndex, playerEntityId);

            if (canAccess)
            {
                // First check whether the world segment has been fully loaded.
                // If not, tell the client to try again later.
                if (!worldManagementServices.IsWorldSegmentLoaded(segmentIndex))
                {
                    Logger.DebugFormat("Delaying response for segment {0} while load in progress.", segmentIndex);
                    ctx.Response.StatusCode = 503;
                    await ctx.Response.Send();
                    return;
                }

                // Get the data and send it over.
                var dataTask = worldManagementServices.GetWorldSegmentBlockData(segmentIndex);

                // Get the latest version of the block data and encode it for transfer.
                var blockData = await dataTask;
                var blockDataBytes = blockData.Item2;
                if (blockDataBytes.Length == 0)
                {
                    // Segment data was processed but is empty.     
                    Logger.ErrorFormat("Got empty block data for world segment {0}.", segmentIndex);
                    ctx.Response.StatusCode = 500;
                    await ctx.Response.Send();
                    return;
                }

                ctx.Response.ContentType = "application/octet-stream";
                ctx.Response.ContentLength = blockDataBytes.Length;
                await ctx.Response.Send(blockDataBytes);
            }
            else
            {
                // User isn't subscribed to this world segment, deny access.
                ctx.Response.StatusCode = 403;
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