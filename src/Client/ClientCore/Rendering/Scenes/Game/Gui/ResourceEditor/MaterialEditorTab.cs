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

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Resource editor tab for Materials and MaterialModifiers.
/// </summary>
public class MaterialEditorTab
{
    /// <summary>
    ///     Color for selected row in the browser.
    /// </summary>
    private const uint SelectionColor = 0xFF773333;

    /// <summary>
    ///     Currently edited material.
    /// </summary>
    private readonly Material editingMaterial = new();

    private readonly GuiExtensions guiExtensions;
    private readonly MaterialManager materialManager;

    public MaterialEditorTab(MaterialManager materialManager, GuiExtensions guiExtensions)
    {
        this.materialManager = materialManager;
        this.guiExtensions = guiExtensions;
    }

    /// <summary>
    ///     Renders the Materials resource editor tab.
    /// </summary>
    public void Render()
    {
        if (!ImGui.BeginTabItem("Materials")) return;

        if (ImGui.BeginTable("materialOuter", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 254.0f);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextColumn();
            RenderBrowser();

            ImGui.TableNextColumn();
            RenderEditor();

            ImGui.EndTable();
        }

        ImGui.EndTabItem();
    }

    /// <summary>
    ///     Renders the material browser that allows the user to select a material for editing.
    /// </summary>
    private void RenderBrowser()
    {
        // Browser selector.
        var maxSize = ImGui.GetWindowSize();
        if (ImGui.BeginTable("materialBrowser", 3,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.RowBg, new Vector2 { X = 254.0f, Y = maxSize.Y - 90 }))
        {
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Material");
            ImGui.TableSetupColumn("Name");
            ImGui.TableHeadersRow();

            for (var i = 0; i < materialManager.Materials.Count; ++i)
            {
                var mat = materialManager.Materials[i];

                // ID column
                ImGui.TableNextColumn();
                ImGui.Text($"{i}");

                // Material column
                ImGui.TableNextColumn();
                if (guiExtensions.TileSpriteButton($"##matButtonFront{i}", mat.MaterialSubtypes[0].SideFaceTileSpriteId,
                        TileSprite.Wildcard, TileSprite.Wildcard,
                        TileSprite.Wildcard, TileSprite.Wildcard)) Select(i);
                ImGui.SameLine();
                if (guiExtensions.TileSpriteButton($"##matButtonTop{i}", mat.MaterialSubtypes[0].TopFaceTileSpriteId,
                        TileSprite.Wildcard, TileSprite.Wildcard,
                        TileSprite.Wildcard, TileSprite.Wildcard)) Select(i);
                ImGui.SameLine();
                if (guiExtensions.TileSpriteButton($"##matButtonObsc{i}",
                        mat.MaterialSubtypes[0].ObscuredTopFaceTileSpriteId,
                        TileSprite.Wildcard, TileSprite.Wildcard,
                        TileSprite.Wildcard, TileSprite.Wildcard)) Select(i);

                // Name column.
                ImGui.TableNextColumn();
                ImGui.Text(mat.MaterialName);

                // Highlight if selected.
                if (i == editingMaterial.Id)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, SelectionColor);
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, SelectionColor);
                }
            }

            ImGui.EndTable();

            // Bottom control row.
            if (ImGui.BeginTable("browserControls", 3, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("##Span", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextColumn();

                // Insert new sprite button.
                ImGui.TableNextColumn();
                if (ImGui.Button("+")) InsertNewMaterial();
                if (ImGui.IsItemHovered())
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text("Insert New");
                        ImGui.EndTooltip();
                    }

                // Remove selected sprite button.
                ImGui.TableNextColumn();
                var canRemove = CanRemoveMaterial(out var reason);
                if (!canRemove) ImGui.BeginDisabled();
                if (ImGui.Button("-")) RemoveSelectedMaterial();
                if (!canRemove) ImGui.EndDisabled();
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text(canRemove ? "Remove Selected" : reason);
                        ImGui.EndTooltip();
                    }


                ImGui.EndTable();
            }
        }
    }

    /// <summary>
    ///     Renders the editor pane.
    /// </summary>
    private void RenderEditor()
    {
    }

    private void Select(int id)
    {
    }

    private void InsertNewMaterial()
    {
    }

    private void RemoveSelectedMaterial()
    {
    }

    private bool CanRemoveMaterial([NotNullWhen(false)] out string? reason)
    {
        reason = null;
        return true;
    }
}