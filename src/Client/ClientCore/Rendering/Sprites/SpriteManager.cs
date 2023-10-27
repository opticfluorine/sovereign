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
    private const string SpriteDefinitionsFile = "SpriteDefinitions.yaml";

    /// <summary>
    ///     Sprite definitions loader.
    /// </summary>
    private readonly SpriteDefinitionsLoader loader;

    /// <summary>
    ///     Resource path builder.
    /// </summary>
    private readonly IResourcePathBuilder resourcePathBuilder;

    public SpriteManager(SpriteDefinitionsLoader loader,
        IResourcePathBuilder resourcePathBuilder)
    {
        this.loader = loader;
        this.resourcePathBuilder = resourcePathBuilder;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

    /// <summary>
    ///     Sprites.
    /// </summary>
    public IList<Sprite> Sprites { get; private set; }

    /// <summary>
    ///     Initializes the sprites.
    /// </summary>
    public void InitializeSprites()
    {
        var definitions = LoadDefinitions();
        Sprites = UnpackSprites(definitions);

        Logger.Info("Loaded " + Sprites.Count + " sprites.");
    }

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
            Logger.Fatal("Failed to load the sprite definitions.", e);
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
    private IList<Sprite> UnpackSprites(SpriteDefinitions definitions)
    {
        return definitions.Sprites
            .OrderBy(sprite => sprite.Id)
            .ToList();
    }
}