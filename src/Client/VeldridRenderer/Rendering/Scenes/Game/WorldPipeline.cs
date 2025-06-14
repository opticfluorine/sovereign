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
using Sovereign.VeldridRenderer.Rendering.Scenes.Game.NonBlockShadow;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Manages the rendering pipeline for game world rendering.
/// </summary>
public class WorldPipeline : IDisposable
{
    private readonly VeldridDevice device;
    private readonly GameResourceManager gameResMgr;

    public WorldPipeline(VeldridDevice device, GameResourceManager gameResMgr)
    {
        this.device = device;
        this.gameResMgr = gameResMgr;

        ResourceLayout = new Lazy<ResourceLayout>(CreateResourceLayout);
        BlockShadowResourceLayout = new Lazy<ResourceLayout>(CreateBlockShadowResourceLayout);

        Pipeline = new Lazy<Pipeline>(() =>
        {
            var pipelineDesc = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.DepthOnlyLessEqualRead,
                CreateRasterizerState(),
                PrimitiveTopology.TriangleList,
                CreateShaderSet(),
                ResourceLayout.Value,
                device.Device!.SwapchainFramebuffer.OutputDescription
            );

            return device.Device.ResourceFactory.CreateGraphicsPipeline(pipelineDesc);
        });

        PipelineWithDepthWrite = new Lazy<Pipeline>(() =>
        {
            var pipelineDesc = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                CreateRasterizerState(),
                PrimitiveTopology.TriangleList,
                CreateShaderSet(),
                ResourceLayout.Value,
                device.Device!.SwapchainFramebuffer.OutputDescription
            );

            return device.Device.ResourceFactory.CreateGraphicsPipeline(pipelineDesc);
        });

        BlockShadowPipeline = new Lazy<Pipeline>(() =>
        {
            var shadowMapPipelineDesc = new GraphicsPipelineDescription(
                BlendStateDescription.Empty,
                DepthStencilStateDescription.DepthOnlyGreaterEqual,
                CreateBlockShadowRasterizerState(),
                PrimitiveTopology.TriangleList,
                CreateBlockShadowShaderSet(),
                BlockShadowResourceLayout.Value,
                gameResMgr.ShadowMapFramebuffer!.OutputDescription);

            return device.Device!.ResourceFactory.CreateGraphicsPipeline(shadowMapPipelineDesc);
        });
    }

    /// <summary>
    ///     Resource layout for the block shadow map rendering pipeline.
    /// </summary>
    public Lazy<ResourceLayout> BlockShadowResourceLayout { get; }

    /// <summary>
    ///     Veldrid pipeline for world rendering.
    /// </summary>
    public Lazy<Pipeline> Pipeline { get; }

    public Lazy<Pipeline> PipelineWithDepthWrite { get; }

    /// <summary>
    ///     Veldrid pipeline for block shadow map rendering.
    /// </summary>
    public Lazy<Pipeline> BlockShadowPipeline { get; }

    /// <summary>
    ///     Resource layout used by the world rendering pipeline.
    /// </summary>
    public Lazy<ResourceLayout> ResourceLayout { get; }

    public void Dispose()
    {
        if (Pipeline.IsValueCreated) Pipeline.Value.Dispose();
        if (PipelineWithDepthWrite.IsValueCreated) PipelineWithDepthWrite.Value.Dispose();
        if (BlockShadowPipeline.IsValueCreated) BlockShadowPipeline.Value.Dispose();
        if (ResourceLayout.IsValueCreated) ResourceLayout.Value.Dispose();
        if (BlockShadowResourceLayout.IsValueCreated) BlockShadowResourceLayout.Value.Dispose();
    }

    /// <summary>
    ///     Gets the vertex description shared by various world rendering pipelines.
    /// </summary>
    /// <returns>Vertex description.</returns>
    public VertexLayoutDescription GetVertexDescription()
    {
        return new VertexLayoutDescription(
            gameResMgr.VertexBuffer!.ElementSize,
            new VertexElementDescription(
                "position",
                VertexElementFormat.Float3,
                VertexElementSemantic.Position
            ), new VertexElementDescription(
                "velocity",
                VertexElementFormat.Float3,
                VertexElementSemantic.Position
            ), new VertexElementDescription(
                "texCoord",
                VertexElementFormat.Float2,
                VertexElementSemantic.TextureCoordinate
            ), new VertexElementDescription(
                "lightFactor",
                VertexElementFormat.Float1,
                VertexElementSemantic.Color
            ), new VertexElementDescription(
                "shadowFloor",
                VertexElementFormat.Float1,
                VertexElementSemantic.Color
            ), new VertexElementDescription(
                "opacity",
                VertexElementFormat.Float1,
                VertexElementSemantic.Color
            ));
    }

    /// <summary>
    ///     Creates the rasterizer state for the world rendering pipeline.
    /// </summary>
    /// <returns>Rasterizer state.</returns>
    private RasterizerStateDescription CreateRasterizerState()
    {
        return new RasterizerStateDescription(
            FaceCullMode.None,
            PolygonFillMode.Solid,
            FrontFace.CounterClockwise,
            false,
            false);
    }

    /// <summary>
    ///     Creates the rasterizer state for the shadow map.
    /// </summary>
    /// <returns>Rasterizer state.</returns>
    private RasterizerStateDescription CreateBlockShadowRasterizerState()
    {
        return new RasterizerStateDescription(
            FaceCullMode.None,
            PolygonFillMode.Solid,
            FrontFace.CounterClockwise,
            false,
            false);
    }

    /// <summary>
    ///     Creates the shader set for the world rendering pipeline.
    /// </summary>
    /// <returns>Shader set for the world rendering pipeline.</returns>
    private ShaderSetDescription CreateShaderSet()
    {
        // Describe the vertex format.
        if (gameResMgr.VertexBuffer == null)
            throw new InvalidOperationException("Vertex buffer not ready.");

        var vertexDesc = GetVertexDescription();

        // Create the shader set.
        return new ShaderSetDescription(
            new[] { vertexDesc },
            new[]
            {
                gameResMgr.WorldVertexShader,
                gameResMgr.WorldFragmentShader
            }
        );
    }

    /// <summary>
    ///     Creates the block shadow mapping shader set.
    /// </summary>
    /// <returns>Block shadow mapping shader set.</returns>
    private ShaderSetDescription CreateBlockShadowShaderSet()
    {
        // Describe the vertex format.
        if (gameResMgr.VertexBuffer == null)
            throw new InvalidOperationException("Vertex buffer not ready.");

        // Create the shader set.
        return new ShaderSetDescription(
            new[] { GetVertexDescription() },
            new[]
            {
                gameResMgr.BlockShadowVertexShader,
                gameResMgr.BlockShadowFragmentShader
            }
        );
    }

    /// <summary>
    ///     Creates the resource layout for the world rendering pipeline.
    /// </summary>
    /// <returns>Resource layout for the world rendering pipeline.</returns>
    private ResourceLayout CreateResourceLayout()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");

        var desc = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                GameResourceManager.ResShaderConstants,
                ResourceKind.UniformBuffer,
                ShaderStages.Vertex),
            new ResourceLayoutElementDescription(
                nameof(NonBlockShadowShaderConstants),
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
                GameResourceManager.ResNonBlockShadowMapTexture,
                ResourceKind.TextureReadOnly,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                GameResourceManager.ResNonBlockShadowMapTextureSampler,
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

        return device.Device.ResourceFactory.CreateResourceLayout(desc);
    }

    /// <summary>
    ///     Creates the resource layout for the block shadow map rendering.
    /// </summary>
    /// <returns>Resource layout for the block shadow map rendering.</returns>
    private ResourceLayout CreateBlockShadowResourceLayout()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");

        var desc = new ResourceLayoutDescription(new ResourceLayoutElementDescription(
            GameResourceManager.ResShaderConstants,
            ResourceKind.UniformBuffer,
            ShaderStages.Vertex
        ));
        return device.Device.ResourceFactory.CreateResourceLayout(desc);
    }
}