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
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.EngineUtil.Ranges;

/// <summary>
///     Contains utility methods for working with ranges.
/// </summary>
public static class RangeUtil
{
    /// <summary>
    ///     Determines whether a point is interior to the given range.
    /// </summary>
    /// <param name="rangeMin">Lower bound (inclusive) of range.</param>
    /// <param name="rangeMax">Upper bound (exclusive) of range.</param>
    /// <param name="point">Point to check.</param>
    /// <returns>true if the point is interior, false otherwise.</returns>
    public static bool IsPointInRange(Vector2 rangeMin, Vector2 rangeMax, Vector2 point)
    {
        return rangeMin.LessThanOrEqualAll(point)
               && point.LessThanAll(rangeMax);
    }

    /// <summary>
    ///     Determines whether a point is interior to the given range.
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
    ///     Determines whether two ranges intersect.
    /// </summary>
    /// <param name="firstRangeMin">Lower bound (inclusive) of first range.</param>
    /// <param name="firstRangeMax">Upper bound (exclusive) of first range.</param>
    /// <param name="secondRangeMin">Lower bound (inclusive) of second range.</param>
    /// <param name="secondRangeMax">Upper bound (exclusive) of second range.</param>
    /// <returns>true if the ranges intersect, false otherwise.</returns>
    public static bool RangesIntersect(Vector2 firstRangeMin, Vector2 firstRangeMax,
        Vector2 secondRangeMin, Vector2 secondRangeMax)
    {
        return firstRangeMax.GreaterThanAll(secondRangeMin)
               && secondRangeMax.GreaterThanAll(firstRangeMin);
    }

    /// <summary>
    ///     Determines whether two 1D ranges intersect.
    /// </summary>
    /// <param name="firstRange">First range (min, max).</param>
    /// <param name="secondRange">Second range (min, max).</param>
    /// <returns>true if intersect, false otherwise.</returns>
    public static bool RangesIntersect(Vector2 firstRange, Vector2 secondRange)
    {
        return firstRange.X <= secondRange.Y && secondRange.X <= firstRange.Y;
    }

    /// <summary>
    ///     Determines whether two ranges intersect.
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
    ///     Determines whether one range is completely interior to another.
    /// </summary>
    /// <param name="innerRangeMin">Lower bound (inclusive) of the inner range.</param>
    /// <param name="innerRangeMax">Upper bound (exclusive) of the inner range.</param>
    /// <param name="outerRangeMin">Lower bound (inclusive) of the outer range.</param>
    /// <param name="outerRangeMax">Upper bound (exclusive) of the outer range.</param>
    /// <returns>true if the inner range is completely interior, false otherwise.</returns>
    public static bool IsRangeInterior(Vector2 innerRangeMin, Vector2 innerRangeMax,
        Vector2 outerRangeMin, Vector2 outerRangeMax)
    {
        return outerRangeMin.LessThanOrEqualAll(innerRangeMin)
               && innerRangeMax.LessThanAll(outerRangeMax);
    }

    /// <summary>
    ///     Determines whether one range is completely interior to another.
    /// </summary>
    /// <param name="innerRangeMin">Lower bound (inclusive) of the inner range.</param>
    /// <param name="innerRangeMax">Upper bound (exclusive) of the inner range.</param>
    /// <param name="outerRangeMin">Lower bound (inclusive) of the outer range.</param>
    /// <param name="outerRangeMax">Upper bound (exclusive) of the outer range.</param>
    /// <returns>true if the inner range is completely interior, false otherwise.</returns>
    public static bool IsRangeInterior(Vector3 innerRangeMin, Vector3 innerRangeMax,
        Vector3 outerRangeMin, Vector3 outerRangeMax)
    {
        return outerRangeMin.LessThanOrEqualAll(innerRangeMin)
               && innerRangeMax.LessThanAll(outerRangeMax);
    }
}