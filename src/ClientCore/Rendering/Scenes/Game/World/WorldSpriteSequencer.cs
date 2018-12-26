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

using Sovereign.ClientCore.Rendering.Resources.Buffers;
using System.Collections.Generic;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Responsible for sequencing animated sprites into the buffers.
    /// </summary>
    public sealed class WorldSpriteSequencer
    {

        /// <summary>
        /// Sequences animated sprites into the buffers.
        /// </summary>
        /// <param name="animatedSprites">Animated sprites to sequence.</param>
        /// <param name="vertexBuffer">Vertex buffer.</param>
        /// <param name="indexBuffer">Index buffer.</param>
        /// <param name="bufferOffset">Offset into the vertex buffer.</param>
        /// <param name="indexBufferOffset">Offset into the index buffer.</param>
        /// <param name="verticesAdded">Number of vertices sequenced by this call.</param>
        /// <param name="indicesAdded">Number of indices sequenced by this call.</param>
        public void SequenceAnimatedSprites(IList<Pos3Id> animatedSprites,
            Pos3Tex2Vertex[] vertexBuffer, uint[] indexBuffer,
            int bufferOffset, int indexBufferOffset,
            out int verticesAdded, out int indicesAdded)
        {
            verticesAdded = indicesAdded = 0;
        }

    }

}
