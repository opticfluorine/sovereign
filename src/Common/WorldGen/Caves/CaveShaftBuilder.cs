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
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen.Caves;

/// <summary>
///     Result of building shafts and surface mouths for a cave system.
/// </summary>
public sealed class CaveShaftResult
{
    /// <summary>
    ///     Staircase shafts between adjacent level pairs.
    /// </summary>
    public required IReadOnlyList<CaveShaft> Shafts { get; init; }

    /// <summary>
    ///     Surface mouths on the shallowest level.
    /// </summary>
    public required IReadOnlyList<CaveMouth> Mouths { get; init; }
}

/// <summary>
///     Places spiral staircase shafts between adjacent cave level pairs and surface mouths
///     on the shallowest level. All steps rise exactly one block; the ring outside each
///     shaft footprint is recorded for re-solidification so shafts crossing chamber walls
///     merge cleanly.
/// </summary>
public sealed class CaveShaftBuilder
{
    /// <summary>
    ///     Grid spacing of the jittered site scan, in blocks.
    /// </summary>
    private const int SiteGridSpacing = 64;

    /// <summary>
    ///     Maximum seeded jitter of a site off its grid point, in blocks.
    /// </summary>
    private const int SiteJitter = 16;

    /// <summary>
    ///     Minimum Chebyshev distance in cells between a shaft site and existing shafts on
    ///     either level of the pair.
    /// </summary>
    private const int MinShaftSpacing = 8;

    /// <summary>
    ///     Headroom in blocks carved above each mouth step.
    /// </summary>
    private const int MouthHeadroom = 2;

    /// <summary>
    ///     Ring walk offsets around a shaft center, in rotation order.
    /// </summary>
    private static readonly (int Dx, int Dy)[] RingWalk =
    {
        (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1)
    };

    /// <summary>
    ///     Builds shafts between each adjacent level pair and surface mouths on the
    ///     shallowest level.
    /// </summary>
    /// <param name="levels">Per-level carved maps, ordered from top to bottom.</param>
    /// <param name="terrain">Terrain map with heights and water flags populated.</param>
    /// <param name="biomes">Classified biome map, or null when biomes are not configured.</param>
    /// <param name="options">Cave generation options.</param>
    /// <param name="shaftsSeed">Sub-seed for shaft placement.</param>
    /// <param name="mouthsSeed">Sub-seed for mouth placement.</param>
    /// <returns>The placed shafts and mouths.</returns>
    public CaveShaftResult Build(IReadOnlyList<CaveLevelMap> levels, TerrainMap terrain,
        BiomeMap? biomes, CaveOptions options, ulong shaftsSeed, ulong mouthsSeed)
    {
        var shafts = BuildShafts(levels, options, shaftsSeed);
        var mouths = BuildMouths(levels, terrain, biomes, options, mouthsSeed);
        return new CaveShaftResult { Shafts = shafts, Mouths = mouths };
    }

    /// <summary>
    ///     Builds the staircase shafts: for each adjacent level pair, scans a jittered grid
    ///     and takes the first valid sites.
    /// </summary>
    /// <param name="levels">Per-level carved maps, ordered from top to bottom.</param>
    /// <param name="options">Cave generation options.</param>
    /// <param name="seed">Sub-seed for shaft placement.</param>
    /// <returns>Placed shafts.</returns>
    private List<CaveShaft> BuildShafts(IReadOnlyList<CaveLevelMap> levels, CaveOptions options,
        ulong seed)
    {
        var shafts = new List<CaveShaft>();
        if (options.ShaftsPerLevelPair <= 0) return shafts;

        // Shaft centers seen per level, used for the spacing check.
        var centersByLevel = new List<List<(int X, int Y)>>(levels.Count);
        for (var i = 0; i < levels.Count; ++i) centersByLevel.Add(new List<(int X, int Y)>());

        for (var upper = 0; upper + 1 < levels.Count; ++upper)
        {
            var upperMap = levels[upper];
            var lowerMap = levels[upper + 1];
            var placedForPair = 0;
            var state = SeedDerivation.DeriveSubSeed(seed, $"Pair{upper}");

            foreach (var (x, y) in SiteScan(upperMap.Width, upperMap.Height, ref state))
            {
                if (placedForPair >= options.ShaftsPerLevelPair) break;
                if (!IsOpenAt(upperMap, x, y) || !IsOpenAt(lowerMap, x, y)) continue;
                if (TooCloseToShafts(centersByLevel[upper], x, y)
                    || TooCloseToShafts(centersByLevel[upper + 1], x, y))
                {
                    continue;
                }

                shafts.Add(BuildShaft(levels, upper, x, y));
                centersByLevel[upper].Add((x, y));
                centersByLevel[upper + 1].Add((x, y));
                ++placedForPair;
            }
        }

        return shafts;
    }

    /// <summary>
    ///     Builds one spiral staircase shaft between the given level pair at the site.
    /// </summary>
    /// <param name="levels">Per-level carved maps.</param>
    /// <param name="upper">Upper level index.</param>
    /// <param name="x">Site X coordinate.</param>
    /// <param name="y">Site Y coordinate.</param>
    /// <returns>The shaft.</returns>
    private static CaveShaft BuildShaft(IReadOnlyList<CaveLevelMap> levels, int upper, int x,
        int y)
    {
        var upperMap = levels[upper];
        var lowerMap = levels[upper + 1];
        var lowerFloorZ = lowerMap.FloorZ[x, y];
        var upperFloorZ = upperMap.FloorZ[x, y];
        var gap = Math.Max(upperFloorZ - lowerFloorZ, 1);
        var headroom = Math.Max(upperMap.Headroom, lowerMap.Headroom);

        var columns = new List<CaveShaftColumn>();
        for (var ringIndex = 0; ringIndex < RingWalk.Length; ++ringIndex)
        {
            var stepZs = new List<int>();
            for (var step = ringIndex + 1; step <= gap; step += RingWalk.Length)
            {
                stepZs.Add(lowerFloorZ + step);
            }

            if (stepZs.Count == 0) continue;
            columns.Add(new CaveShaftColumn
            {
                X = x + RingWalk[ringIndex].Dx,
                Y = y + RingWalk[ringIndex].Dy,
                StepZs = stepZs
            });
        }

        return new CaveShaft
        {
            UpperLevel = upper,
            LowerLevel = upper + 1,
            CenterX = x,
            CenterY = y,
            LowerFloorZ = lowerFloorZ,
            UpperFloorZ = upperFloorZ,
            Headroom = headroom,
            Columns = columns,
            Ring = RingCells(x, y, upperMap.Width, upperMap.Height)
        };
    }

    /// <summary>
    ///     Builds the surface mouths on the shallowest level.
    /// </summary>
    /// <param name="levels">Per-level carved maps, ordered from top to bottom.</param>
    /// <param name="terrain">Terrain map.</param>
    /// <param name="biomes">Classified biome map, or null when biomes are not configured.</param>
    /// <param name="options">Cave generation options.</param>
    /// <param name="seed">Sub-seed for mouth placement.</param>
    /// <returns>Placed mouths.</returns>
    private List<CaveMouth> BuildMouths(IReadOnlyList<CaveLevelMap> levels, TerrainMap terrain,
        BiomeMap? biomes, CaveOptions options, ulong seed)
    {
        var mouths = new List<CaveMouth>();
        if (options.SurfaceMouths <= 0 || levels.Count == 0) return mouths;

        var level = levels[0];
        var state = seed;

        foreach (var (x, y) in SiteScan(terrain.Width, terrain.Height, ref state))
        {
            if (mouths.Count >= options.SurfaceMouths) break;
            if (!IsValidMouthSite(level, terrain, biomes, options, x, y)) continue;

            var mouth = BuildMouth(levels[0], terrain, x, y);
            mouths.Add(mouth);
        }

        return mouths;
    }

    /// <summary>
    ///     Determines whether the site admits a surface mouth.
    /// </summary>
    /// <param name="level">Shallowest cave level map.</param>
    /// <param name="terrain">Terrain map.</param>
    /// <param name="biomes">Classified biome map, or null when biomes are not configured.</param>
    /// <param name="options">Cave generation options.</param>
    /// <param name="x">Site X coordinate.</param>
    /// <param name="y">Site Y coordinate.</param>
    /// <returns>true if the site is valid, false otherwise.</returns>
    private static bool IsValidMouthSite(CaveLevelMap level, TerrainMap terrain,
        BiomeMap? biomes, CaveOptions options, int x, int y)
    {
        if (x + 1 >= terrain.Width || y + 1 >= terrain.Height) return false;
        if (terrain.IsOcean[x, y]) return false;
        if (SlopeOf(terrain, x, y) > 1) return false;
        if (HasWaterWithin(terrain, x, y, options.MouthMinLandDistance)) return false;
        if (biomes is not null
            && (biomes.Biome[x, y] is BiomeId.Beach or BiomeId.Snowcap
                || biomes.Biome[x + 1, y] is BiomeId.Beach or BiomeId.Snowcap
                || biomes.Biome[x, y + 1] is BiomeId.Beach or BiomeId.Snowcap
                || biomes.Biome[x + 1, y + 1] is BiomeId.Beach or BiomeId.Snowcap))
        {
            return false;
        }

        var floorZ = level.FloorZ[x, y];
        var surfaceZ = terrain.Heights[x, y];
        return surfaceZ - floorZ <= options.MaxMouthDepthZ;
    }

    /// <summary>
    ///     Builds one surface mouth and forces its 2x2 columns open on the shallowest level.
    /// </summary>
    /// <param name="level">Shallowest cave level map, updated in place.</param>
    /// <param name="terrain">Terrain map.</param>
    /// <param name="x">Upper-left X coordinate of the 2x2 shaft.</param>
    /// <param name="y">Upper-left Y coordinate of the 2x2 shaft.</param>
    /// <returns>The mouth.</returns>
    private static CaveMouth BuildMouth(CaveLevelMap level, TerrainMap terrain, int x, int y)
    {
        var floorZ = level.FloorZ[x, y];
        var surfaceZ = terrain.Heights[x, y];

        var columns = new List<CaveShaftColumn>();
        var cells = new List<(int X, int Y)>();
        var index = 0;
        foreach (var (cx, cy) in new[]
                 {
                     (x, y), (x + 1, y), (x, y + 1), (x + 1, y + 1)
                 })
        {
            cells.Add((cx, cy));
            var stepZs = new List<int>();
            for (var z = floorZ + index + 1; z <= surfaceZ; z += 4)
            {
                stepZs.Add(z);
            }

            if (stepZs.Count > 0)
            {
                columns.Add(new CaveShaftColumn
                {
                    X = cx,
                    Y = cy,
                    StepZs = stepZs
                });
            }

            ++index;
        }

        // The mouth bottoms out on the cave level: force the 2x2 columns open there.
        var touched = new List<(int X, int Y)>();
        foreach (var (cx, cy) in cells)
        {
            if (level.Open[cx, cy]) continue;
            level.Open[cx, cy] = true;
            level.CarveHeight[cx, cy] = (byte)Math.Max(level.Headroom, MouthHeadroom);
            touched.Add((cx, cy));
        }

        FloorSlopePass.Apply(touched, level.Open, level.FloorZ, level.Width, level.Height,
            level.BaseFloorZ, 2);

        return new CaveMouth
        {
            X = x,
            Y = y,
            SurfaceZ = surfaceZ,
            FloorZ = floorZ,
            Headroom = MouthHeadroom,
            Columns = columns
        };
    }

    /// <summary>
    ///     Enumerates the jittered site scan grid in seeded shuffle order. Shuffling
    ///     distributes selected sites across the whole footprint instead of clustering
    ///     them at the scan origin; the shuffle is deterministic for a given state.
    /// </summary>
    /// <param name="width">Map width in blocks.</param>
    /// <param name="height">Map height in blocks.</param>
    /// <param name="state">Jitter stream state; advanced with each drawn jitter.</param>
    /// <returns>Site coordinates in visitation order.</returns>
    private static List<(int X, int Y)> SiteScan(int width, int height, ref ulong state)
    {
        var sites = new List<(int X, int Y)>();
        for (var gy = 0; gy * SiteGridSpacing < height; ++gy)
        {
            for (var gx = 0; gx * SiteGridSpacing < width; ++gx)
            {
                var jx = (int)(SeedDerivation.Next(ref state) % (2 * SiteJitter + 1)) - SiteJitter;
                var jy = (int)(SeedDerivation.Next(ref state) % (2 * SiteJitter + 1)) - SiteJitter;
                var x = Math.Clamp(gx * SiteGridSpacing + jx, 1, width - 2);
                var y = Math.Clamp(gy * SiteGridSpacing + jy, 1, height - 2);
                sites.Add((x, y));
            }
        }

        // Fisher-Yates shuffle on the same stream: deterministic, unbiased ordering.
        for (var i = sites.Count - 1; i > 0; --i)
        {
            var j = (int)(SeedDerivation.Next(ref state) % (ulong)(i + 1));
            (sites[i], sites[j]) = (sites[j], sites[i]);
        }

        return sites;
    }

    /// <summary>
    ///     Determines whether an open cell exists at the site on the level.
    /// </summary>
    /// <param name="level">Level map.</param>
    /// <param name="x">Site X coordinate.</param>
    /// <param name="y">Site Y coordinate.</param>
    /// <returns>true if the cell is open, false otherwise.</returns>
    private static bool IsOpenAt(CaveLevelMap level, int x, int y)
    {
        return level.Open[x, y];
    }

    /// <summary>
    ///     Determines whether any existing shaft center lies within the minimum spacing of
    ///     the site.
    /// </summary>
    /// <param name="centers">Existing shaft centers on the level.</param>
    /// <param name="x">Site X coordinate.</param>
    /// <param name="y">Site Y coordinate.</param>
    /// <returns>true if a shaft is too close, false otherwise.</returns>
    private static bool TooCloseToShafts(List<(int X, int Y)> centers, int x, int y)
    {
        foreach (var (cx, cy) in centers)
        {
            if (Math.Max(Math.Abs(cx - x), Math.Abs(cy - y)) < MinShaftSpacing) return true;
        }

        return false;
    }

    /// <summary>
    ///     Collects the ring cells outside the 3x3 shaft footprint: the border of the 5x5
    ///     box, clamped to the map.
    /// </summary>
    /// <param name="x">Shaft center X coordinate.</param>
    /// <param name="y">Shaft center Y coordinate.</param>
    /// <param name="width">Map width in blocks.</param>
    /// <param name="height">Map height in blocks.</param>
    /// <returns>Ring cell coordinates.</returns>
    private static List<(int X, int Y)> RingCells(int x, int y, int width, int height)
    {
        var ring = new List<(int X, int Y)>(16);
        for (var dy = -2; dy <= 2; ++dy)
        {
            for (var dx = -2; dx <= 2; ++dx)
            {
                if (Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1) continue;

                var cx = x + dx;
                var cy = y + dy;
                if (cx < 0 || cy < 0 || cx >= width || cy >= height) continue;
                ring.Add((cx, cy));
            }
        }

        return ring;
    }

    /// <summary>
    ///     Computes the surface slope of a column: the maximum height difference to its
    ///     in-bounds 4-neighbors.
    /// </summary>
    /// <param name="terrain">Terrain map.</param>
    /// <param name="x">Column X coordinate.</param>
    /// <param name="y">Column Y coordinate.</param>
    /// <returns>Slope in blocks.</returns>
    private static int SlopeOf(TerrainMap terrain, int x, int y)
    {
        var h = terrain.Heights[x, y];
        var slope = 0;
        if (x > 0) slope = Math.Max(slope, Math.Abs(terrain.Heights[x - 1, y] - h));
        if (y > 0) slope = Math.Max(slope, Math.Abs(terrain.Heights[x, y - 1] - h));
        if (x < terrain.Width - 1) slope = Math.Max(slope, Math.Abs(terrain.Heights[x + 1, y] - h));
        if (y < terrain.Height - 1) slope = Math.Max(slope, Math.Abs(terrain.Heights[x, y + 1] - h));
        return slope;
    }

    /// <summary>
    ///     Determines whether any water, bank, river, or ocean cell lies within the given
    ///     Chebyshev distance of the site.
    /// </summary>
    /// <param name="terrain">Terrain map.</param>
    /// <param name="x">Site X coordinate.</param>
    /// <param name="y">Site Y coordinate.</param>
    /// <param name="distance">Exclusion radius in cells.</param>
    /// <returns>true if water is within the radius, false otherwise.</returns>
    private static bool HasWaterWithin(TerrainMap terrain, int x, int y, int distance)
    {
        var minX = Math.Max(x - distance, 0);
        var minY = Math.Max(y - distance, 0);
        var maxX = Math.Min(x + distance, terrain.Width - 1);
        var maxY = Math.Min(y + distance, terrain.Height - 1);
        for (var cy = minY; cy <= maxY; ++cy)
        {
            for (var cx = minX; cx <= maxX; ++cx)
            {
                if (terrain.IsRiver[cx, cy] || terrain.IsLake[cx, cy] || terrain.IsBank[cx, cy]
                    || terrain.IsOcean[cx, cy])
                {
                    return true;
                }
            }
        }

        return false;
    }
}
