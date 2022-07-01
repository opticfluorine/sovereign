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

namespace Sovereign.ClientCore.Rendering.Sprites.Atlas
{

    /// <summary>
    /// Encodes an entry in the texture atlas.
    /// </summary>
    public class AtlasMapElement
    {
        /// <summary>
        /// Width as a multiple of the tile width.
        /// </summary>
        public float WidthInTiles;

        /// <summary>
        /// Height as a multiple of the tile width.
        /// </summary>
        public float HeightInTiles;

        /// <summary>
        /// Normalized texel coordinate of the left edge.
        /// </summary>
        public float NormalizedLeftX;

        /// <summary>
        /// Normalized texel coordinate of the right edge.
        /// </summary>
        public float NormalizedRightX;

        /// <summary>
        /// Normalized texel coordinate of the top edge.
        /// </summary>
        public float NormalizedTopY;

        /// <summary>
        /// Normalized texel coordinate of the bottom edge.
        /// </summary>
        public float NormalizedBottomY;
    }

}
