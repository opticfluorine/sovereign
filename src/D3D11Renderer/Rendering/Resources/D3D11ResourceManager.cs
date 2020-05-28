/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Castle.Core.Logging;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
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
