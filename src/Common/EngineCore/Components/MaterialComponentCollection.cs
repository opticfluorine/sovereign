/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.EngineCore.Entities;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Components;

/// <summary>
///     The Material component specifies the material index for a block entity.
/// </summary>
[ScriptableComponents("material")]
public sealed class MaterialComponentCollection : BaseComponentCollection<int>
{
    /// <summary>
    ///     Initial size of component collection.
    /// </summary>
    public const int InitialSize = 65536;

    public MaterialComponentCollection(EntityTable entityTable, ComponentManager componentManager)
        : base(entityTable, componentManager, InitialSize, ComponentOperators.IntOperators,
            ComponentType.Material)
    {
    }
}