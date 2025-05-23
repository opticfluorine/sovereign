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
///     Reusable GUI component for viewing and selecting spritesheets.
/// </summary>
public class SpritesheetSelector
{
    private readonly GuiExtensions guiExtensions;

    /// <summary>
    ///     Preferred size of selector if sufficient space is available.
    /// </summary>
    private readonly Vector2 preferredSize = new(500.0f, 400.0f);

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
    ///     Flag indicating whether the sheet selection combo is open.
    /// </summary>
    private bool isSheetComboOpen;

    /// <summary>
    ///     Alphabetically-ordered sprite sheets.
    /// </summary>
    private List<string> orderedSpriteSheets = new();

    public SpritesheetSelector(SpriteSheetManager spriteSheetManager, GuiExtensions guiExtensions,
        SpriteManager spriteManager)
    {
        this.spriteSheetManager = spriteSheetManager;
        this.guiExtensions = guiExtensions;
        this.spriteManager = spriteManager;
    }

    /// <summary>
    ///     Name of currently selected spritesheet.
    /// </summary>
    public string SelectedSpritesheetName => orderedSpriteSheets[currentSheetIdx];

    /// <summary>
    ///     Renders the selector popup if open, otherwise does nothing.
    /// </summary>
    public void Render()
    {
        if (!initialized) Initialize();

        DrawTopBar();
        DrawSpritesheetView();
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
        var fontSize = ImGui.GetFontSize();

        // Wrap the view in a single-cell table to get scrollbars for larger spritesheets.
        var screenSize = ImGui.GetIO().DisplaySize;
        var basePos = ImGui.GetCursorPos();
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

            for (var i = 0; i < rows; ++i)
            for (var j = 0; j < cols; ++j)
                if (coverageMap[i, j] != null)
                {
                    // Sprite exists, draw a box around it.
                    var minPos = start + fontSize * GuiExtensions.SpriteScaleFactor * new Vector2(
                        j * sheet.Definition.SpriteWidth,
                        i * sheet.Definition.SpriteHeight);
                    var maxPos = minPos + fontSize * GuiExtensions.SpriteScaleFactor *
                        new Vector2(sheet.Definition.SpriteWidth, sheet.Definition.SpriteHeight);
                    drawList.AddRect(minPos, maxPos, 0x7FFFFFFF);
                }

            // If mouse is over a covered sprite, show the sprite ID in a tooltip.
            var relMousePos = ImGui.GetMousePos() - start;
            if (spriteHovered)
            {
                // Mouse is overlapping the spritesheet.
                var scaledHeight = fontSize * GuiExtensions.SpriteScaleFactor * sheet.Definition.SpriteHeight;
                var scaledWidth = fontSize * GuiExtensions.SpriteScaleFactor * sheet.Definition.SpriteWidth;
                var row = (int)Math.Floor(relMousePos.Y / scaledHeight);
                var col = (int)Math.Floor(relMousePos.X / scaledWidth);
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