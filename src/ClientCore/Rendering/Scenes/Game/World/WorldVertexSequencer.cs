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
using Sovereign.EngineCore.Components.Indexers;
using System.Collections.Generic;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Sequences the vertex buffer for world rendering.
    /// </summary>
    public sealed class WorldVertexSequencer
    {

        /// <summary>
        /// Default size of the drawable buffer.
        /// </summary>
        public const int DefaultDrawableListSize = 4096;

        private readonly WorldLayerGrouper grouper;
        private readonly WorldLayerVertexSequencer layerVertexSequencer;
        private readonly WorldEntityRetriever entityRetriever;
        private readonly IList<PositionedEntity> drawables
            = new List<PositionedEntity>(DefaultDrawableListSize);

        public WorldVertexSequencer(WorldLayerGrouper grouper,
            WorldLayerVertexSequencer layerVertexSequencer,
            WorldEntityRetriever entityRetriever)
        {
            this.grouper = grouper;
            this.layerVertexSequencer = layerVertexSequencer;
            this.entityRetriever = entityRetriever;
        }

        /// <summary>
        /// Produces the vertex buffer for world rendering.
        /// </summary>
        /// <param name="vertexBuffer">Vertex buffer.</param>
        /// <param name="indexBuffer">Index buffer.</param>
        /// <param name="drawLengths">Draw lengths for each layer.</param>
        /// <param name="drawCount">Number of layers to draw one at a time.</param>
        /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
        public void SequenceVertices(WorldVertex[] vertexBuffer,
            uint[] indexBuffer, int[] drawLengths,
            out int drawCount, float timeSinceTick)
        {
            RetrieveEntities(timeSinceTick);
            GroupLayers(out drawCount);
            PrepareLayers(vertexBuffer, indexBuffer, drawLengths);
        }

        /// <summary>
        /// Retrieves the drawable entities in range.
        /// </summary>
        /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
        private void RetrieveEntities(float timeSinceTick)
        {
            drawables.Clear();
            entityRetriever.RetrieveEntities(drawables, timeSinceTick);
        }

        /// <summary>
        /// Groups the layers by their z positions.
        /// </summary>
        /// <param name="drawCount">Number of layers to draw.</param>
        private void GroupLayers(out int drawCount)
        {
            grouper.GroupDrawables(drawables);
            drawCount = grouper.Layers.Count;
        }

        /// <summary>
        /// Prepares the layers and populates the vertex buffer.
        /// </summary>
        /// <param name="vertexBuffer">Vertex buffer.</param>
        /// <param name="indexBuffer">Index buffer.</param>
        /// <param name="drawLengths">Draw lengths for each layer.</param>
        private void PrepareLayers(WorldVertex[] vertexBuffer, uint[] indexBuffer,
            int[] drawLengths)
        {
            var bufferOffset = 0;
            var indexBufferOffset = 0;
            var layerIndex = 0;
            foreach (var layer in grouper.Layers.Values)
            {
                AddLayerToVertexBuffer(layer, vertexBuffer,
                    indexBuffer, bufferOffset, indexBufferOffset,
                    out var verticesAdded,
                    out var indicesAdded);
                drawLengths[layerIndex] = indicesAdded;

                bufferOffset += verticesAdded;
                indexBufferOffset += indicesAdded;
                layerIndex++;
            }
        }

        /// <summary>
        /// Adds a single layer to the vertex buffer.
        /// </summary>
        /// <param name="layer">Layer to be added.</param>
        /// <param name="vertexBuffer">Vertex buffer.</param>
        /// <param name="indexBuffer">Index buffer.</param>
        /// <param name="bufferOffset">Offset into the vertex buffer.</param>
        /// <param name="indexBufferOffset">Offset into the index buffer.</param>
        /// <param name="verticesAdded">Number of vertices added to the buffer.</param>
        /// <param name="indicesAdded">Number of indices added to the buffer.</param>
        private void AddLayerToVertexBuffer(WorldLayer layer, WorldVertex[] vertexBuffer,
            uint[] indexBuffer, int bufferOffset, int indexBufferOffset,
            out int verticesAdded, out int indicesAdded)
        {
            layerVertexSequencer.AddLayer(layer, vertexBuffer, indexBuffer,
                bufferOffset, indexBufferOffset,
                out verticesAdded, out indicesAdded);
        }

    }

}
