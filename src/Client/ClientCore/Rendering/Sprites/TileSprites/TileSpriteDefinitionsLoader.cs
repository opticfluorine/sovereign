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
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites;

/// <summary>
///     Loads tile sprite definitions from a YAML file.
/// </summary>
public sealed class TileSpriteDefinitionsLoader
{
    /// <summary>
    ///     YAML deserializer.
    /// </summary>
    private readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    /// <summary>
    ///     Definitions validator.
    /// </summary>
    private readonly TileSpriteDefinitionsValidator validator;

    public TileSpriteDefinitionsLoader(TileSpriteDefinitionsValidator validator)
    {
        this.validator = validator;
    }

    /// <summary>
    ///     Loads tile sprite definitions from the given file.
    /// </summary>
    /// <param name="filename">Definitions filename.</param>
    /// <returns>Tile sprite definitions.</returns>
    /// <exception cref="TileSpriteDefinitionsException">
    ///     Thrown if the definitions cannot be loaded or are invalid.
    /// </exception>
    public TileSpriteDefinitions LoadDefinitions(string filename)
    {
        /* Attempt to load the definitions. */
        TileSpriteDefinitions definitions;
        try
        {
            using (var reader = new StreamReader(filename))
            {
                definitions = deserializer.Deserialize<TileSpriteDefinitions>(reader);
            }
        }
        catch (Exception e)
        {
            /* Wrap the exception. */
            throw new TileSpriteDefinitionsException("Failed to load tile sprite definitions.", e);
        }

        /* Validate the definitions. */
        validator.Validate(definitions);

        /* Postprocess the definitions. */

        /* Done. */
        return definitions;
    }
}