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
using System.Threading.Tasks;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen.Biomes;

/// <summary>
///     Assigns block materials to every column of a plan footprint from its biome and the
///     terrain map flags. The stage is fully deterministic: apart from the seeded per-column
///     surface modifier jitter it draws no randomness at all.
/// </summary>
public sealed class MaterialStage
{
    /// <summary>
    ///     Chebyshev range in blocks within which an ocean or shelf cell counts as near-coast
    ///     and receives the beach materials.
    /// </summary>
    private const int CoastRange = 4;

    /// <summary>
    ///     Subsurface depth in blocks of beach and near-coast columns.
    /// </summary>
    private const int BeachSubSurfaceDepth = 3;

    /// <summary>
    ///     Assigns materials to every column.
    /// </summary>
    /// <param name="map">Terrain map with heights and water flags populated.</param>
    /// <param name="biomes">Classified biome map.</param>
    /// <param name="options">Biome options.</param>
    /// <param name="seed">Sub-seed for the surface modifier jitter.</param>
    /// <returns>Per-column material assignment.</returns>
    public ColumnMaterials Apply(TerrainMap map, BiomeMap biomes, BiomeOptions options, ulong seed)
    {
        var width = map.Width;
        var height = map.Height;
        var definitions = ResolveDefinitions(options);
        var beach = definitions[BiomeId.Beach];
        var nearLand = NearLandDistance(map);

        var surface = new string[width, height];
        var subSurface = new string[width, height];
        var depth = new int[width, height];
        var modifier = new byte[width, height];

        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                var biome = biomes.Biome[x, y];
                AssignMaterials(map, options, definitions, beach, nearLand, biome, x, y,
                    out surface[x, y], out subSurface[x, y], out depth[x, y]);
                if (map.IsCliff[x, y]) surface[x, y] = subSurface[x, y];
                modifier[x, y] = ModifierOf(seed, x, y);
            }
        });

        return new ColumnMaterials
        {
            Width = width,
            Height = height,
            SurfaceTemplate = surface,
            SubSurfaceTemplate = subSurface,
            SubSurfaceDepth = depth,
            SurfaceModifier = modifier
        };
    }

    /// <summary>
    ///     Assigns the materials of a single column; the first matching rule wins.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="options">Biome options.</param>
    /// <param name="definitions">Material definitions by biome ID.</param>
    /// <param name="beach">Definition of the Beach biome.</param>
    /// <param name="nearLand">Chebyshev distance in blocks to the nearest land cell.</param>
    /// <param name="biome">Biome of the column.</param>
    /// <param name="x">Column X coordinate.</param>
    /// <param name="y">Column Y coordinate.</param>
    /// <param name="surface">Surface template of the column.</param>
    /// <param name="subSurface">Subsurface template of the column.</param>
    /// <param name="depth">Subsurface depth of the column.</param>
    private static void AssignMaterials(TerrainMap map, BiomeOptions options,
        IReadOnlyDictionary<BiomeId, BiomeDefinition> definitions, BiomeDefinition beach,
        byte[,] nearLand, BiomeId biome, int x, int y, out string surface, out string subSurface,
        out int depth)
    {
        switch (biome)
        {
            case BiomeId.Ocean or BiomeId.Shelf:
                if (nearLand[x, y] <= CoastRange)
                {
                    surface = beach.SurfaceTemplate;
                    subSurface = beach.SubSurfaceTemplate;
                    depth = BeachSubSurfaceDepth;
                }
                else
                {
                    surface = options.OceanFloorTemplate;
                    subSurface = options.OceanFloorTemplate;
                    depth = 0;
                }

                return;

            case BiomeId.Lake or BiomeId.River:
                surface = options.WaterFloorTemplate;
                subSurface = options.WaterFloorTemplate;
                depth = 0;
                return;

            case BiomeId.Beach:
                surface = beach.SurfaceTemplate;
                subSurface = beach.SubSurfaceTemplate;
                depth = BeachSubSurfaceDepth;
                return;

            default:
            {
                var definition = definitions[biome];
                surface = biome == BiomeId.Swamp && options.Swamp is { } swamp
                    ? swamp.Template
                    : definition.SurfaceTemplate;
                subSurface = definition.SubSurfaceTemplate;
                depth = definition.SubSurfaceDepth;
                return;
            }
        }
    }

    /// <summary>
    ///     Computes the Chebyshev distance in blocks from every cell to the nearest land cell,
    ///     saturating at 255.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <returns>Distance field indexed [x, y].</returns>
    private static byte[,] NearLandDistance(TerrainMap map)
    {
        var width = map.Width;
        var height = map.Height;
        const byte far = 255;

        var distance = new byte[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                distance[x, y] = map.IsOcean[x, y] ? far : (byte)0;
            }
        }

        // Two-pass chamfer propagation yields the exact Chebyshev distance to a land cell.
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (distance[x, y] == 0) continue;

                var best = far;
                if (y > 0)
                {
                    best = Math.Min(best, (byte)Math.Min(distance[x, y - 1] + 1, far));
                    if (x > 0) best = Math.Min(best, (byte)Math.Min(distance[x - 1, y - 1] + 1, far));
                    if (x < width - 1)
                        best = Math.Min(best, (byte)Math.Min(distance[x + 1, y - 1] + 1, far));
                }

                if (x > 0) best = Math.Min(best, (byte)Math.Min(distance[x - 1, y] + 1, far));
                distance[x, y] = Math.Min(distance[x, y], best);
            }
        }

        for (var y = height - 1; y >= 0; --y)
        {
            for (var x = width - 1; x >= 0; --x)
            {
                if (distance[x, y] == 0) continue;

                var best = distance[x, y];
                if (y < height - 1)
                {
                    best = Math.Min(best, (byte)Math.Min(distance[x, y + 1] + 1, far));
                    if (x > 0) best = Math.Min(best, (byte)Math.Min(distance[x - 1, y + 1] + 1, far));
                    if (x < width - 1)
                        best = Math.Min(best, (byte)Math.Min(distance[x + 1, y + 1] + 1, far));
                }

                if (x < width - 1) best = Math.Min(best, (byte)Math.Min(distance[x + 1, y] + 1, far));
                distance[x, y] = Math.Min(best, (byte)far);
            }
        }

        return distance;
    }

    /// <summary>
    ///     Computes the seeded surface modifier of a column, uniform over 0..3.
    /// </summary>
    /// <param name="seed">Jitter sub-seed.</param>
    /// <param name="x">Column X coordinate.</param>
    /// <param name="y">Column Y coordinate.</param>
    /// <returns>Surface modifier in [0, 3].</returns>
    private static byte ModifierOf(ulong seed, int x, int y)
    {
        return (byte)(SeedDerivation.SplitMix64(seed ^ ColumnKey(x, y)) & 3);
    }

    /// <summary>
    ///     Packs footprint-local column coordinates into a single hash key.
    /// </summary>
    /// <param name="x">Column X coordinate.</param>
    /// <param name="y">Column Y coordinate.</param>
    /// <returns>Packed key.</returns>
    private static ulong ColumnKey(int x, int y)
    {
        return (ulong)(uint)x << 32 | (uint)y;
    }

    /// <summary>
    ///     Resolves the profile biome definitions to material definitions by biome ID.
    /// </summary>
    /// <param name="options">Biome options.</param>
    /// <returns>Definitions by biome ID.</returns>
    private static IReadOnlyDictionary<BiomeId, BiomeDefinition> ResolveDefinitions(
        BiomeOptions options)
    {
        var definitions = new Dictionary<BiomeId, BiomeDefinition>();
        foreach (var (name, definition) in options.Definitions)
        {
            definitions[Enum.Parse<BiomeId>(name)] = definition;
        }

        return definitions;
    }
}
