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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites
{

    /// <summary>
    /// Describes a context for converting tile sprites to sprites.
    /// </summary>
    public sealed class TileContext
    {

        /// <summary>
        /// Index of the north tile sprite to match (-1 for wildcard).
        /// </summary>
        public int NorthTileSpriteId { get; set; }

        /// <summary>
        /// Index of the east tile sprite to match (-1 for wildcard).
        /// </summary>
        public int EastTileSpriteId { get; set; }

        /// <summary>
        /// Index of the south tile sprite to match (-1 for wildcard).
        /// </summary>
        public int SouthTileSpriteId { get; set; }


        /// <summary>
        /// Index of the west tile sprite to match (-1 for wildcard).
        /// </summary>
        public int WestTileSpriteId { get; set; }

        /// <summary>
        /// List of sprite IDs to be drawn in order if the context matches.
        /// </summary>
        public List<int> SpriteIds { get; set; }

    }

}
