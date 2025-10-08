// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.Persistence.Players;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.ServerNetwork.Entities.Players;

/// <summary>
///     Helper class responsible for creating new player entities.
/// </summary>
public sealed class PlayerBuilder(
    PersistencePlayerServices playerServices,
    IEntityFactory entityFactory,
    IOptions<NewPlayersOptions> newPlayersOptions,
    IOptions<InventoryOptions> inventoryOptions,
    ILogger<PlayerBuilder> logger)
{
    private readonly Lock createLock = new();

    /// <summary>
    ///     Tracks which names have been used recently in order to avoid duplication.
    /// </summary>
    private readonly HashSet<string> recentNames = new();

    /// <summary>
    ///     Attempts to create a new player.
    /// </summary>
    /// <param name="playerName">Player name.</param>
    /// <param name="accountId">Account ID.</param>
    /// <param name="playerEntityId">Player entity ID. Only meaningful if method returns true.</param>
    /// <returns>true if the player was created, false otherwise.</returns>
    public bool TryCreatePlayer(string? playerName, Guid accountId, out ulong playerEntityId)
    {
        lock (createLock)
        {
            playerEntityId = 0;

            if (string.IsNullOrEmpty(playerName) ||
                recentNames.Contains(playerName) ||
                playerServices.IsPlayerNameTaken(playerName)) return false;

            // Name is available and wasn't created recently - let's take it.
            var builder = entityFactory.GetBuilder()
                .EntityType(EntityType.Player)
                .Account(accountId)
                .Name(playerName)
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

            if (newPlayersOptions.Value.AdminByDefault)
            {
                logger.LogWarning("Player {Name} defaulting to admin; edit server configuration if unintended.",
                    playerName);
                builder.Admin();
            }

            playerEntityId = builder.Build();
            AddInventorySlots(playerEntityId);

            recentNames.Add(playerName);
            return true;
        }
    }

    /// <summary>
    ///     Adds initial inventory slots for a player.
    /// </summary>
    /// <param name="playerId">Player ID.</param>
    private void AddInventorySlots(ulong playerId)
    {
        for (var i = 0; i < inventoryOptions.Value.NewPlayerDefaultSlots; i++)
        {
            var builder = entityFactory.GetBuilder();
            builder
                .EntityType(EntityType.Slot)
                .Parent(playerId)
                .Build();
        }
    }
}