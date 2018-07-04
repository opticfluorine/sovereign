using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Responsible for creating spritesheets from files.
    /// </summary>
    public class SpriteSheetFactory
    {

        /// <summary>
        /// Surface loader.
        /// </summary>
        private readonly SurfaceLoader surfaceLoader;

        public SpriteSheetFactory(SurfaceLoader surfaceLoader)
        {
            this.surfaceLoader = surfaceLoader;
        }

        /// <summary>
        /// Loads the spritesheet described by the given definition.
        /// </summary>
        /// <param name="definition">Spritesheet definition.</param>
        /// <returns>Spritesheet.</returns>
        public SpriteSheet LoadSpriteSheet(SpriteSheetDefinition definition)
        {
            return null;
        }

    }

}
