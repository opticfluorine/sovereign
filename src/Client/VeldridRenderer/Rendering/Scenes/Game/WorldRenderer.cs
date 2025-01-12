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
using Sovereign.ClientCore.Rendering;
using Sovereign.VeldridRenderer.Rendering.Resources;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Responsible for rendering the game world.
/// </summary>
public class WorldRenderer : IDisposable
{
    private readonly VeldridDevice device;
    private readonly FullPointLightMapRenderer fullPointLightMapRenderer;
    private readonly GameResourceManager gameResMgr;
    private readonly WorldPipeline pipeline;
    private readonly PointLightDepthMapRenderer pointLightDepthMapRenderer;
    private readonly VeldridResourceManager resMgr;

    /// <summary>
    ///     Resource set used for block shadow map rendering.
    /// </summary>
    private ResourceSet? blockShadowResourceSet;

    /// <summary>
    ///     Viewport used for block shadow map rendering.
    /// </summary>
    private Viewport blockShadowViewport;

    /// <summary>
    ///     Bindable resource set used by the world renderer.
    /// </summary>
    private ResourceSet? resourceSet;

    /// <summary>
    ///     Viewport used for world rendering.
    /// </summary>
    private Viewport viewport;

    public WorldRenderer(VeldridDevice device, WorldPipeline pipeline,
        VeldridResourceManager resMgr, GameResourceManager gameResMgr,
        PointLightDepthMapRenderer pointLightDepthMapRenderer, FullPointLightMapRenderer fullPointLightMapRenderer)
    {
        this.device = device;
        this.pipeline = pipeline;
        this.resMgr = resMgr;
        this.gameResMgr = gameResMgr;
        this.pointLightDepthMapRenderer = pointLightDepthMapRenderer;
        this.fullPointLightMapRenderer = fullPointLightMapRenderer;
    }

    public void Dispose()
    {
        resourceSet?.Dispose();
        blockShadowResourceSet?.Dispose();
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
        CreateResourceSet();
        CreateBlockShadowResourceSet();

        // Define world rendering viewport.
        viewport = new Viewport(
            0.0f,
            0.0f,
            device.DisplayMode.Width,
            device.DisplayMode.Height,
            0.0f,
            1.0f
        );

        blockShadowViewport = new Viewport(0.0f, 0.0f, GameResourceManager.ShadowMapWidth,
            GameResourceManager.ShadowMapHeight, 0.0f, 1.0f);
    }

    /// <summary>
    ///     Handles the reloading of device-side resources.
    /// </summary>
    public void ReloadResources()
    {
        // Recreate the resource set to use the latest textures.
        CreateResourceSet();
    }

    /// <summary>
    ///     Renders the game world.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    /// <param name="renderPlan">Render plan.</param>
    public void Render(CommandList commandList, RenderPlan renderPlan)
    {
        if (gameResMgr.VertexBuffer == null || gameResMgr.SpriteIndexBuffer == null)
            throw new InvalidOperationException("Buffers not ready.");

        commandList.PushDebugGroup("WorldRenderer.Render");

        // Set pipeline and bind resources.
        commandList.SetVertexBuffer(0, gameResMgr.VertexBuffer.DeviceBuffer);

        // Execute draw commands from the buffer.
        var commands = renderPlan.GetCommands();
        for (var i = 0; i < commands.Length; ++i)
        {
            var command = commands[i];
            switch (command.RenderCommandType)
            {
                case RenderCommandType.DrawSprites:
                    DrawSprites(commandList, command, renderPlan);
                    break;

                case RenderCommandType.DrawGlobalShadowMap:
                    DrawGlobalShadowMap(commandList, renderPlan);
                    break;

                case RenderCommandType.DrawPointLightShadowMaps:
                    pointLightDepthMapRenderer.Render(commandList, renderPlan);
                    break;

                case RenderCommandType.PushDebug:
                    commandList.PushDebugGroup(command.DebugGroupName!);
                    break;

                case RenderCommandType.PopDebug:
                    commandList.PopDebugGroup();
                    break;
            }
        }

        commandList.PopDebugGroup();
    }

    /// <summary>
    ///     Draws the global shadow map.
    /// </summary>
    /// <param name="commandList">Command list.</param>
    /// <param name="renderPlan">Render plan.</param>
    private void DrawGlobalShadowMap(CommandList commandList, RenderPlan renderPlan)
    {
        ConfigureShadowMapPipeline(commandList);
        commandList.ClearDepthStencil(0.0f);
        commandList.DrawIndexed(renderPlan.GlobalSolidIndexCount);

        // Restore index buffer state when done.
        commandList.SetIndexBuffer(gameResMgr.SpriteIndexBuffer!.DeviceBuffer, IndexFormat.UInt32);
    }

    /// <summary>
    ///     Draws a layer of sprites.
    /// </summary>
    /// <param name="commandList">Command list.</param>
    /// <param name="command">Current draw command.</param>
    /// <param name="renderPlan">Render plan.</param>
    private void DrawSprites(CommandList commandList, RenderCommand command, RenderPlan renderPlan)
    {
        // Do early render passes for per-layer lighting, shadows, etc.
        fullPointLightMapRenderer.Render(commandList, renderPlan, command);

        // Final render pass for layer.
        commandList.PushDebugGroup("Sprites");
        ConfigureSpritesPipeline(commandList, command);
        commandList.DrawIndexed(command.IndexCount, 1, command.BaseIndex, 0, 0);
        commandList.PopDebugGroup();
    }

    /// <summary>
    ///     Configures the rendering pipeline to draw sprites.
    /// </summary>
    private void ConfigureSpritesPipeline(CommandList commandList, RenderCommand command)
    {
        commandList.SetViewport(0, viewport);
        commandList.SetPipeline(
            command.EnableDepthTest ? pipeline.PipelineWithDepthTest.Value : pipeline.Pipeline.Value);
        commandList.SetIndexBuffer(gameResMgr.SpriteIndexBuffer!.DeviceBuffer, IndexFormat.UInt32);
        commandList.SetGraphicsResourceSet(0, resourceSet);
        commandList.SetFramebuffer(device.Device!.SwapchainFramebuffer);
    }

    /// <summary>
    ///     Configures the rendering pipeline to draw the block shadow map.
    /// </summary>
    private void ConfigureShadowMapPipeline(CommandList commandList)
    {
        commandList.SetViewport(0, blockShadowViewport);
        commandList.SetPipeline(pipeline.BlockShadowPipeline.Value);
        commandList.SetIndexBuffer(gameResMgr.SolidIndexBuffer!.DeviceBuffer, IndexFormat.UInt32);
        commandList.SetGraphicsResourceSet(0, blockShadowResourceSet);
        commandList.SetFramebuffer(gameResMgr.ShadowMapFramebuffer);
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
        if (gameResMgr.ShadowMapTexture == null)
            throw new InvalidOperationException("Shadow map texture not ready.");
        if (gameResMgr.FragmentUniformBuffer == null)
            throw new InvalidOperationException("Fragment uniform buffer not ready.");

        resourceSet?.Dispose();

        var resLayoutDesc = new ResourceLayoutDescription(new ResourceLayoutElementDescription(
                GameResourceManager.ResShaderConstants,
                ResourceKind.UniformBuffer,
                ShaderStages.Vertex),
            new ResourceLayoutElementDescription(
                GameResourceManager.ResTextureAtlas,
                ResourceKind.TextureReadOnly,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                GameResourceManager.ResTextureAtlasSampler,
                ResourceKind.Sampler,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                GameResourceManager.ResShadowMapTexture,
                ResourceKind.TextureReadOnly,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                GameResourceManager.ResShadowMapTextureSampler,
                ResourceKind.Sampler,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                GameResourceManager.ResLightMapTexture,
                ResourceKind.TextureReadOnly,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                GameResourceManager.ResLightMapTextureSampler,
                ResourceKind.Sampler,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                GameResourceManager.ResShaderConstants,
                ResourceKind.UniformBuffer,
                ShaderStages.Fragment));
        var resLayout = device.Device.ResourceFactory.CreateResourceLayout(resLayoutDesc);

        var resSetDesc = new ResourceSetDescription(
            resLayout,
            gameResMgr.VertexUniformBuffer.DeviceBuffer,
            resMgr.AtlasTexture.TextureView,
            device.Device.PointSampler,
            gameResMgr.ShadowMapTexture.TextureView,
            device.Device.LinearSampler,
            gameResMgr.FullPointLightMap.Value.TextureView,
            device.Device.LinearSampler,
            gameResMgr.FragmentUniformBuffer.DeviceBuffer
        );
        resourceSet = device.Device.ResourceFactory.CreateResourceSet(resSetDesc);
    }

    /// <summary>
    ///     Creates the bindable resource set.
    /// </summary>
    private void CreateBlockShadowResourceSet()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");
        if (gameResMgr.BlockShadowVertexUniformBuffer == null)
            throw new InvalidOperationException("Vertex uniform buffer not ready.");

        blockShadowResourceSet?.Dispose();

        var resLayoutDesc = new ResourceLayoutDescription(new ResourceLayoutElementDescription(
            GameResourceManager.ResShaderConstants,
            ResourceKind.UniformBuffer,
            ShaderStages.Vertex
        ));
        var resLayout = device.Device.ResourceFactory.CreateResourceLayout(resLayoutDesc);

        var resSetDesc = new ResourceSetDescription(
            resLayout,
            gameResMgr.BlockShadowVertexUniformBuffer.DeviceBuffer
        );
        blockShadowResourceSet = device.Device.ResourceFactory.CreateResourceSet(resSetDesc);
    }
}