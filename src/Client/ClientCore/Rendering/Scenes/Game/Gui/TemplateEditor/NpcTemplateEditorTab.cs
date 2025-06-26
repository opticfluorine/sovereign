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
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Components.Validators;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;

/// <summary>
///     Template editor tab for block template entities.
/// </summary>
public class NpcTemplateEditorTab
{
    /// <summary>
    ///     Color for selected row in the browser.
    /// </summary>
    private const uint SelectionColor = 0xFF773333;

    private const int DefaultAnimatedSprite = 0;
    private const float DefaultShadowRadius = 0.1f;
    private const string DefaultName = "New NPC";

    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly AppearanceControlGroup appearanceControlGroup;
    private readonly BasicInformationControlGroup basicInformationControlGroup;
    private readonly BehaviorControlGroup behaviorControlGroup;
    private readonly EntityDefinitionGenerator definitionGenerator;
    private readonly EntityDataControlGroup entityDataControlGroup;
    private readonly EntityTable entityTable;
    private readonly GuiExtensions guiExtensions;
    private readonly NpcTemplateEntityIndexer indexer;
    private readonly NameComponentValidator nameComponentValidator;
    private readonly NameComponentCollection names;
    private readonly TemplateEntityDataClient templateEntityDataClient;

    private bool initialized;
    private EntityDefinition selectedDefinition = new();
    private ulong selectedEntityId;
    private int selectedIndex;
    private List<ulong> sortedTemplateEntityIds = new();

    public NpcTemplateEditorTab(NpcTemplateEntityIndexer indexer, GuiExtensions guiExtensions,
        NameComponentCollection names, EntityTable entityTable,
        EntityDefinitionGenerator definitionGenerator, NameComponentValidator nameComponentValidator,
        AnimatedSpriteComponentCollection animatedSprites, TemplateEntityDataClient templateEntityDataClient,
        BasicInformationControlGroup basicInformationControlGroup, AppearanceControlGroup appearanceControlGroup,
        EntityDataControlGroup entityDataControlGroup, BehaviorControlGroup behaviorControlGroup)
    {
        this.indexer = indexer;
        this.guiExtensions = guiExtensions;
        this.names = names;
        this.entityTable = entityTable;
        this.definitionGenerator = definitionGenerator;
        this.nameComponentValidator = nameComponentValidator;
        this.animatedSprites = animatedSprites;
        this.basicInformationControlGroup = basicInformationControlGroup;
        this.appearanceControlGroup = appearanceControlGroup;
        this.templateEntityDataClient = templateEntityDataClient;
        this.entityDataControlGroup = entityDataControlGroup;
        this.behaviorControlGroup = behaviorControlGroup;

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


        if (!ImGui.BeginTabItem("NPCs")) return;

        if (ImGui.BeginTable("npcTemplateOuter", 2, ImGuiTableFlags.SizingFixedFit))
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
        if (ImGui.BeginTable("npcTemplateBrowser", 2,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.RowBg, new Vector2 { X = fontSize * 14.11f, Y = maxSize.Y - fontSize * 5.3f }))
        {
            ImGui.TableSetupColumn("Animated Sprite");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            for (var i = 0; i < sortedTemplateEntityIds.Count; ++i)
            {
                // Gather data.
                var templateEntityId = sortedTemplateEntityIds[i];
                var animatedSprite = animatedSprites.HasComponentForEntity(templateEntityId)
                    ? animatedSprites[templateEntityId]
                    : DefaultAnimatedSprite;
                var name = names.HasComponentForEntity(templateEntityId)
                    ? names[templateEntityId]
                    : $"Block {templateEntityId - EntityConstants.FirstTemplateEntityId}";

                // Animated Sprite column.
                ImGui.TableNextColumn();
                if (guiExtensions.AnimatedSpriteButton($"##animSprButton{i}", animatedSprite, Orientation.South,
                        AnimationPhase.Default)) Select(i);

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
    ///     Renders the editor pane for editing the selected NPC (if any).
    /// </summary>
    private void RenderEditor()
    {
        if (selectedIndex < 0) return;

        RenderComponentTable();
        RenderEditorControls();
    }

    /// <summary>
    ///     Renders the table of editable components for the NPC template.
    /// </summary>
    private void RenderComponentTable()
    {
        var fontSize = ImGui.GetFontSize();
        var maxSize = ImGui.GetWindowSize();
        if (!ImGui.BeginTable("componentTable", 1,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp,
                new Vector2 { X = maxSize.X - fontSize * 15.88f, Y = maxSize.Y - fontSize * 5.3f })) return;

        ImGui.TableNextColumn();

        basicInformationControlGroup.Render(selectedDefinition);
        appearanceControlGroup.Render(selectedDefinition);
        behaviorControlGroup.Render(selectedDefinition);
        entityDataControlGroup.Render();

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
            if (ImGui.Button("Save")) Save(selectedDefinition, entityDataControlGroup.GetInputEntityData());
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
            AnimatedSpriteId = DefaultAnimatedSprite,
            Drawable = true,
            CastShadows = new Shadow
            {
                Radius = DefaultShadowRadius
            },
            Name = DefaultName
        };
        Save(emptyDef, new Dictionary<string, string>());
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

        if (!nameComponentValidator.IsValid(selectedDefinition.Name))
        {
            sb.Append("Name is invalid.\n");
            valid = false;
        }

        reason = sb.ToString().TrimEnd();
        return valid;
    }

    /// <summary>
    ///     Saves the currently edited template entity.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <param name="entityData">Entity key-value data.</param>
    private void Save(EntityDefinition definition, Dictionary<string, string> entityData)
    {
        templateEntityDataClient.SetTemplateEntity(definition, entityData);
    }

    /// <summary>
    ///     Cancels editing and reverts any changes to the currently selected entity.
    /// </summary>
    private void Cancel()
    {
        Select(selectedIndex);
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
            entityDataControlGroup.SelectEntity(selectedEntityId);
        }
    }

    /// <summary>
    ///     Updates the NPC template entity list.
    /// </summary>
    private void RefreshList()
    {
        sortedTemplateEntityIds = indexer.NpcTemplateEntities
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