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
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<TileSpriteEditorTab> logger;
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
        TileSpriteSelectorPopup tileSpriteSelector, ILogger<TileSpriteEditorTab> logger)
    {
        this.tileSpriteManager = tileSpriteManager;
        this.guiExtensions = guiExtensions;
        this.materialManager = materialManager;
        this.animatedSpriteSelector = animatedSpriteSelector;
        this.tileSpriteSelector = tileSpriteSelector;
        this.logger = logger;
    }

    /// <summary>
    ///     Renders the Tile Sprites resource editor tab.
    /// </summary>
    public void Render()
    {
        var fontSize = ImGui.GetFontSize();

        if (!initialized)
        {
            Select(0);
            initialized = true;
        }

        if (ImGui.BeginTabItem("Tile Sprites"))
        {
            if (ImGui.BeginTable("tileSprOuter", 2, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("#browser", ImGuiTableColumnFlags.WidthFixed, fontSize * 12.22f);
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
        var fontSize = ImGui.GetFontSize();

        if (editingSprite == null)
        {
            logger.LogError("RenderBrowser(): editingSprite is null.");
            return;
        }

        // Browser selector.
        var maxSize = ImGui.GetWindowSize();
        if (ImGui.BeginTable("tileSprBrowser", 2,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.RowBg, new Vector2 { X = fontSize * 12.33f, Y = maxSize.Y - fontSize * 7.25f }))
        {
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Tile Sprite");
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            for (var i = 0; i < tileSpriteManager.TileSprites.Count; ++i)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{i}");
                ImGui.TableNextColumn();
                if (guiExtensions.TileSpriteButton($"##spriteButton{i}", i,
                        TileContextKey.AllWildcards)) Select(i);

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
            logger.LogError("RenderTopBar(): editingSprite is null.");
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
            if (ImGui.Button("Add New Context")) editingSprite.TileContexts.Insert(0, new TileContext());

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
        var fontSize = ImGui.GetFontSize();

        if (editingSprite == null)
        {
            logger.LogError("RenderContextTable(): editingSprite is null.");
            return;
        }

        var maxSize = ImGui.GetWindowSize();
        var maxLayers = editingSprite.TileContexts
            .Select(ctx => ctx.AnimatedSpriteIds.Count).Max();
        if (ImGui.BeginTable("contextTable", maxLayers + 12,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg,
                new Vector2 { X = maxSize.X - fontSize * 15.33f, Y = maxSize.Y - fontSize * 9.05f }))
        {
            ImGui.TableSetupColumn(""); // Discard button
            ImGui.TableSetupColumn(""); // Preview
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.None, fontSize * 0.889f); // Spacer
            ImGui.TableSetupColumn("N");
            ImGui.TableSetupColumn("NE");
            ImGui.TableSetupColumn("E");
            ImGui.TableSetupColumn("SE");
            ImGui.TableSetupColumn("S");
            ImGui.TableSetupColumn("SW");
            ImGui.TableSetupColumn("W");
            ImGui.TableSetupColumn("NW");
            ImGui.TableSetupColumn("+/-");
            for (var i = 0; i < maxLayers; ++i) ImGui.TableSetupColumn($"Layer {i + 1}");

            ImGui.TableSetupScrollFreeze(11, 1);
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
            logger.LogError("RenderEditorControls(): editingSprite is null.");
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
            logger.LogError("HandleAnimatedSpriteSelector(): editingSprite is null.");
            return;
        }

        animatedSpriteSelector.Render();
        if (animatedSpriteSelector.TryGetSelection(out var selectedId))
            editingSprite.TileContexts[editingRow].AnimatedSpriteIds[editingCol] = selectedId;

        editingSprite.ClearCache();
    }

    /// <summary>
    ///     Handles the tile sprite selector popup if it is open.
    /// </summary>
    private void HandleTileSpriteSelector()
    {
        if (editingSprite == null)
        {
            logger.LogError("HandleAnimatedSpriteSelector(): editingSprite is null.");
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

                case Orientation.Northeast:
                    row.NortheastTileSpriteId = selectedId;
                    break;

                case Orientation.East:
                    row.EastTileSpriteId = selectedId;
                    break;

                case Orientation.Southeast:
                    row.SoutheastTileSpriteId = selectedId;
                    break;

                case Orientation.South:
                    row.SouthTileSpriteId = selectedId;
                    break;

                case Orientation.Southwest:
                    row.SouthwestTileSpriteId = selectedId;
                    break;

                case Orientation.West:
                    row.WestTileSpriteId = selectedId;
                    break;

                case Orientation.Northwest:
                    row.NorthwestTileSpriteId = selectedId;
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
            logger.LogError("RenderContextRow(): editingSprite is null.");
            return;
        }

        var layers = context.AnimatedSpriteIds;

        ImGui.TableNextColumn();
        var canRemoveContext = rowIndex < editingSprite.TileContexts.Count - 1;
        if (!canRemoveContext) ImGui.BeginDisabled();
        if (ImGui.Button($"-##context-{rowIndex}"))
        {
            editingSprite.TileContexts.RemoveAt(rowIndex);
            editingSprite.ClearCache();
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.BeginTooltip();
            ImGui.Text(canRemoveContext ? "Remove Tile Context" : "Cannot remove default context");
            ImGui.EndTooltip();
        }

        if (!canRemoveContext) ImGui.EndDisabled();

        ImGui.TableNextColumn();
        guiExtensions.TileSprite($"tsPrevCtx{rowIndex}", editingSprite, context.TileContextKey);
        if (ImGui.IsItemHovered()) RenderPreviewTooltip(context, rowIndex);
        ImGui.TableNextColumn();

        ImGui.TableNextColumn();
        NeighborButton(context, rowIndex, Orientation.North);

        ImGui.TableNextColumn();
        NeighborButton(context, rowIndex, Orientation.Northeast);

        ImGui.TableNextColumn();
        NeighborButton(context, rowIndex, Orientation.East);

        ImGui.TableNextColumn();
        NeighborButton(context, rowIndex, Orientation.Southeast);

        ImGui.TableNextColumn();
        NeighborButton(context, rowIndex, Orientation.South);

        ImGui.TableNextColumn();
        NeighborButton(context, rowIndex, Orientation.Southwest);

        ImGui.TableNextColumn();
        NeighborButton(context, rowIndex, Orientation.West);

        ImGui.TableNextColumn();
        NeighborButton(context, rowIndex, Orientation.Northwest);

        ImGui.TableNextColumn();
        if (ImGui.Button($"+##{rowIndex}"))
        {
            // Add new layer to end of this context.
            layers.Add(0);
            editingSprite.ClearCache();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Add New Layer to End");
            ImGui.EndTooltip();
        }

        var canRemoveLayer = layers.Count > 1;
        if (!canRemoveLayer) ImGui.BeginDisabled();
        if (ImGui.Button($"-##layers-{rowIndex}"))
        {
            // Remove the last layer of this context.
            layers.RemoveAt(layers.Count - 1);
            editingSprite.ClearCache();
        }

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
    ///     Adds a button for a neighbor tile sprite.
    /// </summary>
    /// <param name="context">Tile context.</param>
    /// <param name="rowIndex">Row index of the tile context.</param>
    /// <param name="orientation">Direction of neighboring tile sprite.</param>
    private void NeighborButton(TileContext context, int rowIndex, Orientation orientation)
    {
        bool clicked;
        var neighborId = context.GetNeighborTileSpriteId(orientation);
        if (neighborId == TileSprite.Wildcard)
        {
            for (var i = 0; i < 3; ++i) ImGui.Spacing();
            clicked = ImGui.Button($"Any##{rowIndex}{orientation}");
        }
        else if (neighborId == TileSprite.Empty)
        {
            for (var i = 0; i < 3; ++i) ImGui.Spacing();
            clicked = ImGui.Button($"Empty##{rowIndex}{orientation}");
        }
        else
        {
            clicked = guiExtensions.TileSpriteButton($"ctx{rowIndex}{orientation}",
                neighborId, TileContextKey.AllWildcards);
        }

        if (clicked)
        {
            editingNeighbor = orientation;
            editingRow = rowIndex;
            tileSpriteSelector.Open(true);
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
            logger.LogError("RenderPreviewTooltip(): editingSprite is null.");
            return;
        }

        ImGui.BeginTooltip();
        if (ImGui.BeginTable("tsPrevTbl", 3, ImGuiTableFlags.SizingFixedFit))
        {
            // Top row.
            ImGui.TableNextColumn();
            if (context.NorthwestTileSpriteId >= 0)
            {
                var key = new TileContextKey(TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard,
                    editingSprite.Id, TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard,
                    TileSprite.Wildcard);
                guiExtensions.TileSprite(context.NorthwestTileSpriteId, key);
            }

            ImGui.TableNextColumn();
            if (context.NorthTileSpriteId >= 0)
            {
                var key = new TileContextKey(TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard,
                    TileSprite.Wildcard, editingSprite.Id, TileSprite.Wildcard, TileSprite.Wildcard,
                    TileSprite.Wildcard);
                guiExtensions.TileSprite(context.NorthTileSpriteId, key);
            }

            ImGui.TableNextColumn();
            if (context.NortheastTileSpriteId >= 0)
            {
                var key = new TileContextKey(TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard,
                    TileSprite.Wildcard, TileSprite.Wildcard, editingSprite.Id, TileSprite.Wildcard,
                    TileSprite.Wildcard);
                guiExtensions.TileSprite(context.NortheastTileSpriteId, key);
            }

            // Middle row.
            ImGui.TableNextColumn();
            if (context.WestTileSpriteId >= 0)
            {
                var key = new TileContextKey(TileSprite.Wildcard, TileSprite.Wildcard, editingSprite.Id,
                    TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard,
                    TileSprite.Wildcard);
                guiExtensions.TileSprite(context.WestTileSpriteId, key);
            }

            ImGui.TableNextColumn();
            guiExtensions.TileSprite($"tsPrevCtx{rowIndex}", editingSprite, context.TileContextKey);

            ImGui.TableNextColumn();
            if (context.EastTileSpriteId >= 0)
            {
                var key = new TileContextKey(TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard,
                    TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard, editingSprite.Id,
                    TileSprite.Wildcard);
                guiExtensions.TileSprite(context.EastTileSpriteId, key);
            }

            // Bottom row.
            ImGui.TableNextColumn();
            if (context.SouthwestTileSpriteId >= 0)
            {
                var key = new TileContextKey(TileSprite.Wildcard, editingSprite.Id, TileSprite.Wildcard,
                    TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard,
                    TileSprite.Wildcard);
                guiExtensions.TileSprite(context.SouthwestTileSpriteId, key);
            }

            ImGui.TableNextColumn();
            if (context.SouthTileSpriteId >= 0)
            {
                var key = new TileContextKey(editingSprite.Id, TileSprite.Wildcard, TileSprite.Wildcard,
                    TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard,
                    TileSprite.Wildcard);
                guiExtensions.TileSprite(context.SouthTileSpriteId, key);
            }

            ImGui.TableNextColumn();
            if (context.SoutheastTileSpriteId >= 0)
            {
                var key = new TileContextKey(TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard,
                    TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard,
                    editingSprite.Id);
                guiExtensions.TileSprite(context.SoutheastTileSpriteId, key);
            }

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
            logger.LogError("SaveState(): editingSprite is null");
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
            logger.LogError("ResetState(): editingSprite is null");
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
            logger.LogError("InsertNewTileSprite(): editingSprite is null.");
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
            logger.LogError("RemoveSelectedTileSprite(): editingSprite is null.");
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
            logger.LogError("CanRemoveSprite(): editingSprite is null.");
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