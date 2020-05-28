/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Components
{

    /// <summary>
    /// Standard operators for common component types.
    /// </summary>
    public static class ComponentOperators
    {

        /// <summary>
        /// Standard operators for int-valued components.
        /// </summary>
        public static readonly IDictionary<ComponentOperation, Func<int, int, int>>
            IntOperators = new Dictionary<ComponentOperation, Func<int, int, int>>()
            {
                {ComponentOperation.Set, (a, b) => b},
                {ComponentOperation.Add, (a, b) => a + b},
                {ComponentOperation.Multiply, (a, b) => a * b},
                {ComponentOperation.Divide, (a, b) => a / b},
            };

        /// <summary>
        /// Standard operators for ulong-valued components.
        /// </summary>
        public static readonly IDictionary<ComponentOperation, Func<ulong, ulong, ulong>>
            UlongOperators = new Dictionary<ComponentOperation, Func<ulong, ulong, ulong>>()
            {
                {ComponentOperation.Set, (a, b) => b},
                {ComponentOperation.Add, (a, b) => a + b},
                {ComponentOperation.Multiply, (a, b) => a * b},
                {ComponentOperation.Divide, (a, b) => a / b},
            };

        /// <summary>
        /// Standard operators for float-valued components.
        /// </summary>
        public static readonly IDictionary<ComponentOperation, Func<float, float, float>>
            FloatOperators = new Dictionary<ComponentOperation, Func<float, float, float>>()
            {
                {ComponentOperation.Set, (a, b) => b},
                {ComponentOperation.Add, (a, b) => a + b},
                {ComponentOperation.Multiply, (a, b) => a * b},
                {ComponentOperation.Divide, (a, b) => a / b},
            };

        /// <summary>
        /// Standard operators for vector-valued components.
        /// </summary>
        public static readonly IDictionary<ComponentOperation, Func<Vector3, Vector3, Vector3>>
            VectorOperators = new Dictionary<ComponentOperation, Func<Vector3, Vector3, Vector3>>()
            {
                {ComponentOperation.Set, (a, b) => b},
                {ComponentOperation.Add, Vector3.Add},
            };

        /// <summary>
        /// Standard operators for boolean-valued components.
        /// </summary>
        public static readonly IDictionary<ComponentOperation, Func<bool, bool, bool>>
            BoolOperators = new Dictionary<ComponentOperation, Func<bool, bool, bool>>()
            {
                {ComponentOperation.Set, (a, b) => b},
            };

    }

}
