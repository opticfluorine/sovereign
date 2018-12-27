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

using Castle.Core.Logging;
using Sovereign.ClientCore.Rendering.Resources.Buffers;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using System.Collections.Generic;
using System.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Responsible for sequencing animated sprites into the buffers.
    /// </summary>
    public sealed class WorldSpriteSequencer
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        private const int VerticesPerSprite = 4;
        private const int IndicesPerSprite = 6;

        private readonly AnimatedSpriteManager animatedSpriteManager;
        private readonly AtlasMap atlasMap;

        public WorldSpriteSequencer(AnimatedSpriteManager animatedSpriteManager,
            AtlasMap atlasMap)
        {
            this.animatedSpriteManager = animatedSpriteManager;
            this.atlasMap = atlasMap;
        }

        /// <summary>
        /// Sequences animated sprites into the buffers.
        /// </summary>
        /// <param name="animatedSprites">Animated sprites to sequence.</param>
        /// <param name="vertexBuffer">Vertex buffer.</param>
        /// <param name="indexBuffer">Index buffer.</param>
        /// <param name="bufferOffset">Offset into the vertex buffer.</param>
        /// <param name="indexBufferOffset">Offset into the index buffer.</param>
        /// <param name="systemTime">System time of this frame.</param>
        /// <param name="verticesAdded">Number of vertices sequenced by this call.</param>
        /// <param name="indicesAdded">Number of indices sequenced by this call.</param>
        public void SequenceAnimatedSprites(IList<Pos3Id> animatedSprites,
            WorldVertex[] vertexBuffer, uint[] indexBuffer,
            int bufferOffset, int indexBufferOffset, ulong systemTime,
            out int verticesAdded, out int indicesAdded)
        {
            var vertexPos = bufferOffset;
            var indexPos = indexBufferOffset;

            if (!CanBuffersHoldSprites(animatedSprites.Count,
                vertexBuffer.Length, indexBuffer.Length,
                bufferOffset, indexBufferOffset))
            {
                verticesAdded = indicesAdded = 0;
                return;
            }

            foreach (var positionedAnimatedSprite in animatedSprites)
            {
                var pos = positionedAnimatedSprite.Position;
                var animId = positionedAnimatedSprite.Id;
                var animatedSprite = animatedSpriteManager.AnimatedSprites[animId];
                var sprite = animatedSprite.GetSpriteForTime(systemTime);

                AddVerticesForSprite(sprite, pos, vertexBuffer, vertexPos);
                AddIndicesForSprite(indexBuffer, indexPos, (uint)vertexPos);

                vertexPos += VerticesPerSprite;
                indexPos += IndicesPerSprite;
            }

            verticesAdded = vertexPos - bufferOffset;
            indicesAdded = indexPos - indexBufferOffset;
        }

        /// <summary>
        /// Adds the four vertices for the given sprite.
        /// </summary>
        /// <remarks>
        /// Vertices are generated clockwise from top-left.
        /// </remarks>
        /// <param name="sprite"></param>
        /// <param name="vertexBuffer"></param>
        /// <param name="vertexPos"></param>
        private void AddVerticesForSprite(Sprite sprite, Vector3 position,
            WorldVertex[] vertexBuffer, int vertexPos)
        {
            /* Retrieve sprite information. */
            var spriteInfo = atlasMap.MapElements[sprite.Id];

            /* Top left. */
            ref var vertex = ref vertexBuffer[vertexPos];
            vertex.PosX = position.X;
            vertex.PosY = position.Y;
            vertex.PosZ = position.Z;
            vertex.VelX = 0.0f; // TODO
            vertex.VelY = 0.0f; // TODO
            vertex.VelZ = 0.0f; // TODO
            vertex.TexX = spriteInfo.TopLeftX;
            vertex.TexY = spriteInfo.TopLeftY;

            /* Top right. */
            vertex = vertexBuffer[vertexPos + 1];
            vertex.PosX = position.X + spriteInfo.Width;
            vertex.PosY = position.Y;
            vertex.PosZ = position.Z;
            vertex.VelX = 0.0f; // TODO
            vertex.VelY = 0.0f; // TODO
            vertex.VelZ = 0.0f; // TODO
            vertex.TexX = spriteInfo.BottomRightX;
            vertex.TexY = spriteInfo.TopLeftY;

            /* Bottom right. */
            vertex = vertexBuffer[vertexPos + 2];
            vertex.PosX = position.X + spriteInfo.Width;
            vertex.PosY = position.Y - spriteInfo.Height;
            vertex.PosZ = position.Z;
            vertex.VelX = 0.0f; // TODO
            vertex.VelY = 0.0f; // TODO
            vertex.VelZ = 0.0f; // TODO
            vertex.TexX = spriteInfo.BottomRightX;
            vertex.TexY = spriteInfo.BottomRightY;

            /* Bottom left. */
            vertex = vertexBuffer[vertexPos + 3];
            vertex.PosX = position.X;
            vertex.PosY = position.Y - spriteInfo.Height;
            vertex.PosZ = position.Z;
            vertex.VelX = 0.0f; // TODO
            vertex.VelY = 0.0f; // TODO
            vertex.VelZ = 0.0f; // TODO
            vertex.TexX = spriteInfo.TopLeftX;
            vertex.TexY = spriteInfo.BottomRightY;
        }

        /// <summary>
        /// Adds the six indices for the two triangles that compose a sprite.
        /// </summary>
        /// <param name="indexBuffer">Index buffer to populate.</param>
        /// <param name="indexPos">Position for the first index.</param>
        /// <param name="vertexPos">Position of the first vertex.</param>
        private void AddIndicesForSprite(uint[] indexBuffer, int indexPos, uint vertexPos)
        {
            /* Upper left triangle. */
            indexBuffer[indexPos] = vertexPos;
            indexBuffer[indexPos + 1] = vertexPos + 1;
            indexBuffer[indexPos + 2] = vertexPos + 3;

            /* Lower right triangle. */
            indexBuffer[indexPos + 3] = vertexPos + 1;
            indexBuffer[indexPos + 4] = vertexPos + 2;
            indexBuffer[indexPos + 5] = vertexPos + 3;
        }

        /// <summary>
        /// Determines if the buffers can hold the given number of additional sprites.
        /// </summary>
        /// <param name="count">Number of sprites to sequence.</param>
        /// <param name="vertexBufLen">Length of the vertex buffer.</param>
        /// <param name="indexBufLen">Length of the index buffer.</param>
        /// <param name="bufferOffset">Offset into vertex buffer.</param>
        /// <param name="indexBufferOffset">Offset into index buffer.</param>
        /// <returns>true if the buffers are large enough, false otherwise.</returns>
        private bool CanBuffersHoldSprites(int count, int vertexBufLen, int indexBufLen,
            int bufferOffset, int indexBufferOffset)
        {
            var newVertices = VerticesPerSprite * count;
            var newIndices = IndicesPerSprite * count;

            var vertexOverflow = bufferOffset + newVertices - vertexBufLen;
            if (vertexOverflow > 0)
            {
                Logger.ErrorFormat("Layer would add {0} vertices, overflowing by {1}.",
                    newVertices, vertexOverflow);
            }

            var indexOverflow = indexBufferOffset + newIndices - indexBufLen;
            if (indexOverflow > 0)
            {
                Logger.ErrorFormat("Layer would add {0} indices, overflowing by {1}.",
                    newIndices, indexOverflow);
            }

            return vertexOverflow <= 0 && indexOverflow <= 0;
        }

    }

}
