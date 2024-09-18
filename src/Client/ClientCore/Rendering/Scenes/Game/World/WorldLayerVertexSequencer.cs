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
using Castle.Core.Logging;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Sequences the vertices for a single world layer.
/// </summary>
public sealed class WorldLayerVertexSequencer
{
    /// <summary>
    ///     Animated sprites to be sequenced.
    /// </summary>
    private readonly List<PosVelId> sequencedSprites = new();

    private readonly WorldSpriteSequencer spriteSequencer;
    private readonly WorldTileSpriteSequencer tileSpriteSequencer;

    public WorldLayerVertexSequencer(WorldSpriteSequencer spriteSequencer,
        WorldTileSpriteSequencer tileSpriteSequencer)
    {
        this.spriteSequencer = spriteSequencer;
        this.tileSpriteSequencer = tileSpriteSequencer;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Adds a single layer to the vertex buffer.
    /// </summary>
    /// <param name="layer">Layer to be added.</param>
    /// <param name="renderPlan">Rendering plan to populate.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    public void AddLayer(WorldLayer layer, RenderPlan renderPlan, ulong systemTime)
    {
        /* Sequence the tile sprites into the buffers. */
        sequencedSprites.Clear();
        tileSpriteSequencer.SequenceTileSprites(sequencedSprites, layer.TopFaceTileSprites, true);
        tileSpriteSequencer.SequenceTileSprites(sequencedSprites, layer.FrontFaceTileSprites, false);

        spriteSequencer.SequenceAnimatedSprites(sequencedSprites, renderPlan, systemTime, out var tileBaseIndex,
            out var tileIndexCount);

        /* Sequence the remaining animated sprites into the buffers. */
        spriteSequencer.SequenceAnimatedSprites(layer.AnimatedSprites, renderPlan, systemTime, out var spriteBaseIndex,
            out var spriteIndexCount);

        // Schedule sprite draw commands into the render plan.
        if (!renderPlan.TryDrawSprites(tileBaseIndex, tileIndexCount) ||
            !renderPlan.TryDrawSprites(spriteBaseIndex, spriteIndexCount))
        {
            Logger.Error("Not enough room in command list to add draw commands.");
        }
    }
}