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
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.ClientCore.Systems.ClientWorldEdit;
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
    private readonly MaterialSelectorPopup materialSelectorPopup;
    private readonly ClientWorldEditController worldEditController;
    private readonly ClientWorldEditServices worldEditServices;

    /// <summary>
    ///     Backing buffer for modifier input field.
    /// </summary>
    private int modifierBuffer;

    /// <summary>
    ///     Change flag for modifier.
    /// </summary>
    private bool modifierChangeInProgress;

    /// <summary>
    ///     Backing buffer for Z offset input field.
    /// </summary>
    private int zOffsetBuffer;

    /// <summary>
    ///     Change flag for Z offset.
    /// </summary>
    private bool zOffsetChangeInProgress;

    public WorldEditorGui(ClientWorldEditServices worldEditServices, MaterialManager materialManager,
        GuiExtensions guiExtensions, MaterialSelectorPopup materialSelectorPopup, IEventSender eventSender,
        ClientWorldEditController worldEditController)
    {
        this.worldEditServices = worldEditServices;
        this.materialManager = materialManager;
        this.guiExtensions = guiExtensions;
        this.materialSelectorPopup = materialSelectorPopup;
        this.eventSender = eventSender;
        this.worldEditController = worldEditController;
    }

    /// <summary>
    ///     Renders the world editor GUI.
    /// </summary>
    public void Render()
    {
        ImGui.SetNextWindowSize(new Vector2(300.0f, 150.0f), ImGuiCond.Appearing);
        if (!ImGui.Begin("World Editor", ImGuiWindowFlags.NoResize)) return;

        RenderMaterialControl();
        RenderZOffsetControl();
        RenderHelp();

        ImGui.End();
    }

    /// <summary>
    ///     Renders the material/material modifier selection control.
    /// </summary>
    private void RenderMaterialControl()
    {
        // Sync inputs with any backend changes.
        if (modifierBuffer == worldEditServices.MaterialModifier)
            modifierChangeInProgress = false;
        else if (!modifierChangeInProgress)
            modifierBuffer = worldEditServices.MaterialModifier;

        if (!ImGui.BeginTable("WorldEditMaterial", 2, ImGuiTableFlags.SizingStretchProp)) return;
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);

        var material = materialManager.Materials[worldEditServices.Material];
        var tile = material.MaterialSubtypes[worldEditServices.MaterialModifier];

        ImGui.TableNextColumn();
        var needToOpenSelector = guiExtensions.TileSpriteButton("#material", tile.TopFaceTileSpriteId,
            TileSprite.Wildcard,
            TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard);

        ImGui.TableNextColumn();
        ImGui.Text($"{material.MaterialName} (Material {worldEditServices.Material})");

        ImGui.Text("Modifier:");
        ImGui.SameLine();
        ImGui.InputInt("##mod", ref modifierBuffer);
        if (modifierBuffer != worldEditServices.MaterialModifier)
        {
            // New value entered by user, validate.
            if (modifierBuffer < 0 || modifierBuffer >= material.MaterialSubtypes.Count)
            {
                modifierBuffer = worldEditServices.MaterialModifier;
            }
            else
            {
                modifierChangeInProgress = true;
                worldEditController.SetSelectedMaterial(eventSender, worldEditServices.Material, modifierBuffer);
            }
        }

        ImGui.EndTable();

        if (needToOpenSelector) materialSelectorPopup.Open();
        materialSelectorPopup.Render();
        if (materialSelectorPopup.TryGetSelection(out var newMaterialId))
            worldEditController.SetSelectedMaterial(eventSender, newMaterialId, worldEditServices.MaterialModifier);
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
        ImGui.InputInt("##zoff", ref zOffsetBuffer);

        // Validate and update state if needed.
        if (zOffsetBuffer != worldEditServices.ZOffset)
        {
            if (zOffsetBuffer < ClientWorldEditConstants.MinZOffset ||
                zOffsetBuffer > ClientWorldEditConstants.MaxZOffset)
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

    private void RenderHelp()
    {
        ImGui.Separator();
        ImGui.TextColored(helpTextColor, "Scroll to change material.");
        ImGui.TextColored(helpTextColor, "Ctrl+Scroll to change Z offset.");
    }
}