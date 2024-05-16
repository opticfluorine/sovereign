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
///     The AboveBlock component tracks the entity ID of the block, if any, directly
///     above a block.
/// </summary>
public sealed class AboveBlockComponentCollection : BaseComponentCollection<ulong>
{
    /// <summary>
    ///     Initial size of component collection.
    /// </summary>
    public const int InitialSize = 65536;

    public AboveBlockComponentCollection(EntityTable entityTable, ComponentManager manager)
        : base(entityTable, manager, InitialSize, ComponentOperators.UlongOperators,
            ComponentType.AboveBlock)
    {
    }
}