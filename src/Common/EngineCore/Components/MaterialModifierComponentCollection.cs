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

namespace Sovereign.EngineCore.Components;

/// <summary>
///     The MaterialModifier component specifies the material modifier of a
///     material block entity.
/// </summary>
public sealed class MaterialModifierComponentCollection : BaseComponentCollection<int>
{
    /// <summary>
    ///     Initial number of components.
    /// </summary>
    public const int InitialCount = 65536;

    public MaterialModifierComponentCollection(EntityTable entityTable, ComponentManager componentManager)
        : base(entityTable, componentManager, InitialCount, ComponentOperators.IntOperators,
            ComponentType.MaterialModifier)
    {
    }
}