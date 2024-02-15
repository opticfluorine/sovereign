/*
 * Sovereign Engine
 * Copyright (c) 2021 opticfluorine
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
using System.Runtime.InteropServices;
using Castle.Core.Logging;
using ImGuiNET;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Scenes;
using Sovereign.VeldridRenderer.Rendering.Gui;
using Sovereign.VeldridRenderer.Rendering.Resources;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering;

/// <summary>
///     Renderer implementation using the Veldrid graphics library.
/// </summary>
public class VeldridRenderer : IRenderer
{
    /// <summary>
    ///     Veldrid graphics device.
    /// </summary>
    private readonly VeldridDevice device;

    private readonly CommonGuiManager guiManager;
    private readonly GuiPipeline guiPipeline;
    private readonly GuiResourceManager guiResourceManager;

    /// <summary>
    ///     Veldrid resource manager.
    /// </summary>
    private readonly VeldridResourceManager resourceManager;

    /// <summary>
    ///     Scene consumer.
    /// </summary>
    private readonly VeldridSceneConsumer sceneConsumer;

    /// <summary>
    ///     Scene manager.
    /// </summary>
    private readonly SceneManager sceneManager;

    /// <summary>
    ///     Dispose flag.
    /// </summary>
    private bool isDisposed;

    public VeldridRenderer(VeldridDevice device, VeldridResourceManager resourceManager,
        SceneManager sceneManager, VeldridSceneConsumer sceneConsumer, CommonGuiManager guiManager,
        GuiResourceManager guiResourceManager, GuiPipeline guiPipeline)
    {
        this.device = device;
        this.resourceManager = resourceManager;
        this.sceneManager = sceneManager;
        this.sceneConsumer = sceneConsumer;
        this.guiManager = guiManager;
        this.guiResourceManager = guiResourceManager;
        this.guiPipeline = guiPipeline;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void Initialize(IVideoAdapter videoAdapter)
    {
        try
        {
            /* Attempt to create the rendering device. */
            device.CreateDevice();

            /* Install main resources. */
            resourceManager.InitializeBaseResources();
            guiResourceManager.Initialize();
            guiPipeline.Initialize();

            /* Initialize all scenes. */
            sceneConsumer.Initialize();
        }
        catch (Exception e)
        {
            throw new RendererInitializationException("Failed to initialize renderer.", e);
        }
    }

    public void Cleanup()
    {
        if (!isDisposed)
        {
            sceneConsumer.Dispose();
            guiPipeline.Dispose();
            guiResourceManager.Dispose();
            resourceManager.Dispose();
            device.Dispose();
            isDisposed = true;
        }
    }

    public void Render()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");
        if (resourceManager.CommandList == null)
            throw new InvalidOperationException("Command list not ready.");

        try
        {
            /* Prepare for rendering. */
            var commandList = resourceManager.CommandList;
            commandList.Begin();
            commandList.SetFramebuffer(device.Device.SwapchainFramebuffer);
            commandList.ClearColorTarget(0, RgbaFloat.Black);

            /* Do scene processing to fill buffers for GPU. */
            var scene = sceneManager.ActiveScene;
            scene.BeginScene();
            sceneConsumer.ConsumeScene(scene);
            scene.EndScene();

            // GUI rendering from Dear ImGui.
            RenderGui(commandList);

            /* Render and present the next frame. */
            commandList.End();
            device.Device.SubmitCommands(commandList);
            device.Device.WaitForIdle();
            device.Device.SwapBuffers();
        }
        catch (Exception e)
        {
            Logger.Error("Error during rendering.", e);
        }
    }

    /// <summary>
    ///     Renders the GUI layer.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    private unsafe void RenderGui(CommandList commandList)
    {
        // First stage, convert pending ImGui commands to drawing-level data
        var drawData = guiManager.Render();

        // Second stage, set up GUI rendering resources.
        // Check validity.
        var vertexBuf = guiResourceManager.GuiVertexBuffer;
        var indexBuf = guiResourceManager.GuiIndexBuffer;
        if (vertexBuf == null || indexBuf == null)
            throw new InvalidOperationException("Tried to render GUI without buffers.");
        if (drawData.CmdListsCount == 0) return;
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

        // Synchronize buffers to device.
        vertexBuf.Update(commandList);
        indexBuf.Update(commandList);

        // Execute draw commands.
        vertexOffset = 0;
        indexOffset = 0;
        for (var i = 0; i < drawData.CmdListsCount; ++i)
        {
            var curList = drawData.CmdLists[i];
            var listIndexOffset = 0;
            for (var j = 0; j < curList.CmdBuffer.Size; ++j)
            {
                var curCmd = curList.CmdBuffer[j];

                // Resource binding for next draw call.
                commandList.SetPipeline(guiPipeline.Pipeline);

                // Execute draw call.
                commandList.DrawIndexed(curCmd.ElemCount, 1,
                    (uint)(indexOffset + listIndexOffset), vertexOffset, 0);
                listIndexOffset += (int)curCmd.ElemCount;
            }

            vertexOffset += curList.VtxBuffer.Size;
            indexOffset += curList.IdxBuffer.Size;
        }
    }
}