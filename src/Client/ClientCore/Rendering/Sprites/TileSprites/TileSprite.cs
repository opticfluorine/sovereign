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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static Sovereign.ClientCore.Rendering.Sprites.TileSprites.TileSpriteDefinitions;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites;

/// <summary>
///     Describes a tile sprite that changes its appearance based on
///     its context (surrounding tile sprites).
/// </summary>
public sealed class TileSprite
{
    /// <summary>
    ///     Indicates a wildcard neighboring tile sprite.
    /// </summary>
    public const int Wildcard = -1;

    /// <summary>
    ///     Cache of previously resolved tile contexts.
    /// </summary>
    private readonly IDictionary<Tuple<int, int, int, int>, TileContext> lookupCache
        = new ConcurrentDictionary<Tuple<int, int, int, int>, TileContext>();

    /// <summary>
    ///     Tile contexts sorted in priority order.
    /// </summary>
    private readonly IList<TileContext> tileContexts;

    public TileSprite(TileSpriteRecord definition)
    {
        Id = definition.Id;
        tileContexts = SortContexts(definition.TileContexts);
    }

    /// <summary>
    ///     Tile sprite ID.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    ///     Finds the animated sprites for the tile context that matches
    ///     the given bordering tiles.
    /// </summary>
    /// This method should only be called from the rendering thread.
    /// <param name="idNorth">ID of the north tile sprite.</param>
    /// <param name="idEast">ID of the east tile sprite.</param>
    /// <param name="idSouth">ID of the south tile sprite.</param>
    /// <param name="idWest">ID of the west tile sprite.</param>
    /// <returns>Matching tile context.</returns>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown if no matching tile context is found. This should not typically occur as
    ///     there should always be a default context that matches all ID patterns.
    /// </exception>
    public IList<int> GetMatchingAnimatedSpriteIds(int idNorth, int idEast,
        int idSouth, int idWest)
    {
        /* Check if the context is already in the cache. */
        var ids = new Tuple<int, int, int, int>(idNorth, idEast, idSouth, idWest);
        if (lookupCache.TryGetValue(ids, out var cachedValues))
            return cachedValues.AnimatedSpriteIds;

        /* Context not found in cache - resolve. */
        var context = ResolveContext(ids);
        lookupCache[ids] = context;
        return context.AnimatedSpriteIds;
    }

    /// <summary>
    ///     Resolves the tile context for the given neighboring IDs.
    /// </summary>
    /// <param name="ids">4-tuple of neighboring IDs (north, east, south, west).</param>
    /// <returns>Resolved tile context.</returns>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown if no matching tile context is found. This should not typically occur as
    ///     there should always be a default context that matches all ID patterns.
    /// </exception>
    private TileContext ResolveContext(Tuple<int, int, int, int> ids)
    {
        /* Since the contexts are sorted in priority order, iterate until match. */
        foreach (var context in tileContexts)
            if (context.IsMatch(ids.Item1, ids.Item2, ids.Item3, ids.Item4))
                return context;

        /* We should at least match the default context, so throw an exception. */
        throw new KeyNotFoundException("No matching context found.");
    }

    /// <summary>
    ///     Sorts the tile contexts by priority.
    /// </summary>
    /// <param name="contexts">Tile contexts to be sorted.</param>
    /// <returns>Sorted list of tile contexts.</returns>
    private IList<TileContext> SortContexts(IEnumerable<TileContext> contexts)
    {
        return contexts.OrderBy(context => context.GetWildcardCount())
            .ThenByDescending(context => context.NorthTileSpriteId)
            .ThenByDescending(context => context.EastTileSpriteId)
            .ThenByDescending(context => context.SouthTileSpriteId)
            .ThenByDescending(context => context.WestTileSpriteId)
            .ToList();
    }
}