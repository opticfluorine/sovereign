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
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Reusable popup window for visually selecting a Sprite.
/// </summary>
public class SpriteSelectorPopup
{
    private const string PopupName = "Select Sprite";
    private readonly GuiExtensions guiExtensions;
    private readonly SpriteManager spriteManager;
    private readonly SpriteSheetManager spriteSheetManager;

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
    ///     Alphabetically-ordered sprite sheets.
    /// </summary>
    private List<string> orderedSpriteSheets = new();

    /// <summary>
    ///     Selected sprite.
    /// </summary>
    private int selection;

    public SpriteSelectorPopup(SpriteSheetManager spriteSheetManager, GuiExtensions guiExtensions,
        SpriteManager spriteManager)
    {
        this.spriteSheetManager = spriteSheetManager;
        this.guiExtensions = guiExtensions;
        this.spriteManager = spriteManager;
    }

    /// <summary>
    ///     Opens the selector popup.
    /// </summary>
    public void Open()
    {
        isSelected = false;
        selection = 0;

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
            ImGui.EndPopup();
        }
    }

    /// <summary>
    ///     Tries to get the latest selection.
    /// </summary>
    /// <param name="spriteId">Set to the selected sprite ID if returns true.</param>
    /// <returns>true if a selection has been made since the last call to Open(); false otherwise.</returns>
    public bool TryGetSelection(out int spriteId)
    {
        spriteId = selection;
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
        if (ImGui.BeginCombo("##spriteSheetCombo", orderedSpriteSheets[currentSheetIdx]))
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
        if (ImGui.BeginTable("spritesheetView", 1, ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY))
        {
            ImGui.TableNextColumn();
            var sheetName = orderedSpriteSheets[currentSheetIdx];
            guiExtensions.Spritesheet(sheetName);

            var drawList = ImGui.GetWindowDrawList();
            var coverageMap = spriteManager.SpriteSheetCoverage[sheetName];

            var sheet = spriteSheetManager.SpriteSheets[sheetName];
            var rows = sheet.Surface.Properties.Height / sheet.Definition.SpriteHeight;
            var cols = sheet.Surface.Properties.Width / sheet.Definition.SpriteWidth;
            var start = ImGui.GetWindowPos() - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());

            for (var i = 0; i < rows; ++i)
            for (var j = 0; j < cols; ++j)
                if (coverageMap[i, j] != null)
                {
                    // Sprite exists, draw a box around it.
                    var minPos = start + new Vector2(j * sheet.Definition.SpriteWidth,
                        i * sheet.Definition.SpriteHeight);
                    var maxPos = minPos + new Vector2(sheet.Definition.SpriteWidth, sheet.Definition.SpriteHeight);
                    drawList.AddRect(minPos, maxPos, 0x7FFFFFFF);
                }

            // If mouse is over a covered sprite, show the sprite ID in a tooltip.
            var relMousePos = ImGui.GetMousePos() - start;
            var windowSize = ImGui.GetWindowSize();
            if (relMousePos.X >= 0.0f && relMousePos.Y >= 0.0f &&
                relMousePos.X < Math.Min(sheet.Surface.Properties.Width, windowSize.X)
                && relMousePos.Y < Math.Min(sheet.Surface.Properties.Height, windowSize.Y))
            {
                // Mouse is overlapping the spritesheet.
                var row = (int)Math.Floor(relMousePos.Y / sheet.Definition.SpriteHeight);
                var col = (int)Math.Floor(relMousePos.X / sheet.Definition.SpriteWidth);

                var sprite = coverageMap[row, col];
                if (sprite != null)
                    // Mouse is hovering over a defined sprite, show tooltip.
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text($"Sprite {sprite.Id}");
                        ImGui.EndTooltip();
                    }

                // If a sprite is clicked, select it and close the popup.
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    if (sprite != null)
                    {
                        selection = sprite.Id;
                        isSelected = true;
                        ImGui.CloseCurrentPopup();
                    }
                }
            }

            ImGui.EndTable();
        }
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