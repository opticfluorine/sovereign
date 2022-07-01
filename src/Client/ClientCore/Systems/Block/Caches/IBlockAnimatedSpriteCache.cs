/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

namespace Sovereign.ClientCore.Systems.Block.Caches
{

    /// <summary>
    /// Interface to the block system's animated sprite cache.
    /// </summary>
    public interface IBlockAnimatedSpriteCache
    {

        /// <summary>
        /// Gets the front face animated sprite IDs for the given block.
        /// </summary>
        /// <param name="blockId">Block entity ID.</param>
        /// <returns>Front face animated sprite IDs.</returns>
        IList<int> GetFrontFaceAnimatedSpriteIds(ulong blockId);

        /// <summary>
        /// Gets the top face animated sprite IDs for the given block.
        /// </summary>
        /// <param name="blockId">Block entity ID.</param>
        /// <returns>Top face animated sprite IDs.</returns>
        IList<int> GetTopFaceAnimatedSpriteIds(ulong blockId);

    }

}

