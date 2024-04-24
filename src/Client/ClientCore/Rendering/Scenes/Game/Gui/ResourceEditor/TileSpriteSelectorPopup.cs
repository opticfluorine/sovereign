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
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Reusable popup window for visually selecting a Tile Sprite.
/// </summary>
public class TileSpriteSelectorPopup
{
    private const string PopupName = "Select Tile Sprite";

    private const int ColumnCount = 5;
    private readonly GuiExtensions guiExtensions;
    private readonly TileSpriteManager tileSpriteManager;
    private bool allowWildcard;
    private bool isSelected;
    private int selection;

    public TileSpriteSelectorPopup(GuiExtensions guiExtensions,
        TileSpriteManager tileSpriteManager)
    {
        this.guiExtensions = guiExtensions;
        this.tileSpriteManager = tileSpriteManager;
    }

    /// <summary>
    ///     Opens the selector popup.
    /// </summary>
    /// <param name="allowAny">Whether to include an "Any" option.</param>
    public void Open(bool allowAny)
    {
        isSelected = false;
        selection = 0;
        allowWildcard = allowAny;

        ImGui.OpenPopup(PopupName);
    }

    /// <summary>
    ///     Renders the selector popup if open, otherwise does nothing.
    /// </summary>
    public void Render()
    {
        if (!ImGui.BeginPopup(PopupName)) return;

        if (ImGui.BeginTable("tileSprTbl", ColumnCount,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit))
        {
            if (allowWildcard)
            {
                ImGui.TableNextColumn();
                if (ImGui.Button("Any"))
                {
                    selection = TileSprite.Wildcard;
                    isSelected = true;
                    ImGui.CloseCurrentPopup();
                }
            }

            for (var i = 0; i < tileSpriteManager.TileSprites.Count; ++i)
            {
                ImGui.TableNextColumn();
                if (guiExtensions.TileSpriteButton($"btn{i}", i, TileSprite.Wildcard, TileSprite.Wildcard,
                        TileSprite.Wildcard, TileSprite.Wildcard))
                {
                    selection = i;
                    isSelected = true;
                    ImGui.CloseCurrentPopup();
                }

                if (ImGui.IsItemHovered())
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text($"Tile Sprite {i}");
                        ImGui.EndTooltip();
                    }
            }

            ImGui.EndTable();
        }

        ImGui.EndPopup();
    }

    /// <summary>
    ///     Tries to get the latest selection.
    /// </summary>
    /// <param name="tileSpriteId">Set to the selected tile sprite ID if returns true.</param>
    /// <returns>true if a selection has been made since the last call to Open(); false otherwise.</returns>
    public bool TryGetSelection(out int tileSpriteId)
    {
        tileSpriteId = selection;
        var result = isSelected;
        isSelected = false;
        return result;
    }
}