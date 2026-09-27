// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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
using System.Linq;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen.Hydrology;

/// <summary>
///     Counts of extracted hydrology features.
/// </summary>
public sealed class RiverExtractionResult
{
    /// <summary>
    ///     Number of extracted rivers.
    /// </summary>
    public required int RiverCount { get; init; }

    /// <summary>
    ///     Number of extracted lakes.
    /// </summary>
    public required int LakeCount { get; init; }
}

/// <summary>
///     Extracts lakes from the filled routing surface and rivers from the flow routing,
///     grading river widths and marking banks.
/// </summary>
public sealed class RiverExtractor
{
    /// <summary>
    ///     Flow accumulation, in draining cells, at which a land cell carries a river.
    ///     Chosen to yield a sane river density at the reference footprint.
    /// </summary>
    private const int RiverThreshold = 64;

    /// <summary>
    ///     Accumulation multiple below which rivers are one block wide.
    /// </summary>
    private const int WidthGrade1Multiple = 8;

    /// <summary>
    ///     Accumulation multiple below which rivers are two blocks wide.
    /// </summary>
    private const int WidthGrade2Multiple = 32;

    /// <summary>
    ///     Fill depth in blocks at which a depression becomes a lake.
    /// </summary>
    private const int LakeDepthThreshold = 3;

    private readonly ulong seed;

    /// <summary>
    ///     Creates a river extractor.
    /// </summary>
    /// <param name="seed">Sub-seed used to order rivers of equal length.</param>
    public RiverExtractor(ulong seed)
    {
        this.seed = seed;
    }

    /// <summary>
    ///     Extracts lakes and rivers onto the terrain map.
    /// </summary>
    /// <param name="map">Terrain map to update.</param>
    /// <param name="filledHeights">Epsilon-filled routing surface indexed [x, y].</param>
    /// <param name="sillHeights">Sill-filled surface indexed [x, y]; depressions are raised
    /// exactly to their sill level, which is the water plane of a lake.</param>
    /// <param name="routing">Flow routing over the filled surface.</param>
    /// <param name="profile">World generation profile.</param>
    /// <returns>Extraction counts.</returns>
    public RiverExtractionResult Extract(TerrainMap map, int[,] filledHeights, int[,] sillHeights,
        FlowRouting routing, WorldGenProfile profile)
    {
        var lakeCount = ExtractLakes(map, sillHeights, profile);
        if (profile.Rivers is not { } rivers)
        {
            return new RiverExtractionResult { RiverCount = 0, LakeCount = lakeCount };
        }

        var riverCount = ExtractRivers(map, routing, profile, rivers);
        MarkBanks(map, profile.Width, profile.Height);
        return new RiverExtractionResult { RiverCount = riverCount, LakeCount = lakeCount };
    }

    /// <summary>
    ///     Marks lakes: connected depressions deep enough to hold water. The water plane sits
    ///     at the sill height of the depression.
    /// </summary>
    /// <param name="map">Terrain map to update.</param>
    /// <param name="sillHeights">Sill-filled surface.</param>
    /// <param name="profile">World generation profile.</param>
    /// <returns>Number of lakes.</returns>
    private static int ExtractLakes(TerrainMap map, int[,] sillHeights, WorldGenProfile profile)
    {
        var width = profile.Width;
        var height = profile.Height;
        var component = new int[width, height];
        var deepComponents = new HashSet<int>();
        var landLakes = new HashSet<int>();
        var componentCount = 0;

        var queue = new Queue<(int X, int Y)>();
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (component[x, y] != 0) continue;
                if (sillHeights[x, y] - map.Heights[x, y] < 1) continue;

                ++componentCount;
                queue.Clear();
                queue.Enqueue((x, y));
                component[x, y] = componentCount;
                while (queue.Count > 0)
                {
                    var (cx, cy) = queue.Dequeue();
                    if (sillHeights[cx, cy] - map.Heights[cx, cy] >= LakeDepthThreshold)
                    {
                        deepComponents.Add(componentCount);
                    }

                    EnqueueNeighbor(queue, component, sillHeights, map, cx - 1, cy, componentCount);
                    EnqueueNeighbor(queue, component, sillHeights, map, cx + 1, cy, componentCount);
                    EnqueueNeighbor(queue, component, sillHeights, map, cx, cy - 1, componentCount);
                    EnqueueNeighbor(queue, component, sillHeights, map, cx, cy + 1, componentCount);
                }
            }
        }

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                var c = component[x, y];
                if (c == 0 || !deepComponents.Contains(c)) continue;
                if (map.IsOcean[x, y]) continue;

                map.IsLake[x, y] = true;
                map.LakeSurfaceZ[x, y] = sillHeights[x, y];
                landLakes.Add(c);
            }
        }

        return landLakes.Count;
    }

    /// <summary>
    ///     Enqueues one 4-neighbor for depression flood fill if it belongs to the depression.
    /// </summary>
    /// <param name="queue">Breadth-first queue.</param>
    /// <param name="component">Component indices.</param>
    /// <param name="sillHeights">Sill-filled surface.</param>
    /// <param name="map">Terrain map.</param>
    /// <param name="x">Neighbor X coordinate.</param>
    /// <param name="y">Neighbor Y coordinate.</param>
    /// <param name="componentCount">Current component index.</param>
    private static void EnqueueNeighbor(Queue<(int X, int Y)> queue, int[,] component,
        int[,] sillHeights, TerrainMap map, int x, int y, int componentCount)
    {
        var width = component.GetLength(0);
        var height = component.GetLength(1);
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (component[x, y] != 0) return;
        if (sillHeights[x, y] - map.Heights[x, y] < 1) return;

        component[x, y] = componentCount;
        queue.Enqueue((x, y));
    }

    /// <summary>
    ///     Extracts rivers: maximal downstream paths of cells whose accumulation reaches the
    ///     river threshold, starting at cells with no above-threshold upstream. Rivers are
    ///     filtered by the profile minimum length and capped at the profile maximum count.
    /// </summary>
    /// <param name="map">Terrain map to update.</param>
    /// <param name="routing">Flow routing over the filled surface.</param>
    /// <param name="profile">World generation profile.</param>
    /// <param name="rivers">River generation options.</param>
    /// <returns>Number of extracted rivers.</returns>
    private int ExtractRivers(TerrainMap map, FlowRouting routing, WorldGenProfile profile,
        RiverOptions rivers)
    {
        var width = profile.Width;
        var height = profile.Height;
        var paths = new List<List<(int X, int Y)>>();

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (map.IsOcean[x, y]) continue;
                if (routing.Accumulation[x, y] < RiverThreshold) continue;
                if (HasUpstreamRiver(map, routing, x, y, width, height)) continue;

                var path = TracePath(map, routing, x, y, width);
                if (path is null) continue;
                if (path.Count < rivers.MinLength) continue;

                paths.Add(path);
            }
        }

        var selected = paths
            .OrderByDescending(p => p.Count)
            .ThenBy(p => HashKey(p[0]))
            .Take(rivers.MaxCount);

        foreach (var path in selected)
        {
            MarkRiver(map, routing, path, profile);
        }

        return Math.Min(paths.Count, rivers.MaxCount);
    }

    /// <summary>
    ///     Determines whether any upstream neighbor of a cell already carries the river,
    ///     meaning the cell is not a river source.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="routing">Flow routing.</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <param name="width">Footprint width.</param>
    /// <param name="height">Footprint height.</param>
    /// <returns>true if the cell has an upstream river cell, false otherwise.</returns>
    private static bool HasUpstreamRiver(TerrainMap map, FlowRouting routing,
        int x, int y, int width, int height)
    {
        for (var direction = 0; direction < 8; ++direction)
        {
            var nx = x + FlowRouter.DirectionDx[direction];
            var ny = y + FlowRouter.DirectionDy[direction];
            if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

            var receiver = routing.Receiver[nx, ny];
            if (receiver < 0) continue;
            if (receiver == y * width + x && routing.Accumulation[nx, ny] >= RiverThreshold
                && !map.IsOcean[nx, ny])
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Traces the downstream river path from a source cell until the flow reaches water
    ///     or leaves the footprint.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="routing">Flow routing.</param>
    /// <param name="x">Source X coordinate.</param>
    /// <param name="y">Source Y coordinate.</param>
    /// <param name="width">Footprint width.</param>
    /// <returns>The traced path, or null if the path leaves the footprint without reaching water.</returns>
    private static List<(int X, int Y)>? TracePath(TerrainMap map, FlowRouting routing,
        int x, int y, int width)
    {
        var path = new List<(int X, int Y)> { (x, y) };
        var cx = x;
        var cy = y;
        while (true)
        {
            var receiver = routing.Receiver[cx, cy];
            if (receiver < 0) return null;

            var rx = receiver % width;
            var ry = receiver / width;
            if (map.IsOcean[rx, ry]) return path;

            if (routing.Accumulation[rx, ry] < RiverThreshold) return null;

            path.Add((rx, ry));
            cx = rx;
            cy = ry;
        }
    }

    /// <summary>
    ///     Marks a river path on the terrain map, grading widths and carving the channel.
    /// </summary>
    /// <param name="map">Terrain map to update.</param>
    /// <param name="routing">Flow routing over the filled surface.</param>
    /// <param name="path">River path from source to water.</param>
    /// <param name="profile">World generation profile.</param>
    private static void MarkRiver(TerrainMap map, FlowRouting routing, List<(int X, int Y)> path,
        WorldGenProfile profile)
    {
        var minFloor = profile.RockFloorZ + 8;
        foreach (var (x, y) in path)
        {
            map.IsRiver[x, y] = true;
            map.RiverWidth[x, y] = WidthOf(map.RiverWidth[x, y], routing.Accumulation[x, y]);
            map.Heights[x, y] = Math.Max(map.Heights[x, y] - 2, minFloor);
        }
    }

    /// <summary>
    ///     Computes the graded width of a river cell from its flow accumulation.
    /// </summary>
    /// <param name="current">Currently assigned width.</param>
    /// <param name="accumulation">Flow accumulation of the cell.</param>
    /// <returns>Graded width in blocks.</returns>
    private static int WidthOf(int current, int accumulation)
    {
        var width = accumulation switch
        {
            >= RiverThreshold * WidthGrade2Multiple => 3,
            >= RiverThreshold * WidthGrade1Multiple => 2,
            _ => 1
        };
        return Math.Max(current, width);
    }

    /// <summary>
    ///     Marks banks: land cells adjacent to a river cell that do not themselves carry a river.
    /// </summary>
    /// <param name="map">Terrain map to update.</param>
    /// <param name="width">Footprint width.</param>
    /// <param name="height">Footprint height.</param>
    private static void MarkBanks(TerrainMap map, int width, int height)
    {
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (!map.IsRiver[x, y]) continue;

                for (var direction = 0; direction < 8; ++direction)
                {
                    var nx = x + FlowRouter.DirectionDx[direction];
                    var ny = y + FlowRouter.DirectionDy[direction];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    if (map.IsRiver[nx, ny] || map.IsOcean[nx, ny]) continue;

                    map.IsBank[nx, ny] = true;
                }
            }
        }
    }

    /// <summary>
    ///     Computes a deterministic ordering key for a river path by its source cell.
    /// </summary>
    /// <param name="source">Source cell of the path.</param>
    /// <returns>Ordering key.</returns>
    private ulong HashKey((int X, int Y) source)
    {
        return Noise.SeedDerivation.SplitMix64(
            seed ^ ((ulong)(uint)source.X << 32 | (uint)source.Y));
    }
}
