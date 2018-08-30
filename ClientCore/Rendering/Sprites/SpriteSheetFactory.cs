using Castle.Core.Logging;
using Sovereign.EngineCore.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Responsible for creating spritesheets from files.
    /// </summary>
    public class SpriteSheetFactory
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Surface loader.
        /// </summary>
        private readonly SurfaceLoader surfaceLoader;

        /// <summary>
        /// Resource path builder.
        /// </summary>
        private readonly IResourcePathBuilder resourcePathBuilder;

        public SpriteSheetFactory(SurfaceLoader surfaceLoader, 
            IResourcePathBuilder resourcePathBuilder)
        {
            this.surfaceLoader = surfaceLoader;
            this.resourcePathBuilder = resourcePathBuilder;
        }

        /// <summary>
        /// Loads the spritesheet described by the given definition.
        /// </summary>
        /// <param name="definition">Spritesheet definition.</param>
        /// <returns>Spritesheet.</returns>
        /// <exception cref="SurfaceException">Thrown if the spritesheet file cannot be loaded.</exception>
        public SpriteSheet LoadSpriteSheet(SpriteSheetDefinition definition)
        {
            Logger.DebugFormat("Loading spritesheet {0}: {1}", definition.SheetId,
                definition.Filename);

            /* Attempt to load the spritesheet surface. */
            var surface = LoadSurfaceForSpritesheet(definition);

            /* Bundle the spritesheet. */
            return new SpriteSheet(surface, definition);
        }

        /// <summary>
        /// Loads the spritesheet image file into a Surface.
        /// </summary>
        /// <param name="definition">Spritesheet definition.</param>
        /// <returns>Spritesheet surface.</returns>
        /// <exception cref="SurfaceException">
        /// Thrown if the surface cannot be loaded.
        /// </exception>
        private Surface LoadSurfaceForSpritesheet(SpriteSheetDefinition definition)
        {
            var filepath = resourcePathBuilder.BuildPathToResource(ResourceType.Spritesheet,
                definition.Filename);
            return surfaceLoader.LoadSurface(filepath);
        }

    }

}
