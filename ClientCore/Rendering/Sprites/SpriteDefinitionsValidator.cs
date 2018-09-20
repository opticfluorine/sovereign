using Sovereign.EngineUtil.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites
{
    
    /// <summary>
    /// Validates sprite definitions.
    /// </summary>
    public sealed class SpriteDefinitionsValidator
    {

        /// <summary>
        /// Spritesheet manager.
        /// </summary>
        private readonly SpriteSheetManager spriteSheetManager;

        public SpriteDefinitionsValidator(SpriteSheetManager spriteSheetManager)
        {
            this.spriteSheetManager = spriteSheetManager;
        }

        /// <summary>
        /// Validates the given sprite definitions.
        /// </summary>
        /// <param name="definitions">Sprite definitions to validate.</param>
        /// <exception cref="SpriteDefinitionsException">
        /// Thrown if the sprite definitions are invalid.
        /// </exception>
        public void Validate(SpriteDefinitions definitions)
        {
            var sb = new StringBuilder();

            var valid = ValidateSpritesheetNames(definitions, sb)
                && ValidateIds(definitions, sb)
                && ValidateRowsCols(definitions, sb);

            if (!valid)
                throw new SpriteDefinitionsException(sb.ToString().Trim());
        }

        /// <summary>
        /// Checks that there are no references to unknown spritesheets.
        /// </summary>
        /// <param name="definitions">Definitions to validate.</param>
        /// <param name="sb">StringBuilder for error reporting.</param>
        /// <returns>true if valid, false otherwise.</returns>
        private bool ValidateSpritesheetNames(SpriteDefinitions definitions, StringBuilder sb)
        {
            var knownSpritesheets = spriteSheetManager.SpriteSheets.Keys;
            var badSprites = definitions.Sprites
                .Where(sprite => !knownSpritesheets.Contains(sprite.SpritesheetName));
            var valid = badSprites.Count() == 0;

            if (!valid)
            {
                sb.Append("The following sprites refer to unknown spritesheets:\n\n");
                foreach (var sprite in badSprites)
                {
                    sb.Append("Sprite ").Append(sprite.Id).Append(" - ")
                        .Append(sprite.SpritesheetName).Append("\n");
                }
            }

            return valid;
        }

        /// <summary>
        /// Checks that all row and column indices are non-negative.
        /// </summary>
        /// <param name="definitions">Definitions to validate.</param>
        /// <param name="sb">StringBuilder for error reporting.</param>
        /// <returns>true if valid, false otherwise.</returns>
        private bool ValidateRowsCols(SpriteDefinitions definitions, StringBuilder sb)
        {
            var badSprites = definitions.Sprites
                .Where(sprite => sprite.Row < 0 || sprite.Column < 0);
            var valid = badSprites.Count() == 0;

            if (!valid)
            {
                sb.Append("The following sprites have negative row and/or column indices:\n\n");
                foreach (var sprite in badSprites)
                {
                    sb.Append("Sprite ").Append(sprite.Id)
                        .Append(" - Row ").Append(sprite.Row)
                        .Append(", Column ").Append(sprite.Column)
                        .Append("\n");
                }
            }

            return valid;
        }

        /// <summary>
        /// Checks that IDs run from 0 to n consecutively, in any order,
        /// and do not contain duplicates.
        /// </summary>
        /// <param name="definitions">Definitions to validate.</param>
        /// <param name="sb">StringBuilder for error reporting.</param>
        /// <returns>true if valid, false otherwise.</returns>
        private bool ValidateIds(SpriteDefinitions definitions, StringBuilder sb)
        {
            var spriteCount = definitions.Sprites.Count;
            var validator = new ConsecutiveRangeValidation();
            validator.IsRangeConsecutive(definitions.Sprites.Select(sprite => sprite.Id),
                0, spriteCount, out var duplicateIds, out var outOfRangeIds);

            var hasDuplicates = duplicateIds.Count > 0;
            var hasOutOfRanges = outOfRangeIds.Count > 0;
            var valid = hasDuplicates || hasOutOfRanges;

            if (!valid)
            {
                if (hasDuplicates)
                {
                    sb.Append("The following sprite IDs are duplicated:\n\n");
                    foreach (var id in duplicateIds)
                    {
                        sb.Append("Sprite ").Append(id).Append("\n");
                    }
                }

                if (hasDuplicates && hasOutOfRanges) sb.Append("\n");

                if (hasOutOfRanges)
                {
                    sb.Append("Sprite IDs must run consecutively from 0.\n")
                        .Append("The following IDs are out of range:\n\n");
                    foreach (var id in outOfRangeIds)
                    {
                        sb.Append("Sprite ").Append(id).Append("\n");
                    }
                }
            }

            return valid;
        }

    }

}
