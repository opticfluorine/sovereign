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
    private const float QuickSlotLabelOffset = 1.5f;
    private const uint CellBorderColor = 0xff997777;
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
        itemSize = guiExtensions.WorldUnitsToPixels(Vector2.One);

        if (!stateServices.TryGetSelectedPlayer(out var playerId)) return;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.025f * itemSize.X));
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, 0.025f * itemSize);
        if (ImGui.Begin("Inventory", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse))
        {
            slotList.Clear();
            slotIndexer.GetSlotsForEntity(playerId, slotList);

            var hasSelection = stateServices.TryGetSelectedInventorySlot(out var selectedSlotIndex);
            if (hasSelection &&
                hierarchyIndexer.TryGetFirstDirectChild(slotList[selectedSlotIndex], out var selectedItemId) &&
                animatedSprites.TryGetValue(selectedItemId, out var selectedSpriteId))
                // Show the selected item being dragged at the mouse position.
                guiExtensions.AnimatedSpriteForeground(selectedSpriteId, Orientation.South, AnimationPhase.Default,
                    ImGui.GetMousePos());

            ImGui.Indent(0.1f * itemSize.X);
            if (ImGui.BeginTable("invGrid", GridWidthItems,
                    ImGuiTableFlags.SizingFixedSame |
                    ImGuiTableFlags.NoHostExtendX |
                    ImGuiTableFlags.NoPadOuterX))
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
        }

        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
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
        var isSelected = isAnySelected && selectedSlotIndex == slotIndex;
        var startPosLocal = ImGui.GetCursorPos();
        var startPosGlobal = ImGui.GetCursorScreenPos();

        // If the item has a sprite and isn't actively selected, draw it in its grid cell.
        // Otherwise, if it's selected, it will be floating with the mouse cursor.
        // Always blank if there is no sprite to draw.
        if (!isSelected && animatedSprites.TryGetValue(itemId, out var spriteId))
        {
            DrawStyledSlot(startPosGlobal);
            guiExtensions.AnimatedSprite(spriteId, Orientation.South, AnimationPhase.Default, itemSize);
        }
        else
        {
            DrawBlank(gridLabels[slotIndex], startPosGlobal);
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
            // ...then draw the quickslot label at top right.
            DrawQuickSlotLabel(slotIndex, startPosLocal);

        RenderContextMenu(slotIndex, itemId);
    }

    /// <summary>
    ///     Draws a quickslot label.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="startPos">Position of top-left corner of inventory grid cell.</param>
    private void DrawQuickSlotLabel(int slotIndex, Vector2 startPos)
    {
        ImGui.PushFont(fontAtlas.ItemLabelFont);
        var label = quickSlotLabels[slotIndex];
        var labelSize = ImGui.CalcTextSize(label);

        ImGui.SetCursorPos(startPos with { X = startPos.X + itemSize.X - QuickSlotLabelOffset * labelSize.X });
        ImGui.Text(label);
        ImGui.PopFont();
    }


    /// <summary>
    ///     Called when an item is left-clicked in the inventory.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="isAnySelected">Whether any item is actively selected.</param>
    /// <param name="selectedSlotIndex">Currently selected slot index.</param>
    private void OnLeftClickItem(int slotIndex, bool isAnySelected, int selectedSlotIndex)
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerId)) return;

        var isSelected = isAnySelected && slotIndex == selectedSlotIndex;
        if (isAnySelected)
        {
            if (!isSelected) inventoryController.Swap(eventSender, playerId, slotIndex, selectedSlotIndex);
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
        if (!stateServices.TryGetSelectedPlayer(out var playerId)) return;
        inventoryController.Drop(eventSender, playerId, slotIndex);
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
        var startPosLocal = ImGui.GetCursorPos();
        var startPosGlobal = ImGui.GetCursorScreenPos();
        DrawBlank(gridLabels[slotIndex], startPosGlobal);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left)) OnLeftClickEmpty(slotIndex, isAnySelected, selectedSlotIndex);

        if (slotIndex / GridWidthItems == 0) DrawQuickSlotLabel(slotIndex, startPosLocal);
    }

    /// <summary>
    ///     Called when an empty item slot is left-clicked.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="isAnySelected">Whether any item is actively selected.</param>
    /// <param name="selectedSlotIndex">Actively selected slot index. Only meaningful if isAnySelected is true.</param>
    private void OnLeftClickEmpty(int slotIndex, bool isAnySelected, int selectedSlotIndex)
    {
        if (!isAnySelected || !stateServices.TryGetSelectedPlayer(out var playerId)) return;

        inventoryController.Swap(eventSender, playerId, slotIndex, selectedSlotIndex);
        stateController.DeselectItem(eventSender);
    }

    /// <summary>
    ///     Draws a blank item slot (draw only, no behavior).
    /// </summary>
    /// <param name="id">Unique ID for grid cell.</param>
    /// <param name="startPosGlobal">Start position of cell.</param>
    private void DrawBlank(string id, Vector2 startPosGlobal)
    {
        ImGui.InvisibleButton(id, itemSize + new Vector2(4.0f));
        DrawStyledSlot(startPosGlobal);
    }

    /// <summary>
    ///     Draws the styled box for an inventory slot.
    /// </summary>
    /// <param name="startPosGlobal">Start position of cell.</param>
    private void DrawStyledSlot(Vector2 startPosGlobal)
    {
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRect(startPosGlobal, startPosGlobal + itemSize + new Vector2(2.0f),
            CellBorderColor, ImDrawFlags.Closed, 2.0f);
    }
}