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

using Hexa.NET.ImGui;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.ClientCore.Systems.ClientWorldEdit;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.WorldEditor;

public class BlockWorldEditorGui(
    NameComponentCollection names,
    ClientWorldEditServices worldEditServices,
    MaterialComponentCollection materials,
    MaterialModifierComponentCollection materialModifiers,
    MaterialManager materialManager,
    GuiExtensions guiExtensions,
    ClientWorldEditController worldEditController,
    IEventSender eventSender)
{
    /// <summary>
    ///     Backing buffer for pen width input field.
    /// </summary>
    private int penWidthBuffer;

    /// <summary>
    ///     Change flag for pen width.
    /// </summary>
    private bool penWidthChangeInProgress;

    /// <summary>
    ///     Backing buffer for Z offset input field.
    /// </summary>
    private int zOffsetBuffer;

    /// <summary>
    ///     Change flag for Z offset.
    /// </summary>
    private bool zOffsetChangeInProgress;

    /// <summary>
    ///     Renders the block tool controls, including the block template selection,
    /// </summary>
    public void Render()
    {
        RenderBlockTemplateControl();
        RenderBlockDrawControls();
        RenderBlockToolHelp();
    }

    /// <summary>
    ///     Renders the material/material modifier selection control.
    /// </summary>
    private void RenderBlockTemplateControl()
    {
        if (!ImGui.BeginTable("WorldEditMaterial", 2, ImGuiTableFlags.SizingStretchProp)) return;
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);

        var templateName = names.HasComponentForEntity(worldEditServices.BlockTemplateId)
            ? names[worldEditServices.BlockTemplateId]
            : "[no name]";
        var templateMaterialId = materials[worldEditServices.BlockTemplateId];
        var templateMaterialModifier = materialModifiers[worldEditServices.BlockTemplateId];

        var material = materialManager.Materials[templateMaterialId];
        var tile = material.MaterialSubtypes[templateMaterialModifier];

        ImGui.TableNextColumn();
        guiExtensions.TileSprite(tile.TopFaceTileSpriteId, TileContextKey.AllWildcards);

        ImGui.TableNextColumn();
        var relId = worldEditServices.BlockTemplateId - EntityConstants.FirstTemplateEntityId;
        ImGui.Text($"{templateName} (Block Template {relId})");

        ImGui.EndTable();
    }

    /// <summary>
    ///     Renders the z-offset selection control.
    /// </summary>
    private void RenderBlockDrawControls()
    {
        // Sync input buffers with backend.
        if (zOffsetBuffer == worldEditServices.ZOffset)
            zOffsetChangeInProgress = false;
        else if (!zOffsetChangeInProgress) zOffsetBuffer = worldEditServices.ZOffset;

        if (penWidthBuffer == worldEditServices.PenWidth)
            penWidthChangeInProgress = false;
        else if (!penWidthChangeInProgress) penWidthBuffer = worldEditServices.PenWidth;

        if (!ImGui.BeginTable("WorldEditControls", 2, ImGuiTableFlags.SizingStretchProp)) return;

        ImGui.TableNextColumn();
        ImGui.Text("Z Offset:");

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(120.0f);
        ImGui.InputInt("##zoff", ref zOffsetBuffer);

        // Validate and update state if needed.
        if (zOffsetBuffer != worldEditServices.ZOffset)
        {
            if (zOffsetBuffer is < ClientWorldEditConstants.MinZOffset or > ClientWorldEditConstants.MaxZOffset)
            {
                zOffsetBuffer = worldEditServices.ZOffset;
            }
            else
            {
                zOffsetChangeInProgress = true;
                worldEditController.SetZOffset(eventSender, zOffsetBuffer);
            }
        }

        ImGui.TableNextColumn();
        ImGui.Text("Pen Width:");

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(120.0f);
        ImGui.InputInt("##penwidth", ref penWidthBuffer);

        // Validate and update state if needed.
        if (penWidthBuffer != worldEditServices.PenWidth)
        {
            if (penWidthBuffer is < ClientWorldEditConstants.MinPenWidth or > ClientWorldEditConstants.MaxPenWidth)
            {
                penWidthBuffer = worldEditServices.PenWidth;
            }
            else
            {
                penWidthChangeInProgress = true;
                worldEditController.SetPenWidth(eventSender, penWidthBuffer);
            }
        }

        ImGui.EndTable();
    }

    /// <summary>
    ///     Renders the help text for the world editor GUI.
    /// </summary>
    private void RenderBlockToolHelp()
    {
        ImGui.Separator();
        ImGui.TextColored(WorldEditorConstants.HelpTextColor, "Scroll to change block template.");
        ImGui.TextColored(WorldEditorConstants.HelpTextColor, "Ctrl+Scroll to change Z offset.");
        ImGui.TextColored(WorldEditorConstants.HelpTextColor, "Shift+Scroll to change pen width.");
    }
}