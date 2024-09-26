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
using Sovereign.ClientCore.Rendering;
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
    ///     Maximum number of indices in each index buffer.
    /// </summary>
    public const int MaximumIndices = 262144;

    /// <summary>
    ///     Resource name for the shader constants uniform buffer.
    /// </summary>
    public const string ResShaderConstants = "ShaderConstants";

    /// <summary>
    ///     Resource name for the texture atlas.
    /// </summary>
    public const string ResTextureAtlas = "g_textureAtlas";

    /// <summary>
    ///     Resource name for the texture atlas sampler.
    /// </summary>
    public const string ResTextureAtlasSampler = "g_textureAtlasSampler";

    /// <summary>
    ///     Resource name for the shadow map texture.
    /// </summary>
    public const string ResShadowMapTexture = "g_shadowMap";

    /// <summary>
    ///     Resource name for the shadow map texture sampler.
    /// </summary>
    public const string ResShadowMapTextureSampler = "g_shadowMapSampler";

    private readonly VeldridDevice device;

    /// <summary>
    ///     Dispose flag.
    /// </summary>
    private bool isDisposed;

    public GameResourceManager(VeldridDevice device)
    {
        this.device = device;
    }

    /// <summary>
    ///     Framebuffer for rendering the shadow map.
    /// </summary>
    public Framebuffer? ShadowMapFramebuffer { get; private set; }

    /// <summary>
    ///     Updateable vertex buffer.
    /// </summary>
    public VeldridUpdateBuffer<WorldVertex>? VertexBuffer { get; private set; }

    /// <summary>
    ///     Index buffer for rendering sprites (animated and tile).
    /// </summary>
    public VeldridUpdateBuffer<uint>? SpriteIndexBuffer { get; private set; }

    /// <summary>
    ///     Index buffer for rendering solid geometry from blocks.
    /// </summary>
    public VeldridUpdateBuffer<uint>? SolidIndexBuffer { get; private set; }

    /// <summary>
    ///     Uniform buffer for the world vertex shader.
    /// </summary>
    public VeldridUpdateBuffer<WorldVertexShaderConstants>? VertexUniformBuffer { get; private set; }

    /// <summary>
    ///     Uniform buffer for the world fragment shader.
    /// </summary>
    public VeldridUpdateBuffer<WorldFragmentShaderConstants>? FragmentUniformBuffer { get; private set; }

    /// <summary>
    ///     Uniform buffer for the block shadow map vertex shader.
    /// </summary>
    public VeldridUpdateBuffer<BlockShadowShaderConstants>? BlockShadowVertexUniformBuffer { get; private set; }

    /// <summary>
    ///     Render plan for the game scene.
    /// </summary>
    public RenderPlan? RenderPlan { get; private set; }

    /// <summary>
    ///     Vertex shader for game world rendering.
    /// </summary>
    public Shader? WorldVertexShader { get; private set; }

    /// <summary>
    ///     Fragment shader for game world rendering.
    /// </summary>
    public Shader? WorldFragmentShader { get; private set; }

    /// <summary>
    ///     Vertex shader for block shadow map rendering.
    /// </summary>
    public Shader? BlockShadowVertexShader { get; private set; }

    /// <summary>
    ///     Fragment shader for block shadow map rendering.
    /// </summary>
    public Shader? BlockShadowFragmentShader { get; private set; }

    /// <summary>
    ///     Veldrid texture used for holding the shadow map for each frame.
    /// </summary>
    public VeldridTexture? ShadowMapTexture { get; private set; }

    public void Dispose()
    {
        if (!isDisposed)
        {
            ShadowMapFramebuffer?.Dispose();
            ShadowMapTexture?.Dispose();
            BlockShadowFragmentShader?.Dispose();
            BlockShadowVertexShader?.Dispose();
            WorldFragmentShader?.Dispose();
            WorldVertexShader?.Dispose();
            VertexUniformBuffer?.Dispose();
            FragmentUniformBuffer?.Dispose();
            BlockShadowVertexUniformBuffer?.Dispose();
            SpriteIndexBuffer?.Dispose();
            SolidIndexBuffer?.Dispose();
            VertexBuffer?.Dispose();
            isDisposed = true;
        }
    }

    /// <summary>
    ///     Initializes the resources.
    /// </summary>
    public void Initialize()
    {
        VertexBuffer = new VeldridUpdateBuffer<WorldVertex>(device,
            BufferUsage.VertexBuffer, MaximumVertices);
        SpriteIndexBuffer = new VeldridUpdateBuffer<uint>(device,
            BufferUsage.IndexBuffer, MaximumIndices);
        SolidIndexBuffer = new VeldridUpdateBuffer<uint>(device, BufferUsage.IndexBuffer, MaximumIndices);
        VertexUniformBuffer = new VeldridUpdateBuffer<WorldVertexShaderConstants>(device,
            BufferUsage.UniformBuffer, 1);
        FragmentUniformBuffer =
            new VeldridUpdateBuffer<WorldFragmentShaderConstants>(device, BufferUsage.UniformBuffer, 1);
        BlockShadowVertexUniformBuffer = new VeldridUpdateBuffer<BlockShadowShaderConstants>(device,
            BufferUsage.UniformBuffer, 1);

        RenderPlan = new RenderPlan(VertexBuffer.Buffer, SpriteIndexBuffer.Buffer, SolidIndexBuffer.Buffer,
            MaximumDraws);

        CreateDynamicTextures();
        LoadWorldShaders();
    }

    /// <summary>
    ///     Updates all resource buffers.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    public void UpdateBuffers(CommandList commandList)
    {
        VertexBuffer?.Update(commandList);
        SpriteIndexBuffer?.Update(commandList);
        SolidIndexBuffer?.Update(commandList);
        VertexUniformBuffer?.Update(commandList);
        FragmentUniformBuffer?.Update(commandList);
        BlockShadowVertexUniformBuffer?.Update(commandList);
    }

    /// <summary>
    ///     Creates the dynamic textures to support rendering.
    /// </summary>
    private void CreateDynamicTextures()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");
        if (device.DisplayMode == null)
            throw new InvalidOperationException("Display mode not ready.");

        ShadowMapTexture = new VeldridTexture(device, (uint)device.DisplayMode.Width, (uint)device.DisplayMode.Height,
            TexturePurpose.DepthBuffer);

        var framebufferDesc = new FramebufferDescription(ShadowMapTexture.Texture);
        ShadowMapFramebuffer = device.Device.ResourceFactory.CreateFramebuffer(framebufferDesc);
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
        var bsVertexShaderBytes = device.LoadShaderBytes("BlockShadow.vert.spv");
        var bsFragmentShaderBytes = device.LoadShaderBytes("BlockShadow.frag.spv");

        // World vertex shader.
        var vertexDesc = new ShaderDescription(ShaderStages.Vertex,
            vertexShaderBytes, "main", true); // TODO control debug flag from config
        WorldVertexShader = device.Device.ResourceFactory.CreateShader(vertexDesc);

        // World fragment shader.
        var fragmentDesc = new ShaderDescription(ShaderStages.Fragment,
            fragmentShaderBytes, "main", true); // TODO control debug flag from config
        WorldFragmentShader = device.Device.ResourceFactory.CreateShader(fragmentDesc);

        // Block shadow vertex shader.
        var bsVertexDesc = new ShaderDescription(ShaderStages.Vertex,
            bsVertexShaderBytes, "main", true);
        BlockShadowVertexShader = device.Device.ResourceFactory.CreateShader(bsVertexDesc);

        // Block shadow fragment shader.
        var bsFragmentDesc = new ShaderDescription(ShaderStages.Fragment,
            bsFragmentShaderBytes, "main", true);
        BlockShadowFragmentShader = device.Device.ResourceFactory.CreateShader(bsFragmentDesc);
    }
}