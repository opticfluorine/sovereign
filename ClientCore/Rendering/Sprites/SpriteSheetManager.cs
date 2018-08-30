using Castle.Core.Logging;
using Sovereign.ClientCore.Logging;
using Sovereign.EngineCore.Main;
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
        /// Spritesheet definition validator.
        /// </summary>
        private SpriteSheetDefinitionValidator definitionValidator;

        /// <summary>
        /// Resource path builder.
        /// </summary>
        private IResourcePathBuilder resourcePathBuilder;

        public SpriteSheetManager(SpriteSheetFactory spriteSheetFactory,
            SpriteSheetDefinitionLoader definitionLoader,
            SpriteSheetDefinitionValidator definitionValidator,
            IResourcePathBuilder resourcePathBuilder)
        {
            this.spriteSheetFactory = spriteSheetFactory;
            this.definitionLoader = definitionLoader;
            this.definitionValidator = definitionValidator;
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
                Logger.Info("Loading spritesheets.");

                SpriteSheets.AddRange(LoadSpriteSheets());

                Logger.InfoFormat("Successfully loaded {0} spritesheets.", SpriteSheets.Count());
            }
            catch (Exception e)
            {
                /* Fatal error - failed to load the spritesheets. */
                var sb = new StringBuilder();
                sb.Append("Failed to load spritesheets.\n\n").Append(e.Message);
                var message = sb.ToString();

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
        /// <returns>Spritesheet definitions ordered by sheet ID.</returns>
        private IList<SpriteSheetDefinition> LoadDefinitions()
        {
            var defs = from filename in FindDefinitionFiles()
                       select definitionLoader.LoadDefinition(filename);
            return defs.OrderBy(def => def.SheetId).ToList();
        }

        /// <summary>
        /// Loads the spritesheets.
        /// </summary>
        /// <returns>Spritesheets.</returns>
        /// <exception cref="SpriteSheetDefinitionException">
        /// Thrown if the sprite sheets cannot be loaded.
        /// </exception>
        private IEnumerable<SpriteSheet> LoadSpriteSheets()
        {
            /* Validate definitions immediately. */
            var defs = LoadDefinitions();
            definitionValidator.Validate(defs);

            /* Load the spritesheets. */
            return defs.Select(def => spriteSheetFactory.LoadSpriteSheet(def));
        }

    }

}
