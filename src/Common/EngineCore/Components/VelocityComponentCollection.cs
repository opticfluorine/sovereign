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

using System.Numerics;

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Manages the velocities of all mobile entities.
/// </summary>
public sealed class VelocityComponentCollection : BaseComponentCollection<Vector3>
{
    /// <summary>
    ///     Default size of component buffer.
    /// </summary>
    private const int BaseSize = 65536;

    public VelocityComponentCollection(ComponentManager componentManager)
        : base(componentManager, BaseSize, ComponentOperators.VectorOperators,
            ComponentType.Velocity)
    {
    }
}