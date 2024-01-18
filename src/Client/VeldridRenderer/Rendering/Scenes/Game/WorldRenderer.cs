/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
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
using Sovereign.VeldridRenderer.Rendering.Resources;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Responsible for rendering the game world.
/// </summary>
public class WorldRenderer : IDisposable
{
    private readonly VeldridDevice device;
    private readonly GameResourceManager gameResMgr;
    private readonly WorldPipeline pipeline;
    private readonly VeldridResourceManager resMgr;

    /// <summary>
    ///     Bindable resource set used by the world renderer.
    /// </summary>
    private ResourceSet? resourceSet;

    /// <summary>
    ///     Viewport used for world rendering.
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

    public void Dispose()
    {
        resourceSet?.Dispose();
        pipeline.Dispose();
    }

    /// <summary>
    ///     Initializes the world renderer.
    /// </summary>
    public void Initialize()
    {
        if (device.DisplayMode == null)
            throw new InvalidOperationException("Display mode not set.");

        // Create resources.
        pipeline.Initialize();
        CreateResourceSet();

        // Define world rendering viewport.
        viewport = new Viewport(
            0.0f,
            0.0f,
            device.DisplayMode.Width,
            device.DisplayMode.Height,
            0.0f,
            1.0f
        );
    }

    /// <summary>
    ///     Renders the game world.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    public void Render(CommandList commandList)
    {
        if (gameResMgr.VertexBuffer == null || gameResMgr.IndexBuffer == null)
            throw new InvalidOperationException("Buffers not ready.");

        commandList.PushDebugGroup("WorldRenderer.Render");

        // Set pipeline and bind resources.
        commandList.SetViewport(0, viewport);
        commandList.SetPipeline(pipeline.Pipeline);
        commandList.SetGraphicsResourceSet(0, resourceSet);
        commandList.SetVertexBuffer(0, gameResMgr.VertexBuffer.DeviceBuffer);
        commandList.SetIndexBuffer(gameResMgr.IndexBuffer.DeviceBuffer,
            IndexFormat.UInt32);

        // Execute draw commands from the buffer.
        uint offset = 0;
        for (var i = 0; i < gameResMgr.DrawCount; ++i)
        {
            var drawSize = gameResMgr.DrawBuffer[i];
            commandList.DrawIndexed((uint)drawSize, 1, offset, 0, 0);
            offset += (uint)drawSize;
        }

        commandList.PopDebugGroup();
    }

    /// <summary>
    ///     Creates the bindable resource set.
    /// </summary>
    private void CreateResourceSet()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");
        if (gameResMgr.VertexUniformBuffer == null)
            throw new InvalidOperationException("Vertex uniform buffer not ready.");
        if (resMgr.AtlasTexture == null)
            throw new InvalidOperationException("Texture atlas not ready.");

        var resLayoutDesc = new ResourceLayoutDescription(new ResourceLayoutElementDescription(
            GameResourceManager.RES_SHADER_CONSTANTS,
            ResourceKind.UniformBuffer,
            ShaderStages.Vertex
        ), new ResourceLayoutElementDescription(
            GameResourceManager.RES_TEXTURE_ATLAS,
            ResourceKind.TextureReadOnly,
            ShaderStages.Fragment
        ), new ResourceLayoutElementDescription(
            GameResourceManager.RES_TEXTURE_ATLAS_SAMPLER,
            ResourceKind.Sampler,
            ShaderStages.Fragment
        ));
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