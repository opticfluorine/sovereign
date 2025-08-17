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
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Components.Types;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.ServerNetwork.Network.Rest.WorldSegment;

/// <summary>
///     REST service providing batch transfer of world segment block data from
///     server to client.
/// </summary>
public sealed class WorldSegmentRestService(
    WorldManagementServices worldManagementServices,
    AccountServices accountServices,
    ILogger<WorldSegmentRestService> logger)
{
    //public override string Path => RestEndpoints.WorldSegment + "/{x}/{y}/{z}";

    /// <summary>
    ///     GET endpoint for retrieving world segment block data.
    /// </summary>
    /// <param name="x">World segment index X coordinate.</param>
    /// <param name="y">World segment index Y coordinate.</param>
    /// <param name="z">World segment index Z coordinate.</param>
    /// <param name="context">Context.</param>
    /// <returns>Result.</returns>
    public async Task<IResult> WorldSegmentGet([FromRoute] int x, [FromRoute] int y, [FromRoute] int z,
        HttpContext context)
    {
        // Successful parse, try to fulfill the request if the player is subscribed.
        var accountId = Guid.Parse(context.User.FindFirst(RestAuthentication.ClaimTypes.AccountId)?.Value ??
                                   throw new Exception("No AccountId claim for user."));
        var segmentIndex = new GridPosition(x, y, z);
        var canAccess = accountServices.TryGetPlayerForAccount(accountId, out var playerEntityId)
                        && worldManagementServices.IsPlayerSubscribedToWorldSegment(segmentIndex, playerEntityId);

        if (!canAccess) return Results.Forbid();

        // First check whether the world segment has been fully loaded.
        // If not, tell the client to try again later.
        if (!worldManagementServices.IsWorldSegmentLoaded(segmentIndex))
        {
            logger.LogDebug("Delaying response for segment {Index} while load in progress. (Request ID: {Id})",
                segmentIndex, context.TraceIdentifier);

            return Results.StatusCode((int)HttpStatusCode.ServiceUnavailable);
        }

        // Get the latest version of the block data and encode it for transfer.
        var blockData = await worldManagementServices.GetWorldSegmentBlockData(segmentIndex);
        var blockDataBytes = blockData.Item2;
        if (blockDataBytes.Length == 0)
        {
            // Segment data was processed but is empty.     
            logger.LogError("Got empty block data for world segment {Index}. (Request ID: {Id})", segmentIndex,
                context.TraceIdentifier);
            return Results.InternalServerError();
        }

        return Results.Bytes(blockDataBytes, "application/octet-stream");
    }
}