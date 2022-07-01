/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.VeldridRenderer.Rendering.Resources;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
/// Responsible for rendering the game world.
/// </summary>
public class WorldRenderer : IDisposable
{
    private readonly VeldridDevice device;
    private readonly WorldPipeline pipeline;
    private readonly VeldridResourceManager resMgr;
    private readonly GameResourceManager gameResMgr;

    /// <summary>
    /// Bindable resource set used by the world renderer.
    /// </summary>
    private ResourceSet resourceSet;

    /// <summary>
    /// Viewport used for world rendering.
    /// </summary>
    private Viewport viewport;

    public WorldRenderer(VeldridDevice device, WorldPipeline pipeline,
        VeldridResourceManager resMgr, GameResourceManager gameResMgr)
    {
        this.device = device;
        this.pipeline = pipeline;
        this.resMgr = resMgr;
        this.gameResMgr = gameResMgr;
    }

    /// <summary>
    /// Initializes the world renderer.
    /// </summary>
    public void Initialize()
    {
        // Create resources.
        pipeline.Initialize();
        CreateResourceSet();

        // Define world rendering viewport.
        viewport = new Viewport(
            x: 0.0f,
            y: 0.0f,
            width: device.DisplayMode.Width,
            height: device.DisplayMode.Height,
            minDepth: 0.0f,
            maxDepth: 1.0f
        );
    }

    public void Dispose()
    {
        resourceSet?.Dispose();
        pipeline.Dispose();
    }

    /// <summary>
    /// Renders the game world.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    public void Render(CommandList commandList)
    {
        commandList.PushDebugGroup("WorldRenderer.Render");

        // Set pipeline and bind resources.
        commandList.SetViewport(0, viewport);
        commandList.SetPipeline(pipeline.Pipeline);
        commandList.SetGraphicsResourceSet(0, resourceSet);
        commandList.SetVertexBuffer(0, gameResMgr.VertexBuffer.DeviceBuffer);
        commandList.SetIndexBuffer(gameResMgr.IndexBuffer.DeviceBuffer,
            IndexFormat.UInt32);

        // Execute draw commands from the buffer.
        for (int i = 0; i < gameResMgr.DrawCount; ++i)
        {
            int drawSize = gameResMgr.DrawBuffer[i];
            commandList.DrawIndexed((uint)drawSize);
        }

        commandList.PopDebugGroup();
    }

    /// <summary>
    /// Creates the bindable resource set.
    /// </summary>
    private void CreateResourceSet()
    {
        var resLayoutDesc = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription[]
            {
                new ResourceLayoutElementDescription(
                    GameResourceManager.RES_SHADER_CONSTANTS,
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex
                ),
                new ResourceLayoutElementDescription(
                    GameResourceManager.RES_TEXTURE_ATLAS,
                    ResourceKind.TextureReadOnly,
                    ShaderStages.Fragment
                ),
                new ResourceLayoutElementDescription(
                    GameResourceManager.RES_TEXTURE_ATLAS_SAMPLER,
                    ResourceKind.Sampler,
                    ShaderStages.Fragment
                )
            }
        );
        var resLayout = device.Device.ResourceFactory.CreateResourceLayout(resLayoutDesc);

        var resSetDesc = new ResourceSetDescription(
            resLayout,
            gameResMgr.VertexUniformBuffer.DeviceBuffer,
            resMgr.AtlasTexture.TextureView,
            device.Device.PointSampler
        );
        resourceSet = device.Device.ResourceFactory.CreateResourceSet(resSetDesc);
    }

}
