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

using System;
using System.Collections.Generic;
using System.Numerics;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Standard operators for common component types.
/// </summary>
public static class ComponentOperators
{
    /// <summary>
    ///     Standard operators for int-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<int, int, int>>
        IntOperators = new()
        {
            { ComponentOperation.Set, (a, b) => b },
            { ComponentOperation.Add, (a, b) => a + b },
            { ComponentOperation.Multiply, (a, b) => a * b },
            { ComponentOperation.Divide, (a, b) => a / b }
        };

    /// <summary>
    ///     Standard operators for ulong-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<ulong, ulong, ulong>>
        UlongOperators = new()
        {
            { ComponentOperation.Set, (a, b) => b },
            { ComponentOperation.Add, (a, b) => a + b },
            { ComponentOperation.Multiply, (a, b) => a * b },
            { ComponentOperation.Divide, (a, b) => a / b }
        };

    /// <summary>
    ///     Standard operators for float-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<float, float, float>>
        FloatOperators = new()
        {
            { ComponentOperation.Set, (a, b) => b },
            { ComponentOperation.Add, (a, b) => a + b },
            { ComponentOperation.Multiply, (a, b) => a * b },
            { ComponentOperation.Divide, (a, b) => a / b }
        };

    /// <summary>
    ///     Standard operators for vector-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<Vector3, Vector3, Vector3>>
        VectorOperators = new()
        {
            { ComponentOperation.Set, (a, b) => b },
            { ComponentOperation.Add, Vector3.Add }
        };

    /// <summary>
    ///     Standard operators for boolean-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<bool, bool, bool>>
        BoolOperators = new()
        {
            { ComponentOperation.Set, (a, b) => b }
        };

    /// <summary>
    ///     Standard operators for string-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<string, string, string>>
        StringOperators = new()
        {
            { ComponentOperation.Set, (a, b) => b }
        };

    /// <summary>
    ///     Standard operators for GUID-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<Guid, Guid, Guid>>
        GuidOperators = new()
        {
            { ComponentOperation.Set, (a, b) => b }
        };

    /// <summary>
    ///     Standard operators for orientation-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<Orientation, Orientation, Orientation>>
        OrientationOperators = new()
        {
            { ComponentOperation.Set, (a, b) => b }
        };
}