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
using Castle.Core.Logging;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Resources;

/// <summary>
/// Responsible for managing GPU resources via Veldrid.
/// </summary>
public class VeldridResourceManager : IDisposable
{

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    /// Veldrid texture containing the full texture atlas.
    /// </summary>
    public VeldridTexture AtlasTexture { get; private set; }

    /// <summary>
    /// Veldrid command list used for rendering.
    /// </summary>
    public CommandList CommandList { get; private set; }

    /// <summary>
    /// Rendering device.
    /// </summary>
    private readonly VeldridDevice device;

    /// <summary>
    /// Texture atlas manager.
    /// </summary>
    private readonly TextureAtlasManager atlasManager;

    public VeldridResourceManager(VeldridDevice device, TextureAtlasManager atlasManager)
    {
        this.device = device;
        this.atlasManager = atlasManager;
    }

    /// <summary>
    /// Initializes the base resources used by the renderer over its full lifetime.
    /// </summary>
    public void InitializeBaseResources()
    {
        Logger.Info("Initializing base renderer resources.");

        // Command lists.
        CommandList = device.Device.ResourceFactory.CreateCommandList();

        // Textures.
        CreateAtlasTexture();

        Logger.Info("Base renderer resource initialization complete.");
    }

    /// <summary>
    /// Cleans up the resources when done.
    /// </summary>
    public void Dispose()
    {
        AtlasTexture?.Dispose();
        CommandList?.Dispose();
    }

    /// <summary>
    /// Creates the Veldrid texture for the full texture atlas.
    /// </summary>
    private void CreateAtlasTexture()
    {
        AtlasTexture = new VeldridTexture(device,
            atlasManager.TextureAtlas.AtlasSurface);
    }

}
