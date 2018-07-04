using Castle.Core.Logging;
using Engine8.ClientCore.Logging;
using Engine8.EngineCore.Main;
using Engine8.EngineCore.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Responsible for loading and managing the spritesheets.
    /// </summary>
    public class SpriteSheetManager
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Suffix attached to spritesheet definition filenames.
        /// </summary>
        private const string DefinitionSuffix = ".yaml";

        /// <summary>
        /// A list of all loaded spritesheets.
        /// </summary>
        public List<SpriteSheet> SpriteSheets { get; private set; } = new List<SpriteSheet>();

        /// <summary>
        /// Spritesheet factory.
        /// </summary>
        private SpriteSheetFactory spriteSheetFactory;

        /// <summary>
        /// Spritesheet definition file loader.
        /// </summary>
        private SpriteSheetDefinitionLoader definitionLoader;

        /// <summary>
        /// Resource path builder.
        /// </summary>
        private IResourcePathBuilder resourcePathBuilder;

        public SpriteSheetManager(SpriteSheetFactory spriteSheetFactory,
            SpriteSheetDefinitionLoader definitionLoader,
            IResourcePathBuilder resourcePathBuilder)
        {
            this.spriteSheetFactory = spriteSheetFactory;
            this.definitionLoader = definitionLoader;
            this.resourcePathBuilder = resourcePathBuilder;
        }

        /// <summary>
        /// Initializes all spritesheets.
        /// </summary>
        public void InitializeSpriteSheets()
        {
            /* Load the spritesheets. */
            try
            {
                SpriteSheets.AddRange(LoadSpriteSheets());
            }
            catch (Exception e)
            {
                /* Fatal error - failed to load the spritesheets. */
                var message = "Failed to load spritesheets.";
                Logger.Fatal(message, e);
                ErrorHandler.Error(message);
                throw new FatalErrorException(message, e);
            }
        }

        /// <summary>
        /// Releases all loaded spritesheets.
        /// </summary>
        public void ReleaseSpriteSheets()
        {
            SpriteSheets.ForEach(spriteSheet => spriteSheet.Dispose());
            SpriteSheets.Clear();
        }

        /// <summary>
        /// Enumerates the spritesheet definition files.
        /// </summary>
        /// <returns>Spritesheet definition files.</returns>
        private IEnumerable<string> FindDefinitionFiles()
        {
            /* Look in the spritesheet resource directory. */
            var baseDir = resourcePathBuilder.GetBaseDirectoryForResource(ResourceType.Spritesheet);
            return from filename in Directory.EnumerateFiles(baseDir)
                   where filename.EndsWith(DefinitionSuffix)
                   orderby filename ascending
                   select filename;
        }

        /// <summary>
        /// Loads the spritesheet definitions.
        /// </summary>
        /// <returns>Spritesheet definitions.</returns>
        private IEnumerable<SpriteSheetDefinition> LoadDefinitions()
        {
            return from filename in FindDefinitionFiles()
                   select definitionLoader.LoadDefinition(filename);
        }

        /// <summary>
        /// Loads the spritesheets.
        /// </summary>
        /// <returns>Spritesheets.</returns>
        private IEnumerable<SpriteSheet> LoadSpriteSheets()
        {
            return from definition in LoadDefinitions()
                   select spriteSheetFactory.LoadSpriteSheet(definition);
        }

    }

}
