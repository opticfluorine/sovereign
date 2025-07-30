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

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Components.Indexers;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Components.Validators;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;

/// <summary>
///     Template editor tab for block template entities.
/// </summary>
public class BlockTemplateEditorTab
{
    /// <summary>
    ///     Color for selected row in the browser.
    /// </summary>
    private const uint SelectionColor = 0xFF773333;

    private readonly EntityDefinitionGenerator definitionGenerator;

    private readonly EntityTable entityTable;
    private readonly IEventSender eventSender;
    private readonly GuiComponentEditors guiComponentEditors;

    private readonly GuiExtensions guiExtensions;
    private readonly BlockTemplateEntityIndexer indexer;
    private readonly TemplateEditorInternalController internalController;
    private readonly MaterialManager materialManager;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentValidator nameComponentValidator;
    private readonly NameComponentCollection names;
    private bool initialized;
    private bool inputCastBlockShadows;
    private int inputMaterial;
    private int inputMaterialModifier;
    private string inputName = "";
    private EntityDefinition selectedDefinition = new();
    private ulong selectedEntityId;
    private int selectedIndex;
    private List<ulong> sortedTemplateEntityIds = new();

    public BlockTemplateEditorTab(BlockTemplateEntityIndexer indexer, GuiExtensions guiExtensions,
        MaterialComponentCollection materials, MaterialModifierComponentCollection materialModifiers,
        NameComponentCollection names, MaterialManager materialManager, EntityTable entityTable,
        TemplateEditorInternalController internalController, IEventSender eventSender,
        EntityDefinitionGenerator definitionGenerator, NameComponentValidator nameComponentValidator,
        GuiComponentEditors guiComponentEditors)
    {
        this.indexer = indexer;
        this.guiExtensions = guiExtensions;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.names = names;
        this.materialManager = materialManager;
        this.entityTable = entityTable;
        this.internalController = internalController;
        this.eventSender = eventSender;
        this.definitionGenerator = definitionGenerator;
        this.nameComponentValidator = nameComponentValidator;
        this.guiComponentEditors = guiComponentEditors;

        indexer.OnIndexModified += RefreshList;
    }

    /// <summary>
    ///     Renders the block template editor tab if needed.
    /// </summary>
    public void Render()
    {
        var fontSize = ImGui.GetFontSize();

        if (!initialized)
        {
            Select(-1);
            initialized = true;
        }

        if (!ImGui.BeginTabItem("Blocks")) return;

        if (ImGui.BeginTable("blockTemplateOuter", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, fontSize * 14.11f);
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
    ///     Renders the block template browser for selecting block templates.
    /// </summary>
    private void RenderBrowser()
    {
        var fontSize = ImGui.GetFontSize();

        // Browser selector.
        var maxSize = ImGui.GetWindowSize();
        if (ImGui.BeginTable("blockTemplateBrowser", 2,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.RowBg, new Vector2 { X = fontSize * 14.11f, Y = maxSize.Y - fontSize * 5.3f }))
        {
            ImGui.TableSetupColumn("Material");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            for (var i = 0; i < sortedTemplateEntityIds.Count; ++i)
            {
                // Gather data.
                var templateEntityId = sortedTemplateEntityIds[i];
                var name = names.HasComponentForEntity(templateEntityId)
                    ? names[templateEntityId]
                    : $"Block {templateEntityId - EntityConstants.FirstTemplateEntityId}";
                var materialId = materials[templateEntityId];
                var materialModifier = materialModifiers[templateEntityId];
                var mat = materialManager.Materials[materialId];

                // Material column
                ImGui.TableNextColumn();
                if (guiExtensions.TileSpriteButton($"##matButtonFront{i}",
                        mat.MaterialSubtypes[materialModifier].SideFaceTileSpriteId,
                        TileContextKey.AllWildcards)) Select(i);
                ImGui.SameLine();
                if (guiExtensions.TileSpriteButton($"##matButtonTop{i}",
                        mat.MaterialSubtypes[materialModifier].TopFaceTileSpriteId,
                        TileContextKey.AllWildcards)) Select(i);
                ImGui.SameLine();
                if (guiExtensions.TileSpriteButton($"##matButtonObsc{i}",
                        mat.MaterialSubtypes[materialModifier].ObscuredTopFaceTileSpriteId,
                        TileContextKey.AllWildcards)) Select(i);

                // Name column.
                ImGui.TableNextColumn();
                ImGui.Text(name);

                // Highlight if selected.
                if (i == selectedIndex)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, SelectionColor);
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, SelectionColor);
                }
            }

            ImGui.EndTable();

            // Bottom control row.
            if (ImGui.BeginTable("browserControls", 2, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("##Span", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextColumn();

                // Insert new template button.
                ImGui.TableNextColumn();
                if (ImGui.Button("+")) InsertNew();
                if (ImGui.IsItemHovered())
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text("Insert New");
                        ImGui.EndTooltip();
                    }

                ImGui.EndTable();
            }
        }
    }

    /// <summary>
    ///     Renders the editor pane for editing the selected block (if any).
    /// </summary>
    private void RenderEditor()
    {
        if (selectedIndex < 0) return;

        RenderComponentTable();
        RenderEditorControls();
    }

    /// <summary>
    ///     Renders the table of editable components for the block template.
    /// </summary>
    private void RenderComponentTable()
    {
        var fontSize = ImGui.GetFontSize();
        var maxSize = ImGui.GetWindowSize();
        if (!ImGui.BeginTable("componentTable", 1,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp,
                new Vector2 { X = maxSize.X - fontSize * 15.33f, Y = maxSize.Y - fontSize * 5.3f })) return;

        ImGui.TableNextColumn();

        if (ImGui.CollapsingHeader("Basic Information", ImGuiTreeNodeFlags.DefaultOpen))
            if (ImGui.BeginTable("BasicInfo", 2, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableNextColumn();
                ImGui.Text("Name:");
                ImGui.TableNextColumn();
                guiComponentEditors.NameEdit("##name", ref inputName);

                ImGui.EndTable();
            }

        if (ImGui.CollapsingHeader("Appearance", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.BeginTable("Appearance", 2, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableNextColumn();
                ImGui.Text("Cast Block Shadows:");
                ImGui.TableNextColumn();
                ImGui.Checkbox("##castBlockShadows", ref inputCastBlockShadows);

                ImGui.EndTable();
            }

            guiComponentEditors.MaterialEdit(ref inputMaterial, ref inputMaterialModifier);
        }

        ImGui.EndTable();
    }

    /// <summary>
    ///     Renders the save/cancel controls at the bottom of the editor.
    /// </summary>
    private void RenderEditorControls()
    {
        if (ImGui.BeginTable("Controls", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableNextColumn();
            var canSave = CanSave(out var reason);
            if (!canSave) ImGui.BeginDisabled();
            if (ImGui.Button("Save")) Save();
            if (!canSave)
            {
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text(reason);
                        ImGui.EndTooltip();
                    }

                ImGui.EndDisabled();
            }

            ImGui.TableNextColumn();
            if (ImGui.Button("Cancel")) Cancel();

            ImGui.EndTable();
        }
    }

    /// <summary>
    ///     Inserts a new block template at the end of the list.
    /// </summary>
    private void InsertNew()
    {
        // Save a new template entity to the server with default values.
        // Once the server accepts this, it will synchronize the template with all clients,
        // and the template will appear in the editor on the following tick.
        var emptyDef = new EntityDefinition
        {
            EntityId = entityTable.TakeNextTemplateEntityId(),
            Material = new MaterialPair(1, 0),
            CastBlockShadows = true
        };
        SaveDefinition(emptyDef);
    }

    /// <summary>
    ///     Validates the currently edited entity and determines if it can be saved.
    /// </summary>
    /// <param name="reason">If invalid, will be set to the reason(s) why.</param>
    /// <returns>true if valid and can be saved, false otherwise.</returns>
    private bool CanSave(out string reason)
    {
        var sb = new StringBuilder();
        var valid = true;

        if (!nameComponentValidator.IsValid(inputName))
        {
            sb.Append("Name is invalid.\n");
            valid = false;
        }

        if (inputMaterial < 1 || inputMaterial >= materialManager.Materials.Count)
        {
            sb.Append("Material ID is invalid.\n");
            valid = false;
        }
        else if (inputMaterialModifier < 0 ||
                 inputMaterialModifier >= materialManager.Materials[inputMaterial].MaterialSubtypes.Count)
        {
            sb.Append("Material modifier is invalid.\n");
            valid = false;
        }

        reason = sb.ToString().TrimEnd();
        return valid;
    }

    /// <summary>
    ///     Saves the currently edited template entity.
    /// </summary>
    private void Save()
    {
        selectedDefinition.Name = inputName.Length > 0 ? inputName : null;
        selectedDefinition.Material = new MaterialPair(inputMaterial, inputMaterialModifier);
        selectedDefinition.CastBlockShadows = inputCastBlockShadows;
        SaveDefinition(selectedDefinition);
    }

    /// <summary>
    ///     Cancels editing and reverts any changes to the currently selected entity.
    /// </summary>
    private void Cancel()
    {
        Select(selectedIndex);
    }

    /// <summary>
    ///     Saves a template entity to the server given its definition.
    /// </summary>
    /// <param name="definition">Definition.</param>
    private void SaveDefinition(EntityDefinition definition)
    {
        internalController.UpdateTemplateEntity(eventSender, definition);
    }

    /// <summary>
    ///     Selects the block template entity at the given position in the ordered list.
    /// </summary>
    /// <param name="index">Index into ordered list of block template entities.</param>
    private void Select(int index)
    {
        selectedIndex = index;
        if (selectedIndex >= 0 && selectedIndex < sortedTemplateEntityIds.Count)
        {
            selectedEntityId = sortedTemplateEntityIds[index];
            selectedDefinition = definitionGenerator.GenerateDefinition(selectedEntityId);
            inputName = selectedDefinition.Name ?? "";
            inputCastBlockShadows = selectedDefinition.CastBlockShadows;
            if (selectedDefinition.Material != null)
            {
                inputMaterial = selectedDefinition.Material.MaterialId;
                inputMaterialModifier = selectedDefinition.Material.MaterialModifier;
            }
        }
    }

    /// <summary>
    ///     Updates the block template entity list.
    /// </summary>
    private void RefreshList()
    {
        sortedTemplateEntityIds = indexer.BlockTemplateEntities
            .Order()
            .ToList();

        // Adjust selection index if needed.
        for (var i = 0; i < sortedTemplateEntityIds.Count; ++i)
        {
            if (sortedTemplateEntityIds[i] != selectedEntityId) continue;
            selectedIndex = i;
            break;
        }

        // Just in case the index wasn't updated but is now out of bounds...
        if (selectedIndex > sortedTemplateEntityIds.Count) selectedIndex = -1;
    }
}