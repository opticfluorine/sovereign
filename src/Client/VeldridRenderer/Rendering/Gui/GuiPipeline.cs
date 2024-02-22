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
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Gui;

public class GuiPipeline : IDisposable
{
    private readonly VeldridDevice device;
    private readonly GuiResourceManager guiResourceManager;

    public GuiPipeline(VeldridDevice device, GuiResourceManager guiResourceManager)
    {
        this.device = device;
        this.guiResourceManager = guiResourceManager;
    }

    /// <summary>
    ///     Veldrid pipeline for GUI rendering.
    /// </summary>
    public Pipeline? Pipeline { get; private set; }

    public void Dispose()
    {
        Pipeline?.Dispose();
    }

    public void Initialize()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");

        var pipelineDesc = new GraphicsPipelineDescription(
            BlendStateDescription.SingleAlphaBlend,
            DepthStencilStateDescription.Disabled,
            CreateRasterizerState(),
            PrimitiveTopology.TriangleList,
            CreateShaderSet(),
            CreateResourceLayout(),
            device.Device.SwapchainFramebuffer.OutputDescription);

        Pipeline = device.Device.ResourceFactory.CreateGraphicsPipeline(pipelineDesc);
    }

    /// <summary>
    ///     Creates the rasterizer state for the GUI rendering pipeline.
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
    ///     Creates the shader set for the GUI rendering pipeline.
    /// </summary>
    /// <returns>Shader set for the world rendering pipeline.</returns>
    private ShaderSetDescription CreateShaderSet()
    {
        // Describe the vertex format.
        if (guiResourceManager.GuiVertexBuffer == null)
            throw new InvalidOperationException("Vertex buffer not ready.");

        var vertexDesc = new VertexLayoutDescription(
            guiResourceManager.GuiVertexBuffer.ElementSize,
            new VertexElementDescription(
                "vPosition",
                VertexElementFormat.Float2,
                VertexElementSemantic.Position
            ),
            new VertexElementDescription(
                "vTexCoord",
                VertexElementFormat.Float2,
                VertexElementSemantic.TextureCoordinate
            ),
            new VertexElementDescription(
                "vColor",
                VertexElementFormat.Byte4_Norm,
                VertexElementSemantic.Color));

        // Create the shader set.
        return new ShaderSetDescription(
            new[] { vertexDesc },
            new[]
            {
                guiResourceManager.GuiVertexShader,
                guiResourceManager.GuiFragmentShader
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
            GuiResourceManager.RES_SHADER_CONSTANTS,
            ResourceKind.UniformBuffer,
            ShaderStages.Vertex
        ), new ResourceLayoutElementDescription(
            GuiResourceManager.RES_TEXTURE_ATLAS,
            ResourceKind.TextureReadOnly,
            ShaderStages.Fragment
        ), new ResourceLayoutElementDescription(
            GuiResourceManager.RES_TEXTURE_ATLAS_SAMPLER,
            ResourceKind.Sampler,
            ShaderStages.Fragment
        ));
        return device.Device.ResourceFactory.CreateResourceLayout(desc);
    }
}