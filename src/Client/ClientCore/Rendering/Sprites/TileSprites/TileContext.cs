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

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites;

/// <summary>
///     Describes a context for converting tile sprites to sprites.
/// </summary>
public sealed class TileContext
{
    public TileContext()
    {
        AnimatedSpriteIds.Add(0);
    }

    /// <summary>
    ///     Copy constructor.
    /// </summary>
    /// <param name="other">Context to copy.</param>
    public TileContext(TileContext other)
    {
        NorthTileSpriteId = other.NorthTileSpriteId;
        EastTileSpriteId = other.EastTileSpriteId;
        SouthTileSpriteId = other.SouthTileSpriteId;
        WestTileSpriteId = other.WestTileSpriteId;
        AnimatedSpriteIds = new List<int>(other.AnimatedSpriteIds);
    }

    /// <summary>
    ///     Index of the north tile sprite to match (-1 for wildcard).
    /// </summary>
    public int NorthTileSpriteId { get; set; }

    /// <summary>
    ///     Index of the east tile sprite to match (-1 for wildcard).
    /// </summary>
    public int EastTileSpriteId { get; set; }

    /// <summary>
    ///     Index of the south tile sprite to match (-1 for wildcard).
    /// </summary>
    public int SouthTileSpriteId { get; set; }


    /// <summary>
    ///     Index of the west tile sprite to match (-1 for wildcard).
    /// </summary>
    public int WestTileSpriteId { get; set; }

    /// <summary>
    ///     List of animated sprite IDs to be drawn in order if the context matches.
    /// </summary>
    public List<int> AnimatedSpriteIds { get; set; } = new();

    /// <summary>
    ///     Determines whether the context matches the given neighboring tile IDs.
    /// </summary>
    /// <param name="northId">North neighbor tile ID.</param>
    /// <param name="eastId">East neighbor tile ID.</param>
    /// <param name="southId">South neighbor tile ID.</param>
    /// <param name="westId">West neighbor tile ID.</param>
    /// <returns></returns>
    public bool IsMatch(int northId, int eastId, int southId, int westId)
    {
        return (NorthTileSpriteId == TileSprite.Wildcard || NorthTileSpriteId == northId)
               && (EastTileSpriteId == TileSprite.Wildcard || EastTileSpriteId == eastId)
               && (SouthTileSpriteId == TileSprite.Wildcard || SouthTileSpriteId == southId)
               && (WestTileSpriteId == TileSprite.Wildcard || WestTileSpriteId == westId);
    }

    /// <summary>
    ///     Gets the number of wildcards in the context.
    /// </summary>
    /// <returns>Number of wildcards in the context.</returns>
    public int GetWildcardCount()
    {
        return (NorthTileSpriteId == TileSprite.Wildcard ? 1 : 0)
               + (EastTileSpriteId == TileSprite.Wildcard ? 1 : 0)
               + (SouthTileSpriteId == TileSprite.Wildcard ? 1 : 0)
               + (WestTileSpriteId == TileSprite.Wildcard ? 1 : 0);
    }
}