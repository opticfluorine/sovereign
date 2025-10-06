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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.Accounts.Systems.Accounts;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.Persistence.Players;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     REST service for creating new player characters.
/// </summary>
public sealed class CreatePlayerRestService
{
    private readonly AccountsController accountsController;

    /// <summary>
    ///     Object used as a lock to avoid name duplication due to a race condition.
    /// </summary>
    private readonly Lock creationLock = new();

    private readonly IEntityFactory entityFactory;
    private readonly IEventSender eventSender;
    private readonly InventoryOptions inventoryOptions;
    private readonly ILogger<CreatePlayerRestService> logger;
    private readonly NewPlayersOptions newPlayersOptions;
    private readonly PersistencePlayerServices playerServices;

    /// <summary>
    ///     Tracks which names have been used recently in order to avoid duplication.
    /// </summary>
    private readonly HashSet<string> recentNames = new();

    private readonly CreatePlayerRequestValidator requestValidator;

    public CreatePlayerRestService(IEntityFactory entityFactory,
        CreatePlayerRequestValidator requestValidator, PersistencePlayerServices playerServices,
        AccountsController accountsController, IEventSender eventSender, IOptions<NewPlayersOptions> newPlayersOptions,
        IOptions<InventoryOptions> inventoryOptions, ILogger<CreatePlayerRestService> logger)
    {
        this.entityFactory = entityFactory;
        this.requestValidator = requestValidator;
        this.playerServices = playerServices;
        this.accountsController = accountsController;
        this.eventSender = eventSender;
        this.logger = logger;
        this.newPlayersOptions = newPlayersOptions.Value;
        this.inventoryOptions = inventoryOptions.Value;
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
            if (!string.IsNullOrEmpty(request.PlayerName) &&
                !recentNames.Contains(request.PlayerName) &&
                !playerServices.IsPlayerNameTaken(request.PlayerName))
            {
                // Name is available and wasn't created recently - let's take it.
                var builder = entityFactory.GetBuilder()
                    .Account(accountId)
                    .Name(request.PlayerName)
                    .PlayerCharacter()
                    .Positionable(new Vector3(0.0f, 0.0f, 1.0f)) // TODO Configurable start position
                    .Drawable(Vector2.Zero)
                    .Physics()
                    .BoundingBox(new BoundingBox
                    {
                        Position = Vector3.Zero,
                        Size = Vector3.One
                    })
                    .CastShadows(new Shadow { Radius = 0.2f })
                    .AnimatedSprite(221); // TODO Configurable appearance

                if (newPlayersOptions.AdminByDefault)
                {
                    logger.LogWarning("Player {Name} defaulting to admin; edit server configuration if unintended.",
                        request.PlayerName);
                    builder.Admin();
                }

                playerEntityId = builder.Build();
                AddInventorySlots(playerEntityId);

                recentNames.Add(request.PlayerName);
                result = true;

                // Select the newly created character to continue login.
                const bool newPlayer = true;
                accountsController.SelectPlayer(eventSender, accountId, playerEntityId, newPlayer);
            }
        }

        return result;
    }

    /// <summary>
    ///     Adds initial inventory slots for a player.
    /// </summary>
    /// <param name="playerId">Player ID.</param>
    private void AddInventorySlots(ulong playerId)
    {
        for (var i = 0; i < inventoryOptions.NewPlayerDefaultSlots; i++)
        {
            var builder = entityFactory.GetBuilder();
            builder
                .EntityType(EntityType.Slot)
                .Parent(playerId)
                .Build();
        }
    }
}