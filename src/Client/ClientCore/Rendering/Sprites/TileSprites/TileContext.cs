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
using System.Runtime.CompilerServices;
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
    public int NorthTileSpriteId { get; set; } = TileSprite.Wildcard;

    /// <summary>
    ///     Index of the east tile sprite to match (-1 for wildcard).
    /// </summary>
    public int EastTileSpriteId { get; set; } = TileSprite.Wildcard;

    /// <summary>
    ///     Index of the south tile sprite to match (-1 for wildcard).
    /// </summary>
    public int SouthTileSpriteId { get; set; } = TileSprite.Wildcard;

    /// <summary>
    ///     Index of the west tile sprite to match (-1 for wildcard).
    /// </summary>
    public int WestTileSpriteId { get; set; } = TileSprite.Wildcard;

    /// <summary>
    ///     Index of the northeast tile sprite to match.
    /// </summary>
    public int NortheastTileSpriteId { get; set; } = TileSprite.Wildcard;

    /// <summary>
    ///     Index of the southeast tile sprite to match.
    /// </summary>
    public int SoutheastTileSpriteId { get; set; } = TileSprite.Wildcard;

    /// <summary>
    ///     Index of the southwest tile sprite to match.
    /// </summary>
    public int SouthwestTileSpriteId { get; set; } = TileSprite.Wildcard;

    /// <summary>
    ///     Index of the northwest tile sprite to match.
    /// </summary>
    public int NorthwestTileSpriteId { get; set; } = TileSprite.Wildcard;

    /// <summary>
    ///     List of animated sprite IDs to be drawn in order if the context matches.
    /// </summary>
    public List<int> AnimatedSpriteIds { get; set; } = new();

    /// <summary>
    ///     Tile context key associated with this tile context.
    /// </summary>
    public TileContextKey TileContextKey => new(NorthTileSpriteId, NortheastTileSpriteId,
        EastTileSpriteId, SoutheastTileSpriteId, SouthTileSpriteId, SouthwestTileSpriteId, WestTileSpriteId,
        NorthwestTileSpriteId, 0);

    /// <summary>
    ///     Determines whether the context matches the given neighboring tile IDs.
    /// </summary>
    /// <param name="context">Tile context.</param>
    /// <returns>true if a match, false otherwise.</returns>
    public bool IsMatch(TileContextKey context)
    {
        return
            DirectionMatches(NorthTileSpriteId, context.NorthId,
                (context.ObscuredNeighbors & DirectionFlag.North) > 0) &&
            DirectionMatches(NortheastTileSpriteId, context.NortheastId,
                (context.ObscuredNeighbors & DirectionFlag.Northeast) > 0) &&
            DirectionMatches(EastTileSpriteId, context.EastId,
                (context.ObscuredNeighbors & DirectionFlag.East) > 0) &&
            DirectionMatches(SoutheastTileSpriteId, context.SoutheastId,
                (context.ObscuredNeighbors & DirectionFlag.Southeast) > 0) &&
            DirectionMatches(SouthTileSpriteId, context.SouthId,
                (context.ObscuredNeighbors & DirectionFlag.South) > 0) &&
            DirectionMatches(SouthwestTileSpriteId, context.SouthwestId,
                (context.ObscuredNeighbors & DirectionFlag.Southwest) > 0) &&
            DirectionMatches(WestTileSpriteId, context.WestId,
                (context.ObscuredNeighbors & DirectionFlag.West) > 0) &&
            DirectionMatches(NorthwestTileSpriteId, context.NorthwestId,
                (context.ObscuredNeighbors & DirectionFlag.Northwest) > 0);
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

    /// <summary>
    ///     Checks whether a pattern matches a tile along a single direction.
    /// </summary>
    /// <param name="patternId">Tile ID from the pattern rule.</param>
    /// <param name="tileId">Tile ID of the neighboring tile in the direction being checked.</param>
    /// <param name="obscured">If true, indicates the neighboring tile is obscured.</param>
    /// <returns>true if the pattern matches, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool DirectionMatches(int patternId, int tileId, bool obscured)
    {
        return patternId == TileSprite.Wildcard || (patternId == TileSprite.Obscured && obscured) ||
               patternId == tileId;
    }
}