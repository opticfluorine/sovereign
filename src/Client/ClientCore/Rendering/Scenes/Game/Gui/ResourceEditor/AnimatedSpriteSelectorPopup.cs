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

using System.Numerics;
using ImGuiNET;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Reusable popup window for visually selecting an Animated Sprite.
/// </summary>
public class AnimatedSpriteSelectorPopup
{
    private const string PopupName = "Select Animated Sprite";
    private Vector2 basePos;
    private bool isSelected;
    private int selection;

    /// <summary>
    ///     Opens the selector popup.
    /// </summary>
    public void Open()
    {
        isSelected = false;
        selection = 0;
        basePos = ImGui.GetMousePos();

        ImGui.OpenPopup(PopupName);
    }

    /// <summary>
    ///     Renders the selector popup if open, otherwise does nothing.
    /// </summary>
    public void Render()
    {
        if (ImGui.BeginPopup(PopupName))
        {
            ImGui.EndPopup();
        }
    }

    /// <summary>
    ///     Tries to get the latest selection.
    /// </summary>
    /// <param name="animatedSpriteId">Set to the selected animated sprite ID if returns true.</param>
    /// <returns>true if a selection has been made since the last call to Open(); false otherwise.</returns>
    public bool TryGetSelection(out int animatedSpriteId)
    {
        animatedSpriteId = selection;
        var result = isSelected;
        isSelected = false;
        return result;
    }
}