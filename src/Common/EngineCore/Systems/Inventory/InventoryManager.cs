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

using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Logging;

namespace Sovereign.EngineCore.Systems.Inventory;

/// <summary>
///     Manages inventory interactions.
/// </summary>
internal sealed class InventoryManager(
    SlotIndexer slotIndexer,
    EntityHierarchyIndexer hierarchyIndexer,
    ILogger<InventoryManager> logger,
    LoggingUtil loggingUtil,
    ParentComponentCollection parents,
    KinematicsComponentCollection kinematics,
    BoundingBoxComponentCollection boundingBoxes,
    EntityTypeComponentCollection entityTypes,
    IOptions<InventoryOptions> inventoryOptions)
{
    private readonly float maxDropD2 = inventoryOptions.Value.MaxDropDistance * inventoryOptions.Value.MaxDropDistance;

    private readonly float maxPickupD2 =
        inventoryOptions.Value.MaxPickupDistance * inventoryOptions.Value.MaxPickupDistance;

    /// <summary>
    ///     Picks up the given item if possible and places it in the first empty slot in the player's inventory.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="itemId">Item entity ID.</param>
    public void PickUpItem(ulong playerId, ulong itemId)
    {
        if (!IsValidItem(playerId, itemId)) return;
        if (!IsInRangeForPickup(playerId, itemId)) return;

        DoPickupToFirstSlot(playerId, itemId);
    }

    /// <summary>
    ///     Drops the given item from the player's inventory at the player's current position.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="slotIndex">Inventory slot index.</param>
    public void DropItem(ulong playerId, int slotIndex)
    {
        if (!slotIndexer.TryGetSlotForEntity(playerId, slotIndex, out var slotEid))
        {
            logger.LogWarning("Player {Player} tried to drop nonexistent slot.", loggingUtil.FormatEntity(playerId));
            return;
        }

        // Do nothing if the slot is empty.
        if (!hierarchyIndexer.TryGetFirstDirectChild(slotEid, out var itemId)) return;

        if (!kinematics.TryGetValue(playerId, out var posVel))
        {
            logger.LogError("Player {Player} has no position to drop.", loggingUtil.FormatEntity(playerId));
            return;
        }

        DoDrop(playerId, itemId, posVel.Position);
    }

    /// <summary>
    ///     Drops an item at the given position.
    /// </summary>
    /// <param name="playerId">Player ID.</param>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="dropPosition">Requested drop position.</param>
    public void DropItemAtPosition(ulong playerId, int slotIndex, Vector3 dropPosition)
    {
        if (!slotIndexer.TryGetSlotForEntity(playerId, slotIndex, out var slotEid))
        {
            logger.LogWarning("Player {Player} tried to drop nonexistent slot.", loggingUtil.FormatEntity(playerId));
            return;
        }

        // Do nothing if the slot is empty.
        if (!hierarchyIndexer.TryGetFirstDirectChild(slotEid, out var itemId)) return;

        if (!kinematics.TryGetValue(playerId, out var posVel))
        {
            logger.LogError("Player {Player} has no position to drop.", loggingUtil.FormatEntity(playerId));
            return;
        }

        var d2 = (posVel.Position - dropPosition).LengthSquared();
        if (d2 > maxDropD2) return;
        DoDrop(playerId, itemId, dropPosition);
    }

    /// <summary>
    ///     Swaps the contents of two inventory slots.
    /// </summary>
    /// <param name="playerId">Player ID.</param>
    /// <param name="firstSlotIdx">First slot index.</param>
    /// <param name="secondSlotIdx">Second slot index.</param>
    /// <remarks>
    ///     This method is not thread-safe.
    /// </remarks>
    public void SwapItems(ulong playerId, int firstSlotIdx, int secondSlotIdx)
    {
        if (!slotIndexer.TryGetSlotForEntity(playerId, firstSlotIdx, out var firstSlotEid) ||
            !slotIndexer.TryGetSlotForEntity(playerId, secondSlotIdx, out var secondSlotEid))
        {
            logger.LogWarning("Player {Player} tried to swap invalid slot indices, ignoring.",
                loggingUtil.FormatEntity(playerId));
            return;
        }

        var hasFirst = hierarchyIndexer.TryGetFirstDirectChild(firstSlotEid, out var firstItemId);
        var hasSecond = hierarchyIndexer.TryGetFirstDirectChild(secondSlotEid, out var secondItemId);

        if (hasFirst) parents.ModifyComponent(firstItemId, ComponentOperation.Set, secondSlotEid);
        if (hasSecond) parents.ModifyComponent(secondItemId, ComponentOperation.Set, firstSlotEid);
    }

    /// <summary>
    ///     Picks up the given item and places it in the first free inventory slot.
    /// </summary>
    /// <param name="playerId">Player ID.</param>
    /// <param name="itemId">Item ID.</param>
    private void DoPickupToFirstSlot(ulong playerId, ulong itemId)
    {
        // Pick up item in first empty slot.
        if (!slotIndexer.TryFindEmptySlot(playerId, out var slotId)) return;
        kinematics.RemoveComponent(itemId);
        parents.AddComponent(itemId, slotId);
        logger.LogInformation("{Player} picked up item {Item}.", loggingUtil.FormatEntity(playerId),
            loggingUtil.FormatEntity(itemId));
    }


    /// <summary>
    ///     Processes an item drop.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="itemId">Item entity ID.</param>
    /// <param name="position">Drop position.</param>
    private void DoDrop(ulong playerId, ulong itemId, Vector3 position)
    {
        // Drop item: break link to slot, then position at same location as player.
        logger.LogInformation("{Player} dropped item {Item}.", loggingUtil.FormatEntity(playerId),
            loggingUtil.FormatEntity(itemId));
        parents.RemoveComponent(itemId);
        kinematics.AddComponent(itemId, new Kinematics { Position = position, Velocity = Vector3.Zero });
    }

    /// <summary>
    ///     Checks if an entity is a valid item.
    /// </summary>
    /// <param name="playerId">Player ID.</param>
    /// <param name="itemId">Item ID.</param>
    /// <returns>true if the entity is a valid item, false otherwise.</returns>
    private bool IsValidItem(ulong playerId, ulong itemId)
    {
        if (!entityTypes.TryGetValue(itemId, out var entityType) || entityType != EntityType.Item)
        {
            logger.LogError("{Player} tried to pick up non-item entity {Target}.",
                loggingUtil.FormatEntity(playerId), loggingUtil.FormatEntity(itemId));
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Checks if the item is within range for the player to pick up.
    /// </summary>
    /// <param name="playerId">Player ID.</param>
    /// <param name="itemId">Item ID.</param>
    /// <returns>true if in range, false otherwise.</returns>
    private bool IsInRangeForPickup(ulong playerId, ulong itemId)
    {
        if (!kinematics.TryGetValue(playerId, out var playerPosVel) ||
            !kinematics.TryGetValue(itemId, out var itemPosVel))
        {
            logger.LogError("{Player} tried to pick up {Item} but positions are missing.",
                loggingUtil.FormatEntity(playerId), loggingUtil.FormatEntity(itemId));
            return false;
        }

        // Item must be in range of player.
        var playerPos = boundingBoxes.TryGetValue(playerId, out var playerBox)
            ? playerBox.Translate(playerPosVel.Position).CenterXy
            : playerPosVel.Position;
        var itemPos = boundingBoxes.TryGetValue(itemId, out var itemBox)
            ? itemBox.Translate(itemPosVel.Position).CenterXy
            : itemPosVel.Position;
        var d2 = (itemPos - playerPos).LengthSquared();
        if (d2 > maxPickupD2) return false;
        return true;
    }
}