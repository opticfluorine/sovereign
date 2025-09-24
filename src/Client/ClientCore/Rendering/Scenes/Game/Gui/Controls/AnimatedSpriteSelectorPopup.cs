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

using System;
using System.Numerics;
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Controls;

/// <summary>
///     Reusable popup window for visually selecting an Animated Sprite.
/// </summary>
public class AnimatedSpriteSelectorPopup
{
    private const string PopupName = "Select Animated Sprite";

    private const int ColumnCount = 5;
    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly GuiExtensions guiExtensions;

    private readonly Vector2 preferredSize = new(500.0f, 400.0f);
    private Vector2 basePos = Vector2.Zero;
    private bool isSelected;
    private int selection;

    public AnimatedSpriteSelectorPopup(AnimatedSpriteManager animatedSpriteManager, GuiExtensions guiExtensions)
    {
        this.animatedSpriteManager = animatedSpriteManager;
        this.guiExtensions = guiExtensions;
    }

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
        if (!ImGui.BeginPopup(PopupName)) return;

        var screenSize = ImGui.GetIO().DisplaySize;
        var maxSize = new Vector2(screenSize.X - basePos.X - 16, screenSize.Y - basePos.Y - 128);
        var realSize = new Vector2(Math.Min(preferredSize.X, maxSize.X), Math.Min(preferredSize.Y, maxSize.Y));

        if (ImGui.BeginTable("animSprTbl", ColumnCount,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit, realSize))
        {
            for (var i = 0; i < animatedSpriteManager.AnimatedSprites.Count; ++i)
            {
                ImGui.TableNextColumn();
                if (guiExtensions.AnimatedSpriteButton($"btn{i}", i, Orientation.South, AnimationPhase.Default))
                {
                    selection = i;
                    isSelected = true;
                    ImGui.CloseCurrentPopup();
                }

                if (ImGui.IsItemHovered())
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text($"Animated Sprite {i}");
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