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
using Sovereign.ClientCore.Systems.Inventory;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Inventory;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Manages state of inventory GUI actions.
/// </summary>
public sealed class InventoryStateManager : IDisposable
{
    private const int NoneSelected = -1;
    private readonly EntityTable entityTable;
    private readonly IInventoryServices inventoryServices;
    private readonly ILogger<InventoryStateManager> logger;
    private readonly PlayerStateManager playerState;
    private int hotbarSelectedIndex;
    private ulong secondaryEntityId;
    private uint selectedQuantity;
    private int selectedSlotIndex = NoneSelected;

    public InventoryStateManager(ILogger<InventoryStateManager> logger,
        IInventoryServices inventoryServices, PlayerStateManager playerState, EntityTable entityTable)
    {
        this.logger = logger;
        this.inventoryServices = inventoryServices;
        this.playerState = playerState;
        this.entityTable = entityTable;
        entityTable.OnEntityRemoved += OnEntityRemoved;
    }

    public void Dispose()
    {
        entityTable.OnEntityRemoved -= OnEntityRemoved;
    }

    /// <summary>
    ///     Selects an item for GUI operations.
    /// </summary>
    /// <param name="slotIndex">Inventory slot index.</param>
    /// <param name="quantity">Quantity (0 to select entire stack).</param>
    public void Select(int slotIndex, uint quantity)
    {
        if (!playerState.TryGetPlayerEntityId(out var playerId))
        {
            logger.LogError("Select() called without active player.");
            return;
        }

        var maxQuantity = inventoryServices.GetQuantity(playerId, slotIndex);
        var clampedQuantity = quantity;
        if (quantity > maxQuantity)
        {
            logger.LogWarning("Tried to select {Qty} items from slot {SlotIdx} which only has {MaxQty} items.",
                quantity, slotIndex, maxQuantity);
            clampedQuantity = maxQuantity;
        }

        selectedSlotIndex = slotIndex;
        selectedQuantity = quantity > 0 ? clampedQuantity : maxQuantity;
    }

    /// <summary>
    ///     Deselects any currently selected item for GUI operations.
    /// </summary>
    public void Deselect()
    {
        selectedSlotIndex = NoneSelected;
        selectedQuantity = 0;
    }

    /// <summary>
    ///     Gets the currently selected slot index, if any.
    /// </summary>
    /// <param name="slotIndex">Slot index. Only meaningful if method returns true.</param>
    /// <param name="quantity">Selected quantity. Only meaningful if method returns true.</param>
    /// <returns>true if a slot is currently selected, false otherwise.</returns>
    public bool TryGetSelectedSlot(out int slotIndex, out uint quantity)
    {
        slotIndex = selectedSlotIndex;
        quantity = selectedQuantity;
        return selectedSlotIndex != NoneSelected;
    }

    /// <summary>
    ///     Gets the currently selected hotbar slot index, if any.
    /// </summary>
    /// <returns>Currently selected hotbar slot.</returns>
    public int GetSelectedHotbarSlot()
    {
        return hotbarSelectedIndex;
    }

    /// <summary>
    ///     Selects a hotbar slot.
    /// </summary>
    /// <param name="slotIndex"></param>
    public void SelectHotbar(int slotIndex)
    {
        if (slotIndex is < 0 or >= ClientInventoryConstants.HotbarSlotCount)
        {
            logger.LogError("Tried to select invalid hotbar slot {Index}.", slotIndex);
            return;
        }

        hotbarSelectedIndex = slotIndex;
    }

    /// <summary>
    ///     Sets the entity ID of the secondary inventory. Use 0 to clear.
    /// </summary>
    /// <param name="entityId">Entity ID of the entity whose inventory is shown, or 0 to clear.</param>
    public void SetSecondaryEntityId(ulong entityId)
    {
        secondaryEntityId = entityId;
    }

    /// <summary>
    ///     Gets the entity ID of the secondary inventory, if any.
    /// </summary>
    /// <param name="entityId">Entity ID. Only meaningful if method returns true.</param>
    /// <returns>true if a secondary inventory is set, false otherwise.</returns>
    public bool TryGetSecondaryEntityId(out ulong entityId)
    {
        entityId = secondaryEntityId;
        return secondaryEntityId != 0;
    }

    /// <summary>
    ///     Called when an entity is removed from the entity table. If the removed entity
    ///     is the current secondary inventory, clears the secondary inventory.
    /// </summary>
    /// <param name="entityId">Entity ID of the removed entity.</param>
    /// <param name="isUnload">true if this is an unload, false otherwise.</param>
    private void OnEntityRemoved(ulong entityId, bool isUnload)
    {
        if (entityId == secondaryEntityId) secondaryEntityId = 0;
    }
}