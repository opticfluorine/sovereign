using Sovereign.ClientCore.Rendering.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering
{

    /// <summary>
    /// Responsible for managing resources used by rendering.
    /// </summary>
    public class RenderingResourceManager
    {

        /// <summary>
        /// Spritesheet manager.
        /// </summary>
        public SpriteSheetManager SpriteSheetManager { get; private set; }

        /// <summary>
        /// Texture atlas manager.
        /// </summary>
        public TextureAtlasManager TextureAtlasManager { get; private set; }

        public RenderingResourceManager(SpriteSheetManager spriteSheetManager,
            TextureAtlasManager textureAtlasManager)
        {
            SpriteSheetManager = spriteSheetManager;
            TextureAtlasManager = textureAtlasManager;
        }

        /// <summary>
        /// Loads and initializes the resources used by rendering.
        /// </summary>
        public void InitializeResources()
        {
            SpriteSheetManager.InitializeSpriteSheets();
            TextureAtlasManager.InitializeTextureAtlas();
        }

        /// <summary>
        /// Cleans up all resources used by rendering.
        /// </summary>
        public void CleanupResources()
        {
            TextureAtlasManager.ReleaseTextureAtlas();
            SpriteSheetManager.ReleaseSpriteSheets();
        }

    }

}
