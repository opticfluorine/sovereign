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
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;
using MessagePack;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Represents an integer position, useful for aligning blocks on the unit grid.
/// </summary>
[MessagePackObject]
public struct GridPosition : IEquatable<GridPosition>
{
    public static readonly GridPosition OneX = new(1, 0, 0);

    public static readonly GridPosition OneY = new(0, 1, 0);

    public static readonly GridPosition OneZ = new(0, 0, 1);

    /// <summary>
    ///     X position.
    /// </summary>
    [Key(0)]
    public int X { get; set; }

    /// <summary>
    ///     Y position.
    /// </summary>
    [Key(1)]
    public int Y { get; set; }

    /// <summary>
    ///     Z position.
    /// </summary>
    [Key(2)]
    public int Z { get; set; }

    /// <summary>
    ///     Creates a grid position.
    /// </summary>
    /// <param name="x">x coordinate.</param>
    /// <param name="y">y coordinate.</param>
    /// <param name="z">z coordinate.</param>
    public GridPosition(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    ///     Creates a grid position from a floating position.
    /// </summary>
    /// <param name="position">Floating position.</param>
    public GridPosition(Vector3 position)
    {
        X = (int)position.X;
        Y = (int)position.Y;
        Z = (int)position.Z;
    }

    public static explicit operator Vector3(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.X, gridPosition.Y, gridPosition.Z);
    }

    public static explicit operator GridPosition(Vector3 position)
    {
        return new GridPosition(position);
    }

    public static GridPosition operator +(GridPosition left, GridPosition right)
    {
        return new GridPosition
        {
            X = left.X + right.X,
            Y = left.Y + right.Y,
            Z = left.Z + right.Z
        };
    }

    public static GridPosition operator -(GridPosition left, GridPosition right)
    {
        return new GridPosition
        {
            X = left.X - right.X,
            Y = left.Y - right.Y,
            Z = left.Z - right.Z
        };
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("(")
            .Append(X).Append(", ")
            .Append(Y).Append(", ")
            .Append(Z).Append(")");
        return sb.ToString();
    }

    public override int GetHashCode()
    {
        var hash = 17;
        hash = hash * 31 + X.GetHashCode();
        hash = hash * 31 + Y.GetHashCode();
        hash = hash * 31 + Z.GetHashCode();
        return hash;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (!(obj is GridPosition)) return false;
        return Equals((GridPosition)obj);
    }

    public bool Equals(GridPosition other)
    {
        return other.X == X && other.Y == Y && other.Z == Z;
    }

    public static bool operator ==(GridPosition a, GridPosition b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(GridPosition a, GridPosition b)
    {
        return !a.Equals(b);
    }
}