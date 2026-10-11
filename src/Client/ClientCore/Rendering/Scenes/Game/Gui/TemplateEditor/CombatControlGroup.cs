// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;

/// <summary>
///     Editor control group for combat-related components.
/// </summary>
public sealed class CombatControlGroup
{
    /// <summary>
    ///     Renders the combat control group and updates the given entity definition.
    /// </summary>
    /// <param name="entityDefinition">Entity definition.</param>
    public void Render(EntityDefinition entityDefinition)
    {
        if (!ImGui.CollapsingHeader("Combat", ImGuiTreeNodeFlags.DefaultOpen)) return;
        if (!ImGui.BeginTable("CombatControls", 2, ImGuiTableFlags.SizingFixedFit)) return;

        var inputAttackEnabled = entityDefinition.AttackDetails.HasValue;

        ImGui.TableNextColumn();
        ImGui.Text("Attack:");
        ImGui.TableNextColumn();
        ImGui.Checkbox("##attackEnabled", ref inputAttackEnabled);

        ImGui.TableNextColumn();
        ImGui.Text("Attack Range:");
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(!inputAttackEnabled);
        var inputAttackRange = entityDefinition.AttackDetails?.AttackRange ?? 0f;
        ImGui.InputFloat("##attackRange", ref inputAttackRange, 0.1f, 1.0f, "%.2f");

        ImGui.TableNextColumn();
        ImGui.Text("Attack Delay (us):");
        ImGui.TableNextColumn();
        var inputAttackDelayUs = (int)(entityDefinition.AttackDetails?.AttackDelayUs ?? 0);
        ImGui.InputInt("##attackDelayUs", ref inputAttackDelayUs);
        if (inputAttackDelayUs < 0) inputAttackDelayUs = 0;
        ImGui.EndDisabled();

        ImGui.EndTable();

        entityDefinition.AttackDetails = inputAttackEnabled
            ? new AttackDetails
            {
                AttackRange = inputAttackRange,
                AttackDelayUs = (uint)inputAttackDelayUs
            }
            : null;
    }
}
