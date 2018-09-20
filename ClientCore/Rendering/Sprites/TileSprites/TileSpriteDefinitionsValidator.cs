using Sovereign.EngineUtil.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites
{

    /// <summary>
    /// Validates tile sprite definitions.
    /// </summary>
    public sealed class TileSpriteDefinitionsValidator
    {

        /// <summary>
        /// Validates the given definitions.
        /// </summary>
        /// <param name="tileSpriteDefinitions">Definitions to validate.</param>
        /// <exception cref="TileSpriteDefinitionsException">
        /// Thrown if the definitions are invalid.
        /// </exception>
        public void Validate(TileSpriteDefinitions tileSpriteDefinitions)
        {
            var sb = new StringBuilder();

            /* Validate. */
            var valid = ValidateIds(tileSpriteDefinitions, sb)
                && ValidateNoEmptyTileContexts(tileSpriteDefinitions, sb)
                && ValidateTileContextIdRanges(tileSpriteDefinitions, sb);
            if (!valid)
            {
                throw new TileSpriteDefinitionsException(sb.ToString().Trim());
            }
        }

        /// <summary>
        /// Checks that IDs are not duplicated and run from 0 to n - 1.
        /// </summary>
        /// <param name="definitions">Definitions.</param>
        /// <param name="sb">StringBuilder for error reporting.</param>
        /// <returns>true if valid, false otherwise.</returns>
        private bool ValidateIds(TileSpriteDefinitions definitions,
            StringBuilder sb)
        {
            var spriteCount = definitions.TileSprites.Count;
            var validator = new ConsecutiveRangeValidation();
            validator.IsRangeConsecutive(definitions.TileSprites.Select(sprite => sprite.Id),
                0, spriteCount, out var duplicateIds, out var outOfRangeIds);

            var hasDuplicates = duplicateIds.Count > 0;
            var hasOutOfRanges = outOfRangeIds.Count > 0;
            var valid = hasDuplicates || hasOutOfRanges;

            if (!valid)
            {
                if (hasDuplicates)
                {
                    sb.Append("The following tile sprite IDs are duplicated:\n\n");
                    foreach (var id in duplicateIds)
                    {
                        sb.Append("Tile Sprite ").Append(id).Append("\n");
                    }
                }

                if (hasDuplicates && hasOutOfRanges) sb.Append("\n");

                if (hasOutOfRanges)
                {
                    sb.Append("Tile sprite IDs must run consecutively from 0.\n")
                        .Append("The following IDs are out of range:\n\n");
                    foreach (var id in outOfRangeIds)
                    {
                        sb.Append("Tile Sprite ").Append(id).Append("\n");
                    }
                }
            }

            return valid;
        }

        /// <summary>
        /// Validates that the definitions do not contain any empty context lists.
        /// </summary>
        /// <param name="definitions">Tile sprite definitions.</param>
        /// <param name="sb">StringBuilder for errors.</param>
        /// <returns>true if valid, false otherwise.</returns>
        private bool ValidateNoEmptyTileContexts(TileSpriteDefinitions definitions,
            StringBuilder sb)
        {
            /* Identify any empty context lists. */
            var emptyTileSprites = definitions.TileSprites
                .Where(tileSprite => tileSprite.TileContexts == null
                || tileSprite.TileContexts.Count == 0);
            var valid = emptyTileSprites.Count() == 0;
            if (!valid)
            {
                /* Append error message. */
                sb.Append("Tile sprites must have at least one tile context.\n"
                    + "The following tile sprites do not have tile contexts:\n\n");
                foreach (var tileSprite in emptyTileSprites)
                {
                    sb.Append("Tile Sprite ").Append(tileSprite.Id).Append("\n");
                }
                sb.Append("\n");
            }

            return valid;
        }

        /// <summary>
        /// Validates that no tile context neighbor IDs are out of range.
        /// </summary>
        /// <param name="definitions">Definitions to validate.</param>
        /// <param name="sb">StringBuilder for errors.</param>
        /// <returns>true if valid, false otherwise.</returns>
        private bool ValidateTileContextIdRanges(TileSpriteDefinitions definitions,
            StringBuilder sb)
        {
            /* Identify tile sprites that have bad contexts. */
            var spritesWithBadContexts = definitions.TileSprites
                .Where(tileSprite => tileSprite.TileContexts != null
                     && tileSprite.TileContexts.Select(TileContextHasOutOfRangeIds)
                    .Aggregate((a, b) => a && b));
            var valid = spritesWithBadContexts.Count() == 0;

            /* Output error message if needed. */
            if (!valid)
            {
                /* Start error message. */
                sb.Append("Tile context neighbor IDs must be greater than or equal to -1.\n"
                    + "The following tile sprites have invalid contexts:\n\n");
                foreach (var tileSprite in spritesWithBadContexts)
                {
                    sb.Append("Tile Sprite ").Append(tileSprite.Id).Append("\n");
                }
                sb.Append("\n");
            }
            return valid;
        }

        /// <summary>
        /// Determines if the given tile context has any neighbor IDs that are
        /// out of range.
        /// </summary>
        /// <param name="context">Tile context.</param>
        /// <returns>true if out of range, false otherwise.</returns>
        private bool TileContextHasOutOfRangeIds(TileContext context)
        {
            return context.NorthTileSpriteId < -1
                || context.EastTileSpriteId < -1
                || context.SouthTileSpriteId < -1
                || context.WestTileSpriteId < -1;
        }

    }

}
