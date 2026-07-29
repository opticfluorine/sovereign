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

using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Systems.Inventory;
using Sovereign.EngineCore.Systems.Inventory;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Manages state of inventory GUI actions.
/// </summary>
public sealed class InventoryStateManager(
    ILogger<InventoryStateManager> logger,
    IInventoryServices inventoryServices,
    PlayerStateManager playerState)
{
    private const int NoneSelected = -1;
    private int hotbarSelectedIndex;
    private uint selectedQuantity;
    private int selectedSlotIndex = NoneSelected;

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
}