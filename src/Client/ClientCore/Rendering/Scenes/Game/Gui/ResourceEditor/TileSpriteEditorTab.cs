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
using System.Linq;
using System.Numerics;
using System.Text;
using Castle.Core.Logging;
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Resource editor tab for Tile Sprites.
/// </summary>
public class TileSpriteEditorTab
{
    /// <summary>
    ///     Color for selected row in the browser.
    /// </summary>
    private const uint SelectionColor = 0xFF773333;

    private readonly AnimatedSpriteSelectorPopup animatedSpriteSelector;

    private readonly GuiExtensions guiExtensions;
    private readonly MaterialManager materialManager;

    private readonly TileSpriteManager tileSpriteManager;
    private readonly TileSpriteSelectorPopup tileSpriteSelector;
    private int editingCol;
    private Orientation editingNeighbor;
    private int editingRow;

    /// <summary>
    ///     Currently edited tile sprite.
    /// </summary>
    private TileSprite? editingSprite;

    /// <summary>
    ///     Initialization flag.
    /// </summary>
    private bool initialized;

    public TileSpriteEditorTab(TileSpriteManager tileSpriteManager, GuiExtensions guiExtensions,
        MaterialManager materialManager, AnimatedSpriteSelectorPopup animatedSpriteSelector,
        TileSpriteSelectorPopup tileSpriteSelector)
    {
        this.tileSpriteManager = tileSpriteManager;
        this.guiExtensions = guiExtensions;
        this.materialManager = materialManager;
        this.animatedSpriteSelector = animatedSpriteSelector;
        this.tileSpriteSelector = tileSpriteSelector;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Renders the Tile Sprites resource editor tab.
    /// </summary>
    public void Render()
    {
        if (!initialized)
        {
            Select(0);
            initialized = true;
        }

        if (ImGui.BeginTabItem("Tile Sprites"))
        {
            if (ImGui.BeginTable("tileSprOuter", 2, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("#browser", ImGuiTableColumnFlags.WidthFixed, 220.0f);
                ImGui.TableSetupColumn("#editor", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextColumn();
                RenderBrowser();

                ImGui.TableNextColumn();
                RenderEditor();

                ImGui.EndTable();
            }

            ImGui.EndTabItem();
        }
    }

    /// <summary>
    ///     Renders the tile sprite browser that allows the user to select a tile sprite for editing.
    /// </summary>
    private void RenderBrowser()
    {
        if (editingSprite == null)
        {
            Logger.Error("RenderBrowser(): editingSprite is null.");
            return;
        }

        // Browser selector.
        var maxSize = ImGui.GetWindowSize();
        if (ImGui.BeginTable("tileSprBrowser", 2,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.RowBg, new Vector2 { X = 222.0f, Y = maxSize.Y - 90 }))
        {
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Tile Sprite");
            ImGui.TableHeadersRow();

            for (var i = 0; i < tileSpriteManager.TileSprites.Count; ++i)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{i}");
                ImGui.TableNextColumn();
                if (guiExtensions.TileSpriteButton($"##spriteButton{i}", i,
                        TileSprite.Wildcard, TileSprite.Wildcard,
                        TileSprite.Wildcard, TileSprite.Wildcard)) Select(i);

                if (i == editingSprite.Id)
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
                if (ImGui.Button("+")) InsertNewTileSprite();
                if (ImGui.IsItemHovered())
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text("Insert New");
                        ImGui.EndTooltip();
                    }

                // Remove selected sprite button.
                ImGui.TableNextColumn();
                var canRemove = CanRemoveSprite(out var reason);
                if (!canRemove) ImGui.BeginDisabled();
                if (ImGui.Button("-")) RemoveSelectedTileSprite();
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
        RenderContextTable();
        RenderEditorControls();
    }

    /// <summary>
    ///     Renders the top control bar of the editor.
    /// </summary>
    private void RenderEditorTopBar()
    {
        if (editingSprite == null)
        {
            Logger.Error("RenderTopBar(): editingSprite is null.");
            return;
        }

        if (ImGui.BeginTable("Header", 4, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextColumn();
            ImGui.Text($"Tile Sprite {editingSprite.Id}");

            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            if (ImGui.Button("Add New Context")) editingSprite.TileContexts.Add(new TileContext());

            ImGui.TableNextColumn();
            if (ImGui.Button("Sort Contexts")) editingSprite.ReSortContexts();

            ImGui.EndTable();
        }

        ImGui.Separator();
    }

    /// <summary>
    ///     Renders the table of tile contexts.
    /// </summary>
    private void RenderContextTable()
    {
        if (editingSprite == null)
        {
            Logger.Error("RenderContextTable(): editingSprite is null.");
            return;
        }

        var maxSize = ImGui.GetWindowSize();
        var maxLayers = editingSprite.TileContexts
            .Select(ctx => ctx.AnimatedSpriteIds.Count).Max();
        if (ImGui.BeginTable("contextTable", maxLayers + 8,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg,
                new Vector2 { X = maxSize.X - 276, Y = maxSize.Y - 121 }))
        {
            ImGui.TableSetupColumn(""); // Discard button
            ImGui.TableSetupColumn(""); // Preview
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.None, 16.0f); // Spacer
            ImGui.TableSetupColumn("North");
            ImGui.TableSetupColumn("East");
            ImGui.TableSetupColumn("South");
            ImGui.TableSetupColumn("West");
            ImGui.TableSetupColumn("+/-");
            for (var i = 0; i < maxLayers; ++i) ImGui.TableSetupColumn($"Layer {i + 1}");

            ImGui.TableSetupScrollFreeze(7, 1);
            ImGui.TableHeadersRow();

            for (var i = 0; i < editingSprite.TileContexts.Count; ++i)
                RenderContextRow(editingSprite.TileContexts[i], i, maxLayers);

            HandleAnimatedSpriteSelector();
            HandleTileSpriteSelector();

            ImGui.EndTable();
        }
    }

    /// <summary>
    ///     Renders the bottom control bar of the editor.
    /// </summary>
    private void RenderEditorControls()
    {
        if (editingSprite == null)
        {
            Logger.Error("RenderEditorControls(): editingSprite is null.");
            return;
        }

        if (ImGui.BeginTable("Controls", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableNextColumn();
            if (ImGui.Button("Save")) SaveState();

            ImGui.TableNextColumn();
            if (ImGui.Button("Cancel")) ResetState();

            ImGui.EndTable();
        }
    }

    /// <summary>
    ///     Handles the animated sprite selector popup if it is open.
    /// </summary>
    private void HandleAnimatedSpriteSelector()
    {
        if (editingSprite == null)
        {
            Logger.Error("HandleAnimatedSpriteSelector(): editingSprite is null.");
            return;
        }

        animatedSpriteSelector.Render();
        if (animatedSpriteSelector.TryGetSelection(out var selectedId))
            editingSprite.TileContexts[editingRow].AnimatedSpriteIds[editingCol] = selectedId;
    }

    /// <summary>
    ///     Handles the tile sprite selector popup if it is open.
    /// </summary>
    private void HandleTileSpriteSelector()
    {
        if (editingSprite == null)
        {
            Logger.Error("HandleAnimatedSpriteSelector(): editingSprite is null.");
            return;
        }

        tileSpriteSelector.Render();
        if (tileSpriteSelector.TryGetSelection(out var selectedId))
        {
            var row = editingSprite.TileContexts[editingRow];
            switch (editingNeighbor)
            {
                case Orientation.North:
                    row.NorthTileSpriteId = selectedId;
                    break;

                case Orientation.East:
                    row.EastTileSpriteId = selectedId;
                    break;

                case Orientation.South:
                    row.SouthTileSpriteId = selectedId;
                    break;

                case Orientation.West:
                    row.WestTileSpriteId = selectedId;
                    break;
            }

            editingSprite.ClearCache();
        }
    }

    /// <summary>
    ///     Renders a single tile context for the context table.
    /// </summary>
    /// <param name="context">Context to show in row.</param>
    /// <param name="rowIndex">Zero-based index of the row.</param>
    /// <param name="maxLayers">Maximum number of layers across all tile contexts for current tile sprite.</param>
    private void RenderContextRow(TileContext context, int rowIndex, int maxLayers)
    {
        if (editingSprite == null)
        {
            Logger.Error("RenderContextRow(): editingSprite is null.");
            return;
        }

        var layers = context.AnimatedSpriteIds;

        ImGui.TableNextColumn();
        var canRemoveContext = !(context.NorthTileSpriteId == TileSprite.Wildcard &&
                                 context.EastTileSpriteId == TileSprite.Wildcard &&
                                 context.SouthTileSpriteId == TileSprite.Wildcard &&
                                 context.WestTileSpriteId == TileSprite.Wildcard);
        if (!canRemoveContext) ImGui.BeginDisabled();
        if (ImGui.Button($"-##context-{rowIndex}")) editingSprite.TileContexts.RemoveAt(rowIndex);
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.BeginTooltip();
            ImGui.Text(canRemoveContext ? "Remove Tile Context" : "Cannot remove default context");
            ImGui.EndTooltip();
        }

        if (!canRemoveContext) ImGui.EndDisabled();

        ImGui.TableNextColumn();
        guiExtensions.TileSprite($"tsPrevCtx{rowIndex}", editingSprite, context.NorthTileSpriteId,
            context.EastTileSpriteId, context.SouthTileSpriteId, context.WestTileSpriteId);
        if (ImGui.IsItemHovered()) RenderPreviewTooltip(context, rowIndex);
        ImGui.TableNextColumn();

        // North neighbor.
        ImGui.TableNextColumn();
        bool clicked;
        if (context.NorthTileSpriteId == TileSprite.Wildcard)
        {
            for (var i = 0; i < 3; ++i) ImGui.Spacing();
            clicked = ImGui.Button($"Any##{rowIndex}north");
        }
        else
        {
            clicked = guiExtensions.TileSpriteButton($"ctx{rowIndex}north", context.NorthTileSpriteId,
                TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard);
        }

        if (clicked)
        {
            editingNeighbor = Orientation.North;
            editingRow = rowIndex;
            tileSpriteSelector.Open(true);
        }

        // East neighbor.
        ImGui.TableNextColumn();
        if (context.EastTileSpriteId == TileSprite.Wildcard)
        {
            for (var i = 0; i < 3; ++i) ImGui.Spacing();
            clicked = ImGui.Button($"Any##{rowIndex}east");
        }
        else
        {
            clicked = guiExtensions.TileSpriteButton($"ctx{rowIndex}east", context.EastTileSpriteId,
                TileSprite.Wildcard,
                TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard);
        }

        if (clicked)
        {
            editingNeighbor = Orientation.East;
            editingRow = rowIndex;
            tileSpriteSelector.Open(true);
        }

        // South neighbor.
        ImGui.TableNextColumn();
        if (context.SouthTileSpriteId == TileSprite.Wildcard)
        {
            for (var i = 0; i < 3; ++i) ImGui.Spacing();
            clicked = ImGui.Button($"Any##{rowIndex}south");
        }
        else
        {
            clicked = guiExtensions.TileSpriteButton($"ctx{rowIndex}south", context.SouthTileSpriteId,
                TileSprite.Wildcard,
                TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard);
        }

        if (clicked)
        {
            editingNeighbor = Orientation.South;
            editingRow = rowIndex;
            tileSpriteSelector.Open(true);
        }

        // West neighbor.
        ImGui.TableNextColumn();
        if (context.WestTileSpriteId == TileSprite.Wildcard)
        {
            for (var i = 0; i < 3; ++i) ImGui.Spacing();
            clicked = ImGui.Button($"Any##{rowIndex}west");
        }
        else
        {
            clicked = guiExtensions.TileSpriteButton($"ctx{rowIndex}west", context.WestTileSpriteId,
                TileSprite.Wildcard,
                TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard);
        }

        if (clicked)
        {
            editingNeighbor = Orientation.West;
            editingRow = rowIndex;
            tileSpriteSelector.Open(true);
        }

        ImGui.TableNextColumn();
        if (ImGui.Button($"+##{rowIndex}"))
            // Add new layer to end of this context.
            layers.Add(0);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Add New Layer to End");
            ImGui.EndTooltip();
        }

        var canRemoveLayer = layers.Count > 1;
        if (!canRemoveLayer) ImGui.BeginDisabled();
        if (ImGui.Button($"-##layers-{rowIndex}"))
            // Remove the last layer of this context.
            layers.RemoveAt(layers.Count - 1);
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.BeginTooltip();
            ImGui.Text(canRemoveLayer ? "Remove Layer from End" : "Cannot remove last layer");
            ImGui.EndTooltip();
        }

        if (!canRemoveLayer) ImGui.EndDisabled();

        for (var i = 0; i < maxLayers; ++i)
        {
            ImGui.TableNextColumn();
            if (i >= context.AnimatedSpriteIds.Count) continue;

            if (guiExtensions.AnimatedSpriteButton($"ctx{rowIndex}l{i}", context.AnimatedSpriteIds[i],
                    Orientation.South, AnimationPhase.Default))
            {
                editingRow = rowIndex;
                editingCol = i;
                animatedSpriteSelector.Open();
            }
        }
    }

    /// <summary>
    ///     Renders a preview tooltip that shows a specific tile sprite context surrounded by its neighbors.
    /// </summary>
    /// <param name="context">Tile context.</param>
    /// <param name="rowIndex">Zero-based row index in the tile context table.</param>
    private void RenderPreviewTooltip(TileContext context, int rowIndex)
    {
        if (editingSprite == null)
        {
            Logger.Error("RenderPreviewTooltip(): editingSprite is null.");
            return;
        }

        ImGui.BeginTooltip();
        if (ImGui.BeginTable("tsPrevTbl", 3, ImGuiTableFlags.SizingFixedFit))
        {
            // Top row.
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            if (context.NorthTileSpriteId != TileSprite.Wildcard)
                guiExtensions.TileSprite(context.NorthTileSpriteId, TileSprite.Wildcard, TileSprite.Wildcard,
                    editingSprite.Id, TileSprite.Wildcard);
            else
                ImGui.Text(" Any");
            ImGui.TableNextColumn();

            // Middle row.
            ImGui.TableNextColumn();
            if (context.WestTileSpriteId != TileSprite.Wildcard)
            {
                guiExtensions.TileSprite(context.WestTileSpriteId, TileSprite.Wildcard, editingSprite.Id,
                    TileSprite.Wildcard, TileSprite.Wildcard);
            }
            else
            {
                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Text("Any");
            }

            ImGui.TableNextColumn();
            guiExtensions.TileSprite($"tsPrevCtx{rowIndex}", editingSprite, context.NorthTileSpriteId,
                context.EastTileSpriteId, context.SouthTileSpriteId, context.WestTileSpriteId);
            ImGui.TableNextColumn();
            if (context.EastTileSpriteId != TileSprite.Wildcard)
            {
                guiExtensions.TileSprite(context.EastTileSpriteId, TileSprite.Wildcard, TileSprite.Wildcard,
                    TileSprite.Wildcard, editingSprite.Id);
            }
            else
            {
                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Text("Any");
            }

            // Bottom row.
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            if (context.SouthTileSpriteId != TileSprite.Wildcard)
                guiExtensions.TileSprite(context.SouthTileSpriteId, editingSprite.Id, TileSprite.Wildcard,
                    TileSprite.Wildcard, TileSprite.Wildcard);
            else
                ImGui.Text(" Any");
            ImGui.TableNextColumn();

            ImGui.EndTable();
        }

        ImGui.EndTooltip();
    }

    /// <summary>
    ///     Selects a tile sprite for editing.
    /// </summary>
    /// <param name="tileSpriteId">ID of tile sprite to edit.</param>
    private void Select(int tileSpriteId)
    {
        editingSprite = new TileSprite(tileSpriteManager.TileSprites[tileSpriteId]);
    }

    /// <summary>
    ///     Saves the currently edited sprite into the active tile sprite table.
    /// </summary>
    private void SaveState()
    {
        if (editingSprite == null)
        {
            Logger.Error("SaveState(): editingSprite is null");
            return;
        }

        editingSprite.ReSortContexts();
        tileSpriteManager.Update(editingSprite);
    }

    /// <summary>
    ///     Resets the editor state to the stored version of the currently selected tile sprite.
    /// </summary>
    private void ResetState()
    {
        if (editingSprite == null)
        {
            Logger.Error("ResetState(): editingSprite is null");
            return;
        }

        editingSprite = new TileSprite(tileSpriteManager.TileSprites[editingSprite.Id]);
    }

    /// <summary>
    ///     Inserts a new tile sprite after the currently selected tile sprite.
    /// </summary>
    private void InsertNewTileSprite()
    {
        if (editingSprite == null)
        {
            Logger.Error("InsertNewTileSprite(): editingSprite is null.");
            return;
        }

        tileSpriteManager.InsertNew(editingSprite.Id + 1);
    }

    /// <summary>
    ///     Removes the currently selected tile sprite.
    /// </summary>
    private void RemoveSelectedTileSprite()
    {
        if (editingSprite == null)
        {
            Logger.Error("RemoveSelectedTileSprite(): editingSprite is null.");
            return;
        }

        tileSpriteManager.Remove(editingSprite.Id);
        Select(editingSprite.Id >= tileSpriteManager.TileSprites.Count
            ? tileSpriteManager.TileSprites.Count - 1
            : editingSprite.Id);
    }

    /// <summary>
    ///     Determines whether the currently selected tile sprite can be removed.
    /// </summary>
    /// <param name="reason">Reason that the sprite cannot be removed. Only set if the method returns false.</param>
    /// <returns>true if the tile sprite can be removed, false otherwise.</returns>
    private bool CanRemoveSprite([NotNullWhen(false)] out string? reason)
    {
        reason = null;
        if (editingSprite == null)
        {
            Logger.Error("CanRemoveSprite(): editingSprite is null.");
            reason = "Internal error.";
            return false;
        }

        // Prevent removal of last tile sprite.
        if (tileSpriteManager.TileSprites.Count <= 1)
        {
            reason = "Cannot remove last tile sprite.";
            return false;
        }

        // Prevent removal of any tile sprite depended on by one or more materials.
        var dependentMaterials = materialManager.Materials
            .Where(material => material.MaterialSubtypes
                .Any(subtype => subtype.SideFaceTileSpriteId == editingSprite.Id
                                || subtype.TopFaceTileSpriteId == editingSprite.Id
                                || subtype.ObscuredTopFaceTileSpriteId == editingSprite.Id))
            .Select(material => material.Id);
        if (dependentMaterials.Any())
        {
            var sb = new StringBuilder("Cannot remove with dependencies:");
            foreach (var materialId in dependentMaterials) sb.Append($"\nMaterial {materialId}");

            reason = sb.ToString();
            return false;
        }

        return true;
    }
}