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
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Resources.Buffers;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Sequences the vertex buffer for world rendering.
/// </summary>
public sealed class WorldVertexSequencer
{
    /// <summary>
    ///     Number of vertices per block in the solid geometry.
    /// </summary>
    private const int SolidVerticesPerBlock = 8;

    /// <summary>
    ///     Number of indices per block in the solid geometry.
    /// </summary>
    private const int SolidIndicesPerBlock = 30;

    private readonly WorldEntityRetriever entityRetriever;

    private readonly WorldLayerGrouper grouper;
    private readonly WorldLayerVertexSequencer layerVertexSequencer;
    private readonly ILogger<WorldVertexSequencer> logger;

    public WorldVertexSequencer(WorldLayerGrouper grouper,
        WorldLayerVertexSequencer layerVertexSequencer,
        WorldEntityRetriever entityRetriever,
        ILogger<WorldVertexSequencer> logger)
    {
        this.grouper = grouper;
        this.layerVertexSequencer = layerVertexSequencer;
        this.entityRetriever = entityRetriever;
        this.logger = logger;
    }

    /// <summary>
    ///     Produces the vertex buffer for world rendering.
    /// </summary>
    /// <param name="renderPlan">Render plan to populate.</param>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    public void SequenceVertices(RenderPlan renderPlan, float timeSinceTick, ulong systemTime)
    {
        layerVertexSequencer.NewFrame();

        RetrieveEntities(timeSinceTick, systemTime);
        PrepareLayers(renderPlan, systemTime);
    }

    /// <summary>
    ///     Retrieves the drawable entities in range.
    /// </summary>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    private void RetrieveEntities(float timeSinceTick, ulong systemTime)
    {
        entityRetriever.RetrieveEntities(timeSinceTick, systemTime);
    }

    /// <summary>
    ///     Prepares the layers and populates the vertex buffer.
    /// </summary>
    /// <param name="renderPlan">Rendering plan to populate.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    private void PrepareLayers(RenderPlan renderPlan, ulong systemTime)
    {
        foreach (var layer in grouper.Layers.Values) AddLayerToVertexBuffer(layer, renderPlan, systemTime);
        AddBlockGeometry(renderPlan);
    }

    /// <summary>
    ///     Adds a single layer to the vertex buffer.
    /// </summary>
    /// <param name="layer">Layer to be added.</param>
    /// <param name="renderPlan">Rendering plan to populate.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    private void AddLayerToVertexBuffer(WorldLayer layer, RenderPlan renderPlan, ulong systemTime)
    {
        layerVertexSequencer.AddLayer(layer, renderPlan, systemTime);
    }

    /// <summary>
    ///     Adds block geometry for shadows and lighting to the render plan.
    /// </summary>
    /// <param name="renderPlan">Render plan.</param>
    private void AddBlockGeometry(RenderPlan renderPlan)
    {
        // Prepare resource layout in render plan.
        GenerateLightingPlan(renderPlan, out var totalIndexCount);
        GenerateNameLabelPlan(renderPlan);

        // Allocate resources within the render plan.
        var blocks = grouper.SolidBlocks;
        if (!renderPlan.TryAddVertices(SolidVerticesPerBlock * blocks.Count, out var vertices, out var baseIndex))
        {
            logger.LogError("Not enough room in vertex buffer for solid geometry.");
            return;
        }

        if (!renderPlan.TryAddSolidIndices((int)totalIndexCount, out var indices,
                out _))
            logger.LogError("Not enough room in solid index buffer for solid geometry.");

        for (var i = 0; i < blocks.Count; ++i)
        {
            var basePos = blocks[i];
            var blockVertices = vertices.Slice(i * SolidVerticesPerBlock, SolidVerticesPerBlock);
            var blockIndices = indices.Slice(i * SolidIndicesPerBlock, SolidIndicesPerBlock);

            AddVerticesForBlock(basePos, blockVertices);
            AddIndicesForBlock(baseIndex + (uint)i * SolidVerticesPerBlock, blockIndices);
        }

        // Point lighting.
        var offset = 0;
        foreach (var light in entityRetriever.Lights)
        {
            if (grouper.SolidBlocksPerLight.Count <= light.Index) continue;
            var blockBaseIndices = grouper.SolidBlocksPerLight[light.Index];
            foreach (var blockBaseIndex in blockBaseIndices)
            {
                var blockIndices = indices.Slice((blocks.Count + offset) * SolidIndicesPerBlock, SolidIndicesPerBlock);
                AddIndicesForBlock(baseIndex + blockBaseIndex * SolidVerticesPerBlock, blockIndices);
                offset++;
            }
        }
    }

    /// <summary>
    ///     Generates the name label rendering plan for the current frame.
    /// </summary>
    /// <param name="renderPlan">Render plan.</param>
    private void GenerateNameLabelPlan(RenderPlan renderPlan)
    {
        foreach (var nameLabel in entityRetriever.NameLabels)
            renderPlan.AddNameLabel(nameLabel);
    }

    /// <summary>
    ///     Generates a lighting resource plan for the render plan.
    /// </summary>
    /// <param name="renderPlan">Render plan.</param>
    private void GenerateLightingPlan(RenderPlan renderPlan, out uint totalSolidIndexCount)
    {
        totalSolidIndexCount = (uint)grouper.SolidBlocks.Count * SolidIndicesPerBlock;
        renderPlan.SetWorldSolidIndexCount(totalSolidIndexCount);
        foreach (var light in entityRetriever.Lights)
        {
            if (grouper.SolidBlocksPerLight.Count <= light.Index ||
                grouper.SolidBlocksPerLight[light.Index].Count == 0) continue;

            var indexCount = grouper.SolidBlocksPerLight[light.Index].Count * SolidIndicesPerBlock;
            renderPlan.AddLight(indexCount, light);
            totalSolidIndexCount += (uint)indexCount;
        }
    }

    /// <summary>
    ///     Adds vertices for the given block.
    /// </summary>
    /// <param name="basePos">Base position of the block.</param>
    /// <param name="blockVertices">Vertices to populate for this block.</param>
    private void AddVerticesForBlock(Vector3 basePos, Span<WorldVertex> blockVertices)
    {
        // Vertex 0 = (0, 0, 0) [left, front, bottom]
        blockVertices[0] = new WorldVertex
        {
            PosX = basePos.X,
            PosY = basePos.Y,
            PosZ = basePos.Z,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };

        // Vertex 1 = (1, 0, 0) [right, front, bottom]
        blockVertices[1] = new WorldVertex
        {
            PosX = basePos.X + 1,
            PosY = basePos.Y,
            PosZ = basePos.Z,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };

        // Vertex 2 = (1, 1, 0) [right, back, bottom]
        blockVertices[2] = new WorldVertex
        {
            PosX = basePos.X + 1,
            PosY = basePos.Y + 1,
            PosZ = basePos.Z,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };

        // Vertex 3 = (0, 1, 0) [left, back, bottom]
        blockVertices[3] = new WorldVertex
        {
            PosX = basePos.X,
            PosY = basePos.Y + 1,
            PosZ = basePos.Z,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };

        // Vertex 4 = (0, 0, 1) [left, front, top]
        blockVertices[4] = new WorldVertex
        {
            PosX = basePos.X,
            PosY = basePos.Y,
            PosZ = basePos.Z + 1,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };

        // Vertex 5 = (1, 0, 1) [right, front, top]
        blockVertices[5] = new WorldVertex
        {
            PosX = basePos.X + 1,
            PosY = basePos.Y,
            PosZ = basePos.Z + 1,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };

        // Vertex 6 = (1, 1, 1) [right, back, top]
        blockVertices[6] = new WorldVertex
        {
            PosX = basePos.X + 1,
            PosY = basePos.Y + 1,
            PosZ = basePos.Z + 1,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };

        // Vertex 7 = (0, 1, 1) [left, back, top]
        blockVertices[7] = new WorldVertex
        {
            PosX = basePos.X,
            PosY = basePos.Y + 1,
            PosZ = basePos.Z + 1,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };
    }

    /// <summary>
    ///     Adds indices for the given solid block.
    /// </summary>
    /// <param name="baseIndex">Base index.</param>
    /// <param name="blockIndices">Block indices.</param>
    public void AddIndicesForBlock(uint baseIndex, Span<uint> blockIndices)
    {
        // Top face.
        blockIndices[0] = baseIndex + 4;
        blockIndices[1] = baseIndex + 5;
        blockIndices[2] = baseIndex + 7;
        blockIndices[3] = baseIndex + 7;
        blockIndices[4] = baseIndex + 5;
        blockIndices[5] = baseIndex + 6;

        // Front face.
        blockIndices[6] = baseIndex + 4;
        blockIndices[7] = baseIndex + 5;
        blockIndices[8] = baseIndex + 7;
        blockIndices[9] = baseIndex + 7;
        blockIndices[10] = baseIndex + 5;
        blockIndices[11] = baseIndex + 6;

        // Bottom face is omitted. Might need to eventually add to handle point light
        // sources that radiate upward?

        // Back face.
        blockIndices[12] = baseIndex + 2;
        blockIndices[13] = baseIndex + 3;
        blockIndices[14] = baseIndex + 6;
        blockIndices[15] = baseIndex + 6;
        blockIndices[16] = baseIndex + 3;
        blockIndices[17] = baseIndex + 7;

        // Left face.
        blockIndices[18] = baseIndex + 3;
        blockIndices[19] = baseIndex + 0;
        blockIndices[20] = baseIndex + 7;
        blockIndices[21] = baseIndex + 7;
        blockIndices[22] = baseIndex + 0;
        blockIndices[23] = baseIndex + 4;

        // Right face.
        blockIndices[24] = baseIndex + 1;
        blockIndices[25] = baseIndex + 2;
        blockIndices[26] = baseIndex + 5;
        blockIndices[27] = baseIndex + 5;
        blockIndices[28] = baseIndex + 2;
        blockIndices[29] = baseIndex + 6;
    }
}