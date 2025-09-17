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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Resources;

/// <summary>
///     Responsible for managing GPU resources via Veldrid.
/// </summary>
public class VeldridResourceManager : IDisposable
{
    /// <summary>
    ///     Texture atlas manager.
    /// </summary>
    private readonly TextureAtlasManager atlasManager;

    /// <summary>
    ///     Rendering device.
    /// </summary>
    private readonly VeldridDevice device;

    private readonly DisplayOptions displayOptions;

    private readonly ILogger<VeldridResourceManager> logger;

    public VeldridResourceManager(VeldridDevice device, TextureAtlasManager atlasManager,
        ILogger<VeldridResourceManager> logger, IOptions<DisplayOptions> displayOptions)
    {
        this.device = device;
        this.atlasManager = atlasManager;
        this.logger = logger;
        this.displayOptions = displayOptions.Value;
    }

    /// <summary>
    ///     Veldrid texture containing the full texture atlas.
    /// </summary>
    public VeldridTexture? AtlasTexture { get; private set; }


    /// <summary>
    ///     Veldrid command list used for rendering.
    /// </summary>
    public CommandList? CommandList { get; private set; }

    /// <summary>
    ///     Cleans up the resources when done.
    /// </summary>
    public void Dispose()
    {
        AtlasTexture?.Dispose();
        CommandList?.Dispose();
    }

    /// <summary>
    ///     Initializes the base resources used by the renderer over its full lifetime.
    /// </summary>
    public void InitializeBaseResources()
    {
        logger.LogInformation("Initializing base renderer resources.");

        // Command lists.
        if (device.Device == null)
            throw new InvalidOperationException("Tried to create resources without a device.");
        CommandList = device.Device.ResourceFactory.CreateCommandList();

        // Textures.
        CreateAtlasTexture();

        logger.LogInformation("Base renderer resource initialization complete.");
    }

    /// <summary>
    ///     Reloads textures.
    /// </summary>
    public void ReloadTextures()
    {
        logger.LogInformation("Reloading textures.");

        AtlasTexture?.Dispose();
        CreateAtlasTexture();
    }

    /// <summary>
    ///     Creates the Veldrid texture for the full texture atlas.
    /// </summary>
    private void CreateAtlasTexture()
    {
        if (atlasManager.TextureAtlas == null)
            throw new InvalidOperationException("Tried to create Vulkan texture atlas without data.");
        AtlasTexture = new VeldridTexture(device, atlasManager.TextureAtlas.AtlasSurface, displayOptions.UseSrgb);
    }
}