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
///     NpcFlags component. NPC-specific flags for behavioral characteristics.
/// </summary>
[ScriptableComponents]
public class NpcFlagsComponentCollection(
    EntityTable entityTable,
    ComponentManager componentManager)
    : BaseComponentCollection<NpcFlag>(entityTable, componentManager, InitialSize,
        ComponentOperators.NpcFlagOperators, ComponentType.NpcFlags)
{
    private const int InitialSize = 16384;
}