using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Engine8.EngineUtil.Ranges
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
        public static bool IsPointInRange(Vector<float> rangeMin, Vector<float> rangeMax,
            Vector<float> point)
        {
            return Vector.LessThanOrEqualAll(rangeMin, point) 
                && Vector.LessThanAll(point, rangeMax);
        }

        /// <summary>
        /// Determines whether two ranges intersect.
        /// </summary>
        /// <param name="firstRangeMin">Lower bound (inclusive) of first range.</param>
        /// <param name="firstRangeMax">Upper bound (exclusive) of first range.</param>
        /// <param name="secondRangeMin">Lower bound (inclusive) of second range.</param>
        /// <param name="secondRangeMax">Upper bound (exclusive) of second range.</param>
        /// <returns>true if the ranges intersect, false otherwise.</returns>
        public static bool RangesIntersect(Vector<float> firstRangeMin, Vector<float> firstRangeMax,
            Vector<float> secondRangeMin, Vector<float> secondRangeMax)
        {
            return Vector.GreaterThanAll(firstRangeMax, secondRangeMin)
                && Vector.GreaterThanAll(secondRangeMax, firstRangeMin);
        }

        /// <summary>
        /// Determines whether one range is completely interior to another.
        /// </summary>
        /// <param name="innerRangeMin">Lower bound (inclusive) of the inner range.</param>
        /// <param name="innerRangeMax">Upper bound (exclusive) of the inner range.</param>
        /// <param name="outerRangeMin">Lower bound (inclusive) of the outer range.</param>
        /// <param name="outerRangeMax">Upper bound (exclusive) of the outer range.</param>
        /// <returns></returns>
        public static bool IsRangeInterior(Vector<float> innerRangeMin, Vector<float> innerRangeMax,
            Vector<float> outerRangeMin, Vector<float> outerRangeMax)
        {
            return Vector.LessThanOrEqualAll(outerRangeMin, innerRangeMin)
                && Vector.LessThanAll(innerRangeMax, outerRangeMax);
        }

    }

}
