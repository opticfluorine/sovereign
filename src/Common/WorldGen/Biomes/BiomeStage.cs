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
using System.Threading.Tasks;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen.Biomes;

/// <summary>
///     Classifies every cell of a plan footprint into a <see cref="BiomeId" />. Water,
///     shelf, beach, river, and lake biomes follow the terrain map flags; remaining land
///     cells are classified by a Whittaker table over temperature and moisture fields, with
///     alpine, snowcap, and swamp overrides applied above the table.
/// </summary>
public sealed class BiomeStage
{
    /// <summary>
    ///     Wavelength in blocks of the base temperature octave.
    /// </summary>
    private const float TemperatureWavelength = 400f;

    /// <summary>
    ///     Wavelength in blocks of the base moisture octave.
    /// </summary>
    private const float MoistureWavelength = 350f;

    /// <summary>
    ///     Number of fBm octaves in the climate fields.
    /// </summary>
    private const int FieldOctaves = 4;

    /// <summary>
    ///     Chebyshev radius in blocks within which a bank cell counts as water-adjacent for
    ///     the swamp override.
    /// </summary>
    private const int SwampWaterRadius = 3;

    /// <summary>
    ///     Temperature and moisture bands are cut at fixed fractions of the observed field
    ///     range.
    /// </summary>
    private const float BandCut = 1f / 3f;

    /// <summary>
    ///     Classifies the biome of every cell.
    /// </summary>
    /// <param name="map">Terrain map with heights and water flags populated.</param>
    /// <param name="continentalness">Banded continentalness classification.</param>
    /// <param name="profile">World generation profile.</param>
    /// <param name="seed">Sub-seed for biome classification.</param>
    /// <returns>Biome map with every cell classified.</returns>
    public BiomeMap Apply(TerrainMap map, ContinentalnessResult continentalness,
        WorldGenProfile profile, ulong seed)
    {
        var width = profile.Width;
        var height = profile.Height;
        var options = profile.Biomes
                      ?? throw new ArgumentException("The profile has no biomes section.", nameof(profile));

        var temperature = ClimateField(width, height,
            SeedDerivation.DeriveSubSeed(seed, "BiomeTemp"), TemperatureWavelength);
        var moisture = ClimateField(width, height,
            SeedDerivation.DeriveSubSeed(seed, "BiomeMoist"), MoistureWavelength);

        return Apply(map, continentalness, options, ResolveTable(options), temperature, moisture);
    }

    /// <summary>
    ///     Classifies the biome of every cell from precomputed climate fields. Intended for
    ///     testing; the seed-based overload derives the climate fields itself.
    /// </summary>
    /// <param name="map">Terrain map with heights and water flags populated.</param>
    /// <param name="continentalness">Banded continentalness classification.</param>
    /// <param name="options">Biome options.</param>
    /// <param name="table">Resolved Whittaker table indexed [temperature, moisture].</param>
    /// <param name="temperature">Normalized temperature field in [0, 1].</param>
    /// <param name="moisture">Normalized moisture field in [0, 1].</param>
    /// <returns>Biome map with every cell classified.</returns>
    internal BiomeMap Apply(TerrainMap map, ContinentalnessResult continentalness,
        BiomeOptions options, BiomeId[,] table, float[,] temperature, float[,] moisture)
    {
        var width = map.Width;
        var height = map.Height;
        var biome = new BiomeId[width, height];
        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                biome[x, y] = Classify(map, continentalness, options, table, temperature, moisture,
                    x, y);
            }
        });

        return new BiomeMap { Width = width, Height = height, Biome = biome };
    }

    /// <summary>
    ///     Classifies a single cell; the first matching rule wins.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="continentalness">Banded continentalness classification.</param>
    /// <param name="options">Biome options.</param>
    /// <param name="table">Resolved Whittaker table indexed [temperature, moisture].</param>
    /// <param name="temperature">Normalized temperature field in [0, 1].</param>
    /// <param name="moisture">Normalized moisture field in [0, 1].</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <returns>Biome ID of the cell.</returns>
    private static BiomeId Classify(TerrainMap map, ContinentalnessResult continentalness,
        BiomeOptions options, BiomeId[,] table, float[,] temperature, float[,] moisture,
        int x, int y)
    {
        if (map.IsOcean[x, y])
        {
            return continentalness.Classes[x, y] == ContinentalClass.DeepOcean
                ? BiomeId.Ocean
                : BiomeId.Shelf;
        }

        if (map.IsLake[x, y]) return BiomeId.Lake;
        if (map.IsRiver[x, y]) return BiomeId.River;
        if (map.IsBeach[x, y]) return BiomeId.Beach;

        var h = map.Heights[x, y];
        if (h >= options.SnowcapZ) return BiomeId.Snowcap;
        if (h >= options.AlpineZ) return BiomeId.Alpine;

        var tableBiome = table[TemperatureBand(temperature[x, y]), MoistureBand(moisture[x, y])];
        if (options.Swamp is { } swamp && tableBiome is not (BiomeId.Desert or BiomeId.Alpine
                or BiomeId.Snowcap)
            && h <= swamp.MaxHeightZ
            && (map.IsBank[x, y] || HasWaterNearby(map, x, y)))
        {
            return BiomeId.Swamp;
        }

        return tableBiome;
    }

    /// <summary>
    ///     Determines whether any lake or river cell lies within the swamp water radius of
    ///     the given cell.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <returns>true if fresh water is within the radius, false otherwise.</returns>
    private static bool HasWaterNearby(TerrainMap map, int x, int y)
    {
        var minX = Math.Max(x - SwampWaterRadius, 0);
        var minY = Math.Max(y - SwampWaterRadius, 0);
        var maxX = Math.Min(x + SwampWaterRadius, map.Width - 1);
        var maxY = Math.Min(y + SwampWaterRadius, map.Height - 1);

        for (var ny = minY; ny <= maxY; ++ny)
        {
            for (var nx = minX; nx <= maxX; ++nx)
            {
                if (map.IsLake[nx, ny] || map.IsRiver[nx, ny]) return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Maps a normalized climate value to its band index with cuts at 1/3 and 2/3.
    /// </summary>
    /// <param name="value">Normalized climate value in [0, 1].</param>
    /// <returns>Band index in [0, 2].</returns>
    private static int Band(float value)
    {
        return value < BandCut ? 0 : value < 2f * BandCut ? 1 : 2;
    }

    /// <summary>
    ///     Computes the temperature band of a cell.
    /// </summary>
    /// <param name="value">Normalized temperature in [0, 1].</param>
    /// <returns>Band index: 0 cold, 1 mild, 2 hot.</returns>
    private static int TemperatureBand(float value)
    {
        return Band(value);
    }

    /// <summary>
    ///     Computes the moisture band of a cell.
    /// </summary>
    /// <param name="value">Normalized moisture in [0, 1].</param>
    /// <returns>Band index: 0 dry, 1 temperate, 2 wet.</returns>
    private static int MoistureBand(float value)
    {
        return Band(value);
    }

    /// <summary>
    ///     Samples an fBm climate field normalized to [0, 1] over the observed range.
    /// </summary>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <param name="seed">Sub-seed of the field.</param>
    /// <param name="wavelength">Wavelength of the base octave in blocks.</param>
    /// <returns>Normalized field in [0, 1].</returns>
    private static float[,] ClimateField(int width, int height, ulong seed, float wavelength)
    {
        var noise = new SeededNoise(seed);
        var raw = new float[width, height];
        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                raw[x, y] = noise.Fbm(x, y, wavelength, FieldOctaves);
            }
        });

        var min = float.MaxValue;
        var max = float.MinValue;
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                var value = raw[x, y];
                if (value < min) min = value;
                if (value > max) max = value;
            }
        }

        var normalized = new float[width, height];
        var range = max - min;
        if (range <= 0f) return normalized;

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                normalized[x, y] = (raw[x, y] - min) / range;
            }
        }

        return normalized;
    }

    /// <summary>
    ///     Resolves the profile Whittaker table to biome IDs, indexed
    ///     <c>[temperature, moisture]</c> with bands 0..2 each.
    /// </summary>
    /// <param name="options">Biome options.</param>
    /// <returns>Resolved table.</returns>
    private static BiomeId[,] ResolveTable(BiomeOptions options)
    {
        var table = options.Table;
        return new BiomeId[3, 3]
        {
            { Parse(table.Cold.Dry), Parse(table.Cold.Temperate), Parse(table.Cold.Wet) },
            { Parse(table.Mild.Dry), Parse(table.Mild.Temperate), Parse(table.Mild.Wet) },
            { Parse(table.Hot.Dry), Parse(table.Hot.Temperate), Parse(table.Hot.Wet) }
        };
    }

    /// <summary>
    ///     Parses a biome name from the profile table.
    /// </summary>
    /// <param name="name">Biome name.</param>
    /// <returns>Biome ID.</returns>
    private static BiomeId Parse(string name)
    {
        return Enum.TryParse<BiomeId>(name, out var biome)
            ? biome
            : throw new ArgumentException($"Unknown biome name in biomes.table: \"{name}\".", nameof(name));
    }
}
