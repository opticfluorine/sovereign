using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
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

        /// <summary>
        /// Tile sprite manager.
        /// </summary>
        public TileSpriteManager TileSpriteManager { get; private set; }

        /// <summary>
        /// Animated sprite manager.
        /// </summary>
        public AnimatedSpriteManager AnimatedSpriteManager { get; private set; }

        /// <summary>
        /// Sprite manager.
        /// </summary>
        public SpriteManager SpriteManager { get; private set; }

        public RenderingResourceManager(SpriteSheetManager spriteSheetManager,
            TextureAtlasManager textureAtlasManager,
            TileSpriteManager tileSpriteManager,
            AnimatedSpriteManager animatedSpriteManager,
            SpriteManager spriteManager)
        {
            SpriteSheetManager = spriteSheetManager;
            TextureAtlasManager = textureAtlasManager;
            TileSpriteManager = tileSpriteManager;
            AnimatedSpriteManager = animatedSpriteManager;
            SpriteManager = spriteManager;
        }

        /// <summary>
        /// Loads and initializes the resources used by rendering.
        /// </summary>
        public void InitializeResources()
        {
            /* Initialize spritesheets. */
            SpriteSheetManager.InitializeSpriteSheets();
            TextureAtlasManager.InitializeTextureAtlas();

            /* Initialize sprite abstractions. */
            SpriteManager.InitializeSprites();
            AnimatedSpriteManager.InitializeAnimatedSprites();
            TileSpriteManager.InitializeTileSprites();
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
