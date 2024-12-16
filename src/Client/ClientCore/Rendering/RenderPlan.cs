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
using System.Numerics;
using Sovereign.ClientCore.Rendering.Resources.Buffers;
using Sovereign.ClientCore.Rendering.Scenes.Game.World;

namespace Sovereign.ClientCore.Rendering;

/// <summary>
///     Denotes the type of action supported by a render command.
/// </summary>
public enum RenderCommandType
{
    /// <summary>
    ///     Draw sprites.
    /// </summary>
    DrawSprites,

    /// <summary>
    ///     Draw the global block shadow map to its texture.
    /// </summary>
    DrawGlobalShadowMap,

    /// <summary>
    ///     Draw the shadow maps for point light sources.
    /// </summary>
    DrawPointLightShadowMaps,

    /// <summary>
    ///     Push a debug group onto the stack.
    /// </summary>
    PushDebug,

    /// <summary>
    ///     Pop a debug group from the stack.
    /// </summary>
    PopDebug
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

    /// <summary>
    ///     Debug group name (for PushDebugGroup commands only).
    /// </summary>
    public string? DebugGroupName;
}

/// <summary>
///     Details of a rendered light.
/// </summary>
public struct RenderLight
{
    /// <summary>
    ///     First index in the solid geometry index buffer to use for this light's shadow map.
    /// </summary>
    public uint BaseIndex;

    /// <summary>
    ///     Number of indices from the solid geometry index buffer to use for this light's shadow map.
    /// </summary>
    public uint IndexCount;

    /// <summary>
    ///     Details of the point light source.
    /// </summary>
    public PositionedLight Light;
}

/// <summary>
///     Specifies information for an entity that requires a name label to be drawn.
/// </summary>
public struct NameLabel
{
    /// <summary>
    ///     Entity ID.
    /// </summary>
    public ulong EntityId;

    /// <summary>
    ///     Entity position interpolated to the time of the current frame.
    /// </summary>
    public Vector3 InterpolatedPosition;

    /// <summary>
    ///     Sprite ID to use for size reference.
    /// </summary>
    public int SpriteId;
}

/// <summary>
///     Manages the plan for rendering a frame.
/// </summary>
public class RenderPlan
{
    /// <summary>
    ///     Initial size of point light list.
    /// </summary>
    private const int InitialLightSize = 128;

    /// <summary>
    ///     Initial size of name label list.
    /// </summary>
    private const int InitialNameLabelSize = 128;

    private readonly uint[] solidIndexBuffer;

    /// <summary>
    ///     Index buffer being planned.
    /// </summary>
    private readonly uint[] spriteIndexBuffer;

    /// <summary>
    ///     Vertex buffer being planned.
    /// </summary>
    private readonly WorldVertex[] vertexBuffer;

    /// <summary>
    ///     Number of rendering commands in the plan.
    /// </summary>
    private int commandCount;

    /// <summary>
    ///     Number of point lights to include in rendering.
    /// </summary>
    private int lightCount;

    /// <summary>
    ///     Point lights to include in rendering.
    /// </summary>
    public RenderLight[] Lights;

    /// <summary>
    ///     Running total of used name labels.
    /// </summary>
    private int nameLabelCount;

    /// <summary>
    ///     Name labels to include in rendering.
    /// </summary>
    public NameLabel[] NameLabels;

    /// <summary>
    ///     Running total of used solid geometry indices already allocated to point lights.
    /// </summary>
    private uint pointLightSolidIndexCount;

    /// <summary>
    ///     Rendering commands to be executed for this plan.
    /// </summary>
    private RenderCommand[] renderCommands;

    /// <summary>
    ///     Number of indices in the solid geometry index buffer.
    /// </summary>
    private int solidIndexCount;

    /// <summary>
    ///     Number of indices in the sprite index buffer.
    /// </summary>
    private int spriteIndexCount;

    /// <summary>
    ///     Number of vertices in the vertex buffer.
    /// </summary>
    private int vertexCount;

    /// <summary>
    ///     Number of indices at the beginning of the solid geometry index buffer to use for world rendering.
    /// </summary>
    private uint worldSolidIndexCount;

    public RenderPlan(WorldVertex[] vertexBuffer, uint[] spriteIndexBuffer, uint[] solidIndexBuffer,
        int commandListSize)
    {
        this.vertexBuffer = vertexBuffer;
        this.spriteIndexBuffer = spriteIndexBuffer;
        this.solidIndexBuffer = solidIndexBuffer;
        renderCommands = new RenderCommand[commandListSize];
        Lights = new RenderLight[InitialLightSize];
        NameLabels = new NameLabel[InitialNameLabelSize];
    }

    /// <summary>
    ///     Vertex count.
    /// </summary>
    public int VertexCount => vertexCount;

    /// <summary>
    ///     Sprite index count.
    /// </summary>
    public int SpriteIndexCount => spriteIndexCount;

    /// <summary>
    ///     Solid geometry index count.
    /// </summary>
    public int SolidIndexCount => solidIndexCount;

    /// <summary>
    ///     Number of solid geometry indices used for world shadow map.
    /// </summary>
    public uint GlobalSolidIndexCount => worldSolidIndexCount;

    /// <summary>
    ///     Number of point light sources in the render plan.
    /// </summary>
    public int LightCount => lightCount;

    /// <summary>
    ///     Number of name labels in the render plan.
    /// </summary>
    public int NameLabelCount => nameLabelCount;

    /// <summary>
    ///     Camera position in world coordinates.
    /// </summary>
    public Vector3 CameraPosition { get; set; }

    /// <summary>
    ///     Resets the render plan for a new frame.
    /// </summary>
    public void Reset()
    {
        vertexCount = 0;
        spriteIndexCount = 0;
        solidIndexCount = 0;
        commandCount = 0;
        worldSolidIndexCount = 0;
        lightCount = 0;
        pointLightSolidIndexCount = 0;
        nameLabelCount = 0;
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
    public bool TryAddSpriteIndices(int count, out Span<uint> indices, out uint baseIndex)
    {
        if (spriteIndexCount + count > spriteIndexBuffer.Length)
        {
            indices = new Span<uint>();
            baseIndex = 0;
            return false;
        }

        indices = new Span<uint>(spriteIndexBuffer, spriteIndexCount, count);
        baseIndex = (uint)spriteIndexCount;
        spriteIndexCount += count;
        return true;
    }

    /// <summary>
    ///     Tries to reserve a contiguous block of solid geometry indices in the plan if space is available.
    /// </summary>
    /// <param name="count">Number of contiguous indices to reserve.</param>
    /// <param name="indices">Block of indices to populate.</param>
    /// <param name="baseIndex">Index to first index in the block.</param>
    /// <returns>true if successful, false otherwise.</returns>
    public bool TryAddSolidIndices(int count, out Span<uint> indices, out uint baseIndex)
    {
        if (solidIndexCount + count > solidIndexBuffer.Length)
        {
            indices = new Span<uint>();
            baseIndex = 0;
            return false;
        }

        indices = new Span<uint>(solidIndexBuffer, solidIndexCount, count);
        baseIndex = (uint)solidIndexCount;
        solidIndexCount += count;
        return true;
    }

    /// <summary>
    ///     Adds a DrawSprites command to the plan.
    /// </summary>
    /// <param name="drawBaseIndex">Base index.</param>
    /// <param name="drawIndexCount">Index count.</param>
    public void DrawSprites(uint drawBaseIndex, uint drawIndexCount)
    {
        if (commandCount == renderCommands.Length) ExpandCommandList();

        // Skip zero-length draw commands.
        if (drawIndexCount == 0) return;

        renderCommands[commandCount] = new RenderCommand
        {
            RenderCommandType = RenderCommandType.DrawSprites,
            BaseIndex = drawBaseIndex,
            IndexCount = drawIndexCount
        };
        commandCount++;
    }

    /// <summary>
    ///     Adds a DrawBlockShadowMap command to the plan.
    /// </summary>
    public void DrawBlockShadowMap()
    {
        if (commandCount == renderCommands.Length) ExpandCommandList();

        renderCommands[commandCount] = new RenderCommand
        {
            RenderCommandType = RenderCommandType.DrawGlobalShadowMap
        };
        commandCount++;
    }

    /// <summary>
    ///     Adds a DrawPointLightShadowMaps command to the plan.
    /// </summary>
    public void DrawPointLightShadowMaps()
    {
        if (commandCount == renderCommands.Length) ExpandCommandList();

        renderCommands[commandCount] = new RenderCommand
        {
            RenderCommandType = RenderCommandType.DrawPointLightShadowMaps
        };
        commandCount++;
    }

    /// <summary>
    ///     Pushes a new debug group onto the stack for subsequent calls.
    /// </summary>
    /// <param name="groupName">Debug group name.</param>
    public void PushDebugGroup(string groupName)
    {
        if (commandCount == renderCommands.Length) ExpandCommandList();

        renderCommands[commandCount] = new RenderCommand
        {
            RenderCommandType = RenderCommandType.PushDebug,
            DebugGroupName = groupName
        };
        commandCount++;
    }

    /// <summary>
    ///     Pops a debug group from the stack for subsequent calls.
    /// </summary>
    public void PopDebugGroup()
    {
        if (commandCount == renderCommands.Length) ExpandCommandList();

        renderCommands[commandCount] = new RenderCommand
        {
            RenderCommandType = RenderCommandType.PopDebug
        };
        commandCount++;
    }

    /// <summary>
    ///     Sets the number of solid indices to use for the global shadowmap.
    /// </summary>
    /// <param name="count">Index count.</param>
    public void SetWorldSolidIndexCount(uint count)
    {
        worldSolidIndexCount = count;
    }

    /// <summary>
    ///     Adds a point light source to the render plan data.
    /// </summary>
    /// <param name="indexCount">Solid geometry indices used by this point light source.</param>
    /// <param name="light">Light source details.</param>
    public void AddLight(int indexCount, PositionedLight light)
    {
        if (lightCount == Lights.Length)
        {
            var newLights = new RenderLight[Lights.Length + InitialLightSize];
            Array.Copy(Lights, newLights, Lights.Length);
            Lights = newLights;
        }

        Lights[lightCount++] = new RenderLight
        {
            BaseIndex = worldSolidIndexCount + pointLightSolidIndexCount,
            IndexCount = (uint)indexCount,
            Light = light
        };
        pointLightSolidIndexCount += (uint)indexCount;
    }

    /// <summary>
    ///     Adds a name label to the render plan data.
    /// </summary>
    /// <param name="nameLabel">Name label to add.</param>
    public void AddNameLabel(NameLabel nameLabel)
    {
        if (nameLabelCount == NameLabels.Length)
        {
            var newNameLabels = new NameLabel[NameLabels.Length + InitialNameLabelSize];
            Array.Copy(NameLabels, newNameLabels, NameLabels.Length);
            NameLabels = newNameLabels;
        }

        NameLabels[nameLabelCount++] = nameLabel;
    }

    /// <summary>
    ///     Finalizes the rendering plan and generates the ReadOnlySpans to be consumed by the renderer.
    /// </summary>
    /// <returns>Rendering commands to execute.</returns>
    public ReadOnlySpan<RenderCommand> GetCommands()
    {
        return new ReadOnlySpan<RenderCommand>(renderCommands, 0, commandCount);
    }

    /// <summary>
    ///     Grows the command array.
    /// </summary>
    private void ExpandCommandList()
    {
        var newCommands = new RenderCommand[2 * renderCommands.Length];
        Array.Copy(renderCommands, newCommands, renderCommands.Length);
        renderCommands = newCommands;
    }
}