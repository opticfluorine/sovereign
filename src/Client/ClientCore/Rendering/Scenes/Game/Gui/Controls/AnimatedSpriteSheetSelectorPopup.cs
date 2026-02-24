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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Controls;

/// <summary>
///     Reusable popup window for selecting an Animated Sprite by clicking an associated Sprite in its sheet.
/// </summary>
public sealed class AnimatedSpriteSheetSelectorPopup(
    SpriteSheetManager spriteSheetManager,
    GuiExtensions guiExtensions,
    SpriteManager spriteManager,
    AnimatedSpriteManager animatedSpriteManager)
{
    private const string PopupName = "Select Animated Sprite##animSheet";
    private const string AnimSpritePopupName = "Select##animSheet";
    private const string ErrorPopupName = "Error##animSheet";
    private const int AnimSpriteGridWidth = 5;

    private readonly GuiLabelCache animButtonLabels = new("##animSpr");

    /// <summary>
    ///     Preferred size of selector if sufficient space is available.
    /// </summary>
    private readonly Vector2 preferredSize = new(500.0f, 400.0f);

    /// <summary>
    ///     Popup base position.
    /// </summary>
    private Vector2 basePos;

    /// <summary>
    ///     Index of currently selected spritesheet.
    /// </summary>
    private int currentSheetIdx;

    /// <summary>
    ///     Initialization flag.
    /// </summary>
    private bool initialized;

    /// <summary>
    ///     Flag indicating user has selected sprite since last Open() call.
    /// </summary>
    private bool isSelected;

    /// <summary>
    ///     Flag indicating whether the sheet selection combo is open.
    /// </summary>
    private bool isSheetComboOpen;

    /// <summary>
    ///     Alphabetically-ordered sprite sheets.
    /// </summary>
    private List<string> orderedSpriteSheets = new();

    private int selectedSprite;

    /// <summary>
    ///     Selected animated sprite.
    /// </summary>
    private int selection;

    /// <summary>
    ///     Opens the selector popup.
    /// </summary>
    public void Open()
    {
        isSelected = false;
        selection = 0;
        selectedSprite = 0;
        basePos = ImGui.GetMousePos();

        ImGui.OpenPopup(PopupName);
    }

    /// <summary>
    ///     Renders the selector popup if open, otherwise does nothing.
    /// </summary>
    public void Render()
    {
        if (!initialized) Initialize();

        if (ImGui.BeginPopup(PopupName))
        {
            DrawTopBar();
            DrawSpritesheetView();

            if (isSelected) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }

        DrawAnimatedSpritePopup();
        DrawErrorModal();
    }

    /// <summary>
    ///     Tries to get the latest selection.
    /// </summary>
    /// <param name="animatedSpriteId">Set to the selected sprite ID if returns true.</param>
    /// <returns>true if a selection has been made since the last call to Open(); false otherwise.</returns>
    public bool TryGetSelection(out int animatedSpriteId)
    {
        animatedSpriteId = selection;
        var result = isSelected;
        isSelected = false;
        return result;
    }

    /// <summary>
    ///     Draws the top bar with controls.
    /// </summary>
    private void DrawTopBar()
    {
        // Combo box for selecting the current spritesheet.
        ImGui.Text("Spritesheet:");
        ImGui.SameLine();
        isSheetComboOpen = ImGui.BeginCombo("##spriteSheetCombo", orderedSpriteSheets[currentSheetIdx]);
        if (isSheetComboOpen)
        {
            for (var i = 0; i < orderedSpriteSheets.Count; ++i)
            {
                var sheetName = orderedSpriteSheets[i];
                var isSheetSelected = i == currentSheetIdx;
                if (ImGui.Selectable(sheetName, i == currentSheetIdx)) currentSheetIdx = i;
                if (isSheetSelected) ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }
    }

    /// <summary>
    ///     Draws the information view for the currently selected spritesheet.
    /// </summary>
    private void DrawSpritesheetView()
    {
        // Wrap the view in a single-cell table to get scrollbars for larger spritesheets.
        var screenSize = ImGui.GetIO().DisplaySize;
        var maxSize = new Vector2(screenSize.X - basePos.X - 16, screenSize.Y - basePos.Y - 128);
        var realSize = new Vector2(Math.Min(preferredSize.X, maxSize.X), Math.Min(preferredSize.Y, maxSize.Y));

        if (ImGui.BeginTable("spriteSelectorView", 1, ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY, realSize))
        {
            ImGui.TableNextColumn();
            var sheetName = orderedSpriteSheets[currentSheetIdx];
            guiExtensions.Spritesheet(sheetName);
            var spriteHovered = ImGui.IsItemHovered();

            var drawList = ImGui.GetWindowDrawList();
            var coverageMap = spriteManager.SpriteSheetCoverage[sheetName];

            var sheet = spriteSheetManager.SpriteSheets[sheetName];
            var rows = sheet.Surface.Properties.Height / sheet.Definition.SpriteHeight;
            var cols = sheet.Surface.Properties.Width / sheet.Definition.SpriteWidth;
            var start = ImGui.GetWindowPos() - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());

            var fontSize = ImGui.GetFontSize();
            var scale = GuiExtensions.SpriteScaleFactor * fontSize;

            for (var i = 0; i < rows; ++i)
            for (var j = 0; j < cols; ++j)
                if (coverageMap[i, j] != null)
                {
                    // Sprite exists, draw a box around it.
                    var minPos = start + scale * new Vector2(j * sheet.Definition.SpriteWidth,
                        i * sheet.Definition.SpriteHeight);
                    var maxPos = minPos + scale *
                        new Vector2(sheet.Definition.SpriteWidth, sheet.Definition.SpriteHeight);
                    drawList.AddRect(minPos, maxPos, 0x7FFFFFFF);
                }

            // If mouse is over a covered sprite, show the sprite ID in a tooltip.
            var relMousePos = ImGui.GetMousePos() - start;
            if (spriteHovered)
            {
                // Mouse is overlapping the spritesheet.
                var row = (int)Math.Floor(relMousePos.Y /
                                          (sheet.Definition.SpriteHeight * scale));
                var col = (int)Math.Floor(relMousePos.X /
                                          (sheet.Definition.SpriteWidth * scale));
                if (row == rows) row--;
                if (col == cols) col--;

                var sprite = coverageMap[row, col];
                if (sprite != null)
                    // Mouse is hovering over a defined sprite, show tooltip.
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text($"Sprite {sprite.Id}");
                        ImGui.EndTooltip();
                    }

                // If a sprite is clicked, select it and close the popup.
                if (spriteHovered && !isSheetComboOpen && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    if (sprite != null)
                        OnSpriteClicked(sprite.Id);
            }

            ImGui.EndTable();
        }
    }

    /// <summary>
    ///     Draws the popup for selecting a specific animated sprite.
    /// </summary>
    private void DrawAnimatedSpritePopup()
    {
        if (!ImGui.BeginPopup(AnimSpritePopupName)) return;

        if (!animatedSpriteManager.AnimatedSpritesContainingSprite.TryGetValue(selectedSprite, out var animSpriteIds))
        {
            // No longer any choices available - shouldn't happen, but easy to handle gracefully.
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        if (ImGui.BeginTable("animSpriteChoices", AnimSpriteGridWidth,
                ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY))
            for (var i = 0; i < animSpriteIds.Count; ++i)
            {
                var id = animSpriteIds[i];
                ImGui.TableNextColumn();
                if (guiExtensions.AnimatedSpriteButton(animButtonLabels[i], id, Orientation.South,
                        AnimationPhase.Default))
                    SelectAnimatedSprite(id);

                if (ImGui.BeginItemTooltip())
                {
                    ImGui.Text($"Animated Sprite {id}");
                    ImGui.EndTooltip();
                }
            }

        if (!ImGui.IsWindowFocused()) ImGui.CloseCurrentPopup();
        if (isSelected) ImGui.CloseCurrentPopup();
        ImGui.EndPopup();
    }

    /// <summary>
    ///     Draws the error message modal popup if it is open.
    /// </summary>
    private void DrawErrorModal()
    {
        if (!ImGui.BeginPopupModal(ErrorPopupName)) return;

        ImGui.Text("No animated sprites available.");
        if (ImGui.Button("OK")) ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }

    /// <summary>
    ///     Called when the player clicks a defined sprite.
    /// </summary>
    /// <param name="spriteId">Sprite ID.</param>
    private void OnSpriteClicked(int spriteId)
    {
        if (!animatedSpriteManager.AnimatedSpritesContainingSprite.TryGetValue(spriteId, out var animSpriteIds))
        {
            // No animated sprites available for this sprite.
            ImGui.OpenPopup(ErrorPopupName);
            return;
        }

        if (animSpriteIds.Count == 1)
        {
            // If only one animated sprite is available, select it without prompting the player.
            SelectAnimatedSprite(animSpriteIds[0]);
            return;
        }

        // Otherwise, we need to prompt the player to select from a list of animated sprites.
        selectedSprite = spriteId;
        ImGui.OpenPopup(AnimSpritePopupName);
    }

    /// <summary>
    ///     Selects an animated sprite.
    /// </summary>
    /// <param name="animSpriteId">Animated sprite ID.</param>
    private void SelectAnimatedSprite(int animSpriteId)
    {
        isSelected = true;
        selection = animSpriteId;
    }

    /// <summary>
    ///     Lazy-initializes the editor on first render.
    /// </summary>
    private void Initialize()
    {
        // Order the spritesheets. They can't be changed after startup, so this only needs to be done once.
        orderedSpriteSheets = spriteSheetManager.SpriteSheets.Keys.OrderBy(sheetName => sheetName).ToList();
        initialized = true;
    }
}