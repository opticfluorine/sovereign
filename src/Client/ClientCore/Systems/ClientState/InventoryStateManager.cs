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

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Manages state of inventory GUI actions.
/// </summary>
public sealed class InventoryStateManager
{
    private const int NoneSelected = -1;
    private int selectedSlotIndex = NoneSelected;

    /// <summary>
    ///     Selects an item for GUI operations.
    /// </summary>
    /// <param name="slotIndex">Inventory slot index.</param>
    public void Select(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
    }

    /// <summary>
    ///     Deselects any currently selected item for GUI operations.
    /// </summary>
    public void Deselect()
    {
        selectedSlotIndex = NoneSelected;
    }

    /// <summary>
    ///     Gets the currently selected slot index, if any.
    /// </summary>
    /// <param name="slotIndex">Slot index. Only meaningful if method returns true.</param>
    /// <returns>true if a slot is currently selected, false otherwise.</returns>
    public bool TryGetSelectedSlot(out int slotIndex)
    {
        slotIndex = selectedSlotIndex;
        return selectedSlotIndex != NoneSelected;
    }
}