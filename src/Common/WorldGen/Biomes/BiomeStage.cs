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
using System.Threading;
using System.Threading.Tasks;
using Sovereign.WorldGen.Layout;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen.Biomes;

/// <summary>
///     Elevation thresholds for one classification pass: the profile snowcap and alpine
///     scalars plus the optional quarter-resolution mountain-bias shift fields that lower
///     or raise them per cell.
/// </summary>
internal sealed class ClimateThresholds
{
    /// <summary>
    ///     Minimum Z between sea level and the snowcap line.
    /// </summary>
    public const int SnowcapFloorAboveSea = 4;

    /// <summary>
    ///     Minimum Z between sea level and the alpine line.
    /// </summary>
    public const int AlpineFloorAboveSea = 2;

    /// <summary>
    ///     Profile snowcap threshold in Z.
    /// </summary>
    public required int SnowcapZ { get; init; }

    /// <summary>
    ///     Profile alpine threshold in Z.
    /// </summary>
    public required int AlpineZ { get; init; }

    /// <summary>
    ///     Sea level in Z, the floor reference for the shifted thresholds.
    /// </summary>
    public required int SeaLevelZ { get; init; }

    /// <summary>
    ///     Quarter-grid width of the shift fields.
    /// </summary>
    public int QuarterWidth { get; init; }

    /// <summary>
    ///     Quarter-grid height of the shift fields.
    /// </summary>
    public int QuarterHeight { get; init; }

    /// <summary>
    ///     Quarter-resolution snowcap shift field in Z, or null for the unshifted line.
    /// </summary>
    public float[,]? SnowcapShift { get; init; }

    /// <summary>
    ///     Quarter-resolution alpine shift field in Z, or null for the unshifted line.
    /// </summary>
    public float[,]? AlpineShift { get; init; }

    /// <summary>
    ///     Computes the effective snowcap threshold at a cell, clamped between the snowcap
    ///     floor and the profile value raised by the full negative shift.
    /// </summary>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <returns>Effective snowcap threshold in Z.</returns>
    public int EffectiveSnowcapZ(int x, int y)
    {
        if (SnowcapShift is null) return SnowcapZ;
        var shift = (int)MathF.Round(LayoutMaskStage.SampleQuarter(SnowcapShift, QuarterWidth,
            QuarterHeight, x, y));
        return Math.Clamp(SnowcapZ - shift, SeaLevelZ + SnowcapFloorAboveSea,
            SnowcapZ + (int)LayoutBiasStage.SnowcapBiasCoefficient);
    }

    /// <summary>
    ///     Computes the effective alpine threshold at a cell, clamped between the alpine
    ///     floor and the profile value raised by the full negative shift.
    /// </summary>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <returns>Effective alpine threshold in Z.</returns>
    public int EffectiveAlpineZ(int x, int y)
    {
        if (AlpineShift is null) return AlpineZ;
        var shift = (int)MathF.Round(LayoutMaskStage.SampleQuarter(AlpineShift, QuarterWidth,
            QuarterHeight, x, y));
        return Math.Clamp(AlpineZ - shift, SeaLevelZ + AlpineFloorAboveSea,
            AlpineZ + (int)LayoutBiasStage.AlpineBiasCoefficient);
    }
}

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
    ///     Fraction of the climate range that a full layout temperature or moisture bias
    ///     contributes.
    /// </summary>
    private const float ClimateBiasCoefficient = 0.25f;

    /// <summary>
    ///     Classifies the biome of every cell.
    /// </summary>
    /// <param name="map">Terrain map with heights and water flags populated.</param>
    /// <param name="continentalness">Banded continentalness classification.</param>
    /// <param name="profile">World generation profile.</param>
    /// <param name="seed">Sub-seed for biome classification.</param>
    /// <param name="layout">Resolved layout fields whose temperature and moisture biases
    /// shift the climate fields and whose mountain bias shifts the alpine and snowcap lines,
    /// or null for the unbiased path.</param>
    /// <returns>Biome map with every cell classified.</returns>
    public BiomeMap Apply(TerrainMap map, ContinentalnessResult continentalness,
        WorldGenProfile profile, ulong seed, LayoutFields? layout = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var width = profile.Width;
        var height = profile.Height;
        var options = profile.Biomes
                      ?? throw new ArgumentException("The profile has no biomes section.", nameof(profile));

        var temperature = ClimateField(width, height,
            SeedDerivation.DeriveSubSeed(seed, "BiomeTemp"), TemperatureWavelength);
        var moisture = ClimateField(width, height,
            SeedDerivation.DeriveSubSeed(seed, "BiomeMoist"), MoistureWavelength);

        if (layout?.TemperatureBias is { } temperatureBias)
        {
            temperature = ApplyBias(temperature, temperatureBias, ClimateBiasCoefficient);
        }

        if (layout?.MoistureBias is { } moistureBias)
        {
            moisture = ApplyBias(moisture, moistureBias, ClimateBiasCoefficient);
        }

        return Apply(map, continentalness, options, ResolveTable(options), temperature, moisture,
            BuildThresholds(options, profile.SeaLevelZ, layout));
    }

    /// <summary>
    ///     Builds the per-cell elevation thresholds from the profile scalars and the layout's
    ///     mountain-bias shift fields, or null when the layout carries no mountain bias.
    /// </summary>
    /// <param name="options">Biome options.</param>
    /// <param name="seaLevelZ">Profile sea level in Z.</param>
    /// <param name="layout">Resolved layout fields, or null.</param>
    /// <returns>Threshold context, or null for the unshifted path.</returns>
    private static ClimateThresholds? BuildThresholds(BiomeOptions options, int seaLevelZ,
        LayoutFields? layout)
    {
        if (layout is null
            || layout.QuarterSnowcapShift is null && layout.QuarterAlpineShift is null)
        {
            return null;
        }

        return new ClimateThresholds
        {
            SnowcapZ = options.SnowcapZ,
            AlpineZ = options.AlpineZ,
            SeaLevelZ = seaLevelZ,
            QuarterWidth = layout.QuarterWidth,
            QuarterHeight = layout.QuarterHeight,
            SnowcapShift = layout.QuarterSnowcapShift,
            AlpineShift = layout.QuarterAlpineShift
        };
    }

    /// <summary>
    ///     Adds a scaled layout bias field to a normalized climate field and clamps to [0, 1].
    /// </summary>
    /// <param name="field">Normalized climate field.</param>
    /// <param name="bias">Bias field in [-1, 1].</param>
    /// <param name="coefficient">Bias scale factor.</param>
    /// <returns>Biased climate field.</returns>
    private static float[,] ApplyBias(float[,] field, float[,] bias, float coefficient)
    {
        var width = field.GetLength(0);
        var height = field.GetLength(1);
        var result = new float[width, height];
        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                result[x, y] = Math.Clamp(field[x, y] + coefficient * bias[x, y], 0f, 1f);
            }
        });

        return result;
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
    /// <param name="thresholds">Per-cell elevation thresholds, or null for the profile
    /// scalars unchanged.</param>
    /// <returns>Biome map with every cell classified.</returns>
    internal BiomeMap Apply(TerrainMap map, ContinentalnessResult continentalness,
        BiomeOptions options, BiomeId[,] table, float[,] temperature, float[,] moisture,
        ClimateThresholds? thresholds = null)
    {
        var width = map.Width;
        var height = map.Height;
        var biome = new BiomeId[width, height];
        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                biome[x, y] = Classify(map, continentalness, options, table, temperature, moisture,
                    thresholds, x, y);
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
    /// <param name="thresholds">Per-cell elevation thresholds, or null for the profile
    /// scalars unchanged.</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <returns>Biome ID of the cell.</returns>
    private static BiomeId Classify(TerrainMap map, ContinentalnessResult continentalness,
        BiomeOptions options, BiomeId[,] table, float[,] temperature, float[,] moisture,
        ClimateThresholds? thresholds, int x, int y)
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
        var snowcapZ = thresholds?.EffectiveSnowcapZ(x, y) ?? options.SnowcapZ;
        if (h >= snowcapZ) return BiomeId.Snowcap;
        var alpineZ = thresholds?.EffectiveAlpineZ(x, y) ?? options.AlpineZ;
        if (h >= alpineZ) return BiomeId.Alpine;

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
