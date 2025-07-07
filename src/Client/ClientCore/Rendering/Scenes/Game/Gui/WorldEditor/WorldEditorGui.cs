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
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.ClientCore.Systems.ClientWorldEdit;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineUtil.Constants;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.WorldEditor;

/// <summary>
///     Manages the GUI for the client-side world editor.
/// </summary>
public class WorldEditorGui
{
    private const string BlockToolLabel = $"{Emoji.Mountain}";
    private const string NpcToolLabel = $"{Emoji.BustInSilhouette}";
    private const string ItemToolLabel = $"{Emoji.RedApple}";

    private static readonly Vector4 SelectedColor = new(0.06f, 0.53f, 0.98f, 1.0f);
    private static readonly Vector4 UnselectedColor = new(0.26f, 0.59f, 0.98f, 0.40f);

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
        ImGui.SetNextWindowSize(fontSize * new Vector2(15.0f, 15.5f));
        if (!ImGui.Begin("World Editor", ImGuiWindowFlags.NoResize)) return;

        RenderToolSelection();

        switch (worldEditServices.WorldEditTool)
        {
            case WorldEditTool.Block:
                RenderBlockTool();
                break;

            case WorldEditTool.Npc:
                RenderNpcTool();
                break;

            case WorldEditTool.Item:
                break;
        }

        ImGui.End();
    }

    /// <summary>
    ///     Renders the tool selection buttons for the world editor.
    /// </summary>
    private void RenderToolSelection()
    {
        PushToolButtonColor(WorldEditTool.Block);
        if (ImGui.Button(BlockToolLabel)) worldEditController.SetTool(eventSender, WorldEditTool.Block);
        ImGui.PopStyleColor();
        if (ImGui.IsItemHovered() && ImGui.BeginTooltip())
        {
            ImGui.Text("Blocks");
            ImGui.EndTooltip();
        }

        ImGui.SameLine();
        PushToolButtonColor(WorldEditTool.Npc);
        if (ImGui.Button(NpcToolLabel)) worldEditController.SetTool(eventSender, WorldEditTool.Npc);
        ImGui.PopStyleColor();
        if (ImGui.IsItemHovered() && ImGui.BeginTooltip())
        {
            ImGui.Text("NPCs");
            ImGui.EndTooltip();
        }

        ImGui.SameLine();
        PushToolButtonColor(WorldEditTool.Item);
        if (ImGui.Button(ItemToolLabel)) worldEditController.SetTool(eventSender, WorldEditTool.Item);
        ImGui.PopStyleColor();
        if (ImGui.IsItemHovered() && ImGui.BeginTooltip())
        {
            ImGui.Text("Items");
            ImGui.EndTooltip();
        }

        ImGui.Separator();
    }

    /// <summary>
    ///     Pushes the style color for the tool button based on whether it is selected or not.
    /// </summary>
    /// <param name="worldEditTool">Tool.</param>
    private void PushToolButtonColor(WorldEditTool worldEditTool)
    {
        ImGui.PushStyleColor(ImGuiCol.Button,
            worldEditServices.WorldEditTool == worldEditTool ? SelectedColor : UnselectedColor);
    }

    /// <summary>
    ///     Renders the block tool controls, including the block template selection,
    /// </summary>
    private void RenderBlockTool()
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
        else if (!zOffsetChangeInProgress)
            zOffsetBuffer = worldEditServices.ZOffset;

        if (penWidthBuffer == worldEditServices.PenWidth)
            penWidthChangeInProgress = false;
        else if (!penWidthChangeInProgress)
            penWidthBuffer = worldEditServices.PenWidth;

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
        ImGui.TextColored(helpTextColor, "Scroll to change block template.");
        ImGui.TextColored(helpTextColor, "Ctrl+Scroll to change Z offset.");
        ImGui.TextColored(helpTextColor, "Shift+Scroll to change pen width.");
    }

    /// <summary>
    ///     Renders the NPC tool controls, including the NPC template selection.
    /// </summary>
    private void RenderNpcTool()
    {
    }
}