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
using System.Collections.Generic;
using Sovereign.EngineCore.Components.Types;

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
        NortheastTileSpriteId = other.NortheastTileSpriteId;
        SoutheastTileSpriteId = other.SoutheastTileSpriteId;
        SouthwestTileSpriteId = other.SouthwestTileSpriteId;
        NorthwestTileSpriteId = other.NorthwestTileSpriteId;
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
    ///     Index of the northeast tile sprite to match.
    /// </summary>
    public int NortheastTileSpriteId { get; set; }

    /// <summary>
    ///     Index of the southeast tile sprite to match.
    /// </summary>
    public int SoutheastTileSpriteId { get; set; }

    /// <summary>
    ///     Index of the southwest tile sprite to match.
    /// </summary>
    public int SouthwestTileSpriteId { get; set; }

    /// <summary>
    ///     Index of the northwest tile sprite to match.
    /// </summary>
    public int NorthwestTileSpriteId { get; set; }

    /// <summary>
    ///     List of animated sprite IDs to be drawn in order if the context matches.
    /// </summary>
    public List<int> AnimatedSpriteIds { get; set; } = new();

    /// <summary>
    ///     Tile context key associated with this tile context.
    /// </summary>
    public TileContextKey TileContextKey => new(NorthTileSpriteId, NortheastTileSpriteId,
        EastTileSpriteId, SoutheastTileSpriteId, SouthTileSpriteId, SouthwestTileSpriteId, WestTileSpriteId,
        NorthwestTileSpriteId);

    /// <summary>
    ///     Determines whether the context matches the given neighboring tile IDs.
    /// </summary>
    /// <param name="northId">North neighbor tile ID.</param>
    /// <param name="northEastId">Northeast neighbor tile ID.</param>
    /// <param name="eastId">East neighbor tile ID.</param>
    /// <param name="southEastId">Southeast neighbor tile ID.</param>
    /// <param name="southId">South neighbor tile ID.</param>
    /// <param name="southWestId">Southwest neighbor tile ID.</param>
    /// <param name="westId">West neighbor tile ID.</param>
    /// <param name="northWestId">Northwest neighbor tile ID.</param>
    /// <returns></returns>
    public bool IsMatch(int northId, int northEastId, int eastId, int southEastId, int southId, int southWestId,
        int westId, int northWestId)
    {
        return (NorthTileSpriteId == TileSprite.Wildcard || NorthTileSpriteId == northId)
               && (EastTileSpriteId == TileSprite.Wildcard || EastTileSpriteId == eastId)
               && (SouthTileSpriteId == TileSprite.Wildcard || SouthTileSpriteId == southId)
               && (WestTileSpriteId == TileSprite.Wildcard || WestTileSpriteId == westId)
               && (NortheastTileSpriteId == TileSprite.Wildcard || NortheastTileSpriteId == northEastId)
               && (SoutheastTileSpriteId == TileSprite.Wildcard || SoutheastTileSpriteId == southEastId)
               && (SouthwestTileSpriteId == TileSprite.Wildcard || SouthwestTileSpriteId == southWestId)
               && (NorthwestTileSpriteId == TileSprite.Wildcard || NorthwestTileSpriteId == northWestId);
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
               + (WestTileSpriteId == TileSprite.Wildcard ? 1 : 0)
               + (NortheastTileSpriteId == TileSprite.Wildcard ? 1 : 0)
               + (SoutheastTileSpriteId == TileSprite.Wildcard ? 1 : 0)
               + (SouthwestTileSpriteId == TileSprite.Wildcard ? 1 : 0)
               + (NortheastTileSpriteId == TileSprite.Wildcard ? 1 : 0);
    }

    /// <summary>
    ///     Gets the tile sprite ID associated with the given direction.
    /// </summary>
    /// <param name="orientation">Direction.</param>
    /// <returns>Tile sprite ID.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if an invalid direction is provided.</exception>
    public int GetNeighborTileSpriteId(Orientation orientation)
    {
        return orientation switch
        {
            Orientation.East => EastTileSpriteId,
            Orientation.North => NorthTileSpriteId,
            Orientation.Northeast => NortheastTileSpriteId,
            Orientation.Northwest => NorthwestTileSpriteId,
            Orientation.South => SouthTileSpriteId,
            Orientation.Southeast => SoutheastTileSpriteId,
            Orientation.Southwest => SouthwestTileSpriteId,
            Orientation.West => WestTileSpriteId,
            _ => throw new ArgumentOutOfRangeException(nameof(orientation))
        };
    }
}