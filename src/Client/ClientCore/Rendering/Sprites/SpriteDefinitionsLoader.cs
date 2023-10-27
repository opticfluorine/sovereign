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

namespace Sovereign.ClientCore.Rendering.Sprites;

/// <summary>
///     Loads sprite definitions from a YAML file.
/// </summary>
public sealed class SpriteDefinitionsLoader
{
    /// <summary>
    ///     YAML deserializer.
    /// </summary>
    private readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

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
        SpriteDefinitions definitions;
        try
        {
            using (var reader = new StreamReader(filename))
            {
                definitions = deserializer.Deserialize<SpriteDefinitions>(reader);
            }
        }
        catch (Exception e)
        {
            throw new SpriteDefinitionsException("Failed to read sprite definitions.", e);
        }

        validator.Validate(definitions);

        return definitions;
    }
}