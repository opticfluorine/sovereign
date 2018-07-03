using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// A set of sprites loaded from a single file.
    /// </summary>
    public class SpriteSheet
    {

        /// <summary>
        /// Width of a single sprite in this sheet.
        /// </summary>
        public int SpriteWidth { get; private set; }

        /// <summary>
        /// Height of a single sprite in this sheet.
        /// </summary>
        public int SpriteHeight { get; private set; }

        /// <summary>
        /// SDL_Surface holding the spriteset.
        /// </summary>
        public SDL.SDL_Surface Surface { get; private set; }

        public SpriteSheet(SDL.SDL_Surface surface, int spriteWidth, int spriteHeight)
        {
            Surface = surface;
            SpriteWidth = spriteWidth;
            SpriteHeight = spriteHeight;
        }

    }

}
