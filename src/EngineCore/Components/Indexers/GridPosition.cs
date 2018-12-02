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

using System.Numerics;

namespace Sovereign.EngineCore.Components.Indexers
{

    /// <summary>
    /// Represents an integer position, useful for aligning blocks on the unit grid.
    /// </summary>
    public struct GridPosition
    {

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

    }

}
