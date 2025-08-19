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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sovereign.Persistence.Players;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     REST service for deleting existing player characters.
/// </summary>
public sealed class DeletePlayerRestService(
    PersistencePlayerServices playerServices,
    ILogger<DeletePlayerRestService> logger)
{
    /// <summary>
    ///     DELETE endpoint for players.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="context">Context.</param>
    /// <returns>Result.</returns>
    public Task<IResult> PlayerDelete([FromRoute] ulong playerId, HttpContext context)
    {
        try
        {
            // Verify that the player belongs to the authenticated account.
            var accountId = Guid.Parse(context.User.FindFirst(RestAuthentication.ClaimTypes.AccountId)?.Value ??
                                       throw new Exception("User missing AccountId claim."));
            if (!playerServices.ValidatePlayerAccountPair(playerId, accountId))
            {
                logger.LogError(
                    "Entity {EntityId:X} is not a player assigned to account {AccountId}. (Request ID: {Id})",
                    playerId, accountId, context.TraceIdentifier);
                return Task.FromResult(Results.Forbid());
            }

            // Logically delete the player.
            playerServices.DeletePlayer(playerId);
            return Task.FromResult(Results.Ok());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling delete player request. (Request ID: {Id}", context.TraceIdentifier);
            return Task.FromResult(Results.InternalServerError());
        }
    }
}