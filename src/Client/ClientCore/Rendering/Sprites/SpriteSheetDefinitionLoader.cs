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
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sovereign.ClientCore.Rendering.Sprites;

/// <summary>
///     Responsible for loading spritesheet definition files.
/// </summary>
public class SpriteSheetDefinitionLoader
{
    /// <summary>
    ///     YAML deserializer for the spritesheet definition file.
    /// </summary>
    private readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    /// <summary>
    ///     Loads a spritesheet definition from the given file.
    /// </summary>
    /// <param name="filename">Definition file to load.</param>
    /// <returns>Spritesheet definition.</returns>
    /// <exception cref="SpriteSheetDefinitionException">
    ///     Thrown if an error occurs while loading the definition.
    /// </exception>
    public SpriteSheetDefinition LoadDefinition(string filename)
    {
        /* Deserialize the definition file. */
        try
        {
            using (var reader = new StreamReader(filename))
            {
                return deserializer.Deserialize<SpriteSheetDefinition>(reader);
            }
        }
        catch (Exception e)
        {
            var sb = new StringBuilder();
            sb.Append("Failed to load spritesheet definition file '")
                .Append(filename).Append("'.");
            throw new SpriteSheetDefinitionException(sb.ToString(), e);
        }
    }
}