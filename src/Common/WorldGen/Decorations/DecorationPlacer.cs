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

namespace Sovereign.WorldGen.Decorations;

/// <summary>
///     Decoration placements of a completed pass.
/// </summary>
public sealed class DecorationPlacerResult
{
    /// <summary>
    ///     Placements in scan order (y ascending, then x ascending).
    /// </summary>
    public required IReadOnlyList<DecorationPlacement> Placements { get; init; }
}

/// <summary>
///     Places decorations over a plan footprint. Each column draws against its biome's
///     decoration pool in declared order and accepts at most one decoration; spacing and
///     surface exclusions prune the accepted candidate. The scan is serial in scan order so
///     that the result is fully deterministic.
/// </summary>
public sealed class DecorationPlacer
{
    /// <summary>
    ///     Exclusion radius in cells around cave mouth columns.
    /// </summary>
    private const int MouthExclusionRadius = 2;

    private readonly ulong seed;

    /// <summary>
    ///     Accepted placements by template name, used for minimum-spacing checks.
    /// </summary>
    private readonly Dictionary<string, List<(int X, int Y)>> occupancy = new();

    /// <summary>
    ///     Creates a decoration placer.
    /// </summary>
    /// <param name="seed">Sub-seed for decoration placement.</param>
    public DecorationPlacer(ulong seed)
    {
        this.seed = seed;
    }

    /// <summary>
    ///     Places decorations over the plan footprint.
    /// </summary>
    /// <param name="map">Terrain map with heights and water flags populated.</param>
    /// <param name="biomes">Classified biome map.</param>
    /// <param name="options">Biome options.</param>
    /// <param name="mouthColumns">Surface mouth columns to keep clear, or null when the
    ///     plan has no caves.</param>
    /// <returns>Placements in scan order.</returns>
    public DecorationPlacerResult Apply(TerrainMap map, BiomeMap biomes, BiomeOptions options,
        IReadOnlyList<(int X, int Y)>? mouthColumns = null)
    {
        var pools = ResolvePools(options);
        var placements = new List<DecorationPlacement>();
        var mouthExclusions = BuildMouthExclusions(mouthColumns);

        for (var y = 0; y < map.Height; ++y)
        {
            for (var x = 0; x < map.Width; ++x)
            {
                var biome = biomes.Biome[x, y];
                if (!pools.TryGetValue(biome, out var pool)) continue;
                if (map.IsCliff[x, y]) continue;

                var state = StreamState(x, y);
                foreach (var (poolIndex, decoration) in pool)
                {
                    var draw = Draw(ref state);
                    if (draw >= decoration.Weight) continue;

                    // The first successful draw is the sole candidate for the column; a
                    // rejected candidate is not retried against the rest of the pool.
                    if (IsExcluded(map, options, biome, decoration, x, y)) break;
                    if (mouthExclusions?.Contains(Encode(x, y)) == true) break;
                    if (ViolatesSpacing(decoration, x, y)) break;

                    placements.Add(new DecorationPlacement
                    {
                        TemplateName = decoration.Template,
                        X = x,
                        Y = y,
                        Z = map.Heights[x, y] + 1,
                        Biome = biome,
                        PoolIndex = poolIndex
                    });
                    if (!occupancy.TryGetValue(decoration.Template, out var cells))
                    {
                        cells = new List<(int X, int Y)>();
                        occupancy[decoration.Template] = cells;
                    }

                    cells.Add((x, y));
                    break;
                }
            }
        }

        return new DecorationPlacerResult { Placements = placements };
    }

    /// <summary>
    ///     Resolves the profile biome definitions to decoration pools by biome ID. Definitions
    ///     with an empty or absent pool contribute nothing.
    /// </summary>
    /// <param name="options">Biome options.</param>
    /// <returns>Decoration pools by biome ID.</returns>
    private static IReadOnlyDictionary<BiomeId, List<(int PoolIndex, DecorationOptions Decoration)>>
        ResolvePools(BiomeOptions options)
    {
        var pools = new Dictionary<BiomeId, List<(int, DecorationOptions)>>();
        foreach (var (name, definition) in options.Definitions)
        {
            if (definition.Decorations is not { Count: > 0 }) continue;
            if (!Enum.TryParse<BiomeId>(name, out var biome)) continue;

            var pool = new List<(int, DecorationOptions)>();
            for (var i = 0; i < definition.Decorations.Count; ++i)
            {
                pool.Add((i, definition.Decorations[i]));
            }

            pools[biome] = pool;
        }

        return pools;
    }

    /// <summary>
    ///     Builds the set of columns excluded by proximity to a surface mouth: every column
    ///     within the mouth exclusion radius of a mouth column.
    /// </summary>
    /// <param name="mouthColumns">Surface mouth columns, or null.</param>
    /// <returns>Encoded excluded columns, or null when there are no mouths.</returns>
    private static HashSet<long>? BuildMouthExclusions(IReadOnlyList<(int X, int Y)>? mouthColumns)
    {
        if (mouthColumns is not { Count: > 0 }) return null;

        var exclusions = new HashSet<long>();
        foreach (var (mx, my) in mouthColumns)
        {
            for (var dy = -MouthExclusionRadius; dy <= MouthExclusionRadius; ++dy)
            {
                for (var dx = -MouthExclusionRadius; dx <= MouthExclusionRadius; ++dx)
                {
                    exclusions.Add(Encode(mx + dx, my + dy));
                }
            }
        }

        return exclusions;
    }

    /// <summary>
    ///     Encodes a column position as a single long for set membership.
    /// </summary>
    /// <param name="x">Column X coordinate.</param>
    /// <param name="y">Column Y coordinate.</param>
    /// <returns>Encoded position.</returns>
    private static long Encode(int x, int y)
    {
        return (long)x << 32 | (uint)y;
    }

    /// <summary>
    ///     Determines whether the candidate placement is excluded by surface conditions.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="options">Biome options.</param>
    /// <param name="biome">Biome of the column.</param>
    /// <param name="decoration">Candidate decoration.</param>
    /// <param name="x">Column X coordinate.</param>
    /// <param name="y">Column Y coordinate.</param>
    /// <returns>true if the candidate must be rejected, false otherwise.</returns>
    private static bool IsExcluded(TerrainMap map, BiomeOptions options, BiomeId biome,
        DecorationOptions decoration, int x, int y)
    {
        if (biome != BiomeId.Beach && (map.IsOcean[x, y] || map.IsLake[x, y] || map.IsRiver[x, y]
                || map.IsBank[x, y]))
        {
            return true;
        }

        if (biome != BiomeId.Snowcap && map.Heights[x, y] > options.SnowcapZ) return true;

        return SlopeOf(map, x, y) > decoration.MaxSlope;
    }

    /// <summary>
    ///     Determines whether an accepted placement of the same template lies within the
    ///     candidate's minimum spacing.
    /// </summary>
    /// <param name="decoration">Candidate decoration.</param>
    /// <param name="x">Column X coordinate.</param>
    /// <param name="y">Column Y coordinate.</param>
    /// <returns>true if the candidate violates spacing, false otherwise.</returns>
    private bool ViolatesSpacing(DecorationOptions decoration, int x, int y)
    {
        if (!occupancy.TryGetValue(decoration.Template, out var cells)) return false;

        foreach (var (cx, cy) in cells)
        {
            if (Math.Max(Math.Abs(cx - x), Math.Abs(cy - y)) < decoration.MinSpacing) return true;
        }

        return false;
    }

    /// <summary>
    ///     Computes the surface slope of a column: the maximum height difference to its
    ///     in-bounds 4-neighbors.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="x">Column X coordinate.</param>
    /// <param name="y">Column Y coordinate.</param>
    /// <returns>Slope in blocks.</returns>
    private static int SlopeOf(TerrainMap map, int x, int y)
    {
        var h = map.Heights[x, y];
        var slope = 0;
        if (x > 0) slope = Math.Max(slope, Math.Abs(map.Heights[x - 1, y] - h));
        if (y > 0) slope = Math.Max(slope, Math.Abs(map.Heights[x, y - 1] - h));
        if (x < map.Width - 1) slope = Math.Max(slope, Math.Abs(map.Heights[x + 1, y] - h));
        if (y < map.Height - 1) slope = Math.Max(slope, Math.Abs(map.Heights[x, y + 1] - h));
        return slope;
    }

    /// <summary>
    ///     Initializes the per-column draw stream from the sub-seed and column coordinates.
    /// </summary>
    /// <param name="x">Column X coordinate.</param>
    /// <param name="y">Column Y coordinate.</param>
    /// <returns>Stream state.</returns>
    private ulong StreamState(int x, int y)
    {
        return SeedDerivation.SplitMix64(seed ^ ((ulong)(uint)x << 32 | (uint)y));
    }

    /// <summary>
    ///     Draws the next uniform variate in [0, 1) from the stream.
    /// </summary>
    /// <param name="state">Stream state; updated with the next state value.</param>
    /// <returns>Uniform variate in [0, 1).</returns>
    private static double Draw(ref ulong state)
    {
        return (SeedDerivation.Next(ref state) >> 11) * (1.0 / 9007199254740992.0);
    }
}
