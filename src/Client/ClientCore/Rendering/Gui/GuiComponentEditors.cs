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

using ImGuiNET;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Rendering.Gui;

/// <summary>
///     Reusable GUI controls for component editing.
/// </summary>
public class GuiComponentEditors
{
    private readonly GuiExtensions guiExtensions;
    private readonly MaterialManager materialManager;
    private readonly MaterialSelectorPopup materialSelectorPopup;

    public GuiComponentEditors(MaterialSelectorPopup materialSelectorPopup, GuiExtensions guiExtensions,
        MaterialManager materialManager)
    {
        this.materialSelectorPopup = materialSelectorPopup;
        this.guiExtensions = guiExtensions;
        this.materialManager = materialManager;
    }

    /// <summary>
    ///     Renders an input control for the Name component.
    /// </summary>
    /// <param name="id">GUI ID.</param>
    /// <param name="buffer">Input buffer to hold user input.</param>
    public void NameEdit(string id, ref string buffer)
    {
        var fontSize = ImGui.GetFontSize();
        ImGui.SetNextItemWidth(fontSize * 16.0f);
        ImGui.InputText(id, ref buffer, EntityConstants.MaxNameLength);
    }

    /// <summary>
    ///     Renders an input control for the Material and MaterialModifier components.
    /// </summary>
    /// <param name="material">Selected material ID.</param>
    /// <param name="modifier">Selected material modifier.</param>
    public void MaterialEdit(ref int material, ref int modifier)
    {
        if (!ImGui.BeginTable("materialEditLayout", 5, ImGuiTableFlags.SizingFixedFit)) return;

        // Clamp selection variables to limits.
        if (material < 1 || material >= materialManager.Materials.Count)
            material = 1;

        if (modifier < 0 || modifier >= materialManager.Materials[material].MaterialSubtypes.Count)
            modifier = 0;

        var mat = materialManager.Materials[material];

        ImGui.TableNextColumn();
        ImGui.Text("Material:");

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(120.0f);
        ImGui.InputInt("##matIntSelect", ref material);

        ImGui.TableNextColumn();
        ImGui.Text("Modifier:");

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(120.0f);
        ImGui.InputInt("##matMod", ref modifier);

        // Reclamp selection variables to limits in case something was modified.
        if (material < 1 || material >= materialManager.Materials.Count)
            material = 1;

        if (modifier < 0 || modifier >= materialManager.Materials[material].MaterialSubtypes.Count)
            modifier = 0;

        ImGui.TableNextColumn();
        var needToOpenPopup = guiExtensions.TileSpriteButton("##matButtonFront",
            mat.MaterialSubtypes[modifier].SideFaceTileSpriteId, TileContextKey.AllWildcards);
        ImGui.SameLine();
        needToOpenPopup = needToOpenPopup || guiExtensions.TileSpriteButton("##matButtonTop",
            mat.MaterialSubtypes[modifier].TopFaceTileSpriteId, TileContextKey.AllWildcards);
        ImGui.SameLine();
        needToOpenPopup = needToOpenPopup || guiExtensions.TileSpriteButton("##matButtonObsc",
            mat.MaterialSubtypes[modifier].ObscuredTopFaceTileSpriteId, TileContextKey.AllWildcards);

        if (needToOpenPopup) materialSelectorPopup.Open();
        materialSelectorPopup.Render();
        if (materialSelectorPopup.TryGetSelection(out var newMaterial))
        {
            material = newMaterial;
            modifier = 0;
        }

        ImGui.EndTable();
    }
}