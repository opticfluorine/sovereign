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

    private readonly uint[] dynamicOffset = new uint[1];
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

    private readonly WorldPipeline worldPipeline;

    public WorldPointLightDepthMapRenderer(GameResourceManager gameResMgr, VeldridDevice device,
        LightingShaderConstantsUpdater lightingShaderConstantsUpdater, WorldPipeline worldPipeline)
    {
        this.gameResMgr = gameResMgr;
        this.device = device;
        this.lightingShaderConstantsUpdater = lightingShaderConstantsUpdater;
        this.worldPipeline = worldPipeline;

        resourceLayout = new Lazy<ResourceLayout>(() =>
        {
            var layoutDesc = new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ShaderConstants", ResourceKind.UniformBuffer,
                    ShaderStages.Vertex | ShaderStages.Fragment, ResourceLayoutElementOptions.DynamicBinding)
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
                ShaderSet = new ShaderSetDescription([worldPipeline.GetVertexDescription()],
                [
                    gameResMgr.PointLightDepthMapVertexShader.Value, gameResMgr.PointLightDepthMapFragmentShader.Value
                ])
            };
            return device.Device!.ResourceFactory.CreateGraphicsPipeline(desc);
        });
    }

    /// <summary>
    ///     Draws shadow maps for the point light sources.
    /// </summary>
    /// <param name="commandList">Command list.</param>
    /// <param name="renderPlan">Render plan.</param>
    public unsafe void Render(CommandList commandList, RenderPlan renderPlan)
    {
        gameResMgr.PreparePointLights(renderPlan.LightCount);
        lightingShaderConstantsUpdater.UpdateConstants(renderPlan);

        commandList.SetPipeline(pipeline.Value);
        commandList.SetIndexBuffer(gameResMgr.SolidIndexBuffer!.DeviceBuffer, IndexFormat.UInt32);

        for (var i = 0; i < renderPlan.LightCount; ++i)
        {
            var depthMap = gameResMgr.PointLightDepthMaps[i];
            var light = renderPlan.Lights[i];

            for (var j = 0; j < PointLightDepthMap.LayerCount; ++j)
            {
                dynamicOffset[0] = (uint)((2 * i + j) * sizeof(PointLightDepthMapShaderConstants));
                commandList.PushDebugGroup("Point Light Depth Map");
                commandList.SetGraphicsResourceSet(0, resourceSet.Value, dynamicOffset);
                commandList.SetFramebuffer(depthMap.Framebuffers[j]);
                commandList.ClearDepthStencil(1.0f);
                commandList.DrawIndexed(light.IndexCount, 1, light.BaseIndex, 0, 0);
                commandList.PopDebugGroup();
            }
        }
    }
}