// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Sovereign.EngineCore.Components.Types;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.ServerNetwork.Network.Pipeline.Outbound.ConnectionMappers;

/// <summary>
///     Builds and caches regional connection mapping sets.
/// </summary>
public class RegionalConnectionMapCache
{
    /// <summary>
    ///     Map from central segment index and radius to the corresponding cached entity set.
    /// </summary>
    private readonly Dictionary<Tuple<GridPosition, uint>, HashSet<ulong>> cachedSets = new();

    /// <summary>
    ///     Map from central segment index and radius to corresponding set of world segments.
    /// </summary>
    private readonly Dictionary<Tuple<GridPosition, uint>, HashSet<GridPosition>> radiusSets = new();

    /// <summary>
    ///     Sets which are valid for the current tick.
    /// </summary>
    private readonly HashSet<Tuple<GridPosition, uint>> validSets = new();

    private readonly WorldManagementServices worldManagementServices;

    public RegionalConnectionMapCache(WorldManagementServices worldManagementServices)
    {
        this.worldManagementServices = worldManagementServices;
    }

    /// <summary>
    ///     Gets the set of player entity IDs which are subscribed to any world segment in a cube
    ///     with the given radius centered on the given world segment.
    /// </summary>
    /// <param name="segmentIndex">Central world segment index.</param>
    /// <param name="radius">Radius in world segments.</param>
    /// <returns>Player entity IDs.</returns>
    public IReadOnlySet<ulong> GetPlayersNearWorldSegment(GridPosition segmentIndex, uint radius)
    {
        // For zero-radius requests, no need to build an overlap set or cache any results.
        if (radius == 0) return worldManagementServices.GetPlayersSubscribedToWorldSegment(segmentIndex);

        // Use cached result if available.
        var key = Tuple.Create(segmentIndex, radius);
        if (validSets.Contains(key)) return cachedSets[key];

        // No cached result was available, so compute and cache the result.
        if (!cachedSets.TryGetValue(key, out var playerSet))
        {
            playerSet = new HashSet<ulong>();
            cachedSets[key] = playerSet;
        }

        playerSet.Clear();
        foreach (var currentSegment in GetRadiusSet(key))
            playerSet.UnionWith(worldManagementServices.GetPlayersSubscribedToWorldSegment(currentSegment));

        validSets.Add(key);
        return playerSet;
    }

    /// <summary>
    ///     Called when a new tick begins, invalidating the cache.
    /// </summary>
    /// <remarks>
    ///     We can probably do better to invalidate the cache less often by keying this off
    ///     of subscribe/unsubscribe events. There's enough factors involved, however, that there
    ///     could be an unfavorable tradeoff - so we do the simple thing until we run into a
    ///     performance issue and can benchmark it.
    /// </remarks>
    public void OnTick()
    {
        validSets.Clear();
    }

    /// <summary>
    ///     Gets the set of world segment indices for the given center and radius.
    /// </summary>
    /// <param name="key">Center and radius key.</param>
    /// <returns>Radius set.</returns>
    private HashSet<GridPosition> GetRadiusSet(Tuple<GridPosition, uint> key)
    {
        if (!radiusSets.TryGetValue(key, out var radiusSet))
        {
            radiusSet = new HashSet<GridPosition>();
            var (center, radius) = key;
            for (var i = center.X - (int)radius; i < center.X + radius + 1; ++i)
            for (var j = center.Y - (int)radius; j < center.Y + radius + 1; ++j)
            for (var k = center.Z - (int)radius; k < center.Z + radius + 1; ++k)
                radiusSet.Add(new GridPosition(i, j, k));

            radiusSets[key] = radiusSet;
        }

        return radiusSet;
    }
}