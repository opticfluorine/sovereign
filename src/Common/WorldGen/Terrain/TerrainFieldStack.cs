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

using System.Threading.Tasks;
using Sovereign.WorldGen.Noise;

namespace Sovereign.WorldGen.Terrain;

/// <summary>
///     Sampled terrain noise fields over the plan footprint.
/// </summary>
public sealed class TerrainFields
{
    /// <summary>
    ///     Continentalness normalized to [0, 1] over the observed field range.
    /// </summary>
    public required float[,] Continentalness { get; init; }

    /// <summary>
    ///     Ridged noise used for mountain crests, in [0, 1].
    /// </summary>
    public required float[,] MountainRidge { get; init; }

    /// <summary>
    ///     High-frequency roughness, in [0, 1].
    /// </summary>
    public required float[,] Roughness { get; init; }
}

/// <summary>
///     Samples the terrain noise fields (continentalness, mountain ridges, roughness)
///     over the plan footprint.
/// </summary>
public sealed class TerrainFieldStack
{
    /// <summary>
    ///     Base continentalness wavelength as a fraction of the larger footprint dimension.
    /// </summary>
    private const float ContinentalnessWavelengthFactor = 1.75f;

    /// <summary>
    ///     Mountain ridge wavelength as a fraction of the larger footprint dimension.
    /// </summary>
    private const float RidgeWavelengthFactor = 1f / 6f;

    /// <summary>
    ///     Roughness wavelength in blocks.
    /// </summary>
    private const float RoughnessWavelength = 48f;

    /// <summary>
    ///     Number of octaves in the outer continentalness fBm.
    /// </summary>
    private const int ContinentalnessOctaves = 5;

    /// <summary>
    ///     Number of octaves in the mountain ridge fBm.
    /// </summary>
    private const int RidgeOctaves = 4;

    /// <summary>
    ///     Number of octaves in the roughness fBm.
    /// </summary>
    private const int RoughnessOctaves = 3;

    /// <summary>
    ///     Samples all terrain fields for the given footprint.
    /// </summary>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <param name="seed">Sub-seed for field sampling.</param>
    /// <returns>Sampled fields.</returns>
    public TerrainFields Sample(int width, int height, ulong seed)
    {
        var continentalness = new float[width, height];
        var ridge = new float[width, height];
        var roughness = new float[width, height];

        var maxDimension = System.Math.Max(width, height);
        var continentalnessWavelength = ContinentalnessWavelengthFactor * maxDimension;
        var ridgeWavelength = RidgeWavelengthFactor * maxDimension;

        var baseNoise = new SeededNoise(seed);
        var warpNoise = new SeededNoise(SeedDerivation.DeriveSubSeed(seed, "TerrainFieldStack.Warp"));
        var ridgeNoise = new SeededNoise(SeedDerivation.DeriveSubSeed(seed, "TerrainFieldStack.Ridge"));
        var roughnessNoise = new SeededNoise(SeedDerivation.DeriveSubSeed(seed, "TerrainFieldStack.Roughness"));

        System.Threading.Tasks.Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                continentalness[x, y] = baseNoise.DomainWarpedFbm(x, y, continentalnessWavelength,
                    ContinentalnessOctaves);
                ridge[x, y] = ridgeNoise.Ridged(x, y, ridgeWavelength, RidgeOctaves);
                roughness[x, y] = RoughnessOf(roughnessNoise, x, y);
            }
        });

        NormalizeContinentalness(continentalness, warpNoise, width, height, maxDimension);

        return new TerrainFields
        {
            Continentalness = continentalness,
            MountainRidge = ridge,
            Roughness = roughness
        };
    }

    /// <summary>
    ///     Samples the roughness field value at a cell.
    /// </summary>
    /// <param name="noise">Roughness noise source.</param>
    /// <param name="x">X coordinate in blocks.</param>
    /// <param name="y">Y coordinate in blocks.</param>
    /// <returns>Roughness in [0, 1].</returns>
    private static float RoughnessOf(SeededNoise noise, int x, int y)
    {
        var value = noise.Fbm(x, y, RoughnessWavelength, RoughnessOctaves);
        return System.MathF.Abs(value);
    }

    /// <summary>
    ///     Normalizes the continentalness field to [0, 1] over its observed range and applies
    ///     an edge falloff so that the footprint border always trends below sea level.
    /// </summary>
    /// <param name="continentalness">Continentalness field, updated in place.</param>
    /// <param name="warpNoise">Noise source for the edge falloff.</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    /// <param name="maxDimension">Larger footprint dimension.</param>
    private static void NormalizeContinentalness(float[,] continentalness, SeededNoise warpNoise,
        int width, int height, int maxDimension)
    {
        var min = float.MaxValue;
        var max = float.MinValue;
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                var value = continentalness[x, y];
                if (value < min) min = value;
                if (value > max) max = value;
            }
        }

        var range = System.MathF.Max(max - min, 1e-6f);
        var edgeWavelength = ContinentalnessWavelengthFactor * maxDimension;

        System.Threading.Tasks.Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                var normalized = (continentalness[x, y] - min) / range;
                var falloff = EdgeFalloff(warpNoise, x, y, width, height, edgeWavelength);
                var compressed = normalized * (1f - falloff) - falloff;
                continentalness[x, y] = System.Math.Clamp(compressed, 0f, 1f);
            }
        });
    }

    /// <summary>
    ///     Computes the continentalness edge falloff at a cell. The falloff compresses the field
    ///     toward the ocean range near the border, guaranteeing that border cells fall below the
    ///     ocean threshold, with low-frequency noise so the coast remains irregular.
    /// </summary>
    /// <param name="warpNoise">Noise source for the falloff modulation.</param>
    /// <param name="x">X coordinate in blocks.</param>
    /// <param name="y">Y coordinate in blocks.</param>
    /// <param name="width">Footprint width.</param>
    /// <param name="height">Footprint height.</param>
    /// <param name="edgeWavelength">Wavelength for the falloff modulation in blocks.</param>
    /// <returns>Falloff factor in [0, 1]; 1 at the border and 0 inland.</returns>
    private static float EdgeFalloff(SeededNoise warpNoise, int x, int y, int width, int height,
        float edgeWavelength)
    {
        const float edgeFraction = 0.08f;

        var distanceX = System.Math.Min(x, width - 1 - x);
        var distanceY = System.Math.Min(y, height - 1 - y);
        var tX = System.Math.Clamp(distanceX / (edgeFraction * width), 0f, 1f);
        var tY = System.Math.Clamp(distanceY / (edgeFraction * height), 0f, 1f);
        var t = System.Math.Min(tX, tY);
        var falloff = 1f - t * t * (3f - 2f * t);

        var modulation = 0.6f + 0.4f * warpNoise.Fbm(x, y, edgeWavelength, 2);
        return falloff * modulation;
    }
}
