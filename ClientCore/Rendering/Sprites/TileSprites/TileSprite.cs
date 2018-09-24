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
    /// Describes a tile sprite that changes its appearance based on
    /// its context (surrounding tile sprites).
    /// </summary>
    public sealed class TileSprite
    {

        /// <summary>
        /// Indicates a wildcard neighboring tile sprite.
        /// </summary>
        public const int Wildcard = -1;

        /// <summary>
        /// Tile sprite ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tile contexts.
        /// </summary>
        public IList<TileContext> TileContexts { get; set; }

        /// <summary>
        /// Cache of previously resolved tile contexts.
        /// </summary>
        private IDictionary<Tuple<int, int, int, int>, TileContext> lookupCache
            = new Dictionary<Tuple<int, int, int, int>, TileContext>();

        /// <summary>
        /// Finds the tile context that matches the given bordering tiles.
        /// </summary>
        /// 
        /// This method should only be called from the rendering thread.
        /// 
        /// <param name="idNorth">ID of the north tile sprite.</param>
        /// <param name="idEast">ID of the east tile sprite.</param>
        /// <param name="idSouth">ID of the south tile sprite.</param>
        /// <param name="idWest">ID of the west tile sprite.</param>
        /// <returns>Matching tile context.</returns>
        public TileContext GetMatchingContext(int idNorth, int idEast, int idSouth, int idWest)
        {
            /* Check if the context is already in the cache. */
            var ids = new Tuple<int, int, int, int>(idNorth, idEast, idSouth, idWest);
            if (lookupCache.ContainsKey(ids))
                return lookupCache[ids];

            /* Context not found in cache - resolve. */
            
            return null;
        }

        /// <summary>
        /// Resolves the tile context for the given neighboring IDs.
        /// </summary>
        /// <param name="ids">4-tuple of neighboring IDs (north, east, south, west).</param>
        /// <returns>Resolved tile context.</returns>
        private TileContext ResolveContext(Tuple<int, int, int, int> ids)
        {

        }

    }

}
