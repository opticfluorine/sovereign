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
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Resource editor tab for Animated Sprites.
/// </summary>
public class AnimatedSpriteEditorTab
{
    /// <summary>
    ///     Color for selected rwo in the browser.
    /// </summary>
    private const uint SelectionColor = 0xFF773333;

    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly GuiExtensions guiExtensions;

    /// <summary>
    ///     Currently selected animated sprite ID.
    /// </summary>
    private int selectedId;

    public AnimatedSpriteEditorTab(AnimatedSpriteManager animatedSpriteManager, GuiExtensions guiExtensions)
    {
        this.animatedSpriteManager = animatedSpriteManager;
        this.guiExtensions = guiExtensions;
    }

    /// <summary>
    ///     Renders the Animated Sprites editor tab.
    /// </summary>
    public void Render()
    {
        if (ImGui.BeginTabItem("Animated Sprites"))
        {
            if (ImGui.BeginTable("animSprOuter", 2, ImGuiTableFlags.SizingFixedFit))
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
    ///     Renders the animated sprite browser that allows the user to select an animated sprite for editing.
    /// </summary>
    private void RenderBrowser()
    {
        if (ImGui.BeginTable("animSprBrowser", 2,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Animated Sprite");
            ImGui.TableHeadersRow();

            for (var i = 0; i < animatedSpriteManager.AnimatedSprites.Count; ++i)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{i}");
                ImGui.TableNextColumn();
                if (guiExtensions.AnimatedSpriteButton($"##spriteButton{i}", i)) selectedId = i;

                if (i == selectedId)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, SelectionColor);
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, SelectionColor);
                }
            }

            ImGui.EndTable();
        }
    }

    /// <summary>
    ///     Renders the animated sprite editor for the currently selected animated sprite.
    /// </summary>
    private void RenderEditor()
    {
    }
}