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

using System;
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
    /// <param name="renderPlan">Rendering plan to populate.</param>
    /// <param name="systemTime">System time of this frame.</param>
    public void SequenceAnimatedSprites(List<PosVelId> animatedSprites, RenderPlan renderPlan, ulong systemTime,
        out uint baseIndex, out uint indexCount)
    {
        baseIndex = 0;
        indexCount = 0;

        if (!renderPlan.TryAddVertices(animatedSprites.Count * VerticesPerSprite, out var vertices, out var baseVertex))
        {
            Logger.Error("Render plan vertex buffer too small for update.");
            return;
        }

        if (!renderPlan.TryAddIndices(animatedSprites.Count * IndicesPerSprite, out var indices, out baseIndex))
        {
            Logger.Error("Render plan index buffer too small for update.");
            return;
        }

        /* Update buffers directly. */
        var spriteCount = 0;
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

            var spriteData = animatedSprite.GetPhaseData(animationPhase);
            var sprite = spriteData.GetSpriteForTime(systemTime, positionedAnimatedSprite.Orientation);

            AddVerticesForSprite(sprite, pos, vel,
                vertices.Slice(spriteCount * VerticesPerSprite, VerticesPerSprite));
            AddIndicesForSprite(indices.Slice(spriteCount * IndicesPerSprite, IndicesPerSprite),
                (uint)(baseVertex + spriteCount * VerticesPerSprite));

            spriteCount++;
        }

        indexCount = (uint)spriteCount * IndicesPerSprite;
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
    /// <param name="vertices">Span containing vertices for the single sprite.</param>
    private void AddVerticesForSprite(Sprite sprite, Vector3 position,
        Vector3 velocity, Span<WorldVertex> vertices)
    {
        /* Retrieve sprite information. */
        var spriteInfo = atlasMap.MapElements[sprite.Id];

        /* Top left. */
        vertices[0] = new WorldVertex
        {
            PosX = position.X,
            PosY = position.Y,
            PosZ = position.Z,
            VelX = velocity.X,
            VelY = velocity.Y,
            VelZ = velocity.Z,
            TexX = spriteInfo.NormalizedLeftX,
            TexY = spriteInfo.NormalizedTopY
        };

        /* Top right. */
        vertices[1] = new WorldVertex
        {
            PosX = position.X + spriteInfo.WidthInTiles,
            PosY = position.Y,
            PosZ = position.Z,
            VelX = velocity.X,
            VelY = velocity.Y,
            VelZ = velocity.Z,
            TexX = spriteInfo.NormalizedRightX,
            TexY = spriteInfo.NormalizedTopY
        };

        /* Bottom right. */
        vertices[2] = new WorldVertex
        {
            PosX = position.X + spriteInfo.WidthInTiles,
            PosY = position.Y - spriteInfo.HeightInTiles,
            PosZ = position.Z,
            VelX = velocity.X,
            VelY = velocity.Y,
            VelZ = velocity.Z,
            TexX = spriteInfo.NormalizedRightX,
            TexY = spriteInfo.NormalizedBottomY
        };

        /* Bottom left. */
        vertices[3] = new WorldVertex
        {
            PosX = position.X,
            PosY = position.Y - spriteInfo.HeightInTiles,
            PosZ = position.Z,
            VelX = velocity.X,
            VelY = velocity.Y,
            VelZ = velocity.Z,
            TexX = spriteInfo.NormalizedLeftX,
            TexY = spriteInfo.NormalizedBottomY
        };
    }

    /// <summary>
    ///     Adds the six indices for the two triangles that compose a sprite.
    /// </summary>
    /// <param name="indices">Span containing indices for the single sprite.</param>
    /// <param name="vertexPos">Position of the first vertex.</param>
    private void AddIndicesForSprite(Span<uint> indices, uint vertexPos)
    {
        /* Upper left triangle. */
        indices[0] = vertexPos;
        indices[1] = vertexPos + 1;
        indices[2] = vertexPos + 3;

        /* Lower right triangle. */
        indices[3] = vertexPos + 1;
        indices[4] = vertexPos + 2;
        indices[5] = vertexPos + 3;
    }
}