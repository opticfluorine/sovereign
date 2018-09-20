using Castle.Core.Logging;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Manages the sprites.
    /// </summary>
    public sealed class SpriteManager
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

        /// <summary>
        /// Name of the sprite definitions file.
        /// </summary>
        private const string SpriteDefinitionsFile = "SpriteDefinitions.yaml";

        /// <summary>
        /// Sprite definitions loader.
        /// </summary>
        private readonly SpriteDefinitionsLoader loader;

        /// <summary>
        /// Resource path builder.
        /// </summary>
        private readonly IResourcePathBuilder resourcePathBuilder;

        /// <summary>
        /// Sprites.
        /// </summary>
        public IList<Sprite> Sprites { get; private set; }

        public SpriteManager(SpriteDefinitionsLoader loader,
            IResourcePathBuilder resourcePathBuilder)
        {
            this.loader = loader;
            this.resourcePathBuilder = resourcePathBuilder;
        }

        /// <summary>
        /// Initializes the sprites.
        /// </summary>
        public void InitializeSprites()
        {
            var definitions = LoadDefinitions();
            Sprites = UnpackSprites(definitions);
        }

        /// <summary>
        /// Loads the sprite definitions.
        /// </summary>
        /// <returns>Sprite definitions.</returns>
        /// <exception cref="FatalErrorException">
        /// Thrown if the sprite definitions cannot be loaded.
        /// </exception>
        private SpriteDefinitions LoadDefinitions()
        {
            var definitionsPath = resourcePathBuilder.BuildPathToResource(ResourceType.Sprite,
                SpriteDefinitionsFile);

            try
            {
                return loader.LoadSpriteDefinitions(definitionsPath);
            }
            catch (Exception e)
            {
                Logger.Fatal("Failed to load sprite definitions.", e);
                ErrorHandler.Error(e.Message);
                throw new FatalErrorException();
            }
        }

        /// <summary>
        /// Unpacks the sprite definitions.
        /// </summary>
        /// <param name="definitions">Sprite definitions.</param>
        /// <returns>Sprites.</returns>
        private IList<Sprite> UnpackSprites(SpriteDefinitions definitions)
        {
            return definitions.Sprites
                .OrderBy(sprite => sprite.Id)
                .ToList();
        }

    }

}
