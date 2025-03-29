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
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Systems.Accounts;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.Persistence.Players;
using Sovereign.ServerCore.Configuration;
using WatsonWebserver.Core;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     REST service for creating new player characters.
/// </summary>
public class CreatePlayerRestService : AuthenticatedRestService
{
    /// <summary>
    ///     Maximum request length, in bytes.
    /// </summary>
    private const int MaxRequestLength = 1024;

    private readonly AccountsController accountsController;
    private readonly IServerConfigurationManager configManager;

    /// <summary>
    ///     Object used as a lock to avoid name duplication due to a race condition.
    /// </summary>
    private readonly object creationLock = new();

    private readonly IEntityFactory entityFactory;
    private readonly IEventSender eventSender;
    private readonly PersistencePlayerServices playerServices;

    /// <summary>
    ///     Tracks which names have been used recently in order to avoid duplication.
    /// </summary>
    private readonly HashSet<string> recentNames = new();

    private readonly CreatePlayerRequestValidator requestValidator;

    public CreatePlayerRestService(RestAuthenticator authenticator, IEntityFactory entityFactory,
        CreatePlayerRequestValidator requestValidator, PersistencePlayerServices playerServices,
        AccountsController accountsController, IEventSender eventSender, IServerConfigurationManager configManager,
        ILogger<CreatePlayerRestService> logger)
        : base(authenticator, logger)
    {
        this.entityFactory = entityFactory;
        this.requestValidator = requestValidator;
        this.playerServices = playerServices;
        this.accountsController = accountsController;
        this.eventSender = eventSender;
        this.configManager = configManager;
    }

    public override string Path => RestEndpoints.Player;
    public override RestPathType PathType => RestPathType.Static;
    public override HttpMethod RequestType => HttpMethod.POST;

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
            var requestJson = ctx.Request.DataAsString;
            var requestData = JsonSerializer.Deserialize<CreatePlayerRequest>(requestJson,
                MessageConfig.JsonOptions);
            if (!requestValidator.IsValid(requestData))
            {
                await SendResponse(ctx, 400, "Incomplete input.");
                return;
            }

            // Attempt player creation.
            var result = CreatePlayer(requestData, accountId, out var playerId);
            if (result)
                await SendResponse(ctx, 201, "Success.", playerId);
            else
                await SendResponse(ctx, 409, "Failed.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling create player request.");
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
    /// <param name="result">Human-readable string describing the result.</param>
    /// <param name="playerId">Entity ID of newly created player. Only relevant for successful requests.</param>
    private async Task SendResponse(HttpContextBase ctx, int status, string result, ulong playerId = 0)
    {
        ctx.Response.StatusCode = status;

        var responseData = new CreatePlayerResponse
        {
            Result = result,
            PlayerId = playerId
        };
        var responseJson = JsonSerializer.Serialize(responseData);

        await ctx.Response.Send(Encoding.UTF8.GetBytes(responseJson));
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
        // Lock to avoid creating new player characters in parallel.
        // The persistence scheme for the entity-component-system implementation forces a tradeoff
        // between good component design and the ability to enforce a unique player name constraint
        // in the database. To enforce a unique player name constraint, the player name component
        // would have to be its own component separate from Name - not ideal. Instead we opt to
        // not enforce a constraint in the database, and instead use a mutex to ensure that we don't
        // have a race condition leading to the creation of multiple player characters with the smae
        // name.
        var result = false;
        playerEntityId = 0;

        lock (creationLock)
        {
            if (request.PlayerName != null && !recentNames.Contains(request.PlayerName) &&
                !playerServices.IsPlayerNameTaken(request.PlayerName))
            {
                // Name is available and wasn't created recently - let's take it.
                var builder = entityFactory.GetBuilder()
                    .Account(accountId)
                    .Name(request.PlayerName)
                    .PlayerCharacter()
                    .Positionable(new Vector3(0.0f, 0.0f, 1.0f)) // TODO Configurable start position
                    .Drawable()
                    .Physics()
                    .BoundingBox(new BoundingBox
                    {
                        Position = Vector3.Zero,
                        Size = Vector3.One
                    })
                    .AnimatedSprite(221); // TODO Configurable appearance

                if (configManager.ServerConfiguration.NewPlayers.AdminByDefault)
                {
                    logger.LogWarning("Player {Name} defaulting to admin; edit server configuration if unintended.",
                        request.PlayerName);
                    builder.Admin();
                }

                playerEntityId = builder.Build();

                recentNames.Add(request.PlayerName);
                result = true;

                // Select the newly created character to continue login.
                const bool newPlayer = true;
                accountsController.SelectPlayer(eventSender, accountId, playerEntityId, newPlayer);
            }
        }

        return result;
    }
}