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

using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Resource Editor tab for viewing and editing sprite definitions.
/// </summary>
public class SpriteEditorTab
{
    private readonly GuiExtensions guiExtensions;
    private readonly SpriteSheetManager spriteSheetManager;

    /// <summary>
    ///     Index (into orderedSpriteSheets) of the currently selected spritesheet.
    /// </summary>
    private int currentSheetIdx;

    // Alphabetically-ordered sprite sheets.
    private List<string> orderedSpriteSheets = new();

    public SpriteEditorTab(SpriteSheetManager spriteSheetManager, GuiExtensions guiExtensions)
    {
        this.spriteSheetManager = spriteSheetManager;
        this.guiExtensions = guiExtensions;
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
        ImGui.Text("Spritesheet:");
        ImGui.SameLine();
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
        ImGui.SameLine();
        if (ImGui.Button("Generate Missing Sprites")) GenerateSprites();
    }

    /// <summary>
    ///     Generates a covering set of sprite definitions for the currently selected spritesheet.
    /// </summary>
    private void GenerateSprites()
    {
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
            guiExtensions.Spritesheet(orderedSpriteSheets[currentSheetIdx]);
            ImGui.EndTable();
        }
    }
}