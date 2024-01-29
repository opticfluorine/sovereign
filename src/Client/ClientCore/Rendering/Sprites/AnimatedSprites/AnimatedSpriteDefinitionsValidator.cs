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
using System.Linq;
using System.Text;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineUtil.Validation;

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;

/// <summary>
///     Validates the animated sprite definitions.
/// </summary>
public sealed class AnimatedSpriteDefinitionsValidator
{
    /// <summary>
    ///     Sprite manager.
    /// </summary>
    private readonly SpriteManager spriteManager;

    public AnimatedSpriteDefinitionsValidator(SpriteManager spriteManager)
    {
        this.spriteManager = spriteManager;
    }

    /// <summary>
    ///     Validates the given animated sprite definitions.
    /// </summary>
    /// <param name="definitions">Animated sprite definitions.</param>
    /// <exception cref="AnimatedSpriteDefinitionsException">
    ///     Thrown if the definitions are invalid.
    /// </exception>
    public void Validate(AnimatedSpriteDefinitions definitions)
    {
        var sb = new StringBuilder();
        var valid = ValidateIds(definitions, sb)
                    && ValidateNonzeroAnimationTimestep(definitions, sb)
                    && ValidateSpriteIds(definitions, sb)
                    && ValidateDefaultFaces(definitions, sb);
        if (!valid) throw new AnimatedSpriteDefinitionsException(sb.ToString().Trim());
    }

    /// <summary>
    ///     Checks that IDs are not duplicated and run from 0 to n - 1.
    /// </summary>
    /// <param name="definitions">Definitions.</param>
    /// <param name="sb">StringBuilder for error reporting.</param>
    /// <returns>true if valid, false otherwise.</returns>
    private bool ValidateIds(AnimatedSpriteDefinitions definitions,
        StringBuilder sb)
    {
        var spriteCount = definitions.AnimatedSprites.Count;
        var validator = new ConsecutiveRangeValidation();
        validator.IsRangeConsecutive(definitions.AnimatedSprites.Select(sprite => sprite.Id),
            0, spriteCount, out var duplicateIds, out var outOfRangeIds);

        var hasDuplicates = duplicateIds.Count > 0;
        var hasOutOfRanges = outOfRangeIds.Count > 0;
        var valid = !(hasDuplicates || hasOutOfRanges);

        if (!valid)
        {
            if (hasDuplicates)
            {
                sb.Append("The following animated sprite IDs are duplicated:\n\n");
                foreach (var id in duplicateIds) sb.Append("Animated Sprite ").Append(id).Append("\n");
            }

            if (hasDuplicates && hasOutOfRanges) sb.Append("\n");

            if (hasOutOfRanges)
            {
                sb.Append("Animated sprite IDs must run consecutively from 0.\n")
                    .Append("The following IDs are out of range:\n\n");
                foreach (var id in outOfRangeIds) sb.Append("Animated Sprite ").Append(id).Append("\n");
            }
        }

        return valid;
    }

    /// <summary>
    ///     Checks that all animated sprites have positive animation timesteps.
    /// </summary>
    /// <param name="definitions">Definitions.</param>
    /// <param name="sb">StringBuilder for errors.</param>
    /// <returns>true if valid, false otherwise.</returns>
    private bool ValidateNonzeroAnimationTimestep(AnimatedSpriteDefinitions definitions,
        StringBuilder sb)
    {
        /* Validate. */
        var badSprites = definitions.AnimatedSprites
            .Where(sprite => sprite.AnimationTimestep == 0);
        var valid = badSprites.Count() == 0;

        /* Output error message if necessary. */
        if (!valid)
        {
            sb.Append("All animated sprites must have positive animation timesteps.\n"
                      + "The following animated sprites have zero-valued animation timesteps:\n\n");
            foreach (var sprite in badSprites) sb.Append("Animated Sprite ").Append(sprite.Id).Append("\n");
        }

        return valid;
    }

    /// <summary>
    ///     Checks that there are no unknown references to sprites.
    /// </summary>
    /// <param name="definitions">Definitions to validate.</param>
    /// <param name="sb">StringBuilder for error reporting.</param>
    /// <returns>true if valid, false otherwise.</returns>
    private bool ValidateSpriteIds(AnimatedSpriteDefinitions definitions,
        StringBuilder sb)
    {
        var badSprites = definitions.AnimatedSprites
            .Where(sprite => sprite.Faces.Values.Any(face => HasInvalidSpriteIds(face.SpriteIds)));
        var valid = !badSprites.Any();

        if (!valid)
        {
            sb.Append("The following animated sprites reference unknown sprites:\n\n");
            foreach (var sprite in badSprites) sb.Append("Animated Sprite ").Append(sprite.Id).Append("\n");
        }

        return valid;
    }

    /// <summary>
    ///     Checks whether any sprite IDs are unknown.
    /// </summary>
    /// <param name="spriteIds">Sprite IDs to check.</param>
    /// <returns>true if there are unknown sprite IDs, false otherwise.</returns>
    private bool HasInvalidSpriteIds(IEnumerable<int> spriteIds)
    {
        return !spriteIds
            .Select(id => id >= 0 && id < spriteManager.Sprites.Count)
            .Aggregate((a, b) => a && b);
    }

    /// <summary>
    ///     Checks whether any sprites are missing their default South face.
    /// </summary>
    /// <param name="definitions">Definitions.</param>
    /// <param name="sb">StringBuilder used to build an error message if any.</param>
    /// <returns>true if valid, false otherwise.</returns>
    private bool ValidateDefaultFaces(AnimatedSpriteDefinitions definitions,
        StringBuilder sb)
    {
        var badSprites = definitions.AnimatedSprites
            .Where(sprite => !sprite.Faces.ContainsKey(Orientation.South));
        var valid = !badSprites.Any();

        if (!valid)
        {
            sb.Append("The following animated sprites lack a default South face:\n\n");
            foreach (var sprite in badSprites) sb.Append("Animated Sprite ").Append(sprite.Id).Append("\n");
        }

        return valid;
    }
}