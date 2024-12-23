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

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;

/// <summary>
///     Top-level GUI for template entity editing.
/// </summary>
public class TemplateEditorGui
{
    private readonly BlockTemplateEditorTab blockTemplateEditor;

    public TemplateEditorGui(BlockTemplateEditorTab blockTemplateEditor)
    {
        this.blockTemplateEditor = blockTemplateEditor;
    }

    /// <summary>
    ///     Renders the template entity editor window if needed.
    /// </summary>
    public void Render()
    {
        var fontSize = ImGui.GetFontSize();

        ImGui.SetNextWindowSize(fontSize * new Vector2(50.0f, 35.56f), ImGuiCond.Once);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, fontSize * new Vector2(0.889f, 0.0f));
        if (!ImGui.Begin("Template Entity Editor")) return;

        ImGui.Spacing();
        ImGui.Spacing();
        if (ImGui.BeginTabBar("TemplateEditorTabs", ImGuiTabBarFlags.None))
        {
            blockTemplateEditor.Render();
            ImGui.EndTabBar();
        }

        ImGui.End();
        ImGui.PopStyleVar();
    }
}