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
using Sovereign.ClientCore.Rendering.Resources.Buffers;
using Sovereign.VeldridRenderer.Rendering.Resources;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
/// Manages rendering resources for the game scene.
/// </summary>
public class GameResourceManager : IDisposable
{

    /// <summary>
    /// Resource name for the shader constants uniform buffer.
    /// </summary>
    public static string RES_SHADER_CONSTANTS = "ShaderConstants";

    /// <summary>
    /// Resource name for the texture atlas.
    /// </summary>
    public static string RES_TEXTURE_ATLAS = "g_textureAtlas";

    /// <summary>
    /// Resource name for the texture atlas sampler.
    /// </summary>
    public static string RES_TEXTURE_ATLAS_SAMPLER = "g_textureAtlasSampler";

    /// <summary>
    /// Maximum number of draws.
    /// </summary>
    public const int MaximumDraws = 1024;

    /// <summary>
    /// Maximum number of vertices in the vertex buffer.
    /// </summary>
    public const int MaximumVertices = 16384;

    /// <summary>
    /// Maximum number of indices in the index buffer.
    /// </summary>
    public const int MaximumIndices = 24576;

    /// <summary>
    /// Updateable vertex buffer.
    /// </summary>
    public VeldridUpdateBuffer<WorldVertex> VertexBuffer { get; private set; }

    /// <summary>
    /// Index buffer into the vertex buffer.
    /// </summary>
    public VeldridUpdateBuffer<uint> IndexBuffer { get; private set; }

    /// <summary>
    /// Uniform buffer for the vertex shader.
    /// </summary>
    public VeldridUpdateBuffer<WorldVertexShaderConstants> VertexUniformBuffer { get; private set; }

    /// <summary>
    /// Number of elements to use in each draw.
    /// </summary>
    public int[] DrawBuffer { get; private set; }

    /// <summary>
    /// Number of draws to be performed.
    /// </summary>
    public int DrawCount { get; set; }

    /// <summary>
    /// Vertex shader for game world rendering.
    /// </summary>
    public Shader WorldVertexShader { get; private set; }

    /// <summary>
    /// Fragment shader for game world rendering.
    /// </summary>
    public Shader WorldFragmentShader { get; private set; }

    private VeldridDevice device;

    /// <summary>
    /// Dispose flag.
    /// </summary>
    private bool isDisposed = false;

    public GameResourceManager(VeldridDevice device)
    {
        this.device = device;
        DrawBuffer = new int[MaximumDraws];
    }

    /// <summary>
    /// Initializes the resources.
    /// </summary>
    public void Initialize()
    {
        /* Create buffers. */
        VertexBuffer = new VeldridUpdateBuffer<WorldVertex>(device,
            BufferUsage.VertexBuffer, MaximumVertices);

        IndexBuffer = new VeldridUpdateBuffer<uint>(device,
            BufferUsage.IndexBuffer, MaximumIndices);

        VertexUniformBuffer = new VeldridUpdateBuffer<WorldVertexShaderConstants>(device,
            BufferUsage.UniformBuffer, 1);

        /* Load resources. */
        LoadWorldShaders();
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            WorldFragmentShader?.Dispose();
            WorldVertexShader?.Dispose();
            VertexUniformBuffer?.Dispose();
            IndexBuffer?.Dispose();
            VertexBuffer?.Dispose();
            isDisposed = true;
        }
    }

    /// <summary>
    /// Updates all resource buffers.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    public void UpdateBuffers(CommandList commandList)
    {
        VertexBuffer.Update(commandList);
        IndexBuffer.Update(commandList);
        VertexUniformBuffer.Update(commandList);
    }

    private void LoadWorldShaders()
    {
        // Load shader bytes.
        var vertexShaderBytes = device.LoadShaderBytes("World.vert.spv");
        var fragmentShaderBytes = device.LoadShaderBytes("World.frag.spv");

        // World vertex shader.
        var vertexDesc = new ShaderDescription(ShaderStages.Vertex,
            vertexShaderBytes, "main", true); // TODO control debug flag from config
        WorldVertexShader = device.Device.ResourceFactory.CreateShader(vertexDesc);

        // World fragment shader.
        var fragmentDesc = new ShaderDescription(ShaderStages.Fragment,
            fragmentShaderBytes, "main", true); // TODO control debug flag from config
        WorldFragmentShader = device.Device.ResourceFactory.CreateShader(fragmentDesc);
    }

}
