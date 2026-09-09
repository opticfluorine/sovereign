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

using Sovereign.EngineCore.Entities;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Components;

/// <summary>
///     UseRange component. Specifies the maximum distance in world units at which an item may be used as a tool.
/// </summary>
[ScriptableComponents]
public class UseRangeComponentCollection(
    EntityTable entityTable,
    ComponentManager componentManager)
    : BaseComponentCollection<float>(entityTable, componentManager, InitialSize, ComponentOperators.FloatOperators,
        ComponentType.UseRange)
{
    private const int InitialSize = 16384;
}
