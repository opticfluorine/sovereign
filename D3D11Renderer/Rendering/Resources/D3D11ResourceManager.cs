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
