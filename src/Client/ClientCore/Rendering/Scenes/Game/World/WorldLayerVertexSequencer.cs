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

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Sequences the vertices for a single world layer.
/// </summary>
public sealed class WorldLayerVertexSequencer
{
    /// <summary>
    ///     Number of vertices per block in the solid geometry.
    /// </summary>
    private const int SolidVerticesPerBlock = 8;

    /// <summary>
    ///     Number of indices per block in the solid geometry.
    /// </summary>
    private const int SolidIndicesPerBlock = 30;

    private readonly WorldSpriteSequencer spriteSequencer;

    public WorldLayerVertexSequencer(WorldSpriteSequencer spriteSequencer)
    {
        this.spriteSequencer = spriteSequencer;
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
        spriteSequencer.SequenceSprites(layer.FrontFaceTileSprites, renderPlan, systemTime, out var frontBaseIndex,
            out var frontIndexCount);
        spriteSequencer.SequenceSprites(layer.TopFaceTileSprites, renderPlan, systemTime, out var topBaseIndex,
            out var topIndexCount);
        spriteSequencer.SequenceSprites(layer.AnimatedSprites, renderPlan, systemTime, out var spriteBaseIndex,
            out var spriteIndexCount);
        AddBlockGeometry(layer.TopFaceTileSprites, renderPlan);

        renderPlan.PushDebugGroup($"Layer {layer.ZFloor}");

        renderPlan.PushDebugGroup("Block Front Faces");
        renderPlan.DrawSprites(frontBaseIndex, frontIndexCount);
        renderPlan.PopDebugGroup();

        renderPlan.PushDebugGroup("Block Top Faces");
        renderPlan.DrawSprites(topBaseIndex, topIndexCount);
        renderPlan.PopDebugGroup();

        renderPlan.PushDebugGroup("Animated Sprites");
        renderPlan.DrawSprites(spriteBaseIndex, spriteIndexCount);
        renderPlan.PopDebugGroup();

        renderPlan.PopDebugGroup();
    }

    /// <summary>
    ///     Adds block geometry for shadows and lighting to the render plan.
    /// </summary>
    /// <param name="topFaces">Top faces of blocks to add.</param>
    /// <param name="renderPlan">Render plan.</param>
    private void AddBlockGeometry(List<PosVelId> topFaces, RenderPlan renderPlan)
    {
        if (!renderPlan.TryAddVertices(SolidVerticesPerBlock * topFaces.Count, out var vertices, out var baseIndex))
        {
            Logger.Error("Not enough room in vertex buffer for solid geometry.");
            return;
        }

        if (!renderPlan.TryAddSolidIndices(SolidIndicesPerBlock * topFaces.Count, out var indices,
                out var _))
            Logger.Error("Not enough room in solid index buffer for solid geometry.");

        for (var i = 0; i < topFaces.Count; ++i)
        {
            var basePos = topFaces[i].Position;
            var blockVertices = vertices.Slice(i * SolidVerticesPerBlock, SolidVerticesPerBlock);
            var blockIndices = indices.Slice(i * SolidIndicesPerBlock, SolidIndicesPerBlock);

            AddVerticesForBlock(basePos, blockVertices);
            AddIndicesForBlock(baseIndex, blockIndices);
        }
    }

    /// <summary>
    ///     Adds vertices for the given block.
    /// </summary>
    /// <param name="basePos">Base position of the block.</param>
    /// <param name="blockVertices">Vertices to populate for this block.</param>
    private void AddVerticesForBlock(Vector3 basePos, Span<WorldVertex> blockVertices)
    {
        // Vertex 0 = (0, 0, 0) [left, back, top]
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

        // Vertex 1 = (1, 0, 0) [right, back, top]
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

        // Vertex 2 = (1, -1, 0) [right, front, top]
        blockVertices[2] = new WorldVertex
        {
            PosX = basePos.X + 1,
            PosY = basePos.Y - 1,
            PosZ = basePos.Z,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };

        // Vertex 3 = (0, -1, 0) [left, front, top]
        blockVertices[3] = new WorldVertex
        {
            PosX = basePos.X,
            PosY = basePos.Y - 1,
            PosZ = basePos.Z,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };

        // Vertex 4 = (0, 0, -1) [left, back, bottom]
        blockVertices[4] = new WorldVertex
        {
            PosX = basePos.X,
            PosY = basePos.Y,
            PosZ = basePos.Z - 1,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };

        // Vertex 5 = (1, 0, -1) [right, back, bottom]
        blockVertices[5] = new WorldVertex
        {
            PosX = basePos.X + 1,
            PosY = basePos.Y,
            PosZ = basePos.Z - 1,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };

        // Vertex 6 = (1, -1, -1) [right, front, top]
        blockVertices[6] = new WorldVertex
        {
            PosX = basePos.X + 1,
            PosY = basePos.Y - 1,
            PosZ = basePos.Z - 1,
            TexX = 0.0f, // unused
            TexY = 0.0f, // unused
            VelX = 0.0f, // unused
            VelY = 0.0f, // unused
            VelZ = 0.0f // unused
        };

        // Vertex 7 = (0, -1, -1) [left, front, top]
        blockVertices[7] = new WorldVertex
        {
            PosX = basePos.X,
            PosY = basePos.Y - 1,
            PosZ = basePos.Z - 1,
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
        blockIndices[0] = baseIndex + 0;
        blockIndices[1] = baseIndex + 3;
        blockIndices[2] = baseIndex + 1;
        blockIndices[3] = baseIndex + 1;
        blockIndices[4] = baseIndex + 3;
        blockIndices[5] = baseIndex + 2;

        // Front face.
        blockIndices[6] = baseIndex + 3;
        blockIndices[7] = baseIndex + 7;
        blockIndices[8] = baseIndex + 2;
        blockIndices[9] = baseIndex + 2;
        blockIndices[10] = baseIndex + 7;
        blockIndices[11] = baseIndex + 6;

        // Bottom face is omitted. Might need to eventually add to handle point light
        // sources that radiate upward?

        // Back face.
        blockIndices[12] = baseIndex + 0;
        blockIndices[13] = baseIndex + 1;
        blockIndices[14] = baseIndex + 4;
        blockIndices[15] = baseIndex + 4;
        blockIndices[16] = baseIndex + 1;
        blockIndices[17] = baseIndex + 5;

        // Left face.
        blockIndices[18] = baseIndex + 4;
        blockIndices[19] = baseIndex + 0;
        blockIndices[20] = baseIndex + 7;
        blockIndices[21] = baseIndex + 7;
        blockIndices[22] = baseIndex + 0;
        blockIndices[23] = baseIndex + 3;

        // Right face.
        blockIndices[24] = baseIndex + 2;
        blockIndices[25] = baseIndex + 1;
        blockIndices[26] = baseIndex + 6;
        blockIndices[27] = baseIndex + 6;
        blockIndices[28] = baseIndex + 1;
        blockIndices[29] = baseIndex + 5;
    }
}