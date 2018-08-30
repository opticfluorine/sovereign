using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// A set of sprites loaded from a single file.
    /// </summary>
    public class SpriteSheet : IDisposable
    {

        /// <summary>
        /// Spritesheet definition.
        /// </summary>
        public SpriteSheetDefinition Definition { get; private set; }

        /// <summary>
        /// SDL_Surface holding the spriteset.
        /// </summary>
        public Surface Surface { get; private set; }

        public SpriteSheet(Surface surface, SpriteSheetDefinition definition)
        {
            Surface = surface;
            Definition = definition;
        }

        public void Dispose()
        {
            Surface.Dispose();
        }

    }

}
