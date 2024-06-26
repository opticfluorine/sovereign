﻿/*
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
using Sovereign.ClientCore.Rendering.Resources.Buffers;
using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Sequences the vertex buffer for world rendering.
/// </summary>
public sealed class WorldVertexSequencer
{
    /// <summary>
    ///     Default size of the drawable buffer.
    /// </summary>
    public const int DefaultDrawableListSize = 4096;

    private readonly List<PositionedEntity> drawables = new(DefaultDrawableListSize);

    private readonly WorldEntityRetriever entityRetriever;

    private readonly WorldLayerGrouper grouper;
    private readonly WorldLayerVertexSequencer layerVertexSequencer;

    public WorldVertexSequencer(WorldLayerGrouper grouper,
        WorldLayerVertexSequencer layerVertexSequencer,
        WorldEntityRetriever entityRetriever)
    {
        this.grouper = grouper;
        this.layerVertexSequencer = layerVertexSequencer;
        this.entityRetriever = entityRetriever;
    }

    /// <summary>
    ///     Produces the vertex buffer for world rendering.
    /// </summary>
    /// <param name="vertexBuffer">Vertex buffer.</param>
    /// <param name="indexBuffer">Index buffer.</param>
    /// <param name="drawLengths">Draw lengths for each layer.</param>
    /// <param name="vertexCount">Number of vertices that were added to the buffer.</param>
    /// <param name="indexCount">Number of indices that were added to the buffer.</param>
    /// <param name="drawCount">Number of layers to draw one at a time.</param>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    public void SequenceVertices(WorldVertex[] vertexBuffer,
        uint[] indexBuffer, int[] drawLengths,
        out int vertexCount, out int indexCount, out int drawCount, float timeSinceTick, ulong systemTime)
    {
        RetrieveEntities(timeSinceTick);
        GroupLayers(out drawCount);
        PrepareLayers(vertexBuffer, indexBuffer, drawLengths, systemTime, out vertexCount, out indexCount);
    }

    /// <summary>
    ///     Retrieves the drawable entities in range.
    /// </summary>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    private void RetrieveEntities(float timeSinceTick)
    {
        drawables.Clear();
        entityRetriever.RetrieveEntities(drawables, timeSinceTick);
    }

    /// <summary>
    ///     Groups the layers by their z positions.
    /// </summary>
    /// <param name="drawCount">Number of layers to draw.</param>
    private void GroupLayers(out int drawCount)
    {
        grouper.GroupDrawables(drawables);
        drawCount = 2 * grouper.Layers.Count;
    }

    /// <summary>
    ///     Prepares the layers and populates the vertex buffer.
    /// </summary>
    /// <param name="vertexBuffer">Vertex buffer.</param>
    /// <param name="indexBuffer">Index buffer.</param>
    /// <param name="drawLengths">Draw lengths for each layer.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    /// <param name="vertexCount">Number of vertices that were added to the buffer.</param>
    /// <param name="indexCount">Number of indices that were added to the buffer.</param>
    private void PrepareLayers(WorldVertex[] vertexBuffer, uint[] indexBuffer,
        int[] drawLengths, ulong systemTime, out int vertexCount, out int indexCount)
    {
        vertexCount = 0;
        indexCount = 0;
        var layerIndex = 0;
        foreach (var layer in grouper.Layers.Values)
        {
            AddLayerToVertexBuffer(layer, vertexBuffer,
                indexBuffer, vertexCount, indexCount, systemTime,
                out var blockVerticesAdded,
                out var blockIndicesAdded,
                out var spriteVerticesAdded,
                out var spriteIndicesAdded);
            drawLengths[2 * layerIndex] = blockIndicesAdded;
            drawLengths[2 * layerIndex + 1] = spriteIndicesAdded;

            vertexCount += blockVerticesAdded + spriteVerticesAdded;
            indexCount += blockIndicesAdded + spriteIndicesAdded;
            layerIndex++;
        }
    }

    /// <summary>
    ///     Adds a single layer to the vertex buffer.
    /// </summary>
    /// <param name="layer">Layer to be added.</param>
    /// <param name="vertexBuffer">Vertex buffer.</param>
    /// <param name="indexBuffer">Index buffer.</param>
    /// <param name="bufferOffset">Offset into the vertex buffer.</param>
    /// <param name="indexBufferOffset">Offset into the index buffer.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    /// <param name="blockVerticesAdded">Number of block vertices added to the buffer.</param>
    /// <param name="blockIndicesAdded">Number of block indices added to the buffer.</param>
    /// <param name="spriteVerticesAdded">Number of animated sprite vertices added to the buffer.</param>
    /// <param name="spriteIndicesAdded">Number of animated sprite indices added to the buffer.</param>
    private void AddLayerToVertexBuffer(WorldLayer layer, WorldVertex[] vertexBuffer,
        uint[] indexBuffer, int bufferOffset, int indexBufferOffset, ulong systemTime,
        out int blockVerticesAdded, out int blockIndicesAdded, out int spriteVerticesAdded, out int spriteIndicesAdded)
    {
        layerVertexSequencer.AddLayer(layer, vertexBuffer, indexBuffer,
            bufferOffset, indexBufferOffset, systemTime,
            out blockVerticesAdded, out blockIndicesAdded, out spriteVerticesAdded, out spriteIndicesAdded);
    }
}