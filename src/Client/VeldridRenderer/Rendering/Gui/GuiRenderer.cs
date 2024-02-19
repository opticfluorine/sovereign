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
using System.Runtime.InteropServices;
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.GUI;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Timing;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Gui;

/// <summary>
///     Handles rendering of the GUI layer.
/// </summary>
public class GuiRenderer : IDisposable
{
    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly TextureAtlasManager atlasManager;
    private readonly AtlasMap atlasMap;
    private readonly CommonGuiManager guiManager;
    private readonly GuiPipeline guiPipeline;
    private readonly GuiResourceManager guiResourceManager;
    private readonly ISystemTimer systemTimer;

    /// <summary>
    ///     Working copy of the vertex shader constants.
    /// </summary>
    private GuiVertexShaderConstants vertexConstants;

    public GuiRenderer(CommonGuiManager guiManager, GuiResourceManager guiResourceManager, GuiPipeline guiPipeline,
        TextureAtlasManager atlasManager, ISystemTimer systemTimer, AnimatedSpriteManager animatedSpriteManager,
        AtlasMap atlasMap)
    {
        this.guiManager = guiManager;
        this.guiResourceManager = guiResourceManager;
        this.guiPipeline = guiPipeline;
        this.atlasManager = atlasManager;
        this.systemTimer = systemTimer;
        this.animatedSpriteManager = animatedSpriteManager;
        this.atlasMap = atlasMap;
    }

    public void Dispose()
    {
        guiPipeline.Dispose();
        guiResourceManager.Dispose();
    }

    /// <summary>
    ///     Initializes the GUI renderer and its resources.
    /// </summary>
    public void Initialize()
    {
        // Initialize resources.
        guiResourceManager.Initialize();
        guiPipeline.Initialize();

        // Compute constant projection matrix.
        var io = ImGui.GetIO();
        vertexConstants.ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(
            0.0f,
            io.DisplaySize.X,
            io.DisplaySize.Y,
            0.0f,
            -1.0f,
            1.0f
        );
    }

    /// <summary>
    ///     Ends the current frame after all rendering is complete.
    /// </summary>
    public void EndFrame()
    {
        guiResourceManager.EndFrame();
    }

    /// <summary>
    ///     Renders the GUI layer.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    public void RenderGui(CommandList commandList)
    {
        if (guiResourceManager.GuiUniformBuffer == null)
            throw new InvalidOperationException("GUI uniform buffer is null.");

        // First stage, convert pending ImGui commands to drawing-level data
        var drawData = guiManager.Render();
        if (drawData.CmdListsCount == 0) return;

        // Update and bind resources used across all GUI draw calls.
        UpdateBuffers(commandList, drawData);
        commandList.SetPipeline(guiPipeline.Pipeline);

        // Execute draw commands.
        var vertexOffset = 0;
        var indexOffset = 0;
        var constantsUpdated = false;
        var systemTime = systemTimer.GetTime();
        for (var i = 0; i < drawData.CmdListsCount; ++i)
        {
            var curList = drawData.CmdLists[i];
            var listIndexOffset = 0;
            for (var j = 0; j < curList.CmdBuffer.Size; ++j)
            {
                var curCmd = curList.CmdBuffer[j];

                // Resource binding for next draw call.
                if (TryBindTexture(commandList, curCmd, systemTime) || !constantsUpdated)
                {
                    guiResourceManager.GuiUniformBuffer.Buffer[0] = vertexConstants;
                    guiResourceManager.GuiUniformBuffer.Update(commandList);
                    constantsUpdated = true;
                }

                // Execute draw call.
                commandList.DrawIndexed(curCmd.ElemCount, 1,
                    (uint)(indexOffset + listIndexOffset), vertexOffset, 0);
                listIndexOffset += (int)curCmd.ElemCount;
            }

            vertexOffset += curList.VtxBuffer.Size;
            indexOffset += curList.IdxBuffer.Size;
        }
    }

    /// <summary>
    ///     Binds the texture for a draw call.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    /// <param name="curCmd">Current draw command.</param>
    /// <param name="systemTime">Current system time.</param>
    /// <returns>
    ///     true if vertex shader constants were updated, false otherwise.
    /// </returns>
    private bool TryBindTexture(CommandList commandList, ImDrawCmdPtr curCmd, ulong systemTime)
    {
        if (curCmd.TextureId == IntPtr.Zero) return false;
        if (atlasManager.TextureAtlas == null) throw new InvalidOperationException("Texture atlas is null.");

        // Resolve the texture ID to an offset into the texture atlas.
        float startX, startY, endX, endY;
        if (curCmd.TextureId == GuiFontAtlas.TextureId)
        {
            // Font render.
            (startX, startY, endX, endY) = atlasManager.TextureAtlas.FontAtlasBounds;
        }
        else
        {
            // Animated sprite render. Resolve to sprite.
            const int spriteIdOffset = 2;
            var animSpriteId = (int)curCmd.TextureId - spriteIdOffset;
            var animSprite = animatedSpriteManager.AnimatedSprites[animSpriteId];
            var sprite = animSprite.GetSpriteForTime(systemTime, Orientation.South);

            // Resolve sprite to texture atlas offset.
            var mapElem = atlasMap.MapElements[sprite.Id];
            startX = mapElem.NormalizedLeftX;
            startY = mapElem.NormalizedTopY;
            endX = mapElem.NormalizedRightX;
            endY = mapElem.NormalizedBottomY;
        }

        // Update the vertex shader constants with the new offset.
        vertexConstants.TextureStart = new Vector2(startX, startY);
        vertexConstants.TextureEnd = new Vector2(endX, endY);
        return true;
    }

    /// <summary>
    ///     Updates the GUI vertex and index byffers for the current frame.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    /// <param name="drawData">Dear ImGui draw data for current frame.</param>
    /// <exception cref="InvalidOperationException"></exception>
    private unsafe void UpdateBuffers(CommandList commandList, ImDrawDataPtr drawData)
    {
        // Second stage, set up GUI rendering resources.
        // Check validity.
        var vertexBuf = guiResourceManager.GuiVertexBuffer;
        var indexBuf = guiResourceManager.GuiIndexBuffer;
        if (vertexBuf == null || indexBuf == null)
            throw new InvalidOperationException("Tried to render GUI without buffers.");
        if (drawData.TotalVtxCount > vertexBuf.Length)
            throw new InvalidOperationException("Vertex buffer too small for GUI render.");
        if (drawData.TotalIdxCount > indexBuf.Length)
            throw new InvalidOperationException("Index buffer too small for GUI render.");

        // Populate buffers.
        var vertexOffset = 0;
        var indexOffset = 0;
        var vertexSize = Marshal.SizeOf(typeof(ImDrawVert));
        var indexSize = Marshal.SizeOf<ushort>();
        for (var i = 0; i < drawData.CmdListsCount; ++i)
        {
            var curList = drawData.CmdLists[i];

            // Grab buffers from ImGui.
            var vertexIn = curList.VtxBuffer.Data.ToPointer();
            var vertexOut = (vertexBuf.BufferPtr + vertexOffset * vertexSize).ToPointer();
            var vertexBytes = curList.VtxBuffer.Size * vertexSize;

            var indexIn = curList.IdxBuffer.Data.ToPointer();
            var indexOut = (indexBuf.BufferPtr + indexOffset * indexSize).ToPointer();
            var indexBytes = curList.IdxBuffer.Size * indexSize;

            // Copy into the device buffers. This is a redundant copy but we can optimize it away later
            // if it's a significant performance impact.
            Buffer.MemoryCopy(vertexIn, vertexOut,
                vertexSize * (vertexBuf.Length - vertexOffset),
                vertexBytes);
            Buffer.MemoryCopy(indexIn, indexOut,
                indexSize * (indexBuf.Length - indexOffset),
                indexBytes);

            vertexOffset += curList.VtxBuffer.Size;
            indexOffset += curList.IdxBuffer.Size;
        }

        // Synchronize buffers to device; bind resources that will be
        // used by all GUI draw calls.
        vertexBuf.Update(commandList);
        indexBuf.Update(commandList);
    }
}