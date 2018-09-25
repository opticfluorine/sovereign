/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

using Castle.Core.Logging;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.D3D11Renderer.Rendering.Resources.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering.Resources
{

    /// <summary>
    /// Responsible for managing D3D11 resources used by the renderer.
    /// </summary>
    public class D3D11ResourceManager
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// D3D11 texture containing the full texture atlas.
        /// </summary>
        public D3D11Texture AtlasTexture { get; private set; }

        /// <summary>
        /// Rendering device.
        /// </summary>
        private readonly D3D11Device device;

        /// <summary>
        /// Texture atlas manager.
        /// </summary>
        private readonly TextureAtlasManager atlasManager;

        public D3D11ResourceManager(D3D11Device device, TextureAtlasManager atlasManager)
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

            CreateAtlasTexture();

            Logger.Info("Base renderer resource initialization complete.");
        }

        /// <summary>
        /// Cleans up all remaining resources.
        /// </summary>
        public void Cleanup()
        {
            /* Free base resources. */
            AtlasTexture.Dispose();
        }

        /// <summary>
        /// Creates the D3D11 texture containing the full texture atlas.
        /// </summary>
        private void CreateAtlasTexture()
        {
            Logger.Debug("Creating texture for texture atlas.");

            AtlasTexture = new D3D11Texture(device, atlasManager.TextureAtlas.AtlasSurface);

            Logger.Debug("Texture for texture atlas created.");
        }

    }

}
