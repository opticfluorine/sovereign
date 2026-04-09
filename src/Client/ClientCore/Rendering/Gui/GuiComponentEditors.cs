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

using Hexa.NET.ImGui;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Controls;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Rendering.Gui;

/// <summary>
///     Reusable GUI controls for component editing.
/// </summary>
public class GuiComponentEditors
{
    private readonly GuiExtensions guiExtensions;
    private readonly TileSpriteManager tileSpriteManager;
    private readonly TileSpriteSelectorPopup tileSpriteSelectorPopup;

    private PopupTarget popupTarget;

    public GuiComponentEditors(GuiExtensions guiExtensions,
        TileSpriteManager tileSpriteManager, TileSpriteSelectorPopup tileSpriteSelectorPopup)
    {
        this.guiExtensions = guiExtensions;
        this.tileSpriteManager = tileSpriteManager;
        this.tileSpriteSelectorPopup = tileSpriteSelectorPopup;
    }

    /// <summary>
    ///     Renders an input control for the Name component.
    /// </summary>
    /// <param name="id">GUI ID.</param>
    /// <param name="buffer">Input buffer to hold user input.</param>
    public void NameEdit(string id, ref string buffer)
    {
        var fontSize = ImGui.GetFontSize();
        ImGui.SetNextItemWidth(fontSize * 16.0f);
        ImGui.InputText(id, ref buffer, EntityConstants.MaxNameLength + 1);
    }

    /// <summary>
    ///     Renders an input control for the BlockTile component.
    /// </summary>
    /// <param name="frontTileId">Selected front face tile ID.</param>
    /// <param name="topTileId">Selected top face tile ID.</param>
    public void BlockTileEdit(ref int frontTileId, ref int topTileId)
    {
        if (!ImGui.BeginTable("blockTileEditLayout", 4, ImGuiTableFlags.SizingFixedFit)) return;

        // Clamp selection variables to limits.
        if (frontTileId < 0 || frontTileId >= tileSpriteManager.TileSprites.Count)
            frontTileId = 0;

        if (topTileId < 0 || topTileId >= tileSpriteManager.TileSprites.Count)
            topTileId = 0;

        ImGui.TableNextColumn();
        ImGui.Text("Front Tile Sprite:");

        ImGui.TableNextColumn();
        if (guiExtensions.TileSpriteButton("##tileButtonFront", frontTileId, TileContextKey.AllWildcards))
        {
            tileSpriteSelectorPopup.Open(false);
            popupTarget = PopupTarget.FrontFace;
        }

        ImGui.TableNextColumn();
        ImGui.Text("Top Tile Sprite:");

        ImGui.TableNextColumn();
        if (guiExtensions.TileSpriteButton("##tileButtonTop", topTileId, TileContextKey.AllWildcards))
        {
            tileSpriteSelectorPopup.Open(false);
            popupTarget = PopupTarget.TopFace;
        }

        tileSpriteSelectorPopup.Render();
        if (tileSpriteSelectorPopup.TryGetSelection(out var newTileId))
        {
            switch (popupTarget)
            {
                case PopupTarget.FrontFace:
                    frontTileId = newTileId;
                    break;

                case PopupTarget.TopFace:
                    topTileId = newTileId;
                    break;
            }
        }

        ImGui.EndTable();
    }

    private enum PopupTarget
    {
        FrontFace,
        TopFace
    }
}