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
using System.Text.Json;
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
    public const string TileSpriteDefinitionsFilename = "TileSpriteDefinitions.json";

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

        animatedSpriteManager.OnAnimatedSpriteAdded += OnAnimatedSpriteAdded;
        animatedSpriteManager.OnAnimatedSpriteRemoved += OnAnimatedSpriteRemoved;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

    /// <summary>
    ///     List of tile sprites.
    /// </summary>
    public List<TileSprite> TileSprites { get; } = new();

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

    /// <summary>
    ///     Saves the current tile sprite definitions.
    /// </summary>
    private void SaveDefinitions()
    {
        var defs = new TileSpriteDefinitions
        {
            TileSprites = TileSprites.Select(ts => new TileSpriteDefinitions.TileSpriteRecord
            {
                Id = ts.Id,
                TileContexts = ts.TileContexts
            }).ToList()
        };

        try
        {
            using var stream = new FileStream(TileSpriteDefinitionsFilename, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(stream, defs);
        }
        catch (Exception e)
        {
            Logger.Error("Failed to save tile sprite definitions.", e);
        }
    }

    /// <summary>
    ///     Called when a new animated sprite is added at runtime.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    private void OnAnimatedSpriteAdded(int animatedSpriteId)
    {
        // When an animated sprite is added, any existing animated sprites with id >= animatedSpriteId
        // are incremented by one. We need to scan the tile sprite table and adjust any existing
        // upstream references to match the new IDs.

        // If the animated sprite was added to the end of the table, no IDs were impacted and no
        // updates need to be made.
        if (animatedSpriteId == animatedSpriteManager.AnimatedSprites.Count - 1) return;

        foreach (var tileSprite in TileSprites)
        {
            tileSprite.OnAnimatedSpriteAdded(animatedSpriteId);
        }

        SaveDefinitions();
    }

    /// <summary>
    ///     Called when an animated sprite is removed at runtime.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    private void OnAnimatedSpriteRemoved(int animatedSpriteId)
    {
        // When an animated sprite is removed, any existing animated sprites with id > animatedSpriteId
        // are decremented by one. We need to scan the tile sprite table and adjust any existing
        // upstream references to match the new IDs.

        // We assume that the removed animated sprite is not present in any tile sprite, based on the
        // assumptions in AnimatedSpriteManager's Remove method. Any tile sprite dependencies on the
        // removed animated sprite should have been manually resolved by the user before the call was
        // allowed to be made.

        // If the animated sprite was removed from the end of the table, no IDs were impacted and no
        // updates need to be made.
        if (animatedSpriteId == animatedSpriteManager.AnimatedSprites.Count) return;

        foreach (var tileSprite in TileSprites)
        {
            tileSprite.OnAnimatedSpriteRemoved(animatedSpriteId);
        }

        SaveDefinitions();
    }
}