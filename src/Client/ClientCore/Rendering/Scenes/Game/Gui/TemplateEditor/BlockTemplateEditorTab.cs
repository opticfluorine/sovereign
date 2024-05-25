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
using ImGuiNET;
using Sovereign.ClientCore.Components.Indexers;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
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

    private readonly EntityTable entityTable;
    private readonly IEventSender eventSender;

    private readonly GuiExtensions guiExtensions;
    private readonly BlockTemplateEntityIndexer indexer;
    private readonly TemplateEditorInternalController internalController;
    private readonly MaterialManager materialManager;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private bool initialized;
    private ulong selectedEntityId;
    private int selectedIndex;
    private List<ulong> sortedTemplateEntityIds = new();

    public BlockTemplateEditorTab(BlockTemplateEntityIndexer indexer, GuiExtensions guiExtensions,
        MaterialComponentCollection materials, MaterialModifierComponentCollection materialModifiers,
        NameComponentCollection names, MaterialManager materialManager, EntityTable entityTable,
        TemplateEditorInternalController internalController, IEventSender eventSender)
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

        indexer.OnIndexModified += RefreshList;
    }

    /// <summary>
    ///     Renders the block template editor tab if needed.
    /// </summary>
    public void Render()
    {
        if (!initialized)
        {
            Select(-1);
            initialized = true;
        }

        if (!ImGui.BeginTabItem("Blocks")) return;

        if (ImGui.BeginTable("blockTemplateOuter", 2, ImGuiTableFlags.SizingFixedFit))
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
    ///     Renders the block template browser for selecting block templates.
    /// </summary>
    private void RenderBrowser()
    {
        // Browser selector.
        var maxSize = ImGui.GetWindowSize();
        if (ImGui.BeginTable("blockTemplateBrowser", 2,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.RowBg, new Vector2 { X = 254.0f, Y = maxSize.Y - 90 }))
        {
            ImGui.TableSetupColumn("Material");
            ImGui.TableSetupColumn("Name");
            ImGui.TableHeadersRow();

            for (var i = 0; i < sortedTemplateEntityIds.Count; ++i)
            {
                // Gather data.
                var templateEntityId = sortedTemplateEntityIds[i];
                var name = names.HasComponentForEntity(templateEntityId)
                    ? names[templateEntityId]
                    : $"Block {templateEntityId}";
                var materialId = materials[templateEntityId];
                var materialModifier = materialModifiers[templateEntityId];
                var mat = materialManager.Materials[materialId];

                // Material column
                ImGui.TableNextColumn();
                if (guiExtensions.TileSpriteButton($"##matButtonFront{i}",
                        mat.MaterialSubtypes[materialModifier].SideFaceTileSpriteId,
                        TileSprite.Wildcard, TileSprite.Wildcard,
                        TileSprite.Wildcard, TileSprite.Wildcard)) Select(i);
                ImGui.SameLine();
                if (guiExtensions.TileSpriteButton($"##matButtonTop{i}",
                        mat.MaterialSubtypes[materialModifier].TopFaceTileSpriteId,
                        TileSprite.Wildcard, TileSprite.Wildcard,
                        TileSprite.Wildcard, TileSprite.Wildcard)) Select(i);
                ImGui.SameLine();
                if (guiExtensions.TileSpriteButton($"##matButtonObsc{i}",
                        mat.MaterialSubtypes[materialModifier].ObscuredTopFaceTileSpriteId,
                        TileSprite.Wildcard, TileSprite.Wildcard,
                        TileSprite.Wildcard, TileSprite.Wildcard)) Select(i);

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
            EntityId = entityTable.NextTemplateEntityId,
            Material = new MaterialPair(1, 0)
        };
        SaveDefinition(emptyDef);
    }

    /// <summary>
    ///     Saves a template entity to the server given its definition.
    /// </summary>
    /// <param name="definition">Definition.</param>
    private void SaveDefinition(EntityDefinition definition)
    {
        internalController.UpdateTemplateEntity(eventSender, definition);
    }

    private void Select(int index)
    {
        selectedIndex = index;
        if (selectedIndex >= 0 && selectedIndex < sortedTemplateEntityIds.Count)
            selectedEntityId = sortedTemplateEntityIds[index];
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