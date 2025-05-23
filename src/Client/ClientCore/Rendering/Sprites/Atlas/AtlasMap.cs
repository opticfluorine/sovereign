﻿/*
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;

namespace Sovereign.ClientCore.Rendering.Sprites.Atlas;

/// <summary>
///     Maps spritesheet coordinates into the texture atlas.
/// </summary>
public sealed class AtlasMap
{
    private readonly TextureAtlasManager atlasManager;
    private readonly ILogger<AtlasMap> logger;
    private readonly RendererOptions rendererOptions;
    private readonly SpriteManager spriteManager;
    private readonly SpriteSheetManager spriteSheetManager;

    public AtlasMap(TextureAtlasManager atlasManager, SpriteManager spriteManager,
        SpriteSheetManager spriteSheetManager,
        ILogger<AtlasMap> logger,
        IOptions<RendererOptions> rendererOptions)
    {
        this.atlasManager = atlasManager;
        this.spriteManager = spriteManager;
        this.spriteSheetManager = spriteSheetManager;
        this.logger = logger;
        this.rendererOptions = rendererOptions.Value;

        spriteManager.OnSpritesChanged += InitializeAtlasMap;
    }

    /// <summary>
    ///     Maps each sprite ID to its texture atlas coordinates.
    /// </summary>
    public List<AtlasMapElement> MapElements { get; private set; } = new();

    /// <summary>
    ///     Initializes the atlas map.
    /// </summary>
    /// This must be called after the texture atlas and sprite manager are
    /// initialized.
    public void InitializeAtlasMap()
    {
        MapElements = new List<AtlasMapElement>(spriteManager.Sprites.Count);

        /* Iterate in order of sprite ID. */
        foreach (var sprite in spriteManager.Sprites) AddSprite(sprite);

        logger.LogInformation("Mapped " + MapElements.Count + " sprites to the texture atlas.");
    }

    /// <summary>
    ///     Adds the given sprite to the map.
    /// </summary>
    /// <param name="sprite">Sprite to be added.</param>
    private void AddSprite(Sprite sprite)
    {
        /* Retrieve spritesheet. */
        var sheet = spriteSheetManager.SpriteSheets[sprite.SpritesheetName];
        var spriteWidth = sheet.Definition.SpriteWidth;
        var spriteHeight = sheet.Definition.SpriteHeight;

        /* Locate spritesheet in atlas. */
        if (atlasManager.TextureAtlas == null)
            throw new InvalidOperationException("Tried to add sprite to a nonexistant texture atlas.");

        var (stlx, stly) = atlasManager.TextureAtlas.SpriteSheetMap[sprite.SpritesheetName];
        var atlasWidth = atlasManager.TextureAtlas.Width;
        var atlasHeight = atlasManager.TextureAtlas.Height;

        /* Compute sprite coordinates in atlas. */
        var tlx = (float)(stlx + sprite.Column * spriteWidth);
        var tly = (float)(stly + sprite.Row * spriteHeight);
        var brx = tlx + spriteWidth;
        var bry = tly + spriteHeight;

        /* Add record to map. */
        MapElements.Add(new AtlasMapElement
        {
            NormalizedLeftX = tlx / atlasWidth,
            NormalizedTopY = tly / atlasHeight,
            NormalizedRightX = brx / atlasWidth,
            NormalizedBottomY = bry / atlasHeight,
            WidthInTiles = (float)spriteWidth / rendererOptions.TileWidth,
            HeightInTiles = (float)spriteHeight / rendererOptions.TileWidth
        });
    }
}