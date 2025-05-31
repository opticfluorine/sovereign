// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.ClientCore.Systems.ClientWorldEdit;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.WorldEditor;

/// <summary>
///     Manages the GUI for the client-side world editor.
/// </summary>
public class WorldEditorGui
{
    private readonly IEventSender eventSender;
    private readonly GuiExtensions guiExtensions;

    /// <summary>
    ///     Color used by help text.
    /// </summary>
    private readonly Vector4 helpTextColor = new(0.7f, 0.7f, 0.7f, 1.0f);

    private readonly MaterialManager materialManager;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private readonly ClientWorldEditController worldEditController;
    private readonly ClientWorldEditServices worldEditServices;

    /// <summary>
    ///     Backing buffer for Z offset input field.
    /// </summary>
    private int zOffsetBuffer;
    
    /// <summary>
    ///     Backing buffer for pen width input field.
    /// </summary>
    private int penWidthBuffer;

    /// <summary>
    ///     Change flag for Z offset.
    /// </summary>
    private bool zOffsetChangeInProgress;
    
    /// <summary>
    ///     Change flag for pen width.
    /// </summary>
    private bool penWidthChangeInProgress;

    public WorldEditorGui(ClientWorldEditServices worldEditServices, MaterialManager materialManager,
        GuiExtensions guiExtensions, IEventSender eventSender,
        ClientWorldEditController worldEditController, NameComponentCollection names,
        MaterialComponentCollection materials, MaterialModifierComponentCollection materialModifiers)
    {
        this.worldEditServices = worldEditServices;
        this.materialManager = materialManager;
        this.guiExtensions = guiExtensions;
        this.eventSender = eventSender;
        this.worldEditController = worldEditController;
        this.names = names;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
    }

    /// <summary>
    ///     Renders the world editor GUI.
    /// </summary>
    public void Render()
    {
        var fontSize = ImGui.GetFontSize();
        ImGui.SetNextWindowSize(fontSize * new Vector2(15.0f, 13.5f));
        if (!ImGui.Begin("World Editor", ImGuiWindowFlags.NoResize)) return;

        RenderBlockTemplateControl();
        RenderZOffsetControl();
        RenderPenWidthControl();
        RenderHelp();

        ImGui.End();
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
    private void RenderZOffsetControl()
    {
        // Sync input buffers with backend.
        if (zOffsetBuffer == worldEditServices.ZOffset)
            zOffsetChangeInProgress = false;
        else if (!zOffsetChangeInProgress)
            zOffsetBuffer = worldEditServices.ZOffset;

        ImGui.Separator();
        ImGui.Text("Z Offset:");
        ImGui.SameLine();
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
    }
    
    /// <summary>
    ///     Renders the pen width selection control.
    /// </summary>
    private void RenderPenWidthControl()
    {
        // Sync input buffers with backend.
        if (penWidthBuffer == worldEditServices.PenWidth)
            penWidthChangeInProgress = false;
        else if (!penWidthChangeInProgress)
            penWidthBuffer = worldEditServices.PenWidth;
        
        ImGui.Separator();
        ImGui.Text("Pen Width:");
        ImGui.SameLine();
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
    }

    /// <summary>
    ///     Renders the help text for the world editor GUI.
    /// </summary>
    private void RenderHelp()
    {
        ImGui.Separator();
        ImGui.TextColored(helpTextColor, "Scroll to change block template.");
        ImGui.TextColored(helpTextColor, "Ctrl+Scroll to change Z offset.");
        ImGui.TextColored(helpTextColor, "Shift+Scroll to change pen width.");
    }
}