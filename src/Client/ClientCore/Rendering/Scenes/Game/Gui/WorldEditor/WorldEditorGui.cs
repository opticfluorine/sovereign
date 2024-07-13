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

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.WorldEditor;

/// <summary>
///     Manages the GUI for the client-side world editor.
/// </summary>
public class WorldEditorGui
{
    private readonly GuiExtensions guiExtensions;

    /// <summary>
    ///     Color used by help text.
    /// </summary>
    private readonly Vector4 HelpTextColor = new(0.8f, 0.8f, 0.8f, 1.0f);

    private readonly MaterialManager materialManager;
    private readonly ClientWorldEditServices worldEditServices;

    public WorldEditorGui(ClientWorldEditServices worldEditServices, MaterialManager materialManager,
        GuiExtensions guiExtensions)
    {
        this.worldEditServices = worldEditServices;
        this.materialManager = materialManager;
        this.guiExtensions = guiExtensions;
    }

    /// <summary>
    ///     Renders the world editor GUI.
    /// </summary>
    public void Render()
    {
        ImGui.SetNextWindowSize(new Vector2(300.0f, 150.0f), ImGuiCond.Appearing);
        if (!ImGui.Begin("World Editor")) return;

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
        if (!ImGui.BeginTable("WorldEditMaterial", 2, ImGuiTableFlags.SizingStretchProp)) return;

        var material = materialManager.Materials[worldEditServices.Material];
        var tile = material.MaterialSubtypes[worldEditServices.MaterialModifier];

        ImGui.TableNextColumn();
        guiExtensions.TileSpriteButton("#material", tile.TopFaceTileSpriteId, TileSprite.Wildcard,
            TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard);

        ImGui.TableNextColumn();
        ImGui.Text($"{material.MaterialName} (Material {worldEditServices.Material})");
        ImGui.Text($"Modifier {worldEditServices.MaterialModifier}");

        ImGui.EndTable();
    }

    /// <summary>
    ///     Renders the z-offset selection control.
    /// </summary>
    private void RenderZOffsetControl()
    {
        ImGui.Separator();
        ImGui.Text($"Z Offset: {worldEditServices.ZOffset}");
    }

    private void RenderHelp()
    {
        ImGui.Separator();
        ImGui.TextColored(HelpTextColor, "Scroll to change material.");
        ImGui.TextColored(HelpTextColor, "Ctrl+Scroll to change Z offset.");
    }
}