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

using System.Linq;
using System.Text;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineUtil.Validation;
using static Sovereign.ClientCore.Rendering.Sprites.TileSprites.TileSpriteDefinitions;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites;

/// <summary>
///     Validates tile sprite definitions.
/// </summary>
public sealed class TileSpriteDefinitionsValidator
{
    private readonly AnimatedSpriteManager animatedSpriteManager;

    public TileSpriteDefinitionsValidator(AnimatedSpriteManager animatedSpriteManager)
    {
        this.animatedSpriteManager = animatedSpriteManager;
    }

    /// <summary>
    ///     Validates the given definitions.
    /// </summary>
    /// <param name="tileSpriteDefinitions">Definitions to validate.</param>
    /// <exception cref="TileSpriteDefinitionsException">
    ///     Thrown if the definitions are invalid.
    /// </exception>
    public void Validate(TileSpriteDefinitions tileSpriteDefinitions)
    {
        var sb = new StringBuilder();

        /* Validate. */
        var valid = ValidateIds(tileSpriteDefinitions, sb)
                    && ValidateNoEmptyTileContexts(tileSpriteDefinitions, sb)
                    && ValidateTileContextIdRanges(tileSpriteDefinitions, sb)
                    && ValidateDefaultContextPresent(tileSpriteDefinitions, sb)
                    && ValidateAnimatedSpritesExist(tileSpriteDefinitions, sb);
        if (!valid) throw new TileSpriteDefinitionsException(sb.ToString().Trim());
    }

    /// <summary>
    ///     Checks that IDs are not duplicated and run from 0 to n - 1.
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
        var valid = !(hasDuplicates || hasOutOfRanges);

        if (!valid)
        {
            if (hasDuplicates)
            {
                sb.Append("The following tile sprite IDs are duplicated:\n\n");
                foreach (var id in duplicateIds) sb.Append("Tile Sprite ").Append(id).Append("\n");
            }

            if (hasDuplicates && hasOutOfRanges) sb.Append("\n");

            if (hasOutOfRanges)
            {
                sb.Append("Tile sprite IDs must run consecutively from 0.\n")
                    .Append("The following IDs are out of range:\n\n");
                foreach (var id in outOfRangeIds) sb.Append("Tile Sprite ").Append(id).Append("\n");
            }
        }

        return valid;
    }

    /// <summary>
    ///     Validates that the definitions do not contain any empty context lists.
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
            foreach (var tileSprite in emptyTileSprites) sb.Append("Tile Sprite ").Append(tileSprite.Id).Append("\n");
            sb.Append("\n");
        }

        return valid;
    }

    /// <summary>
    ///     Validates that no tile context neighbor IDs are out of range.
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
                sb.Append("Tile Sprite ").Append(tileSprite.Id).Append("\n");
            sb.Append("\n");
        }

        return valid;
    }

    /// <summary>
    ///     Checks that every tile sprite has at least a default tile context.
    /// </summary>
    /// <param name="definitions">Definitions to validate.</param>
    /// <param name="sb">StringBuilder for error reporting.</param>
    /// <returns>true if valid, false otherwise.</returns>
    private bool ValidateDefaultContextPresent(TileSpriteDefinitions definitions,
        StringBuilder sb)
    {
        var badSprites = definitions.TileSprites
            .Where(sprite => sprite.TileContexts != null)
            .Where(sprite => !sprite.TileContexts
                .Select(IsDefaultContext)
                .Aggregate((a, b) => a || b));
        var valid = badSprites.Count() == 0;

        if (!valid)
        {
            sb.Append("All tile sprites must have a default tile context.\n"
                      + "The following tile sprites do not have a default tile context:\n\n");
            foreach (var sprite in badSprites) sb.Append("Tile Sprite ").Append(sprite.Id).Append("\n");
        }

        return valid;
    }

    /// <summary>
    ///     Checks that all animated sprite references are valid.
    /// </summary>
    /// <param name="definitions">Definitions to validate.</param>
    /// <param name="sb">StringBuilder for error reporting.</param>
    /// <returns>true if valid, false otherwise.</returns>
    private bool ValidateAnimatedSpritesExist(TileSpriteDefinitions definitions,
        StringBuilder sb)
    {
        var badIds = definitions.TileSprites
            .Where(sprite => sprite.TileContexts
                .Any(context => context.AnimatedSpriteIds
                    .Any(id => id >= animatedSpriteManager.AnimatedSprites.Count)))
            .Select(sprite => sprite.Id);
        var valid = !badIds.Any();

        if (!valid)
        {
            sb.Append("The following tile sprites contained references to unknown Animated Sprites:\n\n");
            foreach (var id in badIds) sb.Append("Tile Sprite ").Append(id).Append("\n");
        }

        return valid;
    }

    /// <summary>
    ///     Checks whether the given tile sprite contains duplicate tile contexts.
    /// </summary>
    /// <param name="tile">Tile sprite record to check.</param>
    /// <returns>true if duplicates are found, false otherwise.</returns>
    private bool HasDuplicateContexts(TileSpriteRecord tile)
    {
        var contexts = tile.TileContexts;
        for (var i = 0; i < contexts.Count - 1; ++i)
        for (var j = i + 1; j < contexts.Count; ++j)
            if (contexts[i].NorthTileSpriteId == contexts[j].NorthTileSpriteId
                && contexts[i].EastTileSpriteId == contexts[j].EastTileSpriteId
                && contexts[i].SouthTileSpriteId == contexts[j].SouthTileSpriteId
                && contexts[i].WestTileSpriteId == contexts[j].WestTileSpriteId)
                return true;
        return false;
    }

    /// <summary>
    ///     Determines if the given tile context has any neighbor IDs that are
    ///     out of range.
    /// </summary>
    /// <param name="context">Tile context.</param>
    /// <returns>true if out of range, false otherwise.</returns>
    private bool TileContextHasOutOfRangeIds(TileContext context)
    {
        return context.NorthTileSpriteId < TileSprite.Empty
               || context.EastTileSpriteId < TileSprite.Empty
               || context.SouthTileSpriteId < TileSprite.Empty
               || context.WestTileSpriteId < TileSprite.Empty
               || context.NortheastTileSpriteId < TileSprite.Empty
               || context.SoutheastTileSpriteId < TileSprite.Empty
               || context.SouthwestTileSpriteId < TileSprite.Empty
               || context.NorthwestTileSpriteId < TileSprite.Empty;
    }

    /// <summary>
    ///     Determines if the given tile context is a default context that
    ///     matches all neighboring tiles.
    /// </summary>
    /// <param name="context">Tile context.</param>
    /// <returns>true if default, false otherwise.</returns>
    private bool IsDefaultContext(TileContext context)
    {
        return context.TileContextKey.Equals(TileContextKey.AllWildcards);
    }
}