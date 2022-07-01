/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
/// Manages the rendering pipeline for game world rendering.
/// </summary>
public class WorldPipeline : IDisposable
{
    private readonly VeldridDevice device;
    private readonly GameResourceManager gameResMgr;

    /// <summary>
    /// Veldrid pipeline for world rendering.
    /// </summary>
    public Pipeline Pipeline { get; private set; }

    public WorldPipeline(VeldridDevice device, GameResourceManager gameResMgr)
    {
        this.device = device;
        this.gameResMgr = gameResMgr;
    }

    /// <summary>
    /// Initializes the pipeline.
    /// </summary>
    public void Initialize()
    {
        var pipelineDesc = new GraphicsPipelineDescription(
            blendState: BlendStateDescription.SingleAlphaBlend,
            depthStencilStateDescription: DepthStencilStateDescription.Disabled,
            rasterizerState: CreateRasterizerState(),
            primitiveTopology: PrimitiveTopology.TriangleList,
            shaderSet: CreateShaderSet(),
            resourceLayout: CreateResourceLayout(),
            outputs: device.Device.SwapchainFramebuffer.OutputDescription
        );

        Pipeline = device.Device.ResourceFactory.CreateGraphicsPipeline(pipelineDesc);
    }

    /// <summary>
    /// Creates the rasterizer state for the world rendering pipeline.
    /// </summary>
    /// <returns>Rasterizer state.</returns>
    private RasterizerStateDescription CreateRasterizerState()
    {
        return new RasterizerStateDescription(
            cullMode: FaceCullMode.None,
            fillMode: PolygonFillMode.Solid,
            frontFace: FrontFace.Clockwise,
            depthClipEnabled: false,
            scissorTestEnabled: false);
    }

    /// <summary>
    /// Creates the shader set for the world rendering pipeline.
    /// </summary>
    /// <returns>Shader set for the world rendering pipeline.</returns>
    private ShaderSetDescription CreateShaderSet()
    {
        // Describe the vertex format.
        var vertexDesc = new VertexLayoutDescription(
            gameResMgr.VertexBuffer.ElementSize,
            new VertexElementDescription[]
            {
                new VertexElementDescription(
                    "vPosition",
                    VertexElementFormat.Float3,
                    VertexElementSemantic.Position
                ),
                new VertexElementDescription(
                    "vVelocity",
                    VertexElementFormat.Float3,
                    VertexElementSemantic.Position
                ),
                new VertexElementDescription(
                    "vTexCoord",
                    VertexElementFormat.Float2,
                    VertexElementSemantic.TextureCoordinate
                )
            }
        );

        // Create the shader set.
        return new ShaderSetDescription(
            new VertexLayoutDescription[] { vertexDesc },
            new Shader[]
            {
                gameResMgr.WorldVertexShader,
                gameResMgr.WorldFragmentShader
            }
        );
    }

    /// <summary>
    /// Creates the resource layout for the world rendering pipeline.
    /// </summary>
    /// <returns>Resource layout for the world rendering pipeline.</returns>
    private ResourceLayout CreateResourceLayout()
    {
        var desc = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription[]
            {
                new ResourceLayoutElementDescription(
                    GameResourceManager.RES_SHADER_CONSTANTS,
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex
                ),
                new ResourceLayoutElementDescription(
                    GameResourceManager.RES_TEXTURE_ATLAS,
                    ResourceKind.TextureReadOnly,
                    ShaderStages.Fragment
                ),
                new ResourceLayoutElementDescription(
                    GameResourceManager.RES_TEXTURE_ATLAS_SAMPLER,
                    ResourceKind.Sampler,
                    ShaderStages.Fragment
                )
            }
        );
        return device.Device.ResourceFactory.CreateResourceLayout(desc);
    }

    public void Dispose()
    {
        Pipeline?.Dispose();
    }

}
