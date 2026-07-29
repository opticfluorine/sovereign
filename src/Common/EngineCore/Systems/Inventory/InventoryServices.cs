// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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
using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.EngineCore.Systems.Inventory;

/// <summary>
///     Public service methods exposed by the Inventory system.
/// </summary>
public interface IInventoryServices
{
    /// <summary>
    ///     Checks whether an entity is allowed to pick up the given item.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="itemId">Item ID.</param>
    /// <returns>true if allowed, false otherwise.</returns>
    bool CanPickUp(ulong entityId, ulong itemId);

    /// <summary>
    ///     Gets the number of slots in the given entity's inventory.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Number of slots.</returns>
    int GetSlotCount(ulong entityId);

    /// <summary>
    ///     Gets the current inventory of the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="itemEntityIds">Item entity IDs. Will be populated in slot order. 0 indicates empty slot.</param>
    void GetInventory(ulong entityId, Span<ulong> itemEntityIds);

    /// <summary>
    ///     Gets the item in the given slot index.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="slotIndex">Slot index.</param>
    /// <returns>Item ID, or 0 if the slot is empty or does not exist.</returns>
    ulong GetItem(ulong entityId, int slotIndex);

    /// <summary>
    ///     Gets the slot index to which an item is attached, if any.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="itemId">Item ID.</param>
    /// <returns>Slot index, or -1 if the item is not in the entity's inventory.</returns>
    int GetSlotIndexForItem(ulong entityId, ulong itemId);

    /// <summary>
    ///     Finds the first item (in slot order) in the inventory with the given template ID.
    /// </summary>
    /// <param name="entityId">Entity ID whose inventory should be searched..</param>
    /// <param name="itemTemplateId">Template ID to search for.</param>
    /// <returns>Entity ID of the first matching item, or 0 if no match was found.</returns>
    ulong FindFirstMatchingItem(ulong entityId, ulong itemTemplateId);

    /// <summary>
    ///     Gets the quantity of items in a slot.
    /// </summary>
    /// <param name="entityId">Owner entity ID.</param>
    /// <param name="slotIndex">Slot index.</param>
    /// <returns></returns>
    uint GetQuantity(ulong entityId, int slotIndex);
}

/// <summary>
///     Implementation of IInventoryServices.
/// </summary>
internal sealed class InventoryServices(
    InventoryManager inventoryManager,
    SlotIndexer slotIndexer,
    EntityHierarchyIndexer hierarchyIndexer) : IInventoryServices
{
    public bool CanPickUp(ulong entityId, ulong itemId)
    {
        return inventoryManager.IsPickUpAllowed(entityId, itemId);
    }

    public int GetSlotCount(ulong entityId)
    {
        return slotIndexer.GetSlotCountForEntity(entityId);
    }

    public void GetInventory(ulong entityId, Span<ulong> itemEntityIds)
    {
        // Fill itemEntityIds with slot IDs.
        var slotCount = slotIndexer.GetSlotsForEntity(entityId, itemEntityIds);

        // Transform slot IDs to item IDs.
        for (var i = 0; i < slotCount; ++i)
        {
            itemEntityIds[i] = hierarchyIndexer.TryGetFirstDirectChild(itemEntityIds[i], out var itemId) ? itemId : 0;
        }
    }

    public ulong GetItem(ulong entityId, int slotIndex)
    {
        return inventoryManager.GetItem(entityId, slotIndex);
    }

    public int GetSlotIndexForItem(ulong entityId, ulong itemId)
    {
        return slotIndexer.TryGetSlotIndex(entityId, itemId, out var slotIndex) ? slotIndex : -1;
    }

    public ulong FindFirstMatchingItem(ulong entityId, ulong itemTemplateId)
    {
        return inventoryManager.FindFirstMatchingItem(entityId, itemTemplateId);
    }

    public uint GetQuantity(ulong entityId, int slotIndex)
    {
        return inventoryManager.GetQuantity(entityId, slotIndex);
    }
}