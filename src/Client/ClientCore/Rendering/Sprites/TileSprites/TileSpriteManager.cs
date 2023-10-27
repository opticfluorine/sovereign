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
using System.Linq;
using Castle.Core.Logging;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites;

/// <summary>
///     Manages the tile sprites.
/// </summary>
public sealed class TileSpriteManager
{
    /// <summary>
    ///     Tile sprite definitions filename.
    /// </summary>
    public const string TileSpriteDefinitionsFilename = "TileSpriteDefinitions.yaml";

    /// <summary>
    ///     Animated sprite manager.
    /// </summary>
    private readonly AnimatedSpriteManager animatedSpriteManager;

    /// <summary>
    ///     Tile sprite definitions loader.
    /// </summary>
    private readonly TileSpriteDefinitionsLoader loader;

    /// <summary>
    ///     Resource path builder.
    /// </summary>
    private readonly IResourcePathBuilder pathBuilder;

    public TileSpriteManager(IResourcePathBuilder pathBuilder,
        TileSpriteDefinitionsLoader loader, AnimatedSpriteManager animatedSpriteManager)
    {
        this.pathBuilder = pathBuilder;
        this.loader = loader;
        this.animatedSpriteManager = animatedSpriteManager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

    /// <summary>
    ///     List of tile sprites.
    /// </summary>
    public IList<TileSprite> TileSprites { get; } = new List<TileSprite>();

    /// <summary>
    ///     Initializes the tile sprites.
    /// </summary>
    public void InitializeTileSprites()
    {
        var definitions = LoadDefinitions();
        UnpackDefinitions(definitions);

        Logger.Info("Loaded " + TileSprites.Count + " tile sprites.");
    }

    /// <summary>
    ///     Loads the tile sprite definitions.
    /// </summary>
    /// <returns>Tile sprite definitions.</returns>
    /// <exception cref="FatalErrorException">
    ///     Thrown if the tile sprite definitions cannot be loaded.
    /// </exception>
    private TileSpriteDefinitions LoadDefinitions()
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
            var msg = "Failed to load the tile sprite definitions.";
            Logger.Fatal(msg, e);

            ErrorHandler.Error("Failed to load the tile sprite definitions.\n"
                               + "Refer to the error log for details.");

            throw new FatalErrorException();
        }

        return definitions;
    }

    /// <summary>
    ///     Unpacks the tile sprite definitions to populate the list of tile sprites.
    /// </summary>
    /// <param name="definitions">Tile sprite definitions.</param>
    private void UnpackDefinitions(TileSpriteDefinitions definitions)
    {
        foreach (var definition in definitions.TileSprites.OrderBy(tile => tile.Id))
            TileSprites.Add(new TileSprite(definition));
    }
}