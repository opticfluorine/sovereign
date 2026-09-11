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
/// <remarks>
///     The numeric value affects the order in which the updates are applied - lower values are applied first.
/// </remarks>
public enum ComponentOperation
{
    /// <summary>
    ///     Sets the value of the component to a new value.
    /// </summary>
    Set = 0xffff,

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
    Divide = 3,

    /// <summary>
    ///     For Kinematics components, sets the velocity part of the component, leaving position unchanged.
    /// </summary>
    SetVelocity = 4,

    /// <summary>
    ///     For Kinematics components, adds the position part of the component, leaving velocity unchanged.
    /// </summary>
    AddPosition = 5,

    /// <summary>
    ///     Adds while clamping the resulting value to avoid integer overflow.
    /// </summary>
    AddNoOverflow = 6,

    /// <summary>
    ///     Subtracts while clamping the resulting value to avoid integer underflow.
    /// </summary>
    SubtractNoUnderflow = 7,

    /// <summary>
    ///     For Vital components, sets the value part of the component, leaving the other parts unchanged.
    /// </summary>
    SetValue = 8,

    /// <summary>
    ///     For Vital components, adds to the value part of the component, leaving the other parts unchanged.
    /// </summary>
    AddValue = 9,

    /// <summary>
    ///     For Vital components, sets the max value part of the component, leaving the other parts unchanged.
    /// </summary>
    SetMaxValue = 10,

    /// <summary>
    ///     For Vital components, adds to the max value part of the component, leaving the other parts unchanged.
    /// </summary>
    AddMaxValue = 11,

    /// <summary>
    ///     For Vital components, sets the change rate and change interval parts of the component,
    ///     leaving the value parts unchanged.
    /// </summary>
    SetChangeRate = 12
}
