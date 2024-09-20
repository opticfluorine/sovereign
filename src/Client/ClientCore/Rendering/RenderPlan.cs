// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using Sovereign.ClientCore.Rendering.Resources.Buffers;

namespace Sovereign.ClientCore.Rendering;

/// <summary>
///     Denotes the type of action supported by a render command.
/// </summary>
public enum RenderCommandType
{
    /// <summary>
    ///     Draw sprites.
    /// </summary>
    DrawSprites
}

/// <summary>
///     Describes a single rendering action to take.
/// </summary>
public struct RenderCommand
{
    /// <summary>
    ///     Type of action to perform.
    /// </summary>
    public RenderCommandType RenderCommandType;

    /// <summary>
    ///     Base index to use for actions that consume an index buffer.
    /// </summary>
    public uint BaseIndex;

    /// <summary>
    ///     Number of indices to use for actions that consume an index buffer.
    /// </summary>
    public uint IndexCount;
}

/// <summary>
///     Manages the plan for rendering a frame.
/// </summary>
public class RenderPlan
{
    /// <summary>
    ///     Index buffer being planned.
    /// </summary>
    private readonly uint[] indexBuffer;

    /// <summary>
    ///     Rendering commands to be executed for this plan.
    /// </summary>
    private readonly RenderCommand[] renderCommands;

    /// <summary>
    ///     Vertex buffer being planned.
    /// </summary>
    private readonly WorldVertex[] vertexBuffer;

    /// <summary>
    ///     Number of rendering commands in the plan.
    /// </summary>
    private int commandCount;

    /// <summary>
    ///     Number of indices in the index buffer.
    /// </summary>
    private int indexCount;

    /// <summary>
    ///     Number of vertices in the vertex buffer.
    /// </summary>
    private int vertexCount;

    public RenderPlan(WorldVertex[] vertexBuffer, uint[] indexBuffer, int commandListSize)
    {
        this.vertexBuffer = vertexBuffer;
        this.indexBuffer = indexBuffer;
        renderCommands = new RenderCommand[commandListSize];
    }

    /// <summary>
    ///     Vertex count.
    /// </summary>
    public int VertexCount => vertexCount;

    /// <summary>
    ///     Index count.
    /// </summary>
    public int IndexCount => indexCount;

    /// <summary>
    ///     Resets the render plan for a new frame.
    /// </summary>
    public void Reset()
    {
        vertexCount = 0;
        indexCount = 0;
        commandCount = 0;
    }

    /// <summary>
    ///     Tries to reserve a contiguous block of vertices in the plan if space is available.
    /// </summary>
    /// <param name="count">Number of contiguous vertices to reserve.</param>
    /// <param name="vertices">Block of vertices to populate.</param>
    /// <param name="baseIndex">Index of the first vertex in the block.</param>
    /// <returns>true if successful, false otherwise.</returns>
    public bool TryAddVertices(int count, out Span<WorldVertex> vertices, out uint baseIndex)
    {
        if (vertexCount + count > vertexBuffer.Length)
        {
            vertices = new Span<WorldVertex>();
            baseIndex = 0;
            return false;
        }

        vertices = new Span<WorldVertex>(vertexBuffer, vertexCount, count);
        baseIndex = (uint)vertexCount;
        vertexCount += count;
        return true;
    }

    /// <summary>
    ///     Tries to reserve a contiguous block of indices in the plan if space is available.
    /// </summary>
    /// <param name="count">Number of contiguous indices to reserve.</param>
    /// <param name="indices">Block of indices to populate.</param>
    /// <param name="baseIndex">Index to first index in the block.</param>
    /// <returns>true if successful, false otherwise.</returns>
    public bool TryAddIndices(int count, out Span<uint> indices, out uint baseIndex)
    {
        if (indexCount + count > indexBuffer.Length)
        {
            indices = new Span<uint>();
            baseIndex = 0;
            return false;
        }

        indices = new Span<uint>(indexBuffer, indexCount, count);
        baseIndex = (uint)indexCount;
        indexCount += count;
        return true;
    }

    /// <summary>
    ///     Tries to add a DrawSprites command to the plan.
    /// </summary>
    /// <param name="drawBaseIndex">Base index.</param>
    /// <param name="drawIndexCount">Index count.</param>
    /// <returns>true if successful, false otherwise.</returns>
    public bool TryDrawSprites(uint drawBaseIndex, uint drawIndexCount)
    {
        if (commandCount == renderCommands.Length) return false;

        renderCommands[commandCount] = new RenderCommand
        {
            RenderCommandType = RenderCommandType.DrawSprites,
            BaseIndex = drawBaseIndex,
            IndexCount = drawIndexCount
        };
        commandCount++;
        return true;
    }

    /// <summary>
    ///     Finalizes the rendering plan and generates the ReadOnlySpans to be consumed by the renderer.
    /// </summary>
    /// <returns>Rendering commands to execute.</returns>
    public ReadOnlySpan<RenderCommand> GetCommands()
    {
        return new ReadOnlySpan<RenderCommand>(renderCommands, 0, commandCount);
    }
}