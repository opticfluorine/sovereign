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
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;

namespace Sovereign.ClientCore.Rendering.Sprites;

/// <summary>
///     Manages the sprites.
/// </summary>
public sealed class SpriteManager
{
    /// <summary>
    ///     Name of the sprite definitions file.
    /// </summary>
    private const string SpriteDefinitionsFile = "SpriteDefinitions.json";

    /// <summary>
    ///     Sprite definitions loader.
    /// </summary>
    private readonly SpriteDefinitionsLoader loader;

    /// <summary>
    ///     Resource path builder.
    /// </summary>
    private readonly IResourcePathBuilder resourcePathBuilder;

    private readonly SpriteSheetManager spriteSheetManager;

    /// <summary>
    ///     Map from spritesheet names to a 2D array of booleans indicating whether a sprite exists for
    ///     the given row and column.
    /// </summary>
    public Dictionary<string, Sprite?[,]> SpriteSheetCoverage = new();

    public SpriteManager(SpriteDefinitionsLoader loader,
        IResourcePathBuilder resourcePathBuilder, SpriteSheetManager spriteSheetManager)
    {
        this.loader = loader;
        this.resourcePathBuilder = resourcePathBuilder;
        this.spriteSheetManager = spriteSheetManager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

    /// <summary>
    ///     Sprites.
    /// </summary>
    public List<Sprite> Sprites { get; private set; } = new();

    /// <summary>
    ///     Initializes the sprites.
    /// </summary>
    public void InitializeSprites()
    {
        var definitions = LoadDefinitions();
        Sprites = UnpackSprites(definitions);

        // Initialize the coverage maps for the spritesheets.
        SpriteSheetCoverage.Clear();
        foreach (var spriteSheetName in spriteSheetManager.SpriteSheets.Keys)
        {
            var sheet = spriteSheetManager.SpriteSheets[spriteSheetName];
            var rows = sheet.Surface.Properties.Height / sheet.Definition.SpriteHeight;
            var cols = sheet.Surface.Properties.Width / sheet.Definition.SpriteWidth;

            if (!SpriteSheetCoverage.TryGetValue(spriteSheetName, out var coverageMap))
            {
                coverageMap = new Sprite?[rows, cols];
                SpriteSheetCoverage[spriteSheetName] = coverageMap;
            }
        }

        UpdateCoverageMaps();

        logger.LogInformation("Loaded " + Sprites.Count + " sprites.");
    }

    /// <summary>
    ///     Updates and saves the currently loaded sprite definitions to the file.
    /// </summary>
    public void UpdateAndSaveSprites()
    {
        var definitionsPath = resourcePathBuilder.BuildPathToResource(ResourceType.Sprite,
            SpriteDefinitionsFile);
        loader.SaveSpriteDefinitions(definitionsPath, Sprites);
        logger.LogInformation("Sprite definitions saved.");

        UpdateCoverageMaps();

        OnSpritesChanged?.Invoke();
    }

    /// <summary>
    ///     Event invoked when the sprites have been changed.
    /// </summary>
    public event Action? OnSpritesChanged;

    /// <summary>
    ///     Loads the sprite definitions.
    /// </summary>
    /// <returns>Sprite definitions.</returns>
    /// <exception cref="FatalErrorException">
    ///     Thrown if the sprite definitions cannot be loaded.
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
            logger.LogCritical("Failed to load the sprite definitions.", e);
            ErrorHandler.Error("Failed to load the sprite definitions.\n"
                               + "Refer to the error log for details.");
            throw new FatalErrorException();
        }
    }

    /// <summary>
    ///     Unpacks the sprite definitions.
    /// </summary>
    /// <param name="definitions">Sprite definitions.</param>
    /// <returns>Sprites.</returns>
    private List<Sprite> UnpackSprites(SpriteDefinitions definitions)
    {
        return definitions.Sprites
            .OrderBy(sprite => sprite.Id)
            .ToList();
    }

    /// <summary>
    ///     Updates the coverage maps for all spritesheets based on the current sprite table.
    /// </summary>
    /// <remarks>
    ///     As sprites can only be added, not removed, at runtime, this method will only add
    ///     coverage to the coverage maps. Do not delete sprites at runtime once they are
    ///     created.
    /// </remarks>
    private void UpdateCoverageMaps()
    {
        // Sprites can only be added, not removed, at runtime, so we don't need to
        // reset state between updates.
        foreach (var sprite in Sprites)
        {
            var coverageMap = SpriteSheetCoverage[sprite.SpritesheetName];
            var isNew = coverageMap[sprite.Row, sprite.Column] is null;
            coverageMap[sprite.Row, sprite.Column] = sprite;

            if (isNew)
            {
                // Sprite is newly added, do any post-processing needed.
                CheckOpacity(sprite);
            }
        }
    }

    /// <summary>
    ///     Updates the opacity flag of the given sprite.
    /// </summary>
    /// <param name="sprite">Sprite.</param>
    private void CheckOpacity(Sprite sprite)
    {
        var spriteSheet = spriteSheetManager.SpriteSheets[sprite.SpritesheetName];
        sprite.Opaque = spriteSheet.CheckOpacity(sprite.Row, sprite.Column);
    }
}