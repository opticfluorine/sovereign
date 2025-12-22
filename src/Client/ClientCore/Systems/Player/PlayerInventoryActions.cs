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
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Inventory;

namespace Sovereign.ClientCore.Systems.Player;

/// <summary>
///     Implements inventory actions handled by PlayerSystem (e.g. picking up item under player).
/// </summary>
public sealed class PlayerInventoryActions(
    IPerspectiveServices perspectiveServices,
    ClientStateServices clientStateServices,
    BoundingBoxComponentCollection boundingBoxes,
    KinematicsComponentCollection kinematics,
    IEventSender eventSender,
    IInventoryController inventoryController,
    AnimatedSpriteUtil animatedSpriteUtil,
    ILogger<PlayerInventoryActions> logger)
{
    private const int MaxItemsChecked = 16;

    /// <summary>
    ///     Picks up an item under the player if one is present, or does nothing otherwise.
    /// </summary>
    public void PickUpItemUnder()
    {
        var itemId = SelectItemUnder();
        if (itemId == EntityConstants.NoEntity) return;
        inventoryController.PickUp(eventSender, itemId);
    }

    /// <summary>
    ///     Selects an item under the player based on overlap with the player.
    /// </summary>
    /// <returns>Item ID, or EntityConstants.NoEntity if no suitable item is found.</returns>
    private ulong SelectItemUnder()
    {
        // An item is "under" the player if it has the same z and overlapping x, y extents.
        // Items with physics use BoundingBox to determine their extent.
        // Items without physics use the sprite size for extent.

        if (!clientStateServices.TryGetSelectedPlayer(out var playerId) ||
            !kinematics.TryGetValue(playerId, out var playerPosVel) ||
            !boundingBoxes.TryGetValue(playerId, out var playerBox))
        {
            logger.LogError("Player in invalid state.");
            return EntityConstants.NoEntity;
        }

        playerBox = playerBox.Translate(playerPosVel.Position);

        Span<ulong> items = stackalloc ulong[MaxItemsChecked];
        var count = perspectiveServices.GetItemsUnderPlayer(items);
        var maxOverlap = 0.0f;
        var maxOverlapId = EntityConstants.NoEntity;
        for (var i = 0; i < count; ++i)
        {
            var overlap = DetermineOverlap(items[i], playerBox);
            if (overlap > maxOverlap)
            {
                maxOverlap = overlap;
                maxOverlapId = items[i];
            }
        }

        return maxOverlapId;
    }

    /// <summary>
    ///     Calculates the (positive) overlap between an item and the player, if any.
    /// </summary>
    /// <param name="itemId">Item ID.</param>
    /// <param name="playerBox">Player box at player origin.</param>
    /// <returns>Overlap, or 0.0f if there is no overlap.</returns>
    private float DetermineOverlap(ulong itemId, BoundingBox playerBox)
    {
        if (!kinematics.TryGetValue(itemId, out var itemPosVel))
        {
            logger.LogError("Item ID {Id:X} has no position data.", itemId);
            return 0.0f;
        }

        // Get a bounding box for the item to check overlap.
        if (!boundingBoxes.TryGetValue(itemId, out var itemBox))
            itemBox = animatedSpriteUtil.MakeXyBoundingBoxForEntity(itemId);
        itemBox = itemBox.Translate(itemPosVel.Position);

        return playerBox.OverlapXy(itemBox);
    }
}