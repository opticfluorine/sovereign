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

using Sovereign.EngineCore.Resources;

namespace Sovereign.ClientCore.Rendering.Sprites;

/// <summary>
///     Responsible for creating spritesheets from files.
/// </summary>
public class SpriteSheetFactory
{
    /// <summary>
    ///     Resource path builder.
    /// </summary>
    private readonly IResourcePathBuilder resourcePathBuilder;

    /// <summary>
    ///     Surface loader.
    /// </summary>
    private readonly SurfaceLoader surfaceLoader;

    public SpriteSheetFactory(SurfaceLoader surfaceLoader,
        IResourcePathBuilder resourcePathBuilder)
    {
        this.surfaceLoader = surfaceLoader;
        this.resourcePathBuilder = resourcePathBuilder;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Loads the spritesheet described by the given definition.
    /// </summary>
    /// <param name="definition">Spritesheet definition.</param>
    /// <returns>Spritesheet.</returns>
    /// <exception cref="SurfaceException">Thrown if the spritesheet file cannot be loaded.</exception>
    public SpriteSheet LoadSpriteSheet(SpriteSheetDefinition definition)
    {
        logger.LogDebug("Loading spritesheet {0}", definition.Filename);

        /* Attempt to load the spritesheet surface. */
        var surface = LoadSurfaceForSpritesheet(definition);

        /* Bundle the spritesheet. */
        return new SpriteSheet(surface, definition);
    }

    /// <summary>
    ///     Loads the spritesheet image file into a Surface.
    /// </summary>
    /// <param name="definition">Spritesheet definition.</param>
    /// <returns>Spritesheet surface.</returns>
    /// <exception cref="SurfaceException">
    ///     Thrown if the surface cannot be loaded.
    /// </exception>
    private Surface LoadSurfaceForSpritesheet(SpriteSheetDefinition definition)
    {
        var filepath = resourcePathBuilder.BuildPathToResource(ResourceType.Spritesheet,
            definition.Filename);
        return surfaceLoader.LoadSurface(filepath);
    }
}