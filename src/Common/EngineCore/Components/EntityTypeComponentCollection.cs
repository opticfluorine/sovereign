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

using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Component that denotes whether an entity is of a special type.
/// </summary>
[ScriptableComponents("entity_type")]
public class EntityTypeComponentCollection : BaseComponentCollection<EntityType>
{
    private const int InitialSize = 65536;

    public EntityTypeComponentCollection(EntityTable entityTable, ComponentManager componentManager)
        : base(entityTable, componentManager, InitialSize, ComponentOperators.EntityTypeOperators,
            ComponentType.EntityType)
    {
    }
}