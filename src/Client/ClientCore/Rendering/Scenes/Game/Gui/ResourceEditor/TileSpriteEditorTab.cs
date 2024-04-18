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
using Castle.Core.Logging;
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Gui;
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

    private readonly GuiExtensions guiExtensions;

    private readonly TileSpriteManager tileSpriteManager;

    /// <summary>
    ///     Currently edited tile sprite.
    /// </summary>
    private TileSprite? editingSprite;

    /// <summary>
    ///     Initialization flag.
    /// </summary>
    private bool initialized;

    public TileSpriteEditorTab(TileSpriteManager tileSpriteManager, GuiExtensions guiExtensions)
    {
        this.tileSpriteManager = tileSpriteManager;
        this.guiExtensions = guiExtensions;
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

    private void RenderEditor()
    {
        if (editingSprite == null)
        {
            Logger.Error("RenderEditor(): editingSprite is null.");
            return;
        }

        if (ImGui.BeginTable("Header", 1, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableNextColumn();
            ImGui.Text($"Tile Sprite {editingSprite.Id}");
            ImGui.EndTable();
        }

        ImGui.Separator();

        RenderContextTable();

        if (ImGui.BeginTable("Controls", 4, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextColumn();
            ImGui.Button("Save");

            ImGui.TableNextColumn();
            ImGui.Button("Cancel");

            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGui.Button("+");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Add New Tile Context");
                ImGui.EndTooltip();
            }

            ImGui.EndTable();
        }
    }

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
        if (ImGui.BeginTable("contextTable", maxLayers + 7,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg,
                new Vector2 { X = maxSize.X - 276, Y = maxSize.Y - 115 }))
        {
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("North");
            ImGui.TableSetupColumn("East");
            ImGui.TableSetupColumn("South");
            ImGui.TableSetupColumn("West");
            ImGui.TableSetupColumn("+/-");
            for (var i = 0; i < maxLayers; ++i)
            {
                ImGui.TableSetupColumn($"Layer {i + 1}");
            }

            ImGui.TableSetupScrollFreeze(7, 0);
            ImGui.TableHeadersRow();

            for (var i = 0; i < editingSprite.TileContexts.Count; ++i)
            {
                RenderContextRow(editingSprite.TileContexts[i], i, maxLayers);
            }


            ImGui.EndTable();
        }
    }

    private void RenderContextRow(TileContext context, int rowIndex, int maxLayers)
    {
        if (editingSprite == null)
        {
            Logger.Error("RenderContextRow(): editingSprite is null.");
            return;
        }

        ImGui.TableNextColumn();
        ImGui.Button("-");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Remove Tile Context");
            ImGui.EndTooltip();
        }

        ImGui.TableNextColumn();
        guiExtensions.TileSprite($"tsPrevCtx{rowIndex}", editingSprite, context.NorthTileSpriteId,
            context.EastTileSpriteId, context.SouthTileSpriteId, context.WestTileSpriteId);

        ImGui.TableNextColumn();
        if (context.NorthTileSpriteId == TileSprite.Wildcard)
            ImGui.Text("Any");
        else
            guiExtensions.TileSpriteButton($"ctx{rowIndex}north", context.NorthTileSpriteId, TileSprite.Wildcard,
                TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard);

        ImGui.TableNextColumn();
        if (context.EastTileSpriteId == TileSprite.Wildcard)
            ImGui.Text("Any");
        else
            guiExtensions.TileSpriteButton($"ctx{rowIndex}east", context.EastTileSpriteId, TileSprite.Wildcard,
                TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard);

        ImGui.TableNextColumn();
        if (context.SouthTileSpriteId == TileSprite.Wildcard)
            ImGui.Text("Any");
        else
            guiExtensions.TileSpriteButton($"ctx{rowIndex}south", context.SouthTileSpriteId, TileSprite.Wildcard,
                TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard);

        ImGui.TableNextColumn();
        if (context.WestTileSpriteId == TileSprite.Wildcard)
            ImGui.Text("Any");
        else
            guiExtensions.TileSpriteButton($"ctx{rowIndex}west", context.WestTileSpriteId, TileSprite.Wildcard,
                TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard);

        ImGui.TableNextColumn();
        ImGui.Button("+");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Add New Layer to End");
            ImGui.EndTooltip();
        }

        ImGui.Button("-");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Remove Layer from End");
            ImGui.EndTooltip();
        }

        for (var i = 0; i < maxLayers; ++i)
        {
            ImGui.TableNextColumn();
            if (i < context.AnimatedSpriteIds.Count)
                guiExtensions.AnimatedSpriteButton($"ctx{rowIndex}l{i}", context.AnimatedSpriteIds[i],
                    Orientation.South, AnimationPhase.Default);
        }
    }

    private void Select(int tileSpriteId)
    {
        editingSprite = new TileSprite(tileSpriteManager.TileSprites[tileSpriteId]);
    }

    private void InsertNewTileSprite()
    {
    }

    private void RemoveSelectedTileSprite()
    {
    }

    private bool CanRemoveSprite([NotNullWhen(false)] out string? reason)
    {
        reason = null;
        return true;
    }
}