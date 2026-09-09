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
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Systems.Scripting;
using Sovereign.EngineCore.Systems.WorldManagement;

namespace Sovereign.EngineCore.Systems.Inventory;

/// <summary>
///     Manages inventory interactions.
/// </summary>
public sealed class InventoryManager(
    SlotIndexer slotIndexer,
    EntityHierarchyIndexer hierarchyIndexer,
    ILogger<InventoryManager> logger,
    LoggingUtil loggingUtil,
    ParentComponentCollection parents,
    KinematicsComponentCollection kinematics,
    BoundingBoxComponentCollection boundingBoxes,
    EntityTypeComponentCollection entityTypes,
    IOptions<InventoryOptions> inventoryOptions,
    IEntityFactory entityFactory,
    EntityManager entityManager,
    WorldManagementController worldManagementController,
    StackableTagCollection stackable,
    EntityTable entityTable,
    QuantityComponentCollection quantities,
    UseRangeComponentCollection useRanges,
    IEngineConfiguration engineConfiguration,
    IEventSender eventSender,
    InventoryPermissionService permissionService,
    IScriptingController scriptingController)
{
    private readonly List<(int slotIndex, ulong itemId, uint qty)> consumeCandidates = new();
    private readonly float maxDropD2 = inventoryOptions.Value.MaxDropDistance * inventoryOptions.Value.MaxDropDistance;

    private readonly float maxPickupD2 =
        inventoryOptions.Value.MaxPickupDistance * inventoryOptions.Value.MaxPickupDistance;

    private readonly float maxAccessedInvD2 =
        inventoryOptions.Value.MaxAccessedInventoryDistance *
        inventoryOptions.Value.MaxAccessedInventoryDistance;

    /// <summary>
    ///     Items modified in some way during the current tick. Used to prevent item duplication exploits and other bugs that
    ///     would take advantage of the delay in updating components until the end of a tick.
    /// </summary>
    private readonly HashSet<ulong> modifiedItems = new();

    /// <summary>
    ///     Synchronizes all inventory mutations across executor threads so that
    ///     scan+claim+remove is atomic and <see cref="modifiedItems" /> is never
    ///     accessed concurrently.
    /// </summary>
    private readonly Lock mutationLock = new();

    /// <summary>
    ///     Known pending changes indexed by (entityId, slotIndex).
    /// </summary>
    private readonly Dictionary<(ulong, int), (ulong, uint)> pendingChanges = new();

    /// <summary>
    ///     Called once per tick.
    /// </summary>
    public void OnTick()
    {
        lock (mutationLock)
        {
            pendingChanges.Clear();
            modifiedItems.Clear();
        }
    }

    /// <summary>
    ///     Picks up the given item if possible and places it in the first empty slot in the player's inventory.
    /// </summary>
    /// <param name="entityId">Owner entity ID.</param>
    /// <param name="itemId">Item entity ID.</param>
    public void PickUpItem(ulong entityId, ulong itemId)
    {
        lock (mutationLock)
        {
            if (modifiedItems.Contains(itemId) || !IsPickUpAllowed(entityId, itemId)) return;

            if (stackable.HasTagForEntity(itemId))
            {
                // Stackable items should be merged with an existing stack if possible.
                DoPickupStackable(entityId, itemId);
            }
            else
            {
                // Non-stackable items always go to an empty slot.
                DoPickupToFirstSlot(entityId, itemId);
            }
        }
    }

    /// <summary>
    ///     Determines whether the given entity is allowed to pick up the given item.
    /// </summary>
    /// <param name="entityId">Entity ID of the entity that is picking up.</param>
    /// <param name="itemId">Item entity ID.</param>
    /// <returns>true if allowed, false otherwise.</returns>
    public bool IsPickUpAllowed(ulong entityId, ulong itemId)
    {
        return IsValidItem(entityId, itemId) && IsInRangeForPickup(entityId, itemId);
    }

    /// <summary>
    ///     Drops the given item from the player's inventory at the player's current position.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="slotIndex">Inventory slot index.</param>
    public void DropItem(ulong playerId, int slotIndex)
    {
        lock (mutationLock)
        {
            if (!slotIndexer.TryGetSlotForEntity(playerId, slotIndex, out var slotEid))
            {
                logger.LogWarning("Player {Player} tried to drop nonexistent slot.",
                    loggingUtil.FormatEntity(playerId));
                return;
            }

            // Do nothing if the slot is empty.
            if (!hierarchyIndexer.TryGetFirstDirectChild(slotEid, out var itemId) ||
                modifiedItems.Contains(itemId)) return;

            if (!kinematics.TryGetValue(playerId, out var posVel))
            {
                logger.LogError("Player {Player} has no position to drop.", loggingUtil.FormatEntity(playerId));
                return;
            }

            DoDrop(playerId, itemId, 0, posVel.Position);
        }
    }

    /// <summary>
    ///     Drops an item at the given position.
    /// </summary>
    /// <param name="playerId">Player ID.</param>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="quantity">Quantity.</param>
    /// <param name="dropPosition">Requested drop position.</param>
    public void DropItemAtPosition(ulong playerId, int slotIndex, uint quantity, Vector3 dropPosition)
    {
        lock (mutationLock)
        {
            if (!slotIndexer.TryGetSlotForEntity(playerId, slotIndex, out var slotEid))
            {
                logger.LogWarning("Player {Player} tried to drop nonexistent slot.",
                    loggingUtil.FormatEntity(playerId));
                return;
            }

            // Do nothing if the slot is empty.
            if (!hierarchyIndexer.TryGetFirstDirectChild(slotEid, out var itemId) ||
                modifiedItems.Contains(itemId)) return;

            if (!kinematics.TryGetValue(playerId, out var posVel))
            {
                logger.LogError("Player {Player} has no position to drop.", loggingUtil.FormatEntity(playerId));
                return;
            }

            var d2 = (posVel.Position - dropPosition).LengthSquared();
            if (d2 > maxDropD2) return;
            DoDrop(playerId, itemId, quantity, dropPosition);
        }
    }

    /// <summary>
    ///     Swaps part or all of two slots, which may belong to different inventories, on behalf of
    ///     an actor. The actor must be permitted to modify both affected inventories, and any
    ///     inventory not owned by the actor must be within range of the actor.
    /// </summary>
    /// <param name="actorId">Actor entity ID performing the swap.</param>
    /// <param name="inventory0Id">Entity ID that owns the inventory containing the source slot.</param>
    /// <param name="firstSlotIdx">Source slot index.</param>
    /// <param name="inventory1Id">Entity ID that owns the inventory containing the destination slot.</param>
    /// <param name="secondSlotIdx">Destination slot index.</param>
    /// <param name="quantity">Quantity to move from source to destination; 0 moves the entire stack.</param>
    public void SwapItemsAsPlayer(ulong actorId, ulong inventory0Id, int firstSlotIdx, ulong inventory1Id,
        int secondSlotIdx, uint quantity)
    {
        lock (mutationLock)
        {
            if (!permissionService.IsSwapPermitted(actorId, inventory0Id, inventory1Id))
            {
                logger.LogWarning("[SECURITY] Player {Actor} tried to modify inventory for entity IDs {Inv0Id:X} and {Inv1Id:X}.",
                    loggingUtil.FormatEntity(actorId), inventory0Id, inventory1Id);
                return;
            }

            // Inventories owned by another entity must be within range of the actor.
            // Under the current placeholder permission this range check is effectively
            // a no-op (only the actor's own inventory is accessible). It becomes
            // meaningful once permission logic permits access to other inventories.
            if (inventory0Id != actorId && !IsInRangeForInventoryAccess(actorId, inventory0Id)) return;
            if (inventory1Id != actorId && !IsInRangeForInventoryAccess(actorId, inventory1Id)) return;

            SwapItems(inventory0Id, firstSlotIdx, inventory1Id, secondSlotIdx, quantity);
        }
    }

    /// <summary>
    ///     Uses an item from the player's hotbar as a tool on a target entity. If the use is valid on an
    ///     authoritative engine, the target entity's interaction callback is invoked with the tool entity ID
    ///     as the first argument.
    /// </summary>
    /// <param name="playerId">Player entity ID performing the use.</param>
    /// <param name="toolItemId">Item entity ID to use as a tool.</param>
    /// <param name="targetEntityId">Entity ID of the target.</param>
    public void UseItem(ulong playerId, ulong toolItemId, ulong targetEntityId)
    {
        lock (mutationLock)
        {
            if (!IsToolInHotbar(playerId, toolItemId) ||
                !useRanges.TryGetValue(toolItemId, out var useRange) ||
                !TryGetCenterDistanceSq(playerId, targetEntityId, out var d2) ||
                d2 > useRange * useRange)
            {
                if (engineConfiguration.IsAuthoritative)
                    logger.LogWarning(
                        "[SECURITY] Player {Actor} tried to use item {Tool} on target {Target}.",
                        loggingUtil.FormatEntity(playerId), loggingUtil.FormatEntity(toolItemId),
                        loggingUtil.FormatEntity(targetEntityId));
                return;
            }

            if (engineConfiguration.IsAuthoritative)
                scriptingController.InvokeInteractCallback(eventSender, toolItemId, targetEntityId);
        }
    }

    /// <summary>
    ///     Determines whether the given item is held in one of the player's hotbar slots.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="toolItemId">Item entity ID.</param>
    /// <returns>true if the item is in a hotbar slot, false otherwise.</returns>
    private bool IsToolInHotbar(ulong playerId, ulong toolItemId)
    {
        for (var i = 0; i < InventoryConstants.HotbarSlotCount; ++i)
        {
            if (!slotIndexer.TryGetSlotForEntity(playerId, i, out var slotEid)) continue;
            if (!hierarchyIndexer.TryGetFirstDirectChild(slotEid, out var itemId)) continue;
            if (itemId == toolItemId) return true;
        }

        return false;
    }

    /// <summary>
    ///     Adds slots to an entity's inventory.
    /// </summary>
    /// <param name="entityId">Entity ID that will own the new slots.</param>
    /// <param name="slotCount">Number of new slots to add (> 0).</param>
    public void AddSlots(ulong entityId, int slotCount)
    {
        lock (mutationLock)
        {
            if (slotCount < 1)
            {
                logger.LogError("Bad request to add {Slots} slots to {Entity} ({EntityId:X}).", slotCount,
                    loggingUtil.FormatEntity(entityId), entityId);
                return;
            }

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Add {SlotCount} slots to {Entity} ({EntityId:X}).",
                    slotCount, loggingUtil.FormatEntity(entityId), entityId);

            for (var i = 0; i < slotCount; ++i)
            {
                var slotId = entityFactory.GetBuilder()
                    .EntityType(EntityType.Slot)
                    .Parent(entityId)
                    .Build();

                if (logger.IsEnabled(LogLevel.Trace))
                    logger.LogTrace("Add slot {SlotId:X} to entity {EntityId:X}.", slotId, entityId);
            }
        }
    }

    /// <summary>
    ///     Removes the item in the given inventory slot.
    /// </summary>
    /// <param name="entityId">Entity ID that owns the inventory..</param>
    /// <param name="slotIndex">Slot index.</param>
    public void RemoveItem(ulong entityId, int slotIndex)
    {
        lock (mutationLock)
        {
            RemoveItemCore(entityId, slotIndex);
        }
    }

    /// <summary>
    ///     Lock-free core of <see cref="RemoveItem" />. Caller must hold
    ///     <see cref="mutationLock" />.
    /// </summary>
    /// <param name="entityId">Entity ID that owns the inventory.</param>
    /// <param name="slotIndex">Slot index.</param>
    private void RemoveItemCore(ulong entityId, int slotIndex)
    {
        if (!slotIndexer.TryGetSlotForEntity(entityId, slotIndex, out var slotId))
        {
            logger.LogError("RemoveItem for bad slot index {SlotIndex} on entity {EntityId:X}.", slotIndex, entityId);
            return;
        }

        if (!hierarchyIndexer.TryGetFirstDirectChild(slotId, out var itemId))
        {
            logger.LogError("RemoveItem for empty slot index {SlotIndex} on entity {EntityId:X}.", slotIndex, entityId);
            return;
        }

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Remove item ID {ItemId:X} from entity {EntityId:X}.", itemId, entityId);

        entityManager.RemoveEntity(itemId);
        modifiedItems.Add(itemId);
    }

    /// <summary>
    ///     Gets the item in the given slot index.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="slotIndex">Slot index.</param>
    /// <returns>Item ID, or 0 if the slot is empty or does not exist.</returns>
    public ulong GetItem(ulong entityId, int slotIndex)
    {
        if (!engineConfiguration.IsAuthoritative &&
            pendingChanges.TryGetValue((entityId, slotIndex), out var pendingChange))
        {
            return pendingChange.Item1;
        }

        if (!slotIndexer.TryGetSlotForEntity(entityId, slotIndex, out var slotId)) return 0;
        return hierarchyIndexer.TryGetFirstDirectChild(slotId, out var itemId) ? itemId : 0;
    }

    /// <summary>
    ///     Gets the quantity of the item in a given slot.
    /// </summary>
    /// <param name="entityId">Inventory entity ID.</param>
    /// <param name="slotIndex">Slot index.</param>
    /// <returns>Quantity.</returns>
    public uint GetQuantity(ulong entityId, int slotIndex)
    {
        if (!engineConfiguration.IsAuthoritative &&
            pendingChanges.TryGetValue((entityId, slotIndex), out var pendingChange))
        {
            return pendingChange.Item2;
        }

        var itemId = GetItem(entityId, slotIndex);
        if (itemId == 0) return 0;
        return quantities.TryGetValue(itemId, out var qty) ? qty : 1;
    }

    /// <summary>
    ///     Finds the first item (in slot order) in the inventory with the given template ID.
    /// </summary>
    /// <param name="entityId">Entity ID whose inventory should be searched..</param>
    /// <param name="itemTemplateId">Template ID to search for.</param>
    /// <returns>Entity ID of the first matching item, or 0 if no match was found.</returns>
    public ulong FindFirstMatchingItem(ulong entityId, ulong itemTemplateId)
    {
        var slotCount = slotIndexer.GetSlotCountForEntity(entityId);
        for (var i = 0; i < slotCount; ++i)
        {
            var itemId = GetItem(entityId, i);
            if (itemId == 0) continue;
            if (entityTable.TryGetTemplate(itemId, out var templateId) && templateId == itemTemplateId)
                return itemId;
        }

        return 0;
    }

    /// <summary>
    ///     Checks if the inventory entity is within range of the actor for access (e.g. a swap).
    /// </summary>
    /// <param name="actorId">Actor entity ID.</param>
    /// <param name="inventoryEntityId">Entity ID that owns the inventory.</param>
    /// <returns>true if in range, false otherwise.</returns>
    public bool IsInRangeForInventoryAccess(ulong actorId, ulong inventoryEntityId)
    {
        if (!TryGetCenterDistanceSq(actorId, inventoryEntityId, out var d2))
        {
            logger.LogError("{Actor} tried to access inventory {Inventory} but positions are missing.",
                loggingUtil.FormatEntity(actorId), loggingUtil.FormatEntity(inventoryEntityId));
            return false;
        }

        return d2 <= maxAccessedInvD2;
    }

    /// <summary>
    ///     Picks up the given item and places it in the first free inventory slot.
    /// </summary>
    /// <param name="entityId">Owner ID.</param>
    /// <param name="itemId">Item ID.</param>
    private void DoPickupToFirstSlot(ulong entityId, ulong itemId)
    {
        // Pick up item in first empty slot.
        if (!slotIndexer.TryFindEmptySlot(entityId, out var slotId, out _)) return;
        kinematics.RemoveComponent(itemId);
        parents.AddComponent(itemId, slotId);
        modifiedItems.Add(itemId);

        worldManagementController.ResyncEntityTree(eventSender, slotId);

        logger.LogInformation("{Player} picked up item {Item} ({Id:X}).", loggingUtil.FormatEntity(entityId),
            loggingUtil.FormatEntity(itemId), itemId);
    }

    /// <summary>
    ///     Picks up a stackable item and attempts to merge it into an existing stack if possible.
    /// </summary>
    /// <param name="entityId">Owner ID.</param>
    /// <param name="itemId">Item ID.</param>
    private void DoPickupStackable(ulong entityId, ulong itemId)
    {
        if (!entityTable.TryGetTemplate(itemId, out var templateId))
        {
            // Stackable items without a template can't be merged, so pick up to a new stack.
            DoPickupToFirstSlot(entityId, itemId);
            return;
        }

        // Search for a matching stack.
        var stackId = FindFirstMatchingItem(entityId, templateId);
        if (stackId == 0)
        {
            // No matching stack found, try to pick up item to an empty slot instead.
            DoPickupToFirstSlot(entityId, itemId);
            return;
        }

        // We found a matching stack in the inventory - merge the stacks.
        if (!quantities.TryGetValue(itemId, out var addedQty)) addedQty = 1;
        if (!quantities.HasComponentForEntity(stackId))
            quantities.AddComponent(stackId, 1 + addedQty);
        else
            quantities.ModifyComponent(stackId, ComponentOperation.AddNoOverflow, addedQty);

        logger.LogInformation("{Player} picked up item {Item} ({Id:X}).", loggingUtil.FormatEntity(entityId),
            loggingUtil.FormatEntity(itemId), itemId);
        entityManager.RemoveEntity(itemId);

        modifiedItems.Add(itemId);
        modifiedItems.Add(stackId);

        worldManagementController.ResyncEntity(eventSender, stackId);
    }

    /// <summary>
    ///     Processes an item drop.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="itemId">Item entity ID.</param>
    /// <param name="quantity">Quantity to drop.</param>
    /// <param name="position">Drop position.</param>
    private void DoDrop(ulong playerId, ulong itemId, uint quantity, Vector3 position)
    {
        // Determine drop quantity.
        if (!quantities.TryGetValue(itemId, out var totalQty)) totalQty = 1;
        if (quantity == 0 || quantity > totalQty) quantity = totalQty;

        logger.LogInformation("{Player} dropped {Qty} {Item}.", quantity, loggingUtil.FormatEntity(playerId),
            loggingUtil.FormatEntity(itemId));

        if (quantity == totalQty)
        {
            // Drop full stack of items: break link to slot, then position at same location as player.
            parents.RemoveComponent(itemId);
            kinematics.AddComponent(itemId, new Kinematics { Position = position, Velocity = Vector3.Zero });
            modifiedItems.Add(itemId);
            worldManagementController.ResyncEntityTree(eventSender, itemId);
        }
        else
        {
            // Drop partial stack of items: spawn a new item at the drop position and rebalance the quantities.
            if (!entityTable.TryGetTemplate(itemId, out var templateId))
            {
                logger.LogError("Tried to drop partial stack of entity ID {Id:X} with no template.", itemId);
                return;
            }

            var newId = entityFactory.GetBuilder()
                .Template(templateId)
                .Quantity(quantity)
                .Positionable(position)
                .Build();
            quantities.ModifyComponent(itemId, ComponentOperation.SubtractNoUnderflow, quantity);
            modifiedItems.Add(itemId);
            modifiedItems.Add(newId);
            worldManagementController.ResyncEntityTree(eventSender, itemId);
            worldManagementController.ResyncEntityTree(eventSender, newId);
        }
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
    ///     Gets the distance squared between player and item in the XY plane.
    /// </summary>
    /// <param name="firstEntityId">First entity ID.</param>
    /// <param name="secondEntityId">Second entity ID.</param>
    /// <param name="d2">Squared xy distance, or 0 if it could not be computed.</param>
    /// <returns>true if the distance was computed, false otherwise.</returns>
    private bool TryGetCenterDistanceSq(ulong firstEntityId, ulong secondEntityId, out float d2)
    {
        d2 = 0.0f;
        if (!kinematics.TryGetValue(firstEntityId, out var firstPosVel) ||
            !kinematics.TryGetValue(secondEntityId, out var secondPosVel))
        {
            return false;
        }

        var firstPos = boundingBoxes.TryGetValue(firstEntityId, out var firstBox)
            ? firstBox.Translate(firstPosVel.Position).CenterXy
            : firstPosVel.Position;
        var secondPos = boundingBoxes.TryGetValue(secondEntityId, out var secondBox)
            ? secondBox.Translate(secondPosVel.Position).CenterXy
            : secondPosVel.Position;
        d2 = (secondPos - firstPos).LengthSquared();
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
        if (!TryGetCenterDistanceSq(playerId, itemId, out var d2))
        {
            logger.LogError("{Player} tried to pick up {Item} but positions are missing.",
                loggingUtil.FormatEntity(playerId), loggingUtil.FormatEntity(itemId));
            return false;
        }

        return d2 <= maxPickupD2;
    }

    /// <summary>
    ///     Lock-free helper that subtracts a quantity from a stack and resyncs.
    ///     Caller must hold <see cref="mutationLock" />.
    /// </summary>
    /// <param name="entityId">Inventory owner entity ID.</param>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="quantity">Quantity to subtract.</param>
    private void RemoveQuantityCore(ulong entityId, int slotIndex, uint quantity)
    {
        if (!slotIndexer.TryGetSlotForEntity(entityId, slotIndex, out var slotId))
        {
            logger.LogError("RemoveQuantityCore for bad slot index {SlotIndex} on entity {EntityId:X}.", slotIndex,
                entityId);
            return;
        }

        if (!hierarchyIndexer.TryGetFirstDirectChild(slotId, out var itemId))
        {
            logger.LogError("RemoveQuantityCore for empty slot index {SlotIndex} on entity {EntityId:X}.", slotIndex,
                entityId);
            return;
        }

        if (modifiedItems.Contains(itemId)) return;

        quantities.ModifyComponent(itemId, ComponentOperation.SubtractNoUnderflow, quantity);
        modifiedItems.Add(itemId);
        worldManagementController.ResyncEntity(eventSender, itemId);
    }

    /// <summary>
    ///     Synchronously searches for an item with the given template ID in the
    ///     entity's inventory. If found, removes (destroys) it and returns true;
    ///     otherwise returns false.
    /// </summary>
    /// <param name="entityId">Inventory owner entity ID.</param>
    /// <param name="templateId">Item template entity ID.</param>
    /// <returns>true if an item was found and removed; false otherwise.</returns>
    public bool TryConsumeItem(ulong entityId, ulong templateId)
    {
        lock (mutationLock)
        {
            var slotCount = slotIndexer.GetSlotCountForEntity(entityId);
            if (slotCount == 0) return false;

            for (var i = 0; i < slotCount; ++i)
            {
                if (!slotIndexer.TryGetSlotForEntity(entityId, i, out var slotId)) continue;
                if (!hierarchyIndexer.TryGetFirstDirectChild(slotId, out var itemId)) continue;
                if (modifiedItems.Contains(itemId)) continue;
                if (!entityTable.TryGetTemplate(itemId, out var tmpl) || tmpl != templateId) continue;

                modifiedItems.Add(itemId);
                RemoveItemCore(entityId, i);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    ///     Synchronously searches for enough items with the given template ID to
    ///     satisfy the requested quantity. If sufficient items are present, the
    ///     required quantity is removed (across one or more stacks) and true is
    ///     returned; otherwise nothing is changed and false is returned.
    /// </summary>
    /// <param name="entityId">Inventory owner entity ID.</param>
    /// <param name="templateId">Item template entity ID.</param>
    /// <param name="quantity">Required quantity.</param>
    /// <returns>true if the quantity was removed; false if insufficient.</returns>
    public bool TryConsumeQuantity(ulong entityId, ulong templateId, uint quantity)
    {
        lock (mutationLock)
        {
            if (quantity == 0) return false;

            var slotCount = slotIndexer.GetSlotCountForEntity(entityId);
            if (slotCount == 0) return false;

            // Plan pass: collect matching candidates and tally total.
            uint total = 0;
            consumeCandidates.Clear();

            for (var i = 0; i < slotCount; ++i)
            {
                if (!slotIndexer.TryGetSlotForEntity(entityId, i, out var slotId)) continue;
                if (!hierarchyIndexer.TryGetFirstDirectChild(slotId, out var itemId)) continue;
                if (modifiedItems.Contains(itemId)) continue;
                if (!entityTable.TryGetTemplate(itemId, out var tmpl) || tmpl != templateId) continue;

                var qty = quantities.TryGetValue(itemId, out var q) ? q : 1u;
                consumeCandidates.Add((i, itemId, qty));
                total += qty;
            }

            if (total < quantity) return false;

            // Issue pass: remove exactly the required quantity.
            var remaining = quantity;
            foreach (var (slotIndex, itemId, qty) in consumeCandidates)
            {
                var take = Math.Min(qty, remaining);
                modifiedItems.Add(itemId);

                if (take == qty)
                    RemoveItemCore(entityId, slotIndex);
                else
                    RemoveQuantityCore(entityId, slotIndex, take);

                remaining -= take;
                if (remaining == 0) break;
            }

            return true;
        }
    }

    #region Swap

    //
    // Swap is a very general operation with a number of different cases. The cases that need to be considered:
    //
    //  - Case 1: Swap entire stack with an empty slot
    //    The item stack moves from its current slot to the empty slot, and its current slot becomes empty
    //    Valid for any item paired with any empty slot
    //  - Case 2: Swap two entire stacks (not mutually stackable) with each other
    //    The two stacks exchange slots fully, and quantities are unchanged
    //    Valid for any pair of items
    //  - Case 3: Swap entire stack with another mutually stackable item
    //    The entire quantity of both stacks are combined into the second slot, leaving the first slot empty
    //    Valid for any pair of mutually stackable items
    //  - Case 4: Swap part of one stack with an empty slot
    //    The stack splits into two entities, and the new entity is placed in the empty slot
    //    Valid for any stack with quantity > 1 paired with any empty slot
    //  - Case 5: Swap part of one stack with a mutually stackable item in another slot
    //    A quantity is removed from the first stack and added to the second stack
    //    Valid for any pair of mutually stackable items
    //
    // And the tricky case:
    //
    //  - Case 6: Swap part of one stack with another stack (not mutually stackable)
    //    The first stack splits across both slots. The item in the second slot moves to the next available
    //    free slot. If there is no free slot, the entire operation has no effect.
    //
    // Why does this last case have special behavior? Because the player needs to be able to select part of a stack
    // and then swap it with a filled slot, simultaneously selecting the item in that slot and being able to place
    // it elsewhere. Tricky because while the other cases involve the two slots only, this one depends on a third
    // slot which must be empty and is not guaranteed to exist.
    //
    // The next few methods implement the general Swap operation on inventories.
    //

    /// <summary>
    ///     Swaps the contents of two inventory slots, which may belong to different inventories.
    /// </summary>
    /// <param name="inventory0Id">Inventory owner ID of the source slot.</param>
    /// <param name="firstSlotIdx">First slot index.</param>
    /// <param name="inventory1Id">Inventory owner ID of the destination slot.</param>
    /// <param name="secondSlotIdx">Second slot index.</param>
    /// <param name="quantity">Quantity to move from first slot to second; 0 moves the entire stack.</param>
    private void SwapItems(ulong inventory0Id, int firstSlotIdx, ulong inventory1Id, int secondSlotIdx, uint quantity)
    {
        if (!slotIndexer.TryGetSlotForEntity(inventory0Id, firstSlotIdx, out var firstSlotEid) ||
            !slotIndexer.TryGetSlotForEntity(inventory1Id, secondSlotIdx, out var secondSlotEid))
        {
            logger.LogWarning("Tried to swap invalid slot indices on inventories {Inv0Id:X} and {Inv1Id:X}, ignoring.",
                inventory0Id, inventory1Id);
            return;
        }

        var item0 = GetItem(inventory0Id, firstSlotIdx);
        var item1 = GetItem(inventory1Id, secondSlotIdx);
        if (item0 == 0 || modifiedItems.Contains(item0)) return; // first slot must not be empty
        if (!entityTable.TryGetTemplate(item0, out var template0))
        {
            logger.LogError("Tried to swap item {Item} without a template.", loggingUtil.FormatEntity(item0));
            return;
        }

        if (item1 > 0 && modifiedItems.Contains(item1)) return;
        modifiedItems.Add(item0);
        if (item1 > 0) modifiedItems.Add(item1);

        // Get quantities and clamp the requested swap quantity if needed.
        if (!quantities.TryGetValue(item0, out var qty0)) qty0 = 1u;
        if (quantity == 0 || quantity > qty0) quantity = qty0;

        if (item1 == 0)
        {
            SwapToEmpty(item0, qty0, template0, quantity, secondSlotEid, inventory0Id, inventory1Id, firstSlotIdx,
                secondSlotIdx);
            return;
        }

        // Both slots contain items.
        if (entityTable.TryGetTemplate(item1, out var template1) && template0 == template1)
        {
            SwapMutuallyStackable(item0, item1, qty0, quantity, inventory0Id, inventory1Id, firstSlotIdx,
                secondSlotIdx);
            return;
        }

        // Items are not mutually stackable.
        if (quantity == qty0)
        {
            // Case 2, swap entire stack with non-mutually stackable
            SwapFullNonMutuallyStackable(item0, item1, firstSlotEid, secondSlotEid, inventory0Id, inventory1Id,
                firstSlotIdx, secondSlotIdx);
            return;
        }

        // Finally, if we get here, we have Case 6 and need to move the second item to another empty slot.
        SwapPartialNonMutuallyStackable(inventory0Id, inventory1Id, item0, item1, quantity, template0, secondSlotEid,
            qty0, firstSlotIdx, secondSlotIdx);
    }

    private void SwapToEmpty(ulong item0, uint qty0, ulong template0, uint quantity, ulong secondSlotEid,
        ulong inventory0Id, ulong inventory1Id, int fromSlotIndex, int toSlotIndex)
    {
        if (quantity == qty0)
        {
            // Case 1, swap entire stack with empty slot
            parents.AddOrUpdateComponent(item0, secondSlotEid);
            pendingChanges[(inventory0Id, fromSlotIndex)] = (0, 0);
            pendingChanges[(inventory1Id, toSlotIndex)] = (item0, quantity);
        }
        else
        {
            // Case 4, swap part of stack with empty slot        
            var item1 = entityFactory.GetBuilder()
                .Template(template0)
                .Quantity(quantity)
                .Parent(secondSlotEid)
                .Build();
            modifiedItems.Add(item1);
            quantities.ModifyComponent(item0, ComponentOperation.SubtractNoUnderflow, quantity);
            worldManagementController.ResyncEntity(eventSender, item1);
            pendingChanges[(inventory0Id, fromSlotIndex)] = (item0, qty0 - quantity);
            pendingChanges[(inventory1Id, toSlotIndex)] = (item0, quantity); // use existing item ID for preview
        }

        worldManagementController.ResyncEntity(eventSender, item0);
    }

    private void SwapMutuallyStackable(ulong item0, ulong item1, uint qty0, uint quantity, ulong inventory0Id,
        ulong inventory1Id, int fromSlotIdx, int toSlotIdx)
    {
        if (quantities.HasLocalComponentForEntity(item1))
        {
            quantities.ModifyComponent(item1, ComponentOperation.AddNoOverflow, quantity);
            pendingChanges[(inventory1Id, toSlotIdx)] = (item1, quantities[item1] + quantity);
        }
        else
        {
            quantities.AddComponent(item1, quantity + 1);
            pendingChanges[(inventory1Id, toSlotIdx)] = (item1, quantity + 1);
        }

        if (quantity == qty0)
        {
            // Case 3, swap entire stack with another mutually stackable
            entityManager.RemoveEntity(item0);
            pendingChanges[(inventory0Id, fromSlotIdx)] = (0, 0);
        }
        else
        {
            // Case 5, swap part of stack with another mutually stackable
            quantities.ModifyComponent(item0, ComponentOperation.SubtractNoUnderflow, quantity);
            worldManagementController.ResyncEntity(eventSender, item0);
            pendingChanges[(inventory0Id, fromSlotIdx)] = (item0, qty0 - quantity);
        }

        worldManagementController.ResyncEntity(eventSender, item1);
    }

    private void SwapFullNonMutuallyStackable(ulong item0, ulong item1, ulong firstSlotEid, ulong secondSlotEid,
        ulong inventory0Id, ulong inventory1Id, int fromSlotIdx, int toSlotIdx)
    {
        parents.AddOrUpdateComponent(item0, secondSlotEid);
        parents.AddOrUpdateComponent(item1, firstSlotEid);
        pendingChanges[(inventory0Id, fromSlotIdx)] = (item1, quantities.TryGetValue(item1, out var qty1) ? qty1 : 1);
        pendingChanges[(inventory1Id, toSlotIdx)] = (item0, quantities.TryGetValue(item0, out var qty0) ? qty0 : 1);
        worldManagementController.ResyncEntity(eventSender, item0);
        worldManagementController.ResyncEntity(eventSender, item1);
    }

    private void SwapPartialNonMutuallyStackable(ulong inventory0Id, ulong inventory1Id, ulong item0, ulong item1,
        uint quantity, ulong template0, ulong secondSlotEid, uint qty0, int fromSlotIdx, int toSlotIdx)
    {
        if (!slotIndexer.TryFindEmptySlot(inventory1Id, out var thirdSlotEid, out var thirdSlotIdx)) return;
        parents.AddOrUpdateComponent(item1, thirdSlotEid);
        var item2 = entityFactory.GetBuilder()
            .Template(template0)
            .Quantity(quantity)
            .Parent(secondSlotEid)
            .Build();
        quantities.ModifyComponent(item0, ComponentOperation.SubtractNoUnderflow, quantity);
        modifiedItems.Add(item2);

        pendingChanges[(inventory0Id, fromSlotIdx)] = (item0, qty0 - quantity);
        pendingChanges[(inventory1Id, toSlotIdx)] = (item0, quantity);
        if (!quantities.TryGetValue(item1, out var qty1)) qty1 = 1;
        pendingChanges[(inventory1Id, thirdSlotIdx)] = (item1, qty1);

        worldManagementController.ResyncEntity(eventSender, item0);
        worldManagementController.ResyncEntity(eventSender, item1);
        worldManagementController.ResyncEntity(eventSender, item2);
    }

    #endregion Swap
}