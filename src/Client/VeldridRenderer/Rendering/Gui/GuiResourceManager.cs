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
using Hexa.NET.ImGui;
using Sovereign.VeldridRenderer.Rendering.Resources;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Gui;

/// <summary>
///     Manages Veldrid resources for GUI rendering.
/// </summary>
public class GuiResourceManager : IDisposable
{
    /// <summary>
    ///     Size of GUI vertex buffer.
    /// </summary>
    private const int GuiVertexBufferSize = 65536;

    /// <summary>
    ///     Size of GUI index buffer.
    /// </summary>
    private const int GuiIndexBufferSize = 128 * 1024;

    /// <summary>
    ///     Size of GUI uniform buffer.
    /// </summary>
    private const int GuiUniformBufferSize = 1;

    /// <summary>
    ///     Resource name for the shader constants uniform buffer.
    /// </summary>
    public static string RES_SHADER_CONSTANTS = "ShaderConstants";

    /// <summary>
    ///     Resource name for the texture atlas.
    /// </summary>
    public static string RES_TEXTURE_ATLAS = "g_textureAtlas";

    /// <summary>
    ///     Resource name for the texture atlas sampler.
    /// </summary>
    public static string RES_TEXTURE_ATLAS_SAMPLER = "g_textureAtlasSampler";

    private readonly VeldridDevice device;

    public GuiResourceManager(VeldridDevice device)
    {
        this.device = device;
    }

    /// <summary>
    ///     Vertex buffer for GUI rendering.
    /// </summary>
    public VeldridUpdateBuffer<ImDrawVert>? GuiVertexBuffer { get; private set; }

    /// <summary>
    ///     Index buffer for GUI rendering.
    /// </summary>
    public VeldridUpdateBuffer<ushort>? GuiIndexBuffer { get; private set; }

    public VeldridUpdateBuffer<GuiVertexShaderConstants>? GuiUniformBuffer { get; private set; }

    /// <summary>
    ///     Vertex shader for GUI rendering.
    /// </summary>
    public Shader? GuiVertexShader { get; private set; }

    /// <summary>
    ///     Fragment shader for GUI rendering.
    /// </summary>
    public Shader? GuiFragmentShader { get; private set; }

    public void Dispose()
    {
        GuiVertexShader?.Dispose();
        GuiFragmentShader?.Dispose();
        GuiIndexBuffer?.Dispose();
        GuiVertexBuffer?.Dispose();
    }

    /// <summary>
    ///     Initiaizes GUI rendering resources.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the graphics device has not been created yet.</exception>
    public void Initialize()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Tried to create shaders without device.");

        var vertexShaderBytes = device.LoadShaderBytes("Gui.vert.spv");
        var fragmentShaderBytes = device.LoadShaderBytes("Gui.frag.spv");

        var vertexDesc = new ShaderDescription(ShaderStages.Vertex,
            vertexShaderBytes, "main", true);
        GuiVertexShader = device.Device.ResourceFactory.CreateShader(vertexDesc);

        var fragmentDesc = new ShaderDescription(ShaderStages.Fragment,
            fragmentShaderBytes, "main", true);
        GuiFragmentShader = device.Device.ResourceFactory.CreateShader(fragmentDesc);

        GuiVertexBuffer = new VeldridUpdateBuffer<ImDrawVert>(device,
            BufferUsage.VertexBuffer | BufferUsage.Dynamic, GuiVertexBufferSize);
        GuiIndexBuffer = new VeldridUpdateBuffer<ushort>(device,
            BufferUsage.IndexBuffer | BufferUsage.Dynamic, GuiIndexBufferSize);
        GuiUniformBuffer = new VeldridUpdateBuffer<GuiVertexShaderConstants>(device,
            BufferUsage.UniformBuffer, GuiUniformBufferSize, true);
    }

    public void EndFrame()
    {
        GuiVertexBuffer?.EndFrame();
        GuiIndexBuffer?.EndFrame();
        GuiUniformBuffer?.EndFrame();
    }
}