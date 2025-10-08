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
using Sovereign.Accounts.Systems.Accounts;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.ServerNetwork.Entities.Players;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     REST service for creating new player characters.
/// </summary>
public sealed class CreatePlayerRestService
{
    private readonly AccountsController accountsController;
    private readonly IEventSender eventSender;
    private readonly ILogger<CreatePlayerRestService> logger;
    private readonly PlayerBuilder playerBuilder;


    private readonly CreatePlayerRequestValidator requestValidator;

    public CreatePlayerRestService(CreatePlayerRequestValidator requestValidator,
        AccountsController accountsController, IEventSender eventSender,
        ILogger<CreatePlayerRestService> logger, PlayerBuilder playerBuilder)
    {
        this.requestValidator = requestValidator;
        this.accountsController = accountsController;
        this.eventSender = eventSender;
        this.logger = logger;
        this.playerBuilder = playerBuilder;
    }

    /// <summary>
    ///     POST endpoint for creating players.
    /// </summary>
    /// <param name="request">Request body.</param>
    /// <param name="context">Context.</param>
    /// <returns>Result.</returns>
    public Task<IResult> CreatePlayerPost([FromBody] CreatePlayerRequest request, HttpContext context)
    {
        try
        {
            if (!requestValidator.IsValid(request))
                return Task.FromResult(Results.BadRequest("Malformed input."));

            var accountClaim = context.User.FindFirst(RestAuthentication.ClaimTypes.AccountId) ??
                               throw new Exception("No AccountId claim for user.");
            var accountId = Guid.Parse(accountClaim.Value);

            // Attempt player creation.
            if (CreatePlayer(request, accountId, out var playerId))
                return Task.FromResult(Results.Created($"{context.Request.Path}/{playerId}",
                    new CreatePlayerResponse
                    {
                        Result = "Success.",
                        PlayerId = playerId
                    }));

            return Task.FromResult(Results.Conflict("Failed."));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling create player request.");
            return Task.FromResult(Results.InternalServerError("An error occurred."));
        }
    }

    /// <summary>
    ///     Attempts to create a new player character.
    /// </summary>
    /// <param name="request">Validated request.</param>
    /// <param name="accountId">Account ID.</param>
    /// <param name="playerEntityId">Entity ID of newly created player. Only meaningful if method returns true.</param>
    /// <returns>true on success, false otherwise.</returns>
    private bool CreatePlayer(CreatePlayerRequest request, Guid accountId, out ulong playerEntityId)
    {
        var result = playerBuilder.TryCreatePlayer(request.PlayerName, accountId, out playerEntityId);

        // Select the newly created character to continue login.
        const bool newPlayer = true;
        accountsController.SelectPlayer(eventSender, accountId, playerEntityId, newPlayer);

        return result;
    }
}