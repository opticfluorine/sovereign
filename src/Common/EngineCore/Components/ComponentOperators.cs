/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Numerics;

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
}