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
using System.Runtime.InteropServices;
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Gui;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Gui;

/// <summary>
///     Handles rendering of the GUI layer.
/// </summary>
public class GuiRenderer : IDisposable
{
    private readonly CommonGuiManager guiManager;
    private readonly GuiPipeline guiPipeline;
    private readonly GuiResourceManager guiResourceManager;

    public GuiRenderer(CommonGuiManager guiManager, GuiResourceManager guiResourceManager, GuiPipeline guiPipeline)
    {
        this.guiManager = guiManager;
        this.guiResourceManager = guiResourceManager;
        this.guiPipeline = guiPipeline;
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
        guiResourceManager.Initialize();
        guiPipeline.Initialize();
    }

    /// <summary>
    ///     Renders the GUI layer.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    public void RenderGui(CommandList commandList)
    {
        // First stage, convert pending ImGui commands to drawing-level data
        var drawData = guiManager.Render();
        if (drawData.CmdListsCount == 0) return;

        // Update and bind resources used across all GUI draw calls.
        UpdateBuffers(commandList, drawData);
        commandList.SetPipeline(guiPipeline.Pipeline);

        // Execute draw commands.
        var vertexOffset = 0;
        var indexOffset = 0;
        for (var i = 0; i < drawData.CmdListsCount; ++i)
        {
            var curList = drawData.CmdLists[i];
            var listIndexOffset = 0;
            for (var j = 0; j < curList.CmdBuffer.Size; ++j)
            {
                var curCmd = curList.CmdBuffer[j];

                // Resource binding for next draw call.

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

    /// <summary>
    ///     Ends the current frame after all rendering is complete.
    /// </summary>
    public void EndFrame()
    {
        guiResourceManager.EndFrame();
    }
}