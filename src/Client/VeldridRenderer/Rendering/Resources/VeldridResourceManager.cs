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
using Castle.Core.Logging;
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Resources;

/// <summary>
///     Responsible for managing GPU resources via Veldrid.
/// </summary>
public class VeldridResourceManager : IDisposable
{
    /// <summary>
    ///     Size of GUI vertex buffer.
    /// </summary>
    private const int GuiVertexBufferSize = 8192;

    /// <summary>
    ///     Size of GUI index buffer.
    /// </summary>
    private const int GuiIndexBufferSize = 8192;

    /// <summary>
    ///     Texture atlas manager.
    /// </summary>
    private readonly TextureAtlasManager atlasManager;

    /// <summary>
    ///     Rendering device.
    /// </summary>
    private readonly VeldridDevice device;

    public VeldridResourceManager(VeldridDevice device, TextureAtlasManager atlasManager)
    {
        this.device = device;
        this.atlasManager = atlasManager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Veldrid texture containing the full texture atlas.
    /// </summary>
    public VeldridTexture? AtlasTexture { get; private set; }

    /// <summary>
    ///     Veldrid command list used for rendering.
    /// </summary>
    public CommandList? CommandList { get; private set; }

    /// <summary>
    ///     Vertex buffer for GUI rendering.
    /// </summary>
    public VeldridUpdateBuffer<ImDrawVert>? GuiVertexBuffer { get; private set; }

    /// <summary>
    ///     Index buffer for GUI rendering.
    /// </summary>
    public VeldridUpdateBuffer<ushort>? GuiIndexBuffer { get; private set; }

    /// <summary>
    ///     Cleans up the resources when done.
    /// </summary>
    public void Dispose()
    {
        GuiIndexBuffer?.Dispose();
        GuiVertexBuffer?.Dispose();
        AtlasTexture?.Dispose();
        CommandList?.Dispose();
    }

    /// <summary>
    ///     Initializes the base resources used by the renderer over its full lifetime.
    /// </summary>
    public void InitializeBaseResources()
    {
        Logger.Info("Initializing base renderer resources.");

        // Command lists.
        if (device.Device == null)
            throw new InvalidOperationException("Tried to create resources without a device.");
        CommandList = device.Device.ResourceFactory.CreateCommandList();

        // Textures.
        CreateAtlasTexture();

        // GUI.
        CreateGuiResources();

        Logger.Info("Base renderer resource initialization complete.");
    }

    /// <summary>
    ///     Creates the GPU resources for GUI rendering.
    /// </summary>
    private void CreateGuiResources()
    {
        GuiVertexBuffer = new VeldridUpdateBuffer<ImDrawVert>(device!,
            BufferUsage.VertexBuffer | BufferUsage.Dynamic, GuiVertexBufferSize);
        GuiIndexBuffer = new VeldridUpdateBuffer<ushort>(device!,
            BufferUsage.IndexBuffer | BufferUsage.Dynamic, GuiIndexBufferSize);
    }

    /// <summary>
    ///     Creates the Veldrid texture for the full texture atlas.
    /// </summary>
    private void CreateAtlasTexture()
    {
        if (atlasManager.TextureAtlas == null)
            throw new InvalidOperationException("Tried to create Vulkan texture atlas without data.");
        AtlasTexture = new VeldridTexture(device,
            atlasManager.TextureAtlas.AtlasSurface);
    }
}