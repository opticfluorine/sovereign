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

using Sovereign.EngineUtil.Numerics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Sovereign.EngineUtil.Ranges
{

    /// <summary>
    /// Contains utility methods for working with ranges.
    /// </summary>
    public static class RangeUtil
    {

        /// <summary>
        /// Determines whether a point is interior to the given range.
        /// </summary>
        /// <param name="rangeMin">Lower bound (inclusive) of range.</param>
        /// <param name="rangeMax">Upper bound (exclusive) of range.</param>
        /// <param name="point">Point to check.</param>
        /// <returns>true if the point is interior, false otherwise.</returns>
        public static bool IsPointInRange(Vector3 rangeMin, Vector3 rangeMax,
            Vector3 point)
        {
            return rangeMin.LessThanOrEqualAll(point)
                && point.LessThanAll(rangeMax);
        }

        /// <summary>
        /// Determines whether two ranges intersect.
        /// </summary>
        /// <param name="firstRangeMin">Lower bound (inclusive) of first range.</param>
        /// <param name="firstRangeMax">Upper bound (exclusive) of first range.</param>
        /// <param name="secondRangeMin">Lower bound (inclusive) of second range.</param>
        /// <param name="secondRangeMax">Upper bound (exclusive) of second range.</param>
        /// <returns>true if the ranges intersect, false otherwise.</returns>
        public static bool RangesIntersect(Vector3 firstRangeMin, Vector3 firstRangeMax,
            Vector3 secondRangeMin, Vector3 secondRangeMax)
        {
            return firstRangeMax.GreaterThanAll(secondRangeMin)
                && secondRangeMax.GreaterThanAll(firstRangeMin);
        }

        /// <summary>
        /// Determines whether one range is completely interior to another.
        /// </summary>
        /// <param name="innerRangeMin">Lower bound (inclusive) of the inner range.</param>
        /// <param name="innerRangeMax">Upper bound (exclusive) of the inner range.</param>
        /// <param name="outerRangeMin">Lower bound (inclusive) of the outer range.</param>
        /// <param name="outerRangeMax">Upper bound (exclusive) of the outer range.</param>
        /// <returns></returns>
        public static bool IsRangeInterior(Vector3 innerRangeMin, Vector3 innerRangeMax,
            Vector3 outerRangeMin, Vector3 outerRangeMax)
        {
            return outerRangeMin.LessThanOrEqualAll(innerRangeMin)
                && innerRangeMax.LessThanAll(outerRangeMax);
        }

    }

}
