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
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Renderer for the per-layer full point light map.
/// </summary>
public class FullPointLightMapRenderer : IDisposable
{
    private readonly VeldridDevice device;
    private readonly DisplayViewport displayViewport;
    private readonly Lazy<Framebuffer> framebuffer;
    private readonly GameResourceManager gameResMgr;
    private readonly uint[] offsets = new uint[1];
    private readonly Lazy<Pipeline> pipeline;
    private readonly Lazy<ResourceSet> resourceSet;
    private readonly List<Viewport> viewports = new();

    public FullPointLightMapRenderer(VeldridDevice device, GameResourceManager gameResMgr,
        DisplayViewport displayViewport)
    {
        this.device = device;
        this.gameResMgr = gameResMgr;
        this.displayViewport = displayViewport;

        framebuffer = new Lazy<Framebuffer>(() =>
        {
            var desc = new FramebufferDescription(null, gameResMgr.FullPointLightMap.Value.Texture);
            return device.Device!.ResourceFactory.CreateFramebuffer(desc);
        });

        Lazy<ResourceLayout> resourceLayout = new(() =>
        {
            var desc = new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(nameof(PointLightShaderConstants),
                    ResourceKind.StructuredBufferReadOnly, ShaderStages.Vertex | ShaderStages.Fragment,
                    ResourceLayoutElementOptions.DynamicBinding),
                new ResourceLayoutElementDescription(nameof(PointLightDepthMap), ResourceKind.TextureReadOnly,
                    ShaderStages.Fragment)
            );
            return device.Device!.ResourceFactory.CreateResourceLayout(desc);
        });

        resourceSet = new Lazy<ResourceSet>(() =>
        {
            var desc = new ResourceSetDescription(resourceLayout.Value,
                gameResMgr.PointLightBuffer!.DeviceBuffer);
            return device.Device!.ResourceFactory.CreateResourceSet(desc);
        });

        pipeline = new Lazy<Pipeline>(() =>
        {
            var desc = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAdditiveBlend,
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, false,
                    false),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription([], []),
                resourceLayout.Value,
                framebuffer.Value.OutputDescription);
            return device.Device!.ResourceFactory.CreateGraphicsPipeline(desc);
        });
    }

    public void Dispose()
    {
    }

    /// <summary>
    ///     Renders the full point light map for the current layer.
    /// </summary>
    /// <param name="commandList">Command list.</param>
    /// <param name="renderPlan">Render plan.</param>
    /// <param name="cameraPos">Camera position.</param>
    public void Render(CommandList commandList, RenderPlan renderPlan, Vector3 cameraPos)
    {
        commandList.SetFramebuffer(framebuffer.Value);
        commandList.SetPipeline(pipeline.Value);
        commandList.ClearColorTarget(0, RgbaFloat.Clear);

        for (var i = 0; i < renderPlan.LightCount; ++i)
        {
            var light = renderPlan.Lights[i];

            offsets[0] = gameResMgr.PointLightBuffer!.GetOffset(i);
            commandList.SetGraphicsResourceSet(0, resourceSet.Value, offsets);
            UpdateViewport(commandList, light, cameraPos);
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
        var halfRadius = 0.5f * light.Light.Details.Radius;
        var x = ((lightRelativePos.X - halfRadius) / displayViewport.WidthInTiles + 0.5f) * device.DisplayMode!.Width;
        var y = ((lightRelativePos.Y - halfRadius) / displayViewport.HeightInTiles + 0.5f) * device.DisplayMode!.Height;
        var width = light.Light.Details.Radius / displayViewport.WidthInTiles * device.DisplayMode!.Width;
        var height = light.Light.Details.Radius / displayViewport.HeightInTiles * device.DisplayMode!.Height;

        commandList.SetViewport(0, new Viewport(x, y, width, height, 0.0f, 1.0f));
    }
}