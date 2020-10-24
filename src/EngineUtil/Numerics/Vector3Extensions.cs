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
using System.Text;

namespace Sovereign.EngineUtil.Numerics
{

    /// <summary>
    /// Utility extensions to System.Numerics.Vector3.
    /// </summary>
    public static class Vector3Extensions
    {

        /// <summary>
        /// Checks whether all components in the vector are less than the
        /// corresponding components in another vector.
        /// </summary>
        /// <param name="left">This vector.</param>
        /// <param name="right">Other vector.</param>
        /// <returns>true if all components are less; false otherwise.</returns>
        public static bool LessThanAll(this Vector3 left, Vector3 right)
        {
            return left.X < right.X && left.Y < right.Y && left.Z < right.Z;
        }

        /// <summary>
        /// Checks whether all components in the vector are less than or equal
        /// to the corresponding components in another vector.
        /// </summary>
        /// <param name="left">This vector.</param>
        /// <param name="right">Other vector.</param>
        /// <returns>true if all components are less than or equal; false otherwise.</returns>
        public static bool LessThanOrEqualAll(this Vector3 left, Vector3 right)
        {
            return left.X <= right.X && left.Y <= right.Y && left.Z <= right.Z;
        }

        /// <summary>
        /// Checks whether all components in the vector are greater than or equal
        /// to the corresponding components in another vector.
        /// </summary>
        /// <param name="left">This vector.</param>
        /// <param name="right">Other vector.</param>
        /// <returns>true if all components are greater than or equal; false otherwise.</returns>
        public static bool GreaterThanOrEqualAll(this Vector3 left, Vector3 right)
        {
            return left.X >= right.X && left.Y >= right.Y && left.Z >= right.Z;
        }

        /// <summary>
        /// Checks whether all components in the vector are greater than
        /// the corresponding components in another vector.
        /// </summary>
        /// <param name="left">This vector.</param>
        /// <param name="right">Other vector.</param>
        /// <returns>true if all components are greater; false otherwise.</returns>
        public static bool GreaterThanAll(this Vector3 left, Vector3 right)
        {
            return left.X > right.X && left.Y > right.Y && left.Z > right.Z;
        }

        /// <summary>
        /// Interpolates a vector between two system ticks.
        /// </summary>
        /// <param name="position">This vector.</param>
        /// <param name="velocity">Time rate of change per second of the vector.</param>
        /// <param name="t">Time to extend over in seconds.</param>
        /// <returns></returns>
        public static Vector3 InterpolateByTime(this Vector3 position, 
            Vector3 velocity, float t)
        {
            return position + (t * velocity);
        }

    }
}
