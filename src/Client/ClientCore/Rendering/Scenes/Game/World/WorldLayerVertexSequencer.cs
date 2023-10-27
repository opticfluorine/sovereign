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
using Sovereign.ClientCore.Rendering.Resources.Buffers;

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
    /// <param name="vertexBuffer">Vertex buffer to populate.</param>
    /// <param name="indexBuffer">Index buffer to populate.</param>
    /// <param name="bufferOffset">Offset into the vertex buffer.</param>
    /// <param name="indexBufferOffset">Offset into the index buffer.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    /// <param name="verticesAdded">Number of vertices added to the buffer.</param>
    /// <param name="indicesAdded">Number of indices added to the buffer.</param>
    public void AddLayer(WorldLayer layer, WorldVertex[] vertexBuffer,
        uint[] indexBuffer, int bufferOffset, int indexBufferOffset, ulong systemTime,
        out int verticesAdded, out int indicesAdded)
    {
        /* Sequence the tile sprites into the buffers. */
        sequencedSprites.Clear();
        tileSpriteSequencer.SequenceTileSprites(sequencedSprites, layer.TopFaceTileSprites, true);
        tileSpriteSequencer.SequenceTileSprites(sequencedSprites, layer.FrontFaceTileSprites, false);

        var totalVerticesAdded = 0;
        var totalIndicesAdded = 0;

        spriteSequencer.SequenceAnimatedSprites(sequencedSprites,
            vertexBuffer, indexBuffer,
            bufferOffset, indexBufferOffset, systemTime,
            out var lastVerticesAdded, out var lastIndicesAdded);
        totalVerticesAdded += lastVerticesAdded;
        totalIndicesAdded += lastIndicesAdded;

        /* Sequence the remaining animated sprites into the buffers. */
        spriteSequencer.SequenceAnimatedSprites(layer.AnimatedSprites,
            vertexBuffer, indexBuffer,
            bufferOffset + totalVerticesAdded, indexBufferOffset + totalIndicesAdded,
            systemTime, out lastVerticesAdded, out lastIndicesAdded);

        verticesAdded = totalVerticesAdded + lastVerticesAdded;
        indicesAdded = totalIndicesAdded + lastIndicesAdded;
    }
}