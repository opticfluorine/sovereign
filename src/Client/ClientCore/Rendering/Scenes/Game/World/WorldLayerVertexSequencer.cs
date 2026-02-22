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
using Sovereign.ClientCore.Rendering.Sprites.Atlas;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Sequences the vertices for a single world layer.
/// </summary>
public sealed class WorldLayerVertexSequencer
{
    private const int DefaultFreeSpriteCount = 2048;
    private readonly AtlasMap atlasMap;

    private readonly List<FreeSprite> freeSprites = new(DefaultFreeSpriteCount);
    private readonly WorldSpriteSequencer spriteSequencer;
    private readonly List<PosVelId> toDraw = new();

    public WorldLayerVertexSequencer(WorldSpriteSequencer spriteSequencer, AtlasMap atlasMap)
    {
        this.spriteSequencer = spriteSequencer;
        this.atlasMap = atlasMap;
    }

    /// <summary>
    ///     Resets the sequencer for a new frame.
    /// </summary>
    public void NewFrame()
    {
        freeSprites.Clear();
    }

    /// <summary>
    ///     Adds a single layer to the vertex buffer.
    /// </summary>
    /// <param name="layer">Layer to be added.</param>
    /// <param name="renderPlan">Rendering plan to populate.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    public void AddLayer(WorldLayer layer, RenderPlan renderPlan, ulong systemTime)
    {
        // Blocks. These only need to be handled at the current layer.
        spriteSequencer.SequenceSprites(layer.TopFaceTileSprites, renderPlan, SpritePlane.Xy, false,
            out var topBaseIndex, out var topIndexCount);
        spriteSequencer.SequenceSprites(layer.FrontFaceTileSprites, renderPlan, SpritePlane.Xz, false,
            out var frontBaseIndex, out var frontIndexCount);

        // Free sprites. These must be handled at the current layer plus any future layers with which they overlap.
        spriteSequencer.SequenceSprites(layer.FreeSprites, renderPlan, SpritePlane.Dynamic, true,
            out var spriteBaseIndex, out var spriteIndexCount);

        renderPlan.PushDebugGroup($"Layer {layer.ZFloor}");

        renderPlan.PushDebugGroup("Block Top Faces");
        renderPlan.DrawSprites(topBaseIndex, topIndexCount, true);
        renderPlan.PopDebugGroup();

        renderPlan.PushDebugGroup("Block Front Faces");
        renderPlan.DrawSprites(frontBaseIndex, frontIndexCount, true);
        renderPlan.PopDebugGroup();

        renderPlan.PushDebugGroup("Free Sprite Overdraw");
        OverdrawFreeSpritesForZFloor(layer.ZFloor, renderPlan);
        UpdateFreeSpritesForLayer(layer);
        renderPlan.PopDebugGroup();

        renderPlan.PushDebugGroup("Free Sprites");
        renderPlan.DrawSprites(spriteBaseIndex, spriteIndexCount, false);
        renderPlan.PopDebugGroup();

        renderPlan.PopDebugGroup();
    }

    /// <summary>
    ///     Overdraws free sprites from previous layers which overlap the current layer in
    ///     projected space.
    /// </summary>
    /// <param name="zFloor">Z floor of current layer.</param>
    /// <param name="renderPlan">Render plan.</param>
    private void OverdrawFreeSpritesForZFloor(int zFloor, RenderPlan renderPlan)
    {
        toDraw.Clear();

        foreach (var sprite in freeSprites)
        {
            if (sprite.MaxLayer < zFloor) continue;
            toDraw.Add(sprite.Data);

            spriteSequencer.SequenceSprites(toDraw, renderPlan, SpritePlane.Xz, true, out var baseIndex,
                out var indexCount);
            renderPlan.DrawSprites(baseIndex, indexCount, false);
        }
    }

    /// <summary>
    ///     Updates the tracked free sprites for the given layer.
    /// </summary>
    /// <param name="layer">Current layer.</param>
    private void UpdateFreeSpritesForLayer(WorldLayer layer)
    {
        foreach (var sprite in layer.FreeSprites)
        {
            // Free sprites need to be overdrawn in each layer they overlap.
            var height = atlasMap.MapElements[sprite.Id].HeightInTiles;
            var maxLayer = (int)Math.Floor(sprite.Position.Z + height);
            if (maxLayer > layer.ZFloor)
                freeSprites.Add(new FreeSprite
                {
                    Data = sprite,
                    MaxLayer = maxLayer
                });
        }
    }

    /// <summary>
    ///     Tracks a free sprite to be revisited in later layers.
    /// </summary>
    private struct FreeSprite
    {
        /// <summary>
        ///     Sprite data.
        /// </summary>
        public PosVelId Data;

        /// <summary>
        ///     Highest layer at which this sprite should be rendered.
        /// </summary>
        public int MaxLayer;
    }
}