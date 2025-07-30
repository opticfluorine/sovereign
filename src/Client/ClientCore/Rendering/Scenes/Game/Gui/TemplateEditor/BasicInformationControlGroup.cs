// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using Hexa.NET.ImGui;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;

/// <summary>
///     Control group for editing basic information of a template entity.
/// </summary>
public sealed class BasicInformationControlGroup(GuiComponentEditors guiComponentEditors)
{
    /// <summary>
    ///     Renders control group and updates the given entity definition.
    /// </summary>
    /// <param name="entityDefinition">Entity definition.</param>
    public void Render(EntityDefinition entityDefinition)
    {
        if (ImGui.CollapsingHeader("Basic Information", ImGuiTreeNodeFlags.DefaultOpen))
            if (ImGui.BeginTable("BasicInfo", 2, ImGuiTableFlags.SizingFixedFit))
            {
                NameControls(entityDefinition);

                ImGui.EndTable();
            }
    }

    /// <summary>
    ///     Renders the Name controls.
    /// </summary>
    /// <param name="entityDefinition">Entity definition.</param>
    private void NameControls(EntityDefinition entityDefinition)
    {
        ImGui.TableNextColumn();
        ImGui.Text("Name:");
        ImGui.TableNextColumn();
        var name = entityDefinition.Name ?? "";
        guiComponentEditors.NameEdit("##name", ref name);
        entityDefinition.Name = name;
    }
}