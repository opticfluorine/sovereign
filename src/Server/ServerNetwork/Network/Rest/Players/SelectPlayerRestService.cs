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
using System.Text.Json;
using System.Threading.Tasks;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.Accounts.Systems.Accounts;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Players;
using WatsonWebserver.Core;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     REST service for selecting a player character for use.
/// </summary>
public class SelectPlayerRestService : AuthenticatedRestService
{
    /// <summary>
    ///     Maximum request length, in bytes.
    /// </summary>
    private const int MAX_REQUEST_LENGTH = 0;

    private readonly AccountsController accountsController;
    private readonly AccountServices accountServices;
    private readonly IEventSender eventSender;
    private readonly PersistenceProviderManager persistenceProviderManager;

    private readonly PersistencePlayerServices playerServices;

    public SelectPlayerRestService(RestAuthenticator authenticator, PersistencePlayerServices playerServices,
        AccountsController accountsController, IEventSender eventSender, AccountServices accountServices,
        PersistenceProviderManager persistenceProviderManager) :
        base(authenticator)
    {
        this.playerServices = playerServices;
        this.accountsController = accountsController;
        this.eventSender = eventSender;
        this.accountServices = accountServices;
        this.persistenceProviderManager = persistenceProviderManager;
    }

    public override string Path => RestEndpoints.Player + "/{id}";
    public override RestPathType PathType => RestPathType.Parameter;
    public override HttpMethod RequestType => HttpMethod.POST;

    protected override async Task OnAuthenticatedRequest(HttpContextBase ctx, Guid accountId)
    {
        try
        {
            // Safety check.
            if (ctx.Request.ContentLength > MAX_REQUEST_LENGTH)
            {
                await SendResponse(ctx, 413, "Request too large.");
                return;
            }

            // Decode and validate input.
            // Validation checks that the requested player character is a player
            // character belonging to the currently authenticated account.
            var idParam = ctx.Request.Url.Parameters["id"];
            ulong playerEntityId;
            try
            {
                playerEntityId = Convert.ToUInt64(idParam);
            }
            catch (FormatException)
            {
                Logger.ErrorFormat("Account {0} tried to select invalid player entity {1}.", accountId, idParam);
                await SendResponse(ctx, 400, "Invalid player entity ID.");
                return;
            }

            // Verify tha the entity is a player belonging to the logged in account.
            if (!persistenceProviderManager.PersistenceProvider.GetAccountForPlayerQuery.TryGetAccountForPlayer(
                    playerEntityId, out var trueAccountId))
            {
                Logger.ErrorFormat("No account found for player {0}.", playerEntityId);
                await SendResponse(ctx, 400, "No account found for player.");
                return;
            }

            if (!trueAccountId.Equals(accountId))
            {
                Logger.ErrorFormat("Entity {0} is not a player assigned to account {1}.", playerEntityId,
                    accountId);
                await SendResponse(ctx, 400, "Selected player dues not belong to this account.");
                return;
            }

            // Verify that the player is not in cooldown.
            // If the player is in cooldown, they cannot log in until the next persistence synchronization completes.
            // Otherwise a player could "roll back" the state of their player to the last synchronization point by
            // logging out and back in, triggering a load from the database before the latest changes have been
            // committed.
            if (accountServices.IsPlayerInCooldown(playerEntityId))
            {
                await SendResponse(ctx, 503, "Player is in cooldown.");
                return;
            }

            // Select player.
            const bool newPlayer = false;
            accountsController.SelectPlayer(eventSender, accountId, playerEntityId, newPlayer);
            await SendResponse(ctx, 200, "Success.");
        }
        catch (Exception e)
        {
            Logger.Error("Error handling select player request.", e);
            try
            {
                await SendResponse(ctx, 500, "Error processing request.");
            }
            catch (Exception e2)
            {
                Logger.Error("Error sending error response.", e2);
            }
        }
    }

    /// <summary>
    ///     Sends a response.
    /// </summary>
    /// <param name="ctx">HTTP context.</param>
    /// <param name="status">Response status code.</param>
    /// <param name="result">Human-readable result string.</param>
    private async Task SendResponse(HttpContextBase ctx, int status, string result)
    {
        ctx.Response.StatusCode = status;

        var responseData = new SelectPlayerResponse
        {
            Result = result
        };
        var responseJson = JsonSerializer.Serialize(responseData);

        await ctx.Response.Send(responseJson);
    }
}