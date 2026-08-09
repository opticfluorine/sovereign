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
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Inventory;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Inventory;

/// <summary>
///     Options controlling how an inventory grid is rendered.
/// </summary>
public struct InventoryGridOptions
{
    /// <summary>
    ///     Index of the slot that is actively selected, or -1 for none.
    /// </summary>
    public int SelectedSlotIndex;

    /// <summary>
    ///     Quantity selected from the selected slot. Only meaningful if SelectedSlotIndex >= 0.
    /// </summary>
    public uint SelectedQty;

    /// <summary>
    ///     If true, draws hotbar numeric labels on the first row of the grid.
    /// </summary>
    public bool ShowQuickSlotLabels;
}

/// <summary>
///     Renders an entity's inventory as a grid of item slots. The rendering is entity-agnostic
///     so that it can be reused by any GUI that needs to display an inventory.
/// </summary>
public sealed class InventoryGridRenderer(
    SlotIndexer slotIndexer,
    EntityHierarchyIndexer hierarchyIndexer,
    GuiExtensions guiExtensions,
    AnimatedSpriteComponentCollection animatedSprites,
    GuiFontAtlas fontAtlas,
    StackableTagCollection stackable,
    IInventoryServices inventoryServices,
    NameComponentCollection names)
{
    private const int GridWidthItems = 10;
    private const float QuickSlotLabelOffset = 1.5f;
    private const uint CellBorderColor = 0xff997777;

    /// <summary>
    ///     Offset of quantity/quick slot labels from the slot edge, in label widths.
    /// </summary>
    public const float LabelOffset = QuickSlotLabelOffset;

    private readonly GuiLabelCache gridLabels = new("invg");
    private readonly SparseGuiLabelCache quantityLabels = new("");
    private readonly List<ulong> slotList = new(128);

    private Vector2 itemSize = Vector2.Zero;

    /// <summary>
    ///     Delegate invoked after each cell is rendered, allowing the caller to respond to
    ///     ImGui interactions (e.g. <see cref="ImGui.IsItemClicked(ImGuiMouseButton)"/> or
    ///     <see cref="ImGui.IsItemHovered"/>) against the cell that was just drawn.
    /// </summary>
    /// <param name="slotIndex">Index of the slot.</param>
    /// <param name="itemId">Item entity ID, or 0 if the slot is empty.</param>
    /// <param name="isOccupied">true if the slot contains an item, false otherwise.</param>
    public delegate void CellHandler(int slotIndex, ulong itemId, bool isOccupied);

    /// <summary>
    ///     Renders the inventory of the given entity as a grid.
    /// </summary>
    /// <param name="entityId">Entity whose inventory to render.</param>
    /// <param name="gridId">Unique ImGui ID for the table.</param>
    /// <param name="onCellRendered">Optional callback invoked after each cell is drawn.</param>
    /// <param name="options">Rendering options.</param>
    public void RenderGrid(ulong entityId, string gridId, CellHandler? onCellRendered = null,
        InventoryGridOptions options = default)
    {
        itemSize = guiExtensions.WorldUnitsToPixels(Vector2.One);

        slotList.Clear();
        slotIndexer.GetSlotsForEntity(entityId, slotList);

        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, 0.025f * itemSize);
        if (ImGui.BeginTable(gridId, GridWidthItems,
                ImGuiTableFlags.SizingFixedSame |
                ImGuiTableFlags.NoHostExtendX |
                ImGuiTableFlags.NoPadOuterX))
        {
            for (var i = 0; i < slotList.Count; ++i)
            {
                var slotId = slotList[i];
                ImGui.TableNextColumn();
                var itemId = hierarchyIndexer.TryGetFirstDirectChild(slotId, out var child) ? child : 0UL;
                RenderSlot(i, itemId, entityId, onCellRendered, options);
            }

            ImGui.EndTable();
        }

        ImGui.PopStyleVar();
    }

    /// <summary>
    ///     Renders a single inventory slot.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="itemId">Item entity ID, or 0 if the slot is empty.</param>
    /// <param name="entityId">Entity owning the inventory.</param>
    /// <param name="onCellRendered">Optional callback invoked after the cell is drawn.</param>
    /// <param name="options">Rendering options.</param>
    private void RenderSlot(int slotIndex, ulong itemId, ulong entityId, CellHandler? onCellRendered,
        InventoryGridOptions options)
    {
        var startPosLocal = ImGui.GetCursorPos();
        var startPosGlobal = ImGui.GetCursorScreenPos();
        var isSelected = options.SelectedSlotIndex == slotIndex && itemId != 0;
        var quantity = inventoryServices.GetQuantity(entityId, slotIndex);

        // If the item has a sprite and isn't fully selected, draw it in its cell.
        if ((!isSelected || options.SelectedQty < quantity) && animatedSprites.TryGetValue(itemId, out var spriteId))
        {
            DrawStyledSlot(startPosGlobal);
            guiExtensions.AnimatedSprite(spriteId, Orientation.South, AnimationPhase.Default, itemSize);
        }
        else
        {
            DrawBlank(gridLabels[slotIndex], startPosGlobal);
        }

        onCellRendered?.Invoke(slotIndex, itemId, itemId != 0);

        // Show tooltip if hovered.
        if (ImGui.IsItemHovered() && itemId != 0) ShowItemTooltip(itemId);

        // If this is the first row of the inventory and quick slot labels are enabled...
        if (options.ShowQuickSlotLabels && slotIndex / GridWidthItems == 0)
            DrawQuickSlotLabel(slotIndex, startPosLocal);

        // Draw quantity label for stackable items.
        if (itemId != 0 && stackable.HasTagForEntity(itemId))
            DrawQuantityLabel(slotIndex, startPosLocal, entityId, isSelected, options.SelectedQty);
    }

    /// <summary>
    ///     Draws a quantity label.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="startPos">Position of top-left corner of inventory grid cell.</param>
    /// <param name="entityId">Entity owning the inventory.</param>
    /// <param name="isSelected">Whether the item is selected.</param>
    /// <param name="selectedQty">If selected, the selected quantity.</param>
    private void DrawQuantityLabel(int slotIndex, Vector2 startPos, ulong entityId, bool isSelected, uint selectedQty)
    {
        var quantity = inventoryServices.GetQuantity(entityId, slotIndex);
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
        var label = (slotIndex + 1) % 10 == 0 ? "0" : (slotIndex + 1).ToString();
        var labelSize = ImGui.CalcTextSize(label);

        ImGui.SetCursorPos(startPos with { X = startPos.X + itemSize.X - QuickSlotLabelOffset * labelSize.X });
        ImGui.Text(label);
        ImGui.PopFont();
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
