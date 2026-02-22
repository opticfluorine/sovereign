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
using System.Numerics;

namespace Sovereign.EngineUtil.Numerics;

/// <summary>
///     Utility extensions to System.Numerics.Vector3.
/// </summary>
public static class Vector3Extensions
{
    /// <summary>
    ///     Checks whether all components in the vector are less than the
    ///     corresponding components in another vector.
    /// </summary>
    /// <param name="left">This vector.</param>
    /// <param name="right">Other vector.</param>
    /// <returns>true if all components are less; false otherwise.</returns>
    public static bool LessThanAll(this Vector3 left, Vector3 right)
    {
        return left.X < right.X && left.Y < right.Y && left.Z < right.Z;
    }

    /// <summary>
    ///     Checks whether all components in the vector are less than or equal
    ///     to the corresponding components in another vector.
    /// </summary>
    /// <param name="left">This vector.</param>
    /// <param name="right">Other vector.</param>
    /// <returns>true if all components are less than or equal; false otherwise.</returns>
    public static bool LessThanOrEqualAll(this Vector3 left, Vector3 right)
    {
        return left.X <= right.X && left.Y <= right.Y && left.Z <= right.Z;
    }

    /// <summary>
    ///     Checks whether all components in the vector are greater than or equal
    ///     to the corresponding components in another vector.
    /// </summary>
    /// <param name="left">This vector.</param>
    /// <param name="right">Other vector.</param>
    /// <returns>true if all components are greater than or equal; false otherwise.</returns>
    public static bool GreaterThanOrEqualAll(this Vector3 left, Vector3 right)
    {
        return left.X >= right.X && left.Y >= right.Y && left.Z >= right.Z;
    }

    /// <summary>
    ///     Checks whether all components in the vector are greater than
    ///     the corresponding components in another vector.
    /// </summary>
    /// <param name="left">This vector.</param>
    /// <param name="right">Other vector.</param>
    /// <returns>true if all components are greater; false otherwise.</returns>
    public static bool GreaterThanAll(this Vector3 left, Vector3 right)
    {
        return left.X > right.X && left.Y > right.Y && left.Z > right.Z;
    }

    /// <summary>
    ///     Interpolates a vector between two system ticks.
    /// </summary>
    /// <param name="position">This vector.</param>
    /// <param name="velocity">Time rate of change per second of the vector.</param>
    /// <param name="t">Time to extend over in seconds.</param>
    /// <returns></returns>
    public static Vector3 InterpolateByTime(this Vector3 position,
        Vector3 velocity, float t)
    {
        return position + t * velocity;
    }

    /// <summary>
    ///     Checks whether all components in the vector are finite.
    /// </summary>
    /// <param name="v">This vector.</param>
    /// <returns>true if all components are finite; false otherwise.</returns>
    public static bool IsFinite(this Vector3 v)
    {
        return float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);
    }

    /// <summary>
    ///     Returns the vector whose components are the floor of each component of this vector.
    /// </summary>
    /// <param name="orig">This vector.</param>
    /// <returns>Floor of the input vector.</returns>
    public static Vector3 Floor(this Vector3 orig)
    {
        return new Vector3((float)Math.Floor(orig.X), (float)Math.Floor(orig.Y), (float)Math.Floor(orig.Z));
    }
}