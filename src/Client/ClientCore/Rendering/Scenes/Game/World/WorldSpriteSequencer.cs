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
using Sovereign.ClientCore.Rendering.Resources.Buffers;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Specifies the plane on which a sprite lies.
/// </summary>
public enum SpritePlane
{
    /// <summary>
    ///     Sprite lies in the XY plane.
    /// </summary>
    XY,

    /// <summary>
    ///     Sprite lies in the XZ plane.
    /// </summary>
    XZ
}

/// <summary>
///     Responsible for sequencing animated sprites into the buffers.
/// </summary>
public sealed class WorldSpriteSequencer
{
    private const int VerticesPerSprite = 4;
    private const int IndicesPerSprite = 6;
    private readonly AtlasMap atlasMap;

    private readonly SpriteManager spriteManager;

    public WorldSpriteSequencer(AtlasMap atlasMap, SpriteManager spriteManager)
    {
        this.atlasMap = atlasMap;
        this.spriteManager = spriteManager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Sequences sprites into the buffers.
    /// </summary>
    /// <param name="sprites">Sprites to sequence.</param>
    /// <param name="renderPlan">Rendering plan to populate.</param>
    /// <param name="spritePlane">Sprite plane.</param>
    /// <param name="baseIndex">First index corresponding to the sequenced sprites.</param>
    /// <param name="indexCount">Number of indices added for these sprites.</param>
    public void SequenceSprites(List<PosVelId> sprites, RenderPlan renderPlan, SpritePlane spritePlane,
        out uint baseIndex, out uint indexCount)
    {
        baseIndex = 0;
        indexCount = 0;

        if (!renderPlan.TryAddVertices(sprites.Count * VerticesPerSprite, out var vertices, out var baseVertex))
        {
            Logger.Error("Render plan vertex buffer too small for update.");
            return;
        }

        if (!renderPlan.TryAddSpriteIndices(sprites.Count * IndicesPerSprite, out var indices, out baseIndex))
        {
            Logger.Error("Render plan index buffer too small for update.");
            return;
        }

        /* Update buffers directly. */
        var spriteCount = 0;
        foreach (var positionedAnimatedSprite in sprites)
        {
            var pos = positionedAnimatedSprite.Position;
            var vel = positionedAnimatedSprite.Velocity;
            var lightFactor = positionedAnimatedSprite.LightFactor;
            var spriteId = positionedAnimatedSprite.Id;
            var sprite = spriteManager.Sprites[spriteId];

            switch (spritePlane)
            {
                case SpritePlane.XY:
                    AddVerticesForSpriteXY(sprite, pos, vel, lightFactor,
                        vertices.Slice(spriteCount * VerticesPerSprite, VerticesPerSprite));
                    break;

                case SpritePlane.XZ:
                    AddVerticesForSpriteXZ(sprite, pos, vel, lightFactor,
                        vertices.Slice(spriteCount * VerticesPerSprite, VerticesPerSprite));
                    break;
            }

            AddIndicesForSprite(indices.Slice(spriteCount * IndicesPerSprite, IndicesPerSprite),
                (uint)(baseVertex + spriteCount * VerticesPerSprite));

            spriteCount++;
        }

        indexCount = (uint)spriteCount * IndicesPerSprite;
    }

    /// <summary>
    ///     Adds the four vertices for the given sprite in the XY plane.
    /// </summary>
    /// <remarks>
    ///     Vertices are generated clockwise from top-left.
    /// </remarks>
    /// <param name="sprite">Sprite to sequence.</param>
    /// <param name="position">Position of entity.</param>
    /// <param name="velocity">Velocity of entity.</param>
    /// <param name="lightFactor">Light factor for sprite.</param>
    /// <param name="vertices">Span containing vertices for the single sprite.</param>
    private void AddVerticesForSpriteXY(Sprite sprite, Vector3 position,
        Vector3 velocity, float lightFactor, Span<WorldVertex> vertices)
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
            TexY = spriteInfo.NormalizedTopY,
            LightFactor = lightFactor
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
            TexY = spriteInfo.NormalizedTopY,
            LightFactor = lightFactor
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
            TexY = spriteInfo.NormalizedBottomY,
            LightFactor = lightFactor
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
            TexY = spriteInfo.NormalizedBottomY,
            LightFactor = lightFactor
        };
    }

    /// <summary>
    ///     Adds the four vertices for the given sprite in the XZ plane.
    /// </summary>
    /// <remarks>
    ///     Vertices are generated clockwise from top-left.
    /// </remarks>
    /// <param name="sprite">Sprite to sequence.</param>
    /// <param name="position">Position of entity.</param>
    /// <param name="velocity">Velocity of entity.</param>
    /// <param name="lightFactor">Light factor of sprite.</param>
    /// <param name="vertices">Span containing vertices for the single sprite.</param>
    private void AddVerticesForSpriteXZ(Sprite sprite, Vector3 position,
        Vector3 velocity, float lightFactor, Span<WorldVertex> vertices)
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
            TexY = spriteInfo.NormalizedTopY,
            LightFactor = lightFactor
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
            TexY = spriteInfo.NormalizedTopY,
            LightFactor = lightFactor
        };

        /* Bottom right. */
        vertices[2] = new WorldVertex
        {
            PosX = position.X + spriteInfo.WidthInTiles,
            PosY = position.Y,
            PosZ = position.Z - spriteInfo.HeightInTiles,
            VelX = velocity.X,
            VelY = velocity.Y,
            VelZ = velocity.Z,
            TexX = spriteInfo.NormalizedRightX,
            TexY = spriteInfo.NormalizedBottomY,
            LightFactor = lightFactor
        };

        /* Bottom left. */
        vertices[3] = new WorldVertex
        {
            PosX = position.X,
            PosY = position.Y,
            PosZ = position.Z - spriteInfo.HeightInTiles,
            VelX = velocity.X,
            VelY = velocity.Y,
            VelZ = velocity.Z,
            TexX = spriteInfo.NormalizedLeftX,
            TexY = spriteInfo.NormalizedBottomY,
            LightFactor = lightFactor
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