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
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Popup selector for materials.
/// </summary>
/// <remarks>
///     This popup only allows for the selection of materials, not material modifiers.
///     The intent is for a material-valued control to include toggle buttons for scrolling
///     the modifiers attached to the selected material.
/// </remarks>
public class MaterialSelectorPopup
{
    private const string PopupName = "Select Material";
    private readonly GuiExtensions guiExtensions;
    private readonly MaterialManager materialManager;

    private bool isSelected;
    private int selectedMaterial;

    public MaterialSelectorPopup(MaterialManager materialManager, GuiExtensions guiExtensions)
    {
        this.materialManager = materialManager;
        this.guiExtensions = guiExtensions;
    }

    /// <summary>
    ///     Opens the selector popup.
    /// </summary>
    public void Open()
    {
        isSelected = false;
        selectedMaterial = 0;

        ImGui.OpenPopup(PopupName);
    }

    /// <summary>
    ///     Renders the selector popup if open, otherwise does nothing.
    /// </summary>
    public void Render()
    {
        if (!ImGui.BeginPopup(PopupName)) return;

        if (ImGui.BeginTable("matTbl", 3,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit))
        {
            for (var i = 1; i < materialManager.Materials.Count; ++i)
            {
                var mat = materialManager.Materials[i];

                // ID column
                ImGui.TableNextColumn();
                ImGui.Text($"{i}");

                // Material column
                ImGui.TableNextColumn();
                if (guiExtensions.TileSpriteButton($"##matButtonFront{i}", mat.MaterialSubtypes[0].SideFaceTileSpriteId,
                        TileContextKey.AllWildcards)) Select(i);
                ImGui.SameLine();
                if (guiExtensions.TileSpriteButton($"##matButtonTop{i}", mat.MaterialSubtypes[0].TopFaceTileSpriteId,
                        TileContextKey.AllWildcards)) Select(i);
                ImGui.SameLine();
                if (guiExtensions.TileSpriteButton($"##matButtonObsc{i}",
                        mat.MaterialSubtypes[0].ObscuredTopFaceTileSpriteId,
                        TileContextKey.AllWildcards)) Select(i);

                // Name column.
                ImGui.TableNextColumn();
                ImGui.Text(mat.MaterialName);
            }

            ImGui.EndTable();
        }

        ImGui.EndPopup();
    }

    /// <summary>
    ///     Tries to get the latest selection.
    /// </summary>
    /// <param name="material">Selected material.</param>
    /// <returns>true if a selection has been made since the last Open() call, false otherwise.</returns>
    public bool TryGetSelection(out int material)
    {
        material = selectedMaterial;
        var result = isSelected;
        isSelected = false;
        return result;
    }

    /// <summary>
    ///     Selects a material.
    /// </summary>
    /// <param name="materialId">Selected material ID.</param>
    private void Select(int materialId)
    {
        selectedMaterial = materialId;
        isSelected = true;
        ImGui.CloseCurrentPopup();
    }
}