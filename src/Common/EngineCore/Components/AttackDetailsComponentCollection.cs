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

using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Components;

/// <summary>
///     AttackDetails component. Describes the attack capabilities of an entity.
/// </summary>
[ScriptableComponents]
public class AttackDetailsComponentCollection(
    EntityTable entityTable,
    ComponentManager componentManager)
    : BaseComponentCollection<AttackDetails>(entityTable, componentManager, InitialSize, ComponentOperators.AttackDetailsOperators,
        ComponentType.AttackDetails)
{
    private const int InitialSize = 16384;
}
