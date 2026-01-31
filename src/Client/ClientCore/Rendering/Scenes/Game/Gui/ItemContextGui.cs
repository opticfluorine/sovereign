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

using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Inventory;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui;

/// <summary>
///     In-game item context dialog.
/// </summary>
public sealed class ItemContextGui(
    IInventoryServices inventoryServices,
    KinematicsComponentCollection kinematics,
    ClientStateServices clientStateServices,
    IInventoryController inventoryController,
    IEventSender eventSender,
    SlotIndexer slotIndexer,
    ILogger<ItemContextGui> logger)
{
    /// <summary>
    ///     Dialog ID for the item context dialog.
    /// </summary>
    private const string DialogId = "ItemContextDlg";

    private ulong itemId;

    /// <summary>
    ///     Renders the context dialog.
    /// </summary>
    public void Render()
    {
        if (!ImGui.BeginPopup(DialogId)) return;

        if (kinematics.HasComponentForEntity(itemId)) ShowDroppedItemOptions();
        else ShowHeldItemOptions();

        ImGui.EndPopup();
    }

    /// <summary>
    ///     Opens the context dialog for the given item.
    /// </summary>
    /// <param name="itemId">Item ID.</param>
    public void Open(ulong itemId)
    {
        this.itemId = itemId;
        ImGui.OpenPopup(DialogId);
    }

    /// <summary>
    ///     Shows context menu options for an item which is dropped on the ground (i.e. not in an inventory).
    /// </summary>
    private void ShowDroppedItemOptions()
    {
        if (!clientStateServices.TryGetSelectedPlayer(out var playerId))
        {
            logger.LogError("No player ID.");
            return;
        }

        var optionCount = 0;

        if (inventoryServices.CanPickUp(playerId, itemId))
        {
            optionCount++;
            if (ImGui.Selectable("Pick Up"))
            {
                DoPickUp();
                ImGui.CloseCurrentPopup();
            }
        }

        if (optionCount == 0) ImGui.TextColored(GuiColors.DisabledText, "No Actions");
    }

    /// <summary>
    ///     Shows context menu options for an item which is held in an inventory slot.
    /// </summary>
    private void ShowHeldItemOptions()
    {
        if (ImGui.Selectable("Drop"))
        {
            DoDrop();
            ImGui.CloseCurrentPopup();
        }
    }

    /// <summary>
    ///     Called when the player selects the "Pick Up" item context action.
    /// </summary>
    private void DoPickUp()
    {
        if (!clientStateServices.TryGetSelectedPlayer(out var playerId))
        {
            logger.LogError("No player ID.");
            return;
        }

        inventoryController.PickUp(eventSender, playerId, itemId);
    }

    /// <summary>
    ///     Called when the player selects the "Drop" item context option.
    /// </summary>
    private void DoDrop()
    {
        if (!clientStateServices.TryGetSelectedPlayer(out var playerId))
        {
            logger.LogError("No player ID.");
            return;
        }

        if (!slotIndexer.TryGetSlotIndex(playerId, itemId, out var slotIndex)) return;

        inventoryController.Drop(eventSender, playerId, slotIndex);
    }
}