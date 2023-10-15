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

using System.Collections.Generic;
using System.Text;

namespace Sovereign.ClientCore.Rendering.Sprites;

/// <summary>
///     Responsible for validating the list of loaded spritesheet definitions.
/// </summary>
public class SpriteSheetDefinitionValidator
{
    /// <summary>
    ///     Validates the list of loaded spritesheet definitions.
    /// </summary>
    /// <param name="definitions">Spritesheet definitions.</param>
    /// <exception cref="SpriteSheetDefinitionException">
    ///     Thrown if an error is found in the spritesheet definitions.
    /// </exception>
    public void Validate(IList<SpriteSheetDefinition> definitions)
    {
        CheckIndividualConstraints(definitions);
    }

    /// <summary>
    ///     Checks the definition constraints that apply to individual definitions.
    /// </summary>
    /// <param name="definitions">Spritesheet definitions.</param>
    /// <exception cref="SpriteSheetDefinitionException">
    ///     Thrown if a constraint is violated.
    /// </exception>
    private void CheckIndividualConstraints(IList<SpriteSheetDefinition> definitions)
    {
        foreach (var definition in definitions) CheckSpriteDimensions(definition);
    }

    /// <summary>
    ///     Individual constraint that ensures that the sprite dimensions are positive.
    /// </summary>
    /// <param name="definition">Definition.</param>
    /// <exception cref="SpriteSheetDefinitionException">
    ///     Thrown if the constraint is violated.
    /// </exception>
    private void CheckSpriteDimensions(SpriteSheetDefinition definition)
    {
        if (definition.SpriteWidth <= 0 || definition.SpriteHeight <= 0)
            ReportIndividualViolation("SpriteWidth and SpriteHeight must both be greater than zero.",
                definition);
    }

    /// <summary>
    ///     Reports a constraint violation for an individual definition.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="violatingDefinition">Violating definition.</param>
    /// <exception cref="SpriteSheetDefinitionException">
    ///     Thrown with the given message and definition details.
    /// </exception>
    private void ReportIndividualViolation(string message,
        SpriteSheetDefinition violatingDefinition)
    {
        /* Prepare detailed message. */
        var sb = new StringBuilder();
        sb.Append(message).Append("\n")
            .Append("Check the spritesheet definition file for ")
            .Append(violatingDefinition.Filename);

        /* Fire exception. */
        throw new SpriteSheetDefinitionException(sb.ToString());
    }
}