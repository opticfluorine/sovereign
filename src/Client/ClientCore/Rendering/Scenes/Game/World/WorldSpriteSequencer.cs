/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Numerics;
using Castle.Core.Logging;
using Sovereign.ClientCore.Components;
using Sovereign.ClientCore.Rendering.Resources.Buffers;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Responsible for sequencing animated sprites into the buffers.
/// </summary>
public sealed class WorldSpriteSequencer
{
    private const int VerticesPerSprite = 4;
    private const int IndicesPerSprite = 6;

    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly AnimationPhaseComponentCollection animationPhases;
    private readonly AtlasMap atlasMap;

    public WorldSpriteSequencer(AnimatedSpriteManager animatedSpriteManager,
        AtlasMap atlasMap, AnimationPhaseComponentCollection animationPhases)
    {
        this.animatedSpriteManager = animatedSpriteManager;
        this.atlasMap = atlasMap;
        this.animationPhases = animationPhases;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Sequences animated sprites into the buffers.
    /// </summary>
    /// <param name="animatedSprites">Animated sprites to sequence.</param>
    /// <param name="vertexBuffer">Vertex buffer.</param>
    /// <param name="indexBuffer">Index buffer.</param>
    /// <param name="bufferOffset">Offset into the vertex buffer.</param>
    /// <param name="indexBufferOffset">Offset into the index buffer.</param>
    /// <param name="systemTime">System time of this frame.</param>
    /// <param name="verticesAdded">Number of vertices sequenced by this call.</param>
    /// <param name="indicesAdded">Number of indices sequenced by this call.</param>
    public unsafe void SequenceAnimatedSprites(IList<PosVelId> animatedSprites,
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

        /* Update buffers directly. */
        fixed (WorldVertex* vertexBase = vertexBuffer)
        {
            fixed (uint* indexBase = indexBuffer)
            {
                foreach (var positionedAnimatedSprite in animatedSprites)
                {
                    var pos = positionedAnimatedSprite.Position;
                    var vel = positionedAnimatedSprite.Velocity;
                    var animId = positionedAnimatedSprite.Id;
                    var entityId = positionedAnimatedSprite.EntityId;
                    var animatedSprite = animatedSpriteManager.AnimatedSprites[animId];

                    var animationPhase = animationPhases.HasComponentForEntity(entityId)
                        ? animationPhases[entityId]
                        : AnimationPhase.Default;

                    if (!animatedSprite.Phases.TryGetValue(animationPhase, out var spriteData))
                        spriteData = animatedSprite.Phases[AnimationPhase.Default];

                    var sprite = spriteData.GetSpriteForTime(systemTime, positionedAnimatedSprite.Orientation);

                    AddVerticesForSprite(sprite, pos, vel, vertexBase, vertexPos);
                    AddIndicesForSprite(indexBase, indexPos, (uint)vertexPos);

                    vertexPos += VerticesPerSprite;
                    indexPos += IndicesPerSprite;
                }
            }
        }

        verticesAdded = vertexPos - bufferOffset;
        indicesAdded = indexPos - indexBufferOffset;
    }

    /// <summary>
    ///     Adds the four vertices for the given sprite.
    /// </summary>
    /// <remarks>
    ///     Vertices are generated clockwise from top-left.
    /// </remarks>
    /// <param name="sprite">Sprite to sequence.</param>
    /// <param name="position">Position of entity.</param>
    /// <param name="velocity">Velocity of entity.</param>
    /// <param name="vertexBase">Pointer to beginning of vertex buffer.</param>
    /// <param name="vertexPos">Position of the first vertex to add.</param>
    private unsafe void AddVerticesForSprite(Sprite sprite, Vector3 position,
        Vector3 velocity, WorldVertex* vertexBase, int vertexPos)
    {
        /* Retrieve sprite information. */
        var spriteInfo = atlasMap.MapElements[sprite.Id];

        /* Top left. */
        var vertex = vertexBase + vertexPos;
        vertex->PosX = position.X;
        vertex->PosY = position.Y;
        vertex->PosZ = position.Z;
        vertex->VelX = velocity.X;
        vertex->VelY = velocity.Y;
        vertex->VelZ = velocity.Z;
        vertex->TexX = spriteInfo.NormalizedLeftX;
        vertex->TexY = spriteInfo.NormalizedTopY;

        /* Top right. */
        vertex++;
        vertex->PosX = position.X + spriteInfo.WidthInTiles;
        vertex->PosY = position.Y;
        vertex->PosZ = position.Z;
        vertex->VelX = velocity.X;
        vertex->VelY = velocity.Y;
        vertex->VelZ = velocity.Z;
        vertex->TexX = spriteInfo.NormalizedRightX;
        vertex->TexY = spriteInfo.NormalizedTopY;

        /* Bottom right. */
        vertex++;
        vertex->PosX = position.X + spriteInfo.WidthInTiles;
        vertex->PosY = position.Y - spriteInfo.HeightInTiles;
        vertex->PosZ = position.Z;
        vertex->VelX = velocity.X;
        vertex->VelY = velocity.Y;
        vertex->VelZ = velocity.Z;
        vertex->TexX = spriteInfo.NormalizedRightX;
        vertex->TexY = spriteInfo.NormalizedBottomY;

        /* Bottom left. */
        vertex++;
        vertex->PosX = position.X;
        vertex->PosY = position.Y - spriteInfo.HeightInTiles;
        vertex->PosZ = position.Z;
        vertex->VelX = velocity.X;
        vertex->VelY = velocity.Y;
        vertex->VelZ = velocity.Z;
        vertex->TexX = spriteInfo.NormalizedLeftX;
        vertex->TexY = spriteInfo.NormalizedBottomY;
    }

    /// <summary>
    ///     Adds the six indices for the two triangles that compose a sprite.
    /// </summary>
    /// <param name="indexBase">Pointer to index buffer to populate.</param>
    /// <param name="indexPos">Position for the first index.</param>
    /// <param name="vertexPos">Position of the first vertex.</param>
    private unsafe void AddIndicesForSprite(uint* indexBase, int indexPos, uint vertexPos)
    {
        var index = indexBase + indexPos;

        /* Upper left triangle. */
        *index = vertexPos;
        *(index + 1) = vertexPos + 1;
        *(index + 2) = vertexPos + 3;

        /* Lower right triangle. */
        *(index + 3) = vertexPos + 1;
        *(index + 4) = vertexPos + 2;
        *(index + 5) = vertexPos + 3;
    }

    /// <summary>
    ///     Determines if the buffers can hold the given number of additional sprites.
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
            Logger.ErrorFormat("Layer would add {0} vertices, overflowing by {1}.",
                newVertices, vertexOverflow);

        var indexOverflow = indexBufferOffset + newIndices - indexBufLen;
        if (indexOverflow > 0)
            Logger.ErrorFormat("Layer would add {0} indices, overflowing by {1}.",
                newIndices, indexOverflow);

        return vertexOverflow <= 0 && indexOverflow <= 0;
    }
}