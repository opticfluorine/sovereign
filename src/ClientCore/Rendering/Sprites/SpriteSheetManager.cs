/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Castle.Core.Logging;
using Sovereign.ClientCore.Logging;
using Sovereign.EngineCore.Logging;
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

        public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

        /// <summary>
        /// Suffix attached to spritesheet definition filenames.
        /// </summary>
        private const string DefinitionSuffix = ".yaml";

        /// <summary>
        /// Map from spritesheet names to spritesheets.
        /// </summary>
        public IDictionary<string, SpriteSheet> SpriteSheets { get; private set; } 
            = new Dictionary<string, SpriteSheet>();

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

                foreach (var spriteSheet in LoadSpriteSheets())
                {
                    SpriteSheets[spriteSheet.Definition.Filename] = spriteSheet;
                }

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
            foreach (var spriteSheet in SpriteSheets.Values)
            {
                spriteSheet.Dispose();
            }
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
