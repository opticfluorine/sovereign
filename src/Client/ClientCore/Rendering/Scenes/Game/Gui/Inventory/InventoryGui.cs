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

using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Inventory;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Inventory;

/// <summary>
///     Root class of the inventory GUI.
/// </summary>
public sealed class InventoryGui(
    SlotIndexer slotIndexer,
    ClientStateServices stateServices,
    EntityHierarchyIndexer hierarchyIndexer,
    GuiExtensions guiExtensions,
    AnimatedSpriteComponentCollection animatedSprites,
    GuiFontAtlas fontAtlas,
    IEventSender eventSender,
    IInventoryController inventoryController,
    NameComponentCollection names,
    ClientStateController stateController)
{
    private const int GridWidthItems = 10;
    private const float QuickSlotLabelOffset = 1.1f;
    private readonly GuiLabelCache gridLabels = new("invg");
    private readonly GuiLabelCache gridPopups = new("invp");

    private readonly List<string> quickSlotLabels = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "0"];
    private readonly List<ulong> slotList = new(128);
    private Vector2 itemSize = Vector2.Zero;

    /// <summary>
    ///     Renders the inventory GUI.
    /// </summary>
    public void Render()
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerId)) return;
        if (!ImGui.Begin("Inventory")) return;
        itemSize = guiExtensions.WorldUnitsToPixels(Vector2.One);

        slotList.Clear();
        slotIndexer.GetSlotsForEntity(playerId, slotList);

        var hasSelection = stateServices.TryGetSelectedInventorySlot(out var selectedSlotIndex);
        if (hasSelection &&
            hierarchyIndexer.TryGetFirstDirectChild(slotList[selectedSlotIndex], out var selectedItemId) &&
            animatedSprites.TryGetValue(selectedItemId, out var selectedSpriteId))
        {
            // Show the selected item being dragged at the mouse position.
            guiExtensions.AnimatedSpriteForeground(selectedSpriteId, Orientation.South, AnimationPhase.Default,
                ImGui.GetMousePos());
        }

        if (ImGui.BeginTable("invGrid", GridWidthItems, ImGuiTableFlags.SizingFixedSame))
        {
            for (var i = 0; i < slotList.Count; ++i)
            {
                var slotId = slotList[i];

                ImGui.TableNextColumn();
                if (!hierarchyIndexer.TryGetFirstDirectChild(slotId, out var itemId))
                {
                    // Empty slot
                    RenderEmpty(i, hasSelection, selectedSlotIndex);
                    continue;
                }

                // Occupied slot - show item.
                RenderItem(i, itemId, hasSelection, selectedSlotIndex);
            }

            ImGui.EndTable();
        }

        ImGui.End();
    }

    /// <summary>
    ///     Renders a held item in its table cell.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="itemId">Item ID.</param>
    /// <param name="isAnySelected">Whether any item is actively selected.</param>
    /// <param name="selectedSlotIndex">Actively selected slot index. Only meaningful if isAnySelected is true.</param>
    private void RenderItem(int slotIndex, ulong itemId, bool isAnySelected, int selectedSlotIndex)
    {
        var startPos = ImGui.GetCursorPos();
        var isSelected = isAnySelected && selectedSlotIndex == slotIndex;

        // If the item has a sprite and isn't actively selected, draw it in its grid cell.
        // Otherwise, if it's selected, it will be floating with the mouse cursor.
        // Always blank if there is no sprite to draw.
        if (!isSelected && animatedSprites.TryGetValue(itemId, out var spriteId))
        {
            guiExtensions.AnimatedSprite(spriteId, Orientation.South, AnimationPhase.Default, itemSize);
        }
        else
        {
            DrawBlank(gridLabels[slotIndex]);
        }

        // Handle interactions.
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            OnLeftClickItem(slotIndex, isAnySelected, selectedSlotIndex);
        else if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            OnRightClickItem(slotIndex);

        // Show tooltip if hovered.
        if (ImGui.IsItemHovered()) ShowItemTooltip(itemId);

        // If this is the first row of the inventory...
        if (slotIndex / GridWidthItems == 0)
        {
            // ...then draw the quickslot label at top right.
            ImGui.PushFont(fontAtlas.ItemLabelFont);
            var label = quickSlotLabels[slotIndex];
            var labelSize = ImGui.CalcTextSize(label);

            ImGui.SetCursorPos(startPos with { X = startPos.X + itemSize.X - QuickSlotLabelOffset * labelSize.X });
            ImGui.Text(label);
            ImGui.PopFont();
        }

        RenderContextMenu(slotIndex, itemId);
    }


    /// <summary>
    ///     Called when an item is left-clicked in the inventory.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="isAnySelected">Whether any item is actively selected.</param>
    /// <param name="selectedSlotIndex">Currently selected slot index.</param>
    private void OnLeftClickItem(int slotIndex, bool isAnySelected, int selectedSlotIndex)
    {
        var isSelected = isAnySelected && slotIndex == selectedSlotIndex;
        if (isAnySelected)
        {
            if (!isSelected) inventoryController.Swap(eventSender, slotIndex, selectedSlotIndex);
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
    /// <param name="slotIndex">Slot index.</param>
    private void OnRightClickItem(int slotIndex)
    {
        ImGui.OpenPopup(gridPopups[slotIndex]);
    }

    /// <summary>
    ///     Shows a tooltip for a hovered item.
    /// </summary>
    /// <param name="itemId">Item entity ID.</param>
    private void ShowItemTooltip(ulong itemId)
    {
        ImGui.BeginTooltip();
        ImGui.Text(names.TryGetValue(itemId, out var name) ? name : "[no name]");
        ImGui.EndTooltip();
    }

    /// <summary>
    ///     Renders the context menu for a slot if it is open.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="itemId">Item ID.</param>
    private void RenderContextMenu(int slotIndex, ulong itemId)
    {
        if (!ImGui.BeginPopup(gridPopups[slotIndex])) return;

        if (ImGui.Selectable(InventoryLabels.Drop)) OnDrop(slotIndex);

        ImGui.EndPopup();
    }

    /// <summary>
    ///     Called when the user selects to drop an item at their current position.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    private void OnDrop(int slotIndex)
    {
        inventoryController.Drop(eventSender, slotIndex);
        ImGui.CloseCurrentPopup();
    }

    /// <summary>
    ///     Renders an empty item slot.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="isAnySelected">Whether any item is actively selected.</param>
    /// <param name="selectedSlotIndex">Actively selected slot index. Only meaningful if isAnySelected is true.</param>
    private void RenderEmpty(int slotIndex, bool isAnySelected, int selectedSlotIndex)
    {
        DrawBlank(gridLabels[slotIndex]);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left)) OnLeftClickEmpty(slotIndex, isAnySelected, selectedSlotIndex);
    }

    /// <summary>
    ///     Called when an empty item slot is left-clicked.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="isAnySelected">Whether any item is actively selected.</param>
    /// <param name="selectedSlotIndex">Actively selected slot index. Only meaningful if isAnySelected is true.</param>
    private void OnLeftClickEmpty(int slotIndex, bool isAnySelected, int selectedSlotIndex)
    {
        if (!isAnySelected) return;

        inventoryController.Swap(eventSender, slotIndex, selectedSlotIndex);
        stateController.DeselectItem(eventSender);
    }

    /// <summary>
    ///     Draws a blank item slot (draw only, no behavior).
    /// </summary>
    /// <param name="id">Unique ID for grid cell.</param>
    private void DrawBlank(string id)
    {
        ImGui.InvisibleButton(id, itemSize);
    }
}