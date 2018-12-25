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
using Sovereign.ClientCore.Rendering.Resources.Buffers;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Sequences the vertices for a single world layer.
    /// </summary>
    public sealed class WorldLayerVertexSequencer
    {

        /// <summary>
        /// Adds a single layer to the vertex buffer.
        /// </summary>
        /// <param name="layer">Layer to be added.</param>
        /// <param name="vertexBuffer">Vertex buffer to populate.</param>
        /// <param name="indexBuffer">Index buffer to populate.</param>
        /// <param name="bufferOffset">Offset into the vertex buffer.</param>
        /// <param name="indexBufferOffset">Offset into the index buffer.</param>
        /// <param name="verticesAdded">Number of vertices added to the buffer.</param>
        /// <param name="indicesAdded">Number of indices added to the buffer.</param>
        public void AddLayer(WorldLayer layer, Pos3Tex2Vertex[] vertexBuffer, 
            uint[] indexBuffer, int bufferOffset, int indexBufferOffset,
            out int verticesAdded, out int indicesAdded)
        {
            throw new NotImplementedException();
        }

    }
}
