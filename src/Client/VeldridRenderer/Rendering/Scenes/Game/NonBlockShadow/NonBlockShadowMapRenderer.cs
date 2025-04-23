// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game.NonBlockShadow;

/// <summary>
///     Responsible for rendering the non-block-entity shadow map.
/// </summary>
public class NonBlockShadowMapRenderer : IDisposable
{
    private const string VertexShaderFilename = "NonBlockShadow.vert.spv";
    private const string FragmentShaderFilename = "NonBlockShadow.frag.spv";
    private readonly uint[] dynamicOffsets = new uint[1];
    private readonly Lazy<Shader> fragmentShader;
    private readonly Lazy<Pipeline> pipeline;
    private readonly GameResourceManager resMgr;
    private readonly Lazy<ResourceLayout> resourceLayout;
    private readonly Lazy<ResourceSet> resourceSet;
    private readonly NonBlockShadowMap shadowMap;

    private readonly Lazy<Shader> vertexShader;
    private readonly WorldPipeline worldPipeline;

    public NonBlockShadowMapRenderer(NonBlockShadowMap shadowMap, VeldridDevice device, GameResourceManager resMgr,
        WorldPipeline worldPipeline)
    {
        this.shadowMap = shadowMap;
        this.resMgr = resMgr;
        this.worldPipeline = worldPipeline;
        vertexShader = new Lazy<Shader>(() =>
        {
            var bytes = device.LoadShaderBytes(VertexShaderFilename);
            var desc = new ShaderDescription(ShaderStages.Vertex, bytes, "main", true);
            return device.Device!.ResourceFactory.CreateShader(desc);
        });

        fragmentShader = new Lazy<Shader>(() =>
        {
            var bytes = device.LoadShaderBytes(FragmentShaderFilename);
            var desc = new ShaderDescription(ShaderStages.Fragment, bytes, "main", true);
            return device.Device!.ResourceFactory.CreateShader(desc);
        });

        resourceLayout = new Lazy<ResourceLayout>(() =>
        {
            var desc = new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    nameof(NonBlockShadowShaderConstants),
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex)
            );
            return device.Device!.ResourceFactory.CreateResourceLayout(desc);
        });

        resourceSet = new Lazy<ResourceSet>(() =>
        {
            var desc = new ResourceSetDescription(resourceLayout.Value,
                shadowMap.ConstantsBuffer.Value.DeviceBuffer);
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
                    CullMode = FaceCullMode.None,
                    DepthClipEnabled = false,
                    FillMode = PolygonFillMode.Solid,
                    FrontFace = FrontFace.CounterClockwise,
                    ScissorTestEnabled = false
                },
                ResourceBindingModel = ResourceBindingModel.Default,
                ResourceLayouts = [resourceLayout.Value],
                Outputs = new OutputDescription(new OutputAttachmentDescription(PixelFormat.R32_Float)),
                ShaderSet = new ShaderSetDescription([worldPipeline.GetVertexDescription()],
                    [vertexShader.Value, fragmentShader.Value])
            };
            return device.Device!.ResourceFactory.CreateGraphicsPipeline(desc);
        });
    }

    public void Dispose()
    {
        if (pipeline.IsValueCreated) pipeline.Value.Dispose();
        if (resourceSet.IsValueCreated) resourceSet.Value.Dispose();
        if (resourceLayout.IsValueCreated) resourceLayout.Value.Dispose();
        if (fragmentShader.IsValueCreated) fragmentShader.Value.Dispose();
        if (vertexShader.IsValueCreated) vertexShader.Value.Dispose();
    }

    /// <summary>
    ///     Renders the non-block shadow map.
    /// </summary>
    /// <param name="commandList">Command list.</param>
    /// <param name="renderPlan">Render plan.</param>
    public void Render(CommandList commandList, RenderPlan renderPlan)
    {
        commandList.PushDebugGroup("Non-Block Shadow Map");

        commandList.SetIndexBuffer(resMgr.ShadowIndexBuffer.Value.DeviceBuffer, IndexFormat.UInt32);
        commandList.SetPipeline(pipeline.Value);
        commandList.SetGraphicsResourceSet(0, resourceSet.Value);
        commandList.SetFramebuffer(shadowMap.Framebuffer.Value);

        commandList.ClearDepthStencil(1.0f);
        commandList.DrawIndexed((uint)renderPlan.ShadowIndexCount);

        commandList.PopDebugGroup();
    }
}