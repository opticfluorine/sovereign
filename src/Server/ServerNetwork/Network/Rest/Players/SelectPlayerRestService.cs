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
using Sovereign.Accounts.Systems.Accounts;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.Persistence.Players;
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     REST service for selecting a player character for use.
/// </summary>
public class SelectPlayerRestService : AuthenticatedRestService
{
    /// <summary>
    ///     Maximum request length, in bytes.
    /// </summary>
    private const int MAX_REQUEST_LENGTH = 256;

    private readonly AccountsController accountsController;
    private readonly IEventSender eventSender;

    private readonly PersistencePlayerServices playerServices;
    private readonly SelectPlayerRequestValidator requestValidator;

    public SelectPlayerRestService(RestAuthenticator authenticator, PersistencePlayerServices playerServices,
        SelectPlayerRequestValidator requestValidator, AccountsController accountsController,
        IEventSender eventSender) :
        base(authenticator)
    {
        this.playerServices = playerServices;
        this.requestValidator = requestValidator;
        this.accountsController = accountsController;
        this.eventSender = eventSender;
    }

    public override string Path => RestEndpoints.Player + "/{id}";
    public override RestPathType PathType => RestPathType.Parameter;
    public override HttpMethod RequestType => HttpMethod.POST;

    protected override async Task OnAuthenticatedRequest(HttpContext ctx, Guid accountId)
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
            var requestJson = ctx.Request.DataAsString;
            var requestData = JsonSerializer.Deserialize<SelectPlayerRequest>(requestJson, MessageConfig.JsonOptions);
            if (!requestValidator.IsValid(requestData, accountId))
            {
                await SendResponse(ctx, 403, "Player does not belong to current account.");
                return;
            }

            // Select player.
            accountsController.SelectPlayer(eventSender, accountId, requestData.PlayerEntityId);
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
    private async Task SendResponse(HttpContext ctx, int status, string result)
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