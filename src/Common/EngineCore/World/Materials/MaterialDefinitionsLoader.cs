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
using System.Text.Json;

namespace Sovereign.EngineCore.World.Materials;

/// <summary>
///     Loads materials definitions from a YAML file.
/// </summary>
public sealed class MaterialDefinitionsLoader
{
    /// <summary>
    ///     Validator.
    /// </summary>
    private readonly MaterialDefinitionsValidator validator;

    public MaterialDefinitionsLoader(MaterialDefinitionsValidator validator)
    {
        this.validator = validator;
    }

    /// <summary>
    ///     Loads material definitions from the given file.
    /// </summary>
    /// <param name="filename">Filename.</param>
    /// <returns>Material definitions.</returns>
    /// <exception cref="MaterialDefinitionsException">
    ///     Thrown if the definitions file is invalid.
    /// </exception>
    public MaterialDefinitions LoadDefinitions(string filename)
    {
        /* Deserialize. */
        MaterialDefinitions? materialDefinitions;
        try
        {
            using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            materialDefinitions = JsonSerializer.Deserialize<MaterialDefinitions>(stream);
        }
        catch (Exception e)
        {
            /* Wrap in a MaterialDefinitionsException. */
            var sb = new StringBuilder();
            sb.Append("Failed to load materials definition file '")
                .Append(filename).Append("'.");
            throw new MaterialDefinitionsException(sb.ToString(), e);
        }

        if (materialDefinitions == null)
            throw new MaterialDefinitionsException("Material definitions missing or corrupt.");

        /* Perform additional validation. */
        var isValid = validator.IsValid(materialDefinitions, out var errorMessages);
        if (!isValid)
        {
            var sb = new StringBuilder();
            sb.Append("Errors in materials definition file '")
                .Append(filename).Append("':\n\n").Append(errorMessages);
            throw new MaterialDefinitionsException(sb.ToString());
        }

        return materialDefinitions;
    }
}