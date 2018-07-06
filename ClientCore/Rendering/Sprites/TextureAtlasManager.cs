using Engine8.ClientCore.Rendering.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Responsible for managing the texture atlas.
    /// </summary>
    public class TextureAtlasManager
    {

        /// <summary>
        /// Texture atlas.
        /// </summary>
        public TextureAtlas TextureAtlas { get; private set; }

        /// <summary>
        /// Main display.
        /// </summary>
        private readonly MainDisplay mainDisplay;

        public TextureAtlasManager(MainDisplay mainDisplay)
        {
            this.mainDisplay = mainDisplay;
        }

        /// <summary>
        /// Initializes the texture atlas.
        /// </summary>
        /// <param name="spriteSheets"></param>
        public void InitializeTextureAtlas(IList<SpriteSheet> spriteSheets)
        {

        }

        /// <summary>
        /// Releases the texture atlas.
        /// </summary>
        public void ReleaseTextureAtlas()
        {

        }

    }

}
