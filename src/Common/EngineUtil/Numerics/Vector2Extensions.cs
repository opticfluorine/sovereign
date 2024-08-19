// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Numerics;

namespace Sovereign.EngineUtil.Numerics;

/// <summary>
///     Utility extensions to System.Numerics.Vector2.
/// </summary>
public static class Vector2Extensions
{
    /// <summary>
    ///     Checks whether all components in the vector are less than the
    ///     corresponding components in another vector.
    /// </summary>
    /// <param name="left">This vector.</param>
    /// <param name="right">Other vector.</param>
    /// <returns>true if all components are less; false otherwise.</returns>
    public static bool LessThanAll(this Vector2 left, Vector2 right)
    {
        return left.X < right.X && left.Y < right.Y;
    }

    /// <summary>
    ///     Checks whether all components in the vector are less than or equal
    ///     to the corresponding components in another vector.
    /// </summary>
    /// <param name="left">This vector.</param>
    /// <param name="right">Other vector.</param>
    /// <returns>true if all components are less than or equal; false otherwise.</returns>
    public static bool LessThanOrEqualAll(this Vector2 left, Vector2 right)
    {
        return left.X <= right.X && left.Y <= right.Y;
    }

    /// <summary>
    ///     Checks whether all components in the vector are greater than or equal
    ///     to the corresponding components in another vector.
    /// </summary>
    /// <param name="left">This vector.</param>
    /// <param name="right">Other vector.</param>
    /// <returns>true if all components are greater than or equal; false otherwise.</returns>
    public static bool GreaterThanOrEqualAll(this Vector2 left, Vector2 right)
    {
        return left.X >= right.X && left.Y >= right.Y;
    }

    /// <summary>
    ///     Checks whether all components in the vector are greater than
    ///     the corresponding components in another vector.
    /// </summary>
    /// <param name="left">This vector.</param>
    /// <param name="right">Other vector.</param>
    /// <returns>true if all components are greater; false otherwise.</returns>
    public static bool GreaterThanAll(this Vector2 left, Vector2 right)
    {
        return left.X > right.X && left.Y > right.Y;
    }
}