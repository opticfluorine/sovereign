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
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.Accounts.Systems.Accounts;
using Sovereign.EngineCore.Events;
using Sovereign.Persistence.Database;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     REST service for selecting a player character for use.
/// </summary>
public class SelectPlayerRestService(
    AccountsController accountsController,
    IEventSender eventSender,
    AccountServices accountServices,
    PersistenceProviderManager persistenceProviderManager,
    ILogger<SelectPlayerRestService> logger)
{
    /// <summary>
    ///     POST endpoint for selecting a player and entering the world.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="context">Context.</param>
    /// <returns>Result.</returns>
    public Task<IResult> SelectPlayerPost([FromRoute] ulong playerId, HttpContext context)
    {
        try
        {
            var accountId = Guid.Parse(context.User.FindFirst(RestAuthentication.ClaimTypes.AccountId)?.Value ??
                                       throw new Exception("User has no AccountId claim."));

            // Verify tha the entity is a player belonging to the logged in account.
            if (!persistenceProviderManager.PersistenceProvider.GetAccountForPlayerQuery.TryGetAccountForPlayer(
                    playerId, out var trueAccountId))
            {
                logger.LogError("No account found for player {EntityId:X}. (Request ID: {Id})",
                    playerId, context.TraceIdentifier);
                return Task.FromResult(Results.BadRequest("Bad player ID."));
            }

            if (!trueAccountId.Equals(accountId))
            {
                logger.LogError(
                    "Entity {EntityId:X} is not a player assigned to account {AccountId}. (Request ID: {Id})",
                    playerId, accountId, context.TraceIdentifier);
                return Task.FromResult(Results.BadRequest("Selected player dues not belong to this account."));
            }

            // Verify that the player is not in cooldown.
            // If the player is in cooldown, they cannot log in until the next persistence synchronization completes.
            // Otherwise a player could "roll back" the state of their player to the last synchronization point by
            // logging out and back in, triggering a load from the database before the latest changes have been
            // committed.
            if (accountServices.IsPlayerInCooldown(playerId))
                return Task.FromResult(Results.Text("Player is in cooldown.", null, Encoding.UTF8, 503));

            // Select player.
            const bool newPlayer = false;
            accountsController.SelectPlayer(eventSender, accountId, playerId, newPlayer);
            return Task.FromResult(Results.Ok());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling select player request. (Request ID: {Id})",
                context.TraceIdentifier);
            return Task.FromResult(Results.InternalServerError());
        }
    }
}