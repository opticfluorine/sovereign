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

using System.Collections.Generic;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Organizes a single layer of the world for rendering.
    /// </summary>
    public sealed class WorldLayer
    {

        /// <summary>
        /// Initial number of tile sprite records to allocate.
        /// </summary>
        public const int InitialTileSpriteCount = 2048;

        /// <summary>
        /// Initial number of animated sprite records to allocate.
        /// </summary>
        public const int InitialAnimatedSpriteCount = 512;

        /// <summary>
        /// Minimum z value contained by this layer.
        /// </summary>
        public int ZFloor { get; set; }

        /// <summary>
        /// Tile sprites forming the floor of this layer.
        /// </summary>
        public IList<Pos3Id> TopFaceTileSprites { get; private set; }
            = new List<Pos3Id>(InitialTileSpriteCount);

        /// <summary>
        /// Front face tile sprites of blocks in the next higher layer.
        /// </summary>
        public IList<Pos3Id> FrontFaceTileSprites { get; private set; }
            = new List<Pos3Id>(InitialTileSpriteCount);

        /// <summary>
        /// Animated sprites to additionally be drawn in this layer.
        /// </summary>
        public IList<Pos3Id> AnimatedSprites { get; private set; }
            = new List<Pos3Id>(InitialAnimatedSpriteCount);

        /// <summary>
        /// Resets the layer.
        /// </summary>
        public void Reset()
        {
            TopFaceTileSprites.Clear();
            FrontFaceTileSprites.Clear();
            AnimatedSprites.Clear();
        }

    }

}
