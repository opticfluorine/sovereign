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
using System.Text.Json;

namespace Sovereign.ClientCore.Rendering.Sprites;

/// <summary>
///     Loads sprite definitions from a YAML file.
/// </summary>
public sealed class SpriteDefinitionsLoader
{
    /// <summary>
    ///     Sprite definitions validator.
    /// </summary>
    private readonly SpriteDefinitionsValidator validator;

    public SpriteDefinitionsLoader(SpriteDefinitionsValidator validator)
    {
        this.validator = validator;
    }

    /// <summary>
    ///     Loads sprite definitions from the given file.
    /// </summary>
    /// <param name="filename">Filename.</param>
    /// <returns>Sprite definitions.</returns>
    /// <exception cref="SpriteDefinitionsException">
    ///     Thrown if the definitions cannot be loaded or are invalid.
    /// </exception>
    public SpriteDefinitions LoadSpriteDefinitions(string filename)
    {
        /* Load definitions. */
        SpriteDefinitions? definitions;
        try
        {
            using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            definitions = JsonSerializer.Deserialize<SpriteDefinitions>(stream);
        }
        catch (Exception e)
        {
            throw new SpriteDefinitionsException("Failed to read sprite definitions.", e);
        }

        if (definitions == null) throw new SpriteDefinitionsException("No sprite definitions found.");

        validator.Validate(definitions);

        return definitions;
    }

    /// <summary>
    ///     Saves the sprite definitions to the given file.
    /// </summary>
    /// <param name="filename">Filename.</param>
    /// <param name="sprites">List of sprites.</param>
    /// <exception cref="SpriteDefinitionsException">Thrown if an error occurs. Details in inner exception.</exception>
    public void SaveSpriteDefinitions(string filename, List<Sprite> sprites)
    {
        // Pack definitions.
        var defs = new SpriteDefinitions { Sprites = sprites };

        // Serialize to JSON.
        try
        {
            using var stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            JsonSerializer.Serialize(stream, defs);
        }
        catch (Exception e)
        {
            throw new SpriteDefinitionsException("Failed to write sprite definitions.", e);
        }
    }
}