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
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Inventory;
using Sovereign.EngineCore.Timing;

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
    ClientStateController stateController,
    StackableTagCollection stackable,
    ISystemTimer systemTimer,
    EntityTable entityTable,
    IInventoryServices inventoryServices,
    IOptions<ClientInventoryOptions> options)
{
    private const int GridWidthItems = 10;
    private const float QuickSlotLabelOffset = 1.5f;
    private const uint CellBorderColor = 0xff997777;
    private readonly GuiLabelCache gridLabels = new("invg");
    private readonly SparseGuiLabelCache quantityLabels = new("");

    private readonly List<string> quickSlotLabels = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "0"];
    private readonly List<ulong> slotList = new(128);
    private ulong autoClickNextTime;

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

            var hasSelection =
                stateServices.TryGetSelectedInventorySlot(out var selectedSlotIndex, out var selectedQty);
            if (hasSelection &&
                hierarchyIndexer.TryGetFirstDirectChild(slotList[selectedSlotIndex], out var selectedItemId) &&
                animatedSprites.TryGetValue(selectedItemId, out var selectedSpriteId))
            {
                // Show the selected item being dragged at the mouse position.
                guiExtensions.AnimatedSpriteForeground(selectedSpriteId, Orientation.South, AnimationPhase.Default,
                    ImGui.GetMousePos());
                if (selectedQty > 1)
                {
                    ImGui.PushFont(fontAtlas.BoldItemLabelFont);
                    var labelSize = ImGui.CalcTextSize(quantityLabels[selectedQty]);
                    ImGui.GetForegroundDrawList().AddText(
                        ImGui.GetMousePos() + new Vector2(itemSize.X - QuickSlotLabelOffset * labelSize.X,
                            itemSize.Y - labelSize.Y),
                        0xffffffff, quantityLabels[selectedQty]);
                    ImGui.PopFont();
                }
            }

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
                        RenderEmpty(i, hasSelection, selectedSlotIndex, selectedQty);
                        continue;
                    }

                    // Occupied slot - show item.
                    RenderItem(i, itemId, hasSelection, selectedSlotIndex, selectedQty);
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
    /// <param name="selectedQty">Quantity of selection. Only meaningful if isAnySelected is true.</param>
    private void RenderItem(int slotIndex, ulong itemId, bool isAnySelected, int selectedSlotIndex, uint selectedQty)
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerId)) return;
        var isSelected = isAnySelected && selectedSlotIndex == slotIndex;
        var startPosLocal = ImGui.GetCursorPos();
        var startPosGlobal = ImGui.GetCursorScreenPos();
        var quantity = inventoryServices.GetQuantity(playerId, slotIndex);

        // If the item has a sprite and isn't actively selected, draw it in its grid cell.
        // Otherwise, if it's selected, it will be floating with the mouse cursor.
        // Always blank if there is no sprite to draw.
        if ((!isSelected || selectedQty < quantity) && animatedSprites.TryGetValue(itemId, out var spriteId))
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
            OnLeftClickItem(slotIndex, isAnySelected, selectedSlotIndex, selectedQty);
        else if (ImGui.IsItemHovered() && ImGui.IsMouseDown(ImGuiMouseButton.Right))
            OnRightClickItem(slotIndex);

        // Show tooltip if hovered.
        if (ImGui.IsItemHovered()) ShowItemTooltip(itemId);

        // If this is the first row of the inventory...
        if (slotIndex / GridWidthItems == 0)
            // ...then draw the quickslot label at top right.
            DrawQuickSlotLabel(slotIndex, startPosLocal);

        // Draw quantity label for stackable items.
        if (stackable.HasTagForEntity(itemId)) DrawQuantityLabel(slotIndex, startPosLocal, isSelected, selectedQty);
    }

    /// <summary>
    ///     Draws a quantity label.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="startPos">Position of top-left corner of inventory grid cell.</param>
    /// <param name="isSelected">Whether the item is selected.</param>
    /// <param name="selectedQty">If selected, the selected quantity.</param>
    private void DrawQuantityLabel(int slotIndex, Vector2 startPos, bool isSelected, uint selectedQty)
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerId)) return;
        var quantity = inventoryServices.GetQuantity(playerId, slotIndex);
        var labelQty = isSelected ? quantity - selectedQty : quantity;
        if (labelQty < 2) return;
        var label = quantityLabels[labelQty];

        ImGui.PushFont(fontAtlas.BoldItemLabelFont);
        var labelSize = ImGui.CalcTextSize(label);
        ImGui.SetCursorPos(new Vector2(startPos.X + itemSize.X - QuickSlotLabelOffset * labelSize.X,
            startPos.Y + itemSize.Y - labelSize.Y));
        ImGui.Text(label);
        ImGui.PopFont();
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
    /// <param name="selectedSlotIndex">Currently selected slot index. Only meaningful if isAnySelected is true.</param>
    /// <param name="selectedQty">Selected quantity. Only meaningful if isAnySelected is true.</param>
    private void OnLeftClickItem(int slotIndex, bool isAnySelected, int selectedSlotIndex, uint selectedQty)
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerId)) return;

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
    /// <param name="slotIndex">Slot index.</param>
    private void OnRightClickItem(int slotIndex)
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerId)) return;
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
    ///     Renders an empty item slot.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="isAnySelected">Whether any item is actively selected.</param>
    /// <param name="selectedSlotIndex">Actively selected slot index. Only meaningful if isAnySelected is true.</param>
    /// <param name="selectedQty">If selected, the selected quantity.</param>
    private void RenderEmpty(int slotIndex, bool isAnySelected, int selectedSlotIndex, uint selectedQty)
    {
        var startPosLocal = ImGui.GetCursorPos();
        var startPosGlobal = ImGui.GetCursorScreenPos();
        DrawBlank(gridLabels[slotIndex], startPosGlobal);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            OnLeftClickEmpty(slotIndex, isAnySelected, selectedSlotIndex, selectedQty);

        if (slotIndex / GridWidthItems == 0) DrawQuickSlotLabel(slotIndex, startPosLocal);
    }

    /// <summary>
    ///     Called when an empty item slot is left-clicked.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="isAnySelected">Whether any item is actively selected.</param>
    /// <param name="selectedSlotIndex">Actively selected slot index. Only meaningful if isAnySelected is true.</param>
    /// <param name="selectedQty">Selected quantity.</param>
    private void OnLeftClickEmpty(int slotIndex, bool isAnySelected, int selectedSlotIndex, uint selectedQty)
    {
        if (!isAnySelected || !stateServices.TryGetSelectedPlayer(out var playerId)) return;

        inventoryController.Swap(eventSender, playerId, selectedSlotIndex, slotIndex, selectedQty);
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