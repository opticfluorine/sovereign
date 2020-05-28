﻿/*
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

using System.Numerics;
using System.Text;

namespace Sovereign.EngineCore.Components.Indexers
{

    /// <summary>
    /// Represents an integer position, useful for aligning blocks on the unit grid.
    /// </summary>
    public struct GridPosition
    {

        public static readonly GridPosition OneX = new GridPosition(1, 0, 0);

        public static readonly GridPosition OneY = new GridPosition(0, 1, 0);

        public static readonly GridPosition OneZ = new GridPosition(0, 0, 1);

        /// <summary>
        /// X position.
        /// </summary>
        public int X;

        /// <summary>
        /// Y position.
        /// </summary>
        public int Y;

        /// <summary>
        /// Z position.
        /// </summary>
        public int Z;

        /// <summary>
        /// Creates a grid position.
        /// </summary>
        /// <param name="x">x coordinate.</param>
        /// <param name="y">y coordinate.</param>
        /// <param name="z">z coordinate.</param>
        public GridPosition(int x, int y, int z) { X = x; Y = y; Z = z; }

        /// <summary>
        /// Creates a grid position from a floating position.
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
            return new GridPosition()
            {
                X = left.X + right.X,
                Y = left.Y + right.Y,
                Z = left.Z + right.Z
            };
        }

        public static GridPosition operator -(GridPosition left, GridPosition right)
        {
            return new GridPosition()
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
            int hash = 17;
            hash = hash * 31 + X.GetHashCode();
            hash = hash * 31 + Y.GetHashCode();
            hash = hash * 31 + Z.GetHashCode();
            return hash;
        }

    }

}
