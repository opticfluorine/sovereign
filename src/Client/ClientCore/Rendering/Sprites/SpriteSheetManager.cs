/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;

namespace Sovereign.ClientCore.Rendering.Sprites;

/// <summary>
///     Responsible for loading and managing the spritesheets.
/// </summary>
public class SpriteSheetManager
{
    /// <summary>
    ///     Suffix attached to spritesheet definition filenames.
    /// </summary>
    private const string DefinitionSuffix = ".yaml";

    /// <summary>
    ///     Spritesheet definition file loader.
    /// </summary>
    private readonly SpriteSheetDefinitionLoader definitionLoader;

    /// <summary>
    ///     Spritesheet definition validator.
    /// </summary>
    private readonly SpriteSheetDefinitionValidator definitionValidator;

    /// <summary>
    ///     Resource path builder.
    /// </summary>
    private readonly IResourcePathBuilder resourcePathBuilder;

    /// <summary>
    ///     Spritesheet factory.
    /// </summary>
    private readonly SpriteSheetFactory spriteSheetFactory;

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

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

    /// <summary>
    ///     Map from spritesheet names to spritesheets.
    /// </summary>
    public IDictionary<string, SpriteSheet> SpriteSheets { get; }
        = new Dictionary<string, SpriteSheet>();

    /// <summary>
    ///     Initializes all spritesheets.
    /// </summary>
    public void InitializeSpriteSheets()
    {
        /* Load the spritesheets. */
        try
        {
            Logger.Info("Loading spritesheets.");

            foreach (var spriteSheet in LoadSpriteSheets()) SpriteSheets[spriteSheet.Definition.Filename] = spriteSheet;

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
    ///     Releases all loaded spritesheets.
    /// </summary>
    public void ReleaseSpriteSheets()
    {
        foreach (var spriteSheet in SpriteSheets.Values) spriteSheet.Dispose();
        SpriteSheets.Clear();
    }

    /// <summary>
    ///     Enumerates the spritesheet definition files.
    /// </summary>
    /// <returns>Spritesheet definition files.</returns>
    private IEnumerable<string> FindDefinitionFiles()
    {
        /* Look in the spritesheet resource directory. */
        var baseDir = resourcePathBuilder.GetBaseDirectoryForResource(ResourceType.Spritesheet);
        return from filename in Directory.EnumerateFiles(baseDir)
            where filename.EndsWith(DefinitionSuffix)
            orderby filename
            select filename;
    }

    /// <summary>
    ///     Loads the spritesheet definitions.
    /// </summary>
    /// <returns>Spritesheet definitions ordered by filename.</returns>
    private IList<SpriteSheetDefinition> LoadDefinitions()
    {
        var defs = from filename in FindDefinitionFiles()
            select definitionLoader.LoadDefinition(filename);
        return defs.OrderBy(def => def.Filename).ToList();
    }

    /// <summary>
    ///     Loads the spritesheets.
    /// </summary>
    /// <returns>Spritesheets.</returns>
    /// <exception cref="SpriteSheetDefinitionException">
    ///     Thrown if the sprite sheets cannot be loaded.
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