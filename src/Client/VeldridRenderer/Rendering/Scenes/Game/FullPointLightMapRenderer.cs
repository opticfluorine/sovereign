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
using System.Collections.Generic;
using System.Numerics;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Renderer for the per-layer full point light map.
/// </summary>
public class FullPointLightMapRenderer : IDisposable
{
    private readonly ClientConfigurationManager configManager;
    private readonly VeldridDevice device;
    private readonly DisplayViewport displayViewport;
    private readonly Lazy<Framebuffer> framebuffer;
    private readonly GameResourceManager gameResMgr;
    private readonly uint[] offsets = new uint[1];
    private readonly Lazy<Pipeline> pipeline;
    private readonly Lazy<ResourceLayout> resourceLayout;
    private readonly List<ResourceSet> resourceSets = new();
    private readonly Lazy<Sampler> shadowSampler;

    public FullPointLightMapRenderer(VeldridDevice device, GameResourceManager gameResMgr,
        DisplayViewport displayViewport, WorldPipeline worldPipeline, ClientConfigurationManager configManager)
    {
        this.device = device;
        this.gameResMgr = gameResMgr;
        this.displayViewport = displayViewport;
        this.configManager = configManager;

        framebuffer = new Lazy<Framebuffer>(() =>
        {
            var desc = new FramebufferDescription(null, gameResMgr.FullPointLightMap.Value.Texture);
            return device.Device!.ResourceFactory.CreateFramebuffer(desc);
        });

        shadowSampler = new Lazy<Sampler>(() =>
        {
            var desc = new SamplerDescription(SamplerAddressMode.Clamp, SamplerAddressMode.Clamp,
                SamplerAddressMode.Clamp,
                SamplerFilter.MinLinear_MagLinear_MipLinear, ComparisonKind.LessEqual, 0, 1, 1,
                0, SamplerBorderColor.OpaqueBlack);
            return device.Device!.ResourceFactory.CreateSampler(desc);
        });

        resourceLayout = new Lazy<ResourceLayout>(() =>
        {
            var desc = new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(nameof(PointLightShaderConstants),
                    ResourceKind.StructuredBufferReadOnly, ShaderStages.Vertex | ShaderStages.Fragment,
                    ResourceLayoutElementOptions.DynamicBinding),
                new ResourceLayoutElementDescription("g_depthMap", ResourceKind.TextureReadOnly,
                    ShaderStages.Fragment),
                new ResourceLayoutElementDescription("g_sampler", ResourceKind.Sampler, ShaderStages.Fragment)
            );
            return device.Device!.ResourceFactory.CreateResourceLayout(desc);
        });

        pipeline = new Lazy<Pipeline>(() =>
        {
            var desc = new GraphicsPipelineDescription(
                new BlendStateDescription(RgbaFloat.Clear, false,
                    new BlendAttachmentDescription(true,
                        BlendFactor.One, BlendFactor.One, BlendFunction.Add, BlendFactor.One, BlendFactor.One,
                        BlendFunction.Add)),
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, false,
                    false),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription([worldPipeline.GetVertexDescription()],
                    [gameResMgr.FullPointLightMapVertexShader.Value, gameResMgr.FullPointLightMapFragmentShader.Value]),
                resourceLayout.Value,
                framebuffer.Value.OutputDescription);
            return device.Device!.ResourceFactory.CreateGraphicsPipeline(desc);
        });
    }

    public void Dispose()
    {
        foreach (var resourceSet in resourceSets) resourceSet.Dispose();
        if (pipeline.IsValueCreated) pipeline.Value.Dispose();
        if (resourceLayout.IsValueCreated) resourceLayout.Value.Dispose();
        if (framebuffer.IsValueCreated) framebuffer.Value.Dispose();
        if (shadowSampler.IsValueCreated) shadowSampler.Value.Dispose();
    }

    /// <summary>
    ///     Renders the full point light map for the current layer.
    /// </summary>
    /// <param name="commandList">Command list.</param>
    /// <param name="renderPlan">Render plan.</param>
    public void Render(CommandList commandList, RenderPlan renderPlan, RenderCommand command)
    {
        commandList.PushDebugGroup("Light Map");

        commandList.SetFramebuffer(framebuffer.Value);
        commandList.SetPipeline(pipeline.Value);
        commandList.ClearColorTarget(0, RgbaFloat.Clear);

        for (var i = 0; i < renderPlan.LightCount; ++i)
        {
            var light = renderPlan.Lights[i];

            if (resourceSets.Count <= i)
            {
                var desc = new ResourceSetDescription(resourceLayout.Value,
                    gameResMgr.PointLightBuffer!.DeviceBuffer,
                    gameResMgr.PointLightDepthMaps[i].Texture,
                    shadowSampler.Value);
                resourceSets.Add(device.Device!.ResourceFactory.CreateResourceSet(desc));
            }

            offsets[0] = gameResMgr.PointLightBuffer!.GetOffset(i);
            commandList.SetGraphicsResourceSet(0, resourceSets[i], offsets);
            UpdateViewport(commandList, light, renderPlan.CameraPosition);

            commandList.DrawIndexed(command.IndexCount, 1, command.BaseIndex, 0, 0);

            commandList.PopDebugGroup();
        }
    }

    /// <summary>
    ///     Updates the viewport for the given light.
    /// </summary>
    /// <param name="commandList">Command list.</param>
    /// <param name="light">Light.</param>
    /// <param name="cameraPos">Camera position.</param>
    private void UpdateViewport(CommandList commandList, RenderLight light, Vector3 cameraPos)
    {
        var lightRelativePos = light.Light.Position - cameraPos;
        var radius = light.Light.Details.Radius;
        var tileWidth = configManager.ClientConfiguration.TileWidth;
        var scaleX = (float)device.DisplayMode!.Width / displayViewport.Width;
        var scaleY = (float)device.DisplayMode!.Height / displayViewport.Height;

        var width = 2.0f * radius * tileWidth * scaleX;
        var height = 2.0f * radius * tileWidth * scaleY;

        var lightX = lightRelativePos.X * tileWidth * scaleX + 0.5f * device.DisplayMode!.Width;
        var lightY = lightRelativePos.Y * tileWidth * scaleY + 0.5f * device.DisplayMode!.Height;

        var x = lightX - radius * tileWidth * scaleX;
        var y = lightY - radius * tileWidth * scaleY;

        commandList.SetViewport(0, new Viewport(x, y, width, height, 0.0f, 1.0f));
    }
}