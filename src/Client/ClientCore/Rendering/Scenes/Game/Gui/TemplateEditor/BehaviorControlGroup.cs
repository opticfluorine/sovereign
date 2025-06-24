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

using System;
using System.Numerics;
using ImGuiNET;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;

/// <summary>
///     Editor control group for behavior-related components.
/// </summary>
public sealed class BehaviorControlGroup
{
    /// <summary>
    ///     Renders the behavior control group and updates the given entity definition.
    /// </summary>
    /// <param name="entityDefinition">Entity definition.</param>
    public void Render(EntityDefinition entityDefinition)
    {
        if (ImGui.CollapsingHeader("Behavior", ImGuiTreeNodeFlags.DefaultOpen))
            if (ImGui.BeginTable("Behavior", 2, ImGuiTableFlags.SizingFixedFit))
            {
                PhysicsControls(entityDefinition);
                BoundingBoxControls(entityDefinition);

                ImGui.EndTable();
            }
    }

    /// <summary>
    ///     Renders the Physics component control.
    /// </summary>
    /// <param name="entityDefinition">Entity definition.</param>
    private static void PhysicsControls(EntityDefinition entityDefinition)
    {
        ImGui.TableNextColumn();
        ImGui.Text("Physics:");
        ImGui.TableNextColumn();
        var inputPhysics = entityDefinition.Physics;
        ImGui.Checkbox("##physics", ref inputPhysics);
        entityDefinition.Physics = inputPhysics;
    }

    /// <summary>
    ///     Renders the BoundingBox controls.
    /// </summary>
    /// <param name="entityDefinition">Entity definition.</param>
    private void BoundingBoxControls(EntityDefinition entityDefinition)
    {
        var inputPhysics = entityDefinition.Physics;
        ImGui.TableNextColumn();
        ImGui.Text("Bounding Box Position:");
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(!inputPhysics);
        var fontSize = ImGui.GetFontSize();
        ImGui.SetNextItemWidth(fontSize * 8.0f);
        var inputBoundingBoxPosition = entityDefinition.BoundingBox?.Position ?? Vector3.Zero;
        ImGui.InputFloat3("##boundingBoxPosition", ref inputBoundingBoxPosition);
        ImGui.EndDisabled();

        ImGui.TableNextColumn();
        ImGui.Text("Bounding Box Size:");
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(!inputPhysics);
        ImGui.SetNextItemWidth(fontSize * 8.0f);
        var inputBoundingBoxSize = entityDefinition.BoundingBox?.Size ?? Vector3.One;
        ImGui.InputFloat3("##boundingBoxSize", ref inputBoundingBoxSize);
        inputBoundingBoxSize.X = Math.Max(inputBoundingBoxSize.X, 0.0f);
        inputBoundingBoxSize.Y = Math.Max(inputBoundingBoxSize.Y, 0.0f);
        inputBoundingBoxSize.Z = Math.Max(inputBoundingBoxSize.Z, 0.0f);
        ImGui.EndDisabled();

        if (inputPhysics)
            entityDefinition.BoundingBox = new BoundingBox
            {
                Position = inputBoundingBoxPosition,
                Size = inputBoundingBoxSize
            };
        else
            entityDefinition.BoundingBox = null;
    }
}