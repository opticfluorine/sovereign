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
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.Persistence.Players;
using WatsonWebserver.Core;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     REST service for deleting existing player characters.
/// </summary>
public class DeletePlayerRestService : AuthenticatedRestService
{
    /// <summary>
    ///     Maximum request length in bytes.
    /// </summary>
    private const int MaxRequestLength = 128;

    private readonly PersistencePlayerServices playerServices;

    public DeletePlayerRestService(RestAuthenticator authenticator, PersistencePlayerServices playerServices,
        ILogger<DeletePlayerRestService> logger) :
        base(authenticator, logger)
    {
        this.playerServices = playerServices;
    }

    public override string Path => RestEndpoints.Player + "/{id}";
    public override RestPathType PathType => RestPathType.Parameter;
    public override HttpMethod RequestType => HttpMethod.DELETE;

    protected override async Task OnAuthenticatedRequest(HttpContextBase ctx, Guid accountId)
    {
        try
        {
            // Safety check.
            if (ctx.Request.ContentLength > MaxRequestLength)
            {
                await SendResponse(ctx, 413, "Request too large.");
                return;
            }

            // Decode and validate input.
            var idParam = ctx.Request.Url.Parameters["id"];
            ulong playerEntityId;
            try
            {
                playerEntityId = Convert.ToUInt64(idParam);
            }
            catch (FormatException)
            {
                logger.LogError("Account {0} tried to delete invalid player entity {1}.", accountId, idParam);
                await SendResponse(ctx, 400, "Invalid player entity ID.");
                return;
            }

            // Verify that the player belongs to the authenticated account.
            if (!playerServices.ValidatePlayerAccountPair(playerEntityId, accountId))
            {
                logger.LogError("Entity {0} is not a player assigned to account {1}.", playerEntityId, accountId);
                await SendResponse(ctx, 400, "Player does not belong to account.");
                return;
            }

            // Logically delete the player.
            try
            {
                playerServices.DeletePlayer(playerEntityId);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error deleting player {0}.", playerEntityId);
                await SendResponse(ctx, 500, "Error deleting player.");
                return;
            }

            // Success.
            await SendResponse(ctx, 200, "Success.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling delete player request.");
            try
            {
                await SendResponse(ctx, 500, "Error processing request.");
            }
            catch (Exception e2)
            {
                logger.LogError(e2, "Error sending error response.");
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

        var responseData = new DeletePlayerResponse
        {
            Result = result
        };
        var responseJson = JsonSerializer.Serialize(responseData);

        await ctx.Response.Send(responseJson);
    }
}