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
    ///     Maximum material name length.
    /// </summary>
    private const int MaxNameLen = 32;

    private readonly GuiExtensions guiExtensions;
    private readonly MaterialManager materialManager;
    private readonly TileSpriteSelectorPopup tileSpriteSelector;

    /// <summary>
    ///     Face of subtype currently being edited.
    /// </summary>
    private Face editingFace;

    /// <summary>
    ///     Currently edited material.
    /// </summary>
    private Material editingMaterial = new();

    private string editingName = "";

    /// <summary>
    ///     Material modifier of subtype currently being edited.
    /// </summary>
    private int editingSubtype;

    private bool initialized;

    public MaterialEditorTab(MaterialManager materialManager, GuiExtensions guiExtensions,
        TileSpriteSelectorPopup tileSpriteSelector)
    {
        this.materialManager = materialManager;
        this.guiExtensions = guiExtensions;
        this.tileSpriteSelector = tileSpriteSelector;
    }

    /// <summary>
    ///     Renders the Materials resource editor tab.
    /// </summary>
    public void Render()
    {
        if (!initialized)
        {
            Select(1);
            initialized = true;
        }

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
                ImGuiTableFlags.RowBg, new Vector2 { X = 254.0f, Y = maxSize.Y - 125 }))
        {
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Material");
            ImGui.TableSetupColumn("Name");
            ImGui.TableHeadersRow();

            for (var i = 1; i < materialManager.Materials.Count; ++i)
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
        RenderEditorTopBar();
        RenderSubtypeTable();
        RenderEditorControls();
    }

    /// <summary>
    ///     Renders the top control bar of the editor.
    /// </summary>
    private void RenderEditorTopBar()
    {
        if (ImGui.BeginTable("Header", 5, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextColumn();
            ImGui.Text($"Material {editingMaterial.Id}");

            ImGui.TableNextColumn();

            ImGui.TableNextColumn();
            ImGui.Text("Name:");

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(180.0f);
            ImGui.InputText("##materialName", ref editingName, MaxNameLen);

            ImGui.TableNextColumn();
            if (ImGui.Button("Add New Subtype"))
                editingMaterial.MaterialSubtypes.Add(new MaterialSubtype
                    { MaterialModifier = editingMaterial.MaterialSubtypes.Count });

            ImGui.EndTable();
        }

        ImGui.Separator();
    }

    /// <summary>
    ///     Renders the material subtype editor table.
    /// </summary>
    private void RenderSubtypeTable()
    {
        var maxSize = ImGui.GetWindowSize();
        if (!ImGui.BeginTable("subtypeTable", 5,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg,
                new Vector2 { X = maxSize.X - 276, Y = maxSize.Y - 156 })) return;

        ImGui.TableSetupColumn("Modifier ID");
        ImGui.TableSetupColumn("Front Face");
        ImGui.TableSetupColumn("Top Face");
        ImGui.TableSetupColumn("Obscured Top Face");
        ImGui.TableSetupColumn("");

        ImGui.TableSetupScrollFreeze(1, 1);
        ImGui.TableHeadersRow();

        for (var i = 0; i < editingMaterial.MaterialSubtypes.Count; ++i)
            RenderSubtypeRow(i);

        HandleTileSpriteSelector();

        ImGui.EndTable();
    }

    /// <summary>
    ///     Renders a single table row in the editor.
    /// </summary>
    /// <param name="rowIndex">Row index.</param>
    private void RenderSubtypeRow(int rowIndex)
    {
        var subtype = editingMaterial.MaterialSubtypes[rowIndex];

        ImGui.TableNextColumn();
        ImGui.Text($"{subtype.MaterialModifier}");

        ImGui.TableNextColumn();
        if (guiExtensions.TileSpriteButton($"##front{rowIndex}", subtype.SideFaceTileSpriteId,
                TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard))
            OpenFaceEditor(rowIndex, Face.Front);

        ImGui.TableNextColumn();
        if (guiExtensions.TileSpriteButton($"##top{rowIndex}", subtype.TopFaceTileSpriteId,
                TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard))
            OpenFaceEditor(rowIndex, Face.Top);

        ImGui.TableNextColumn();
        if (guiExtensions.TileSpriteButton($"##obsc{rowIndex}", subtype.ObscuredTopFaceTileSpriteId,
                TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard))
            OpenFaceEditor(rowIndex, Face.ObscuredTop);

        ImGui.TableNextColumn();
        var canRemove = editingMaterial.MaterialSubtypes.Count > 1;
        if (!canRemove) ImGui.BeginDisabled();
        if (ImGui.Button($"-##{rowIndex}")) RemoveSubtype(rowIndex);
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            if (ImGui.BeginTooltip())
            {
                ImGui.Text(canRemove ? "Remove Subtype" : "Cannot remove last subtype");
                ImGui.EndTooltip();
            }
        }

        if (!canRemove) ImGui.EndDisabled();
    }

    /// <summary>
    ///     Handles the tile sprite selector if it is open.
    /// </summary>
    private void HandleTileSpriteSelector()
    {
        tileSpriteSelector.Render();
        if (tileSpriteSelector.TryGetSelection(out var selectedId))
        {
            var subtype = editingMaterial.MaterialSubtypes[editingSubtype];
            switch (editingFace)
            {
                case Face.Front:
                    subtype.SideFaceTileSpriteId = selectedId;
                    break;

                case Face.Top:
                    subtype.TopFaceTileSpriteId = selectedId;
                    break;

                case Face.ObscuredTop:
                    subtype.ObscuredTopFaceTileSpriteId = selectedId;
                    break;
            }
        }
    }

    /// <summary>
    ///     Opens the editor for the given material face.
    /// </summary>
    /// <param name="rowIndex">Material modifier of subtype to edit.</param>
    /// <param name="face">Face of subtype to edit.</param>
    private void OpenFaceEditor(int rowIndex, Face face)
    {
        editingSubtype = rowIndex;
        editingFace = face;
        tileSpriteSelector.Open(false);
    }

    /// <summary>
    ///     Renders the bottom control bar of the editor.
    /// </summary>
    private void RenderEditorControls()
    {
        if (ImGui.BeginTable("Controls", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableNextColumn();
            if (ImGui.Button("Save")) Save();

            ImGui.TableNextColumn();
            if (ImGui.Button("Cancel")) Reset();

            ImGui.EndTable();
        }
    }

    /// <summary>
    ///     Selects the material with the given ID for editing.
    /// </summary>
    /// <param name="id">Material ID to edit.</param>
    private void Select(int id)
    {
        editingMaterial.Id = id;
        Reset();
    }

    /// <summary>
    ///     Saves the currently edited material.
    /// </summary>
    private void Save()
    {
        editingMaterial.MaterialName = editingName;
        materialManager.Update(editingMaterial);
    }

    /// <summary>
    ///     Resets the editing material to the last saved state.
    /// </summary>
    private void Reset()
    {
        editingMaterial = new Material(materialManager.Materials[editingMaterial.Id]);
        editingName = editingMaterial.MaterialName;
    }

    /// <summary>
    ///     Inserts a new material after the currently selected material.
    /// </summary>
    private void InsertNewMaterial()
    {
        materialManager.InsertNew(editingMaterial.Id + 1);
    }

    /// <summary>
    ///     Removes the currently selected material.
    /// </summary>
    private void RemoveSelectedMaterial()
    {
        materialManager.Remove(editingMaterial.Id);
        Select(editingMaterial.Id >= materialManager.Materials.Count
            ? materialManager.Materials.Count - 1
            : editingMaterial.Id);
    }

    /// <summary>
    ///     Checks whether the currently selected material can be removed.
    /// </summary>
    /// <param name="reason">Reason why the material cannot be removed. Only set if method returns false.</param>
    /// <returns>true if the material can be removed, false otherwise.</returns>
    private bool CanRemoveMaterial([NotNullWhen(false)] out string? reason)
    {
        reason = null;

        if (materialManager.Materials.Count <= 2)
        {
            reason = "Cannot remove last material.";
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Removes the subtype at the given row.
    /// </summary>
    /// <param name="rowIndex">Row index (material modifier).</param>
    private void RemoveSubtype(int rowIndex)
    {
    }

    /// <summary>
    ///     Enum for specifying which material face is being edited.
    /// </summary>
    private enum Face
    {
        Front,
        Top,
        ObscuredTop
    }
}