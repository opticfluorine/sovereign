using Castle.Core.Logging;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites
{

    /// <summary>
    /// Manages the tile sprites.
    /// </summary>
    public sealed class TileSpriteManager
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

        /// <summary>
        /// Tile sprite definitions filename.
        /// </summary>
        public const string TileSpriteDefinitionsFilename = "TileSpriteDefinitions.yaml";

        /// <summary>
        /// Resource path builder.
        /// </summary>
        private readonly IResourcePathBuilder pathBuilder;

        /// <summary>
        /// Tile sprite definitions loader.
        /// </summary>
        private readonly TileSpriteDefinitionsLoader loader;

        public TileSpriteManager(IResourcePathBuilder pathBuilder, 
            TileSpriteDefinitionsLoader loader)
        {
            this.pathBuilder = pathBuilder;
            this.loader = loader;
        }

        /// <summary>
        /// Initializes the tile sprites.
        /// </summary>
        public void InitializeTileSprites()
        {
            /* Try to load the tile sprite definitions. */
            TileSpriteDefinitions definitions;
            try
            {
                definitions = loader.LoadDefinitions(pathBuilder
                    .BuildPathToResource(ResourceType.Sprite, 
                    TileSpriteDefinitionsFilename));
            }
            catch (Exception e)
            {
                /* Log and throw a fatal error. */
                var msg = "Failed to load tile sprite definitions.";
                Logger.Fatal(msg, e);

                ErrorHandler.Error(e.Message);

                throw new FatalErrorException();
            }
        }

    }

}
