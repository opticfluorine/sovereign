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
using Sovereign.ClientCore.Rendering;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Sub-renderer responsible for drawing the point light depth maps.
/// </summary>
public class WorldPointLightDepthMapRenderer
{
    private readonly VeldridDevice device;
    private readonly GameResourceManager gameResMgr;
    private readonly LightingShaderConstantsUpdater lightingShaderConstantsUpdater;

    /// <summary>
    ///     Pipeline shared by all depth maps.
    /// </summary>
    private readonly Lazy<Pipeline> pipeline;

    /// <summary>
    ///     Resource layout shared by all depth maps.
    /// </summary>
    private readonly Lazy<ResourceLayout> resourceLayout;

    /// <summary>
    ///     Resource set shared by all depth maps.
    /// </summary>
    private readonly Lazy<ResourceSet> resourceSet;

    public WorldPointLightDepthMapRenderer(GameResourceManager gameResMgr, VeldridDevice device,
        LightingShaderConstantsUpdater lightingShaderConstantsUpdater)
    {
        this.gameResMgr = gameResMgr;
        this.device = device;
        this.lightingShaderConstantsUpdater = lightingShaderConstantsUpdater;

        resourceLayout = new Lazy<ResourceLayout>(() =>
        {
            var layoutDesc = new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ShaderConstants", ResourceKind.UniformBuffer,
                    ShaderStages.Vertex | ShaderStages.Fragment)
            );
            return device.Device!.ResourceFactory.CreateResourceLayout(layoutDesc);
        });

        resourceSet = new Lazy<ResourceSet>(() =>
        {
            var desc = new ResourceSetDescription(resourceLayout.Value,
                gameResMgr.PointLightDepthMapUniformBuffer!.DeviceBuffer);
            return device.Device!.ResourceFactory.CreateResourceSet(desc);
        });

        pipeline = new Lazy<Pipeline>(() =>
        {
            var desc = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.Empty,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                RasterizerState = new RasterizerStateDescription
                {
                    CullMode = FaceCullMode.Back,
                    DepthClipEnabled = false,
                    FillMode = PolygonFillMode.Solid,
                    FrontFace = FrontFace.CounterClockwise,
                    ScissorTestEnabled = false
                },
                ResourceBindingModel = ResourceBindingModel.Default,
                ResourceLayouts = [resourceLayout.Value],
                Outputs = new OutputDescription(new OutputAttachmentDescription(PixelFormat.R32_Float)),
                ShaderSet = new ShaderSetDescription()
            };
            return device.Device!.ResourceFactory.CreateGraphicsPipeline(desc);
        });
    }

    /// <summary>
    ///     Draws shadow maps for the point light sources.
    /// </summary>
    /// <param name="commandList">Command list.</param>
    /// <param name="renderPlan">Render plan.</param>
    public void Render(CommandList commandList, RenderPlan renderPlan)
    {
        // Load light information into the buffers.
        gameResMgr.PreparePointLights(renderPlan.LightCount);
        lightingShaderConstantsUpdater.UpdateConstats(renderPlan);

        // Render depth maps for point lights.
    }
}