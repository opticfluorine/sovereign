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

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Describes the operation to be performed on the component.
/// </summary>
public enum ComponentOperation
{
    /// <summary>
    ///     Sets the value of the component to a new value.
    /// </summary>
    Set = 0,

    /// <summary>
    ///     Adds a constant to the value of the component.
    /// </summary>
    Add = 1,

    /// <summary>
    ///     Multiplies the value of the component by a constant.
    /// </summary>
    Multiply = 2,

    /// <summary>
    ///     Divides the value of the component by a constant.
    /// </summary>
    Divide = 3
}