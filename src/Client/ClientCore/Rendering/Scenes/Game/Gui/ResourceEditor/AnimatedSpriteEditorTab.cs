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
using Sovereign.EngineUtil.Numerics;

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
    private readonly SpriteSelectorPopup spriteSelectorPopup;

    /// <summary>
    ///     Initialization flag.
    /// </summary>
    private bool initialized;

    /// <summary>
    ///     Input value for animation timestep in milliseconds.
    /// </summary>
    private float inputTimestepMs;

    /// <summary>
    ///     Currently selected animated sprite ID.
    /// </summary>
    private int selectedId;

    public AnimatedSpriteEditorTab(AnimatedSpriteManager animatedSpriteManager, GuiExtensions guiExtensions,
        SpriteSelectorPopup spriteSelectorPopup)
    {
        this.animatedSpriteManager = animatedSpriteManager;
        this.guiExtensions = guiExtensions;
        this.spriteSelectorPopup = spriteSelectorPopup;
    }

    /// <summary>
    ///     Renders the Animated Sprites editor tab.
    /// </summary>
    public void Render()
    {
        if (!initialized)
        {
            Select(0);
            initialized = true;
        }

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
        // Browser selector.
        var maxSize = ImGui.GetWindowSize();
        if (ImGui.BeginTable("animSprBrowser", 2,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.RowBg, maxSize with { Y = maxSize.Y - 90 }))
        {
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Animated Sprite");
            ImGui.TableHeadersRow();

            for (var i = 0; i < animatedSpriteManager.AnimatedSprites.Count; ++i)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{i}");
                ImGui.TableNextColumn();
                if (guiExtensions.AnimatedSpriteButton($"##spriteButton{i}", i)) Select(i);

                if (i == selectedId)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, SelectionColor);
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, SelectionColor);
                }
            }

            ImGui.EndTable();

            // Bottom control row.
            if (ImGui.BeginTable("browserControls", 3, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("##Span", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextColumn();

                // Insert new sprite button.
                ImGui.TableNextColumn();
                ImGui.Button("+");
                if (ImGui.IsItemHovered())
                {
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text("Insert New");
                        ImGui.EndTooltip();
                    }
                }

                // Remove selected sprite button.
                ImGui.TableNextColumn();
                ImGui.Button("-");
                if (ImGui.IsItemHovered())
                {
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text("Remove Selected");
                        ImGui.EndTooltip();
                    }
                }

                ImGui.EndTable();
            }
        }
    }

    /// <summary>
    ///     Renders the animated sprite editor for the currently selected animated sprite.
    /// </summary>
    private void RenderEditor()
    {
        ImGui.Text($"Animated Sprite {selectedId}");

        ImGui.Separator();

        ImGui.Text("Animation Timestep:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(120.0f);
        ImGui.InputFloat("ms##timestep", ref inputTimestepMs);

        ImGui.Button("Save");
        ImGui.SameLine();
        if (ImGui.Button("Cancel")) ResetState();
    }

    /// <summary>
    ///     Selects an animated sprite and sets up all input fields.
    /// </summary>
    /// <param name="id"></param>
    private void Select(int id)
    {
        selectedId = id;
        ResetState();
    }

    /// <summary>
    ///     Resets state to match the currently applied definition of the selected animated sprite.
    /// </summary>
    private void ResetState()
    {
        inputTimestepMs = animatedSpriteManager.AnimatedSprites[selectedId].FrameTime * UnitConversions.UsToMs;
    }
}