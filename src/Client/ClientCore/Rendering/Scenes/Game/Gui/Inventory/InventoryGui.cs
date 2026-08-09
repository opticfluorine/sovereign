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
using Hexa.NET.ImGui;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Inventory;
using Sovereign.EngineCore.Timing;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Inventory;

/// <summary>
///     Root class of the inventory GUI for the current player.
/// </summary>
public sealed class InventoryGui(
    ClientStateServices stateServices,
    GuiExtensions guiExtensions,
    AnimatedSpriteComponentCollection animatedSprites,
    GuiFontAtlas fontAtlas,
    IEventSender eventSender,
    IInventoryController inventoryController,
    ClientStateController stateController,
    ISystemTimer systemTimer,
    EntityTable entityTable,
    IInventoryServices inventoryServices,
    IOptions<ClientInventoryOptions> options,
    InventoryGridRenderer gridRenderer)
{
    private ulong autoClickNextTime;

    /// <summary>
    ///     Renders the inventory GUI.
    /// </summary>
    public void Render()
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerId)) return;
        var itemSize = guiExtensions.WorldUnitsToPixels(Vector2.One);

        var hasSelection =
            stateServices.TryGetSelectedInventorySlot(out var selectedSlotIndex, out var selectedQty);
        if (hasSelection)
        {
            // Show the selected item being dragged at the mouse position.
            var selectedItemId = inventoryServices.GetItem(playerId, selectedSlotIndex);
            if (selectedItemId != 0 && animatedSprites.TryGetValue(selectedItemId, out var selectedSpriteId))
            {
                guiExtensions.AnimatedSpriteForeground(selectedSpriteId, Orientation.South, AnimationPhase.Default,
                    ImGui.GetMousePos());
                if (selectedQty > 1)
                {
                    ImGui.PushFont(fontAtlas.BoldItemLabelFont);
                    var quantityLabel = selectedQty.ToString();
                    var labelSize = ImGui.CalcTextSize(quantityLabel);
                    ImGui.GetForegroundDrawList().AddText(
                        ImGui.GetMousePos() + new Vector2(
                            itemSize.X - InventoryGridRenderer.LabelOffset * labelSize.X,
                            itemSize.Y - labelSize.Y),
                        0xffffffff, quantityLabel);
                    ImGui.PopFont();
                }
            }
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.025f * itemSize.X));
        if (ImGui.Begin("Inventory", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Indent(0.1f * itemSize.X);
            gridRenderer.RenderGrid(playerId, "invGrid", OnCellRendered,
                new InventoryGridOptions
                {
                    SelectedSlotIndex = hasSelection ? selectedSlotIndex : -1,
                    SelectedQty = selectedQty,
                    ShowQuickSlotLabels = true
                });
        }

        ImGui.End();
        ImGui.PopStyleVar();
    }

    /// <summary>
    ///     Handles interactions on a grid cell after it has been rendered.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="itemId">Item entity ID, or 0 if the slot is empty.</param>
    /// <param name="isOccupied">true if the slot contains an item, false otherwise.</param>
    private void OnCellRendered(int slotIndex, ulong itemId, bool isOccupied)
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerId)) return;
        var hasSelection = stateServices.TryGetSelectedInventorySlot(out var selectedSlotIndex, out var selectedQty);

        if (isOccupied)
        {
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                OnLeftClickItem(playerId, slotIndex, hasSelection, selectedSlotIndex, selectedQty);
            else if (ImGui.IsItemHovered() && ImGui.IsMouseDown(ImGuiMouseButton.Right))
                OnRightClickItem(playerId, slotIndex);
        }
        else
        {
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                OnLeftClickEmpty(playerId, slotIndex, hasSelection, selectedSlotIndex, selectedQty);
        }
    }

    /// <summary>
    ///     Called when an item is left-clicked in the inventory.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="isAnySelected">Whether any item is actively selected.</param>
    /// <param name="selectedSlotIndex">Actively selected slot index. Only meaningful if isAnySelected is true.</param>
    /// <param name="selectedQty">Selected quantity. Only meaningful if isAnySelected is true.</param>
    private void OnLeftClickItem(ulong playerId, int slotIndex, bool isAnySelected, int selectedSlotIndex, uint selectedQty)
    {
        var isSelected = isAnySelected && slotIndex == selectedSlotIndex;
        if (isAnySelected)
        {
            if (!isSelected) inventoryController.Swap(eventSender, playerId, selectedSlotIndex, slotIndex, selectedQty);
            stateController.DeselectItem(eventSender);
        }
        else
        {
            stateController.SelectItem(eventSender, slotIndex);
        }
    }

    /// <summary>
    ///     Called when an item is right-clicked in the inventory.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="slotIndex">Slot index.</param>
    private void OnRightClickItem(ulong playerId, int slotIndex)
    {
        var itemId = inventoryServices.GetItem(playerId, slotIndex);
        if (itemId == 0) return;

        // If there is already a selected item, then the clicked item must be mutually stackable.
        if (stateServices.TryGetSelectedInventorySlot(out var curSlotIndex, out var curQty))
        {
            var curItemId = inventoryServices.GetItem(playerId, curSlotIndex);
            if (curItemId > 0)
            {
                if (!entityTable.TryGetTemplate(itemId, out var itemTemplateId) ||
                    !entityTable.TryGetTemplate(curItemId, out var curItemTemplateId) ||
                    itemTemplateId != curItemTemplateId) return;
            }
        }
        else
        {
            curSlotIndex = slotIndex;
            curQty = 0;
        }

        if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            // Player just started holding the right button, so take the initial action.
            autoClickNextTime = systemTimer.GetTime() + options.Value.RightMouseFirstDelayUs;
        }
        else
        {
            // Button is still being held down - has it been long enough to add more items?   
            var now = systemTimer.GetTime();
            if (now < autoClickNextTime) return;

            autoClickNextTime += options.Value.RightMouseRepeatDelayUs;
        }

        var shift = ImGui.IsKeyDown(ImGuiKey.LeftShift);
        var qty = inventoryServices.GetQuantity(playerId, slotIndex);
        if (qty == 0) return;

        var newQty = curQty + (shift ? (qty + 1) / 2 : 1);

        // If picking up from two or more stacks at once, quietly merge the stacks prior to
        // updating the selection.
        if (curSlotIndex != slotIndex)
        {
            inventoryController.Swap(eventSender, playerId, curSlotIndex, slotIndex);
        }

        stateController.SelectItem(eventSender, slotIndex, newQty);
    }

    /// <summary>
    ///     Called when an empty item slot is left-clicked.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="isAnySelected">Whether any item is actively selected.</param>
    /// <param name="selectedSlotIndex">Actively selected slot index. Only meaningful if isAnySelected is true.</param>
    /// <param name="selectedQty">Selected quantity.</param>
    private void OnLeftClickEmpty(ulong playerId, int slotIndex, bool isAnySelected, int selectedSlotIndex, uint selectedQty)
    {
        if (!isAnySelected) return;

        inventoryController.Swap(eventSender, playerId, selectedSlotIndex, slotIndex, selectedQty);
        stateController.DeselectItem(eventSender);
    }
}
