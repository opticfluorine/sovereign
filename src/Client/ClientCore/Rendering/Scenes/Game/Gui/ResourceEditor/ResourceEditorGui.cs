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

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Top-level GUI for resource editing.
/// </summary>
public class ResourceEditorGui
{
    private readonly AnimatedSpriteEditorTab animatedSpriteEditorTab;
    private readonly MaterialEditorTab materialEditorTab;
    private readonly SpriteEditorTab spriteEditorTab;
    private readonly TileSpriteEditorTab tileSpriteEditorTab;

    public ResourceEditorGui(SpriteEditorTab spriteEditorTab, AnimatedSpriteEditorTab animatedSpriteEditorTab,
        TileSpriteEditorTab tileSpriteEditorTab, MaterialEditorTab materialEditorTab)
    {
        this.spriteEditorTab = spriteEditorTab;
        this.animatedSpriteEditorTab = animatedSpriteEditorTab;
        this.tileSpriteEditorTab = tileSpriteEditorTab;
        this.materialEditorTab = materialEditorTab;
    }

    /// <summary>
    ///     Renders the resource editor window.
    /// </summary>
    public void Render()
    {
        if (ImGui.Begin("Resource Editor"))
        {
            if (ImGui.BeginTabBar("ResourceEditorTabs", ImGuiTabBarFlags.None))
            {
                spriteEditorTab.Render();
                animatedSpriteEditorTab.Render();
                tileSpriteEditorTab.Render();
                materialEditorTab.Render();
            }

            ImGui.End();
        }
    }
}