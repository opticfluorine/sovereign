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
    }

    /// <summary>
    ///     Veldrid pipeline for world rendering.
    /// </summary>
    public Pipeline? Pipeline { get; private set; }

    /// <summary>
    ///     Veldrid pipeline for block shadow map rendering.
    /// </summary>
    public Pipeline? BlockShadowPipeline { get; private set; }

    public void Dispose()
    {
        Pipeline?.Dispose();
        BlockShadowPipeline?.Dispose();
    }

    /// <summary>
    ///     Initializes the pipeline.
    /// </summary>
    public void Initialize()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");
        if (gameResMgr.ShadowMapFramebuffer == null)
            throw new InvalidOperationException("Shadow map framebuffer not ready.");

        var pipelineDesc = new GraphicsPipelineDescription(
            BlendStateDescription.SingleAlphaBlend,
            DepthStencilStateDescription.Disabled,
            CreateRasterizerState(),
            PrimitiveTopology.TriangleList,
            CreateShaderSet(),
            CreateResourceLayout(),
            device.Device.SwapchainFramebuffer.OutputDescription
        );

        Pipeline = device.Device.ResourceFactory.CreateGraphicsPipeline(pipelineDesc);

        var shadowMapPipelineDesc = new GraphicsPipelineDescription(
            BlendStateDescription.Empty,
            DepthStencilStateDescription.DepthOnlyLessEqual,
            CreateRasterizerState(),
            PrimitiveTopology.TriangleList,
            CreateBlockShadowShaderSet(),
            CreateBlockShadowResourceLayout(),
            gameResMgr.ShadowMapFramebuffer.OutputDescription);

        BlockShadowPipeline = device.Device.ResourceFactory.CreateGraphicsPipeline(shadowMapPipelineDesc);
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
            FrontFace.Clockwise,
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

        var vertexDesc = new VertexLayoutDescription(
            gameResMgr.VertexBuffer.ElementSize,
            new VertexElementDescription(
                "vPosition",
                VertexElementFormat.Float3,
                VertexElementSemantic.Position
            ), new VertexElementDescription(
                "vVelocity",
                VertexElementFormat.Float3,
                VertexElementSemantic.Position
            ), new VertexElementDescription(
                "vTexCoord",
                VertexElementFormat.Float2,
                VertexElementSemantic.TextureCoordinate
            ));

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

        var vertexDesc = new VertexLayoutDescription(
            gameResMgr.VertexBuffer.ElementSize,
            new VertexElementDescription(
                "vPosition",
                VertexElementFormat.Float3,
                VertexElementSemantic.Position
            ), new VertexElementDescription(
                "vVelocity",
                VertexElementFormat.Float3,
                VertexElementSemantic.Position
            ), new VertexElementDescription(
                "vTexCoord",
                VertexElementFormat.Float2,
                VertexElementSemantic.TextureCoordinate
            ));

        // Create the shader set.
        return new ShaderSetDescription(
            new[] { vertexDesc },
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

        var desc = new ResourceLayoutDescription(new ResourceLayoutElementDescription(
            GameResourceManager.ResShaderConstants,
            ResourceKind.UniformBuffer,
            ShaderStages.Vertex
        ), new ResourceLayoutElementDescription(
            GameResourceManager.ResTextureAtlas,
            ResourceKind.TextureReadOnly,
            ShaderStages.Fragment
        ), new ResourceLayoutElementDescription(
            GameResourceManager.ResTextureAtlasSampler,
            ResourceKind.Sampler,
            ShaderStages.Fragment
        ));
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