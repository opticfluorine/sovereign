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
    public Pipeline Pipeline { get; private set; }

    public void Dispose()
    {
        Pipeline?.Dispose();
    }

    /// <summary>
    ///     Initializes the pipeline.
    /// </summary>
    public void Initialize()
    {
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
    ///     Creates the resource layout for the world rendering pipeline.
    /// </summary>
    /// <returns>Resource layout for the world rendering pipeline.</returns>
    private ResourceLayout CreateResourceLayout()
    {
        var desc = new ResourceLayoutDescription(new ResourceLayoutElementDescription(
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
        return device.Device.ResourceFactory.CreateResourceLayout(desc);
    }
}