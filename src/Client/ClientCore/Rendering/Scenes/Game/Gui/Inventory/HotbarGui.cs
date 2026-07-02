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
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.ClientCore.Systems.Inventory;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Inventory;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Inventory;

/// <summary>
///     GUI class for the item hotbar at the bottom of the screen.
/// </summary>
public sealed class HotbarGui(
    GuiExtensions guiExtensions,
    ClientStateServices stateServices,
    IInventoryServices inventoryServices,
    AnimatedSpriteComponentCollection animatedSprites,
    GuiFontAtlas fontAtlas,
    NameComponentCollection names,
    IEventSender eventSender,
    ClientStateController stateController)
{
    private const int GridWidthItems = 10;
    private const int StackLimit = 128;
    private const float QuickSlotLabelOffset = 1.5f;
    private const uint CellBorderColor = 0xff886666;
    private const uint CellBorderColorSelected = 0xff997777;
    private readonly GuiLabelCache gridLabels = new("invg");
    private readonly List<string> quickSlotLabels = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "0"];
    private ulong[]? inventoryBuffer;

    /// <summary>
    ///     Renders the item hotbar.
    /// </summary>
    public void Render()
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerId)) return;

        var dims = ImGui.GetIO().DisplaySize;
        var hPos = dims.X * 0.5f;
        var vPos = ImGui.GetIO().DisplaySize.Y * 0.9f;
        var itemSize = guiExtensions.WorldUnitsToPixels(Vector2.One);

        ImGui.SetNextWindowSize(new Vector2(ClientInventoryConstants.HotbarSlotCount * (1.12f * itemSize.X),
            0.0f));
        ImGui.SetNextWindowPos(new Vector2(hPos, vPos), ImGuiCond.Always, new Vector2(0.5f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.025f * itemSize.X));
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, 0.025f * itemSize);

        if (!ImGui.Begin("Hotbar", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar))
        {
            ImGui.PopStyleVar();
            ImGui.PopStyleVar();
            return;
        }

        var selectedSlotIndex = stateServices.GetSelectedHotbarSlot();
        var invSize = inventoryServices.GetSlotCount(playerId);
        if (invSize > StackLimit && (inventoryBuffer == null || inventoryBuffer.Length < invSize))
        {
            inventoryBuffer = new ulong[invSize];
        }

        DrawHotbar(invSize, playerId, itemSize, selectedSlotIndex);

        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
        ImGui.End();
    }

    /// <summary>
    ///     Draws the hotbar.
    /// </summary>
    /// <param name="invSize">Inventory size.</param>
    /// <param name="playerId">Player ID.</param>
    /// <param name="itemSize">Item size.</param>
    /// <param name="selectedSlotIndex">Selected hotbar slot index.</param>
    private void DrawHotbar(int invSize, ulong playerId, Vector2 itemSize, int selectedSlotIndex)
    {
        var inv = invSize > StackLimit ? inventoryBuffer.AsSpan() : stackalloc ulong[invSize];
        inventoryServices.GetInventory(playerId, inv);

        ImGui.Indent(0.1f * itemSize.X);
        if (ImGui.BeginTable("hotbarGrid", GridWidthItems,
                ImGuiTableFlags.SizingFixedSame |
                ImGuiTableFlags.NoHostExtendX |
                ImGuiTableFlags.NoPadOuterX))
        {
            for (var i = 0; i < GridWidthItems; ++i)
            {
                if (i >= invSize) break;

                ImGui.TableNextColumn();
                if (inv[i] > 0)
                    RenderItem(i, inv[i], selectedSlotIndex, itemSize);
                else
                    RenderEmpty(i, selectedSlotIndex, itemSize);
            }

            ImGui.EndTable();
        }
    }

    /// <summary>
    ///     Renders a held item in its table cell.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="itemId">Item ID.</param>
    /// <param name="selectedSlotIndex">Actively selected slot index. Only meaningful if isAnySelected is true.</param>
    /// <param name="itemSize">Item size.</param>
    private void RenderItem(int slotIndex, ulong itemId, int selectedSlotIndex, Vector2 itemSize)
    {
        var startPosLocal = ImGui.GetCursorPos();
        var startPosGlobal = ImGui.GetCursorScreenPos();

        if (animatedSprites.TryGetValue(itemId, out var spriteId))
        {
            DrawStyledSlot(startPosGlobal, itemSize, slotIndex == selectedSlotIndex);
            guiExtensions.AnimatedSprite(spriteId, Orientation.South, AnimationPhase.Default, itemSize);
        }
        else
        {
            DrawBlank(gridLabels[slotIndex], startPosGlobal, itemSize, slotIndex == selectedSlotIndex);
        }

        // Handle interactions.
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            OnLeftClick(slotIndex);

        // Show tooltip if hovered.
        if (ImGui.IsItemHovered()) ShowItemTooltip(itemId);

        DrawQuickSlotLabel(slotIndex, startPosLocal, itemSize);
    }

    /// <summary>
    ///     Draws a quickslot label.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="startPos">Position of top-left corner of inventory grid cell.</param>
    /// <param name="itemSize">Item size.</param>
    private void DrawQuickSlotLabel(int slotIndex, Vector2 startPos, Vector2 itemSize)
    {
        ImGui.PushFont(fontAtlas.ItemLabelFont);
        var label = quickSlotLabels[slotIndex];
        var labelSize = ImGui.CalcTextSize(label);

        ImGui.SetCursorPos(new Vector2(startPos.X + itemSize.X - QuickSlotLabelOffset * labelSize.X,
            startPos.Y + 0.2f * labelSize.Y));
        ImGui.Text(label);
        ImGui.PopFont();
    }

    /// <summary>
    ///     Called when an item is left-clicked in the inventory.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    private void OnLeftClick(int slotIndex)
    {
        stateController.SelectHotbar(eventSender, slotIndex);
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
    /// <param name="selectedSlotIndex">Actively selected slot index. Only meaningful if isAnySelected is true.</param>
    /// <param name="itemSize">Item size.</param>
    private void RenderEmpty(int slotIndex, int selectedSlotIndex, Vector2 itemSize)
    {
        var startPosLocal = ImGui.GetCursorPos();
        var startPosGlobal = ImGui.GetCursorScreenPos();
        DrawBlank(gridLabels[slotIndex], startPosGlobal, itemSize, slotIndex == selectedSlotIndex);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left)) OnLeftClick(slotIndex);

        DrawQuickSlotLabel(slotIndex, startPosLocal, itemSize);
    }

    /// <summary>
    ///     Draws a blank item slot (draw only, no behavior).
    /// </summary>
    /// <param name="id">Unique ID for grid cell.</param>
    /// <param name="startPosGlobal">Start position of cell.</param>
    /// <param name="itemSize">Item size.</param>
    /// <param name="isSelected">Whether the slot is selected.</param>
    private void DrawBlank(string id, Vector2 startPosGlobal, Vector2 itemSize, bool isSelected)
    {
        ImGui.InvisibleButton(id, itemSize + new Vector2(4.0f));
        DrawStyledSlot(startPosGlobal, itemSize, isSelected);
    }

    /// <summary>
    ///     Draws the styled box for an inventory slot.
    /// </summary>
    /// <param name="startPosGlobal">Start position of cell.</param>
    /// <param name="itemSize">Item size.</param>
    /// <param name="isSelected">Whether the slot is currently selected.</param>
    private void DrawStyledSlot(Vector2 startPosGlobal, Vector2 itemSize, bool isSelected)
    {
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRect(startPosGlobal, startPosGlobal + itemSize + new Vector2(2.0f),
            isSelected ? CellBorderColorSelected : CellBorderColor, ImDrawFlags.Closed, isSelected ? 4.0f : 2.0f);
    }
}