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
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Resource Editor tab for viewing and editing sprite definitions.
/// </summary>
public class SpriteEditorTab
{
    /// <summary>
    ///     Identifier for popup error message.
    /// </summary>
    private const string ErrorPopup = "Error";

    private readonly SpriteDefinitionsGenerator generator;
    private readonly GuiExtensions guiExtensions;
    private readonly ILogger<SpriteEditorTab> logger;
    private readonly SpriteManager spriteManager;
    private readonly SpriteSheetManager spriteSheetManager;

    /// <summary>
    ///     Index (into orderedSpriteSheets) of the currently selected spritesheet.
    /// </summary>
    private int currentSheetIdx;

    /// <summary>
    ///     Exception to report to user, if any.
    /// </summary>
    private Exception? exceptionToReport;

    /// <summary>
    ///     Alphabetically-ordered sprite sheets.
    /// </summary>
    private List<string> orderedSpriteSheets = new();

    public SpriteEditorTab(SpriteSheetManager spriteSheetManager, GuiExtensions guiExtensions,
        SpriteManager spriteManager, SpriteDefinitionsGenerator generator, ILogger<SpriteEditorTab> logger)
    {
        this.spriteSheetManager = spriteSheetManager;
        this.guiExtensions = guiExtensions;
        this.spriteManager = spriteManager;
        this.generator = generator;
        this.logger = logger;
    }

    /// <summary>
    ///     Renders the Sprite Editor tab. Must be called from inside a tab bar.
    /// </summary>
    public void Render()
    {
        // Initialize sprite sheet selector if not yet initialized.
        if (orderedSpriteSheets.Count == 0) Initialize();

        //
        if (ImGui.BeginTabItem("Sprites"))
        {
            DrawTopBar();
            DrawSpritesheetView();
            ImGui.EndTabItem();
        }
    }

    /// <summary>
    ///     Lazy-initializes the editor on first render.
    /// </summary>
    private void Initialize()
    {
        // Order the spritesheets. They can't be changed after startup, so this only needs to be done once.
        orderedSpriteSheets = spriteSheetManager.SpriteSheets.Keys.OrderBy(sheetName => sheetName).ToList();
    }

    /// <summary>
    ///     Draws the top bar with controls.
    /// </summary>
    private void DrawTopBar()
    {
        // Combo box for selecting the current spritesheet.
        if (!ImGui.BeginTable("tileTopBar", 3, ImGuiTableFlags.SizingFixedFit)) return;

        ImGui.TableSetupColumn("");
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextColumn();
        ImGui.Text("Spritesheet:");

        ImGui.TableNextColumn();
        if (ImGui.BeginCombo("##spriteSheetCombo", orderedSpriteSheets[currentSheetIdx]))
        {
            for (var i = 0; i < orderedSpriteSheets.Count; ++i)
            {
                var sheetName = orderedSpriteSheets[i];
                var isSelected = i == currentSheetIdx;
                if (ImGui.Selectable(sheetName, i == currentSheetIdx)) currentSheetIdx = i;
                if (isSelected) ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        // Spritesheet control bar.
        ImGui.TableNextColumn();
        if (ImGui.Button("Generate Missing Sprites")) GenerateSprites();

        ImGui.EndTable();

        // Error modal if needed.
        var open = true;
        if (ImGui.BeginPopupModal(ErrorPopup, ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("An error occured while generating sprites.");
            if (exceptionToReport != null)
            {
                ImGui.Spacing();
                ImGui.Text(exceptionToReport.Message);
            }

            if (ImGui.Button("OK"))
            {
                exceptionToReport = null;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    ///     Generates a covering set of sprite definitions for the currently selected spritesheet.
    /// </summary>
    private void GenerateSprites()
    {
        try
        {
            generator.GenerateMissingSpritesForSheet(orderedSpriteSheets[currentSheetIdx]);
            spriteManager.UpdateAndSaveSprites();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error during sprite generation for {Sheet}.", orderedSpriteSheets[currentSheetIdx]);
            exceptionToReport = e;
            ImGui.OpenPopup(ErrorPopup);
        }
    }

    /// <summary>
    ///     Draws the information view for the currently selected spritesheet.
    /// </summary>
    private void DrawSpritesheetView()
    {
        var fontSize = ImGui.GetFontSize();

        // Wrap the view in a single-cell table to get scrollbars for larger spritesheets.
        if (ImGui.BeginTable("spritesheetView", 1, ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY))
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
                var row = (int)Math.Floor(relMousePos.Y /
                                          (sheet.Definition.SpriteHeight * GuiExtensions.SpriteScaleFactor * fontSize));
                var col = (int)Math.Floor(relMousePos.X /
                                          (sheet.Definition.SpriteWidth * GuiExtensions.SpriteScaleFactor * fontSize));
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
}