﻿/*
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
using Sovereign.ClientCore.Rendering.Resources.Buffers;
using Sovereign.VeldridRenderer.Rendering.Resources;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Manages rendering resources for the game scene.
/// </summary>
public class GameResourceManager : IDisposable
{
    /// <summary>
    ///     Maximum number of draws.
    /// </summary>
    public const int MaximumDraws = 1024;

    /// <summary>
    ///     Maximum number of vertices in the vertex buffer.
    /// </summary>
    public const int MaximumVertices = 262144;

    /// <summary>
    ///     Maximum number of indices in the index buffer.
    /// </summary>
    public const int MaximumIndices = 262144;

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

    /// <summary>
    ///     Dispose flag.
    /// </summary>
    private bool isDisposed;

    public GameResourceManager(VeldridDevice device)
    {
        this.device = device;
        DrawBuffer = new int[MaximumDraws];
    }

    /// <summary>
    ///     Updateable vertex buffer.
    /// </summary>
    public VeldridUpdateBuffer<WorldVertex>? VertexBuffer { get; private set; }

    /// <summary>
    ///     Index buffer into the vertex buffer.
    /// </summary>
    public VeldridUpdateBuffer<uint>? IndexBuffer { get; private set; }

    /// <summary>
    ///     Uniform buffer for the vertex shader.
    /// </summary>
    public VeldridUpdateBuffer<WorldVertexShaderConstants>? VertexUniformBuffer { get; private set; }

    /// <summary>
    ///     Number of elements to use in each draw.
    /// </summary>
    public int[] DrawBuffer { get; private set; }

    /// <summary>
    ///     Number of draws to be performed.
    /// </summary>
    public int DrawCount { get; set; }

    /// <summary>
    ///     Vertex shader for game world rendering.
    /// </summary>
    public Shader? WorldVertexShader { get; private set; }

    /// <summary>
    ///     Fragment shader for game world rendering.
    /// </summary>
    public Shader? WorldFragmentShader { get; private set; }

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
    ///     Initializes the resources.
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

    /// <summary>
    ///     Updates all resource buffers.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    public void UpdateBuffers(CommandList commandList)
    {
        VertexBuffer?.Update(commandList);
        IndexBuffer?.Update(commandList);
        VertexUniformBuffer?.Update(commandList);
    }

    /// <summary>
    ///     Loads shaders.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the device has not yet been created.</exception>
    private void LoadWorldShaders()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Tried to create shaders without device.");

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