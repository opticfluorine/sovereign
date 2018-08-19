using Engine8.EngineUtil.Numerics;
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
