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
using Sovereign.WorldGen.Layout;
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
    ///     Mountain ridge wavelength as a fraction of the larger footprint dimension.
    /// </summary>
    private const float RidgeWavelengthFactor = 1f / 6f;

    /// <summary>
    ///     Roughness wavelength in blocks.
    /// </summary>
    private const float RoughnessWavelength = 48f;

    /// <summary>
    ///     Number of octaves in the mountain ridge fBm.
    /// </summary>
    private const int RidgeOctaves = 4;

    /// <summary>
    ///     Number of octaves in the roughness fBm.
    /// </summary>
    private const int RoughnessOctaves = 3;

    /// <summary>
    ///     Fraction of the ocean threshold at which the contrast stretch re-anchors it;
    ///     the land band above the threshold is spread across the remainder of [0, 1].
    ///     Calibrated so that the tuned constants keep a continental land fraction at the
    ///     reference footprint.
    /// </summary>
    private const float StretchAnchorScale = 0.85f;

    /// <summary>
    ///     Samples all terrain fields for the given footprint.
    /// </summary>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <param name="seed">Sub-seed for field sampling.</param>
    /// <param name="terrain">Terrain generation options.</param>
    /// <param name="layout">Resolved layout fields whose mask is combined into
    /// continentalness, or null for the unmasked path.</param>
    /// <returns>Sampled fields.</returns>
    public TerrainFields Sample(int width, int height, ulong seed, TerrainOptions terrain,
        LayoutFields? layout = null)
    {
        var continentalness = new float[width, height];
        var ridge = new float[width, height];
        var roughness = new float[width, height];

        var maxDimension = System.Math.Max(width, height);
        var continentalnessWavelength = terrain.ContinentalnessWavelengthFactor * maxDimension;
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
                    terrain.ContinentalnessOctaves, terrain.WarpAmplitudeInner,
                    terrain.WarpAmplitudeOuter);
                ridge[x, y] = ridgeNoise.Ridged(x, y, ridgeWavelength, RidgeOctaves);
                roughness[x, y] = RoughnessOf(roughnessNoise, x, y);
            }
        });

        NormalizeAndStretch(continentalness, width, height, terrain);
        if (layout is { Mask: { } mask })
        {
            ApplyMask(continentalness, mask, layout.Strength, width, height);
        }

        ApplyEdgeFalloff(continentalness, warpNoise, width, height, maxDimension, terrain);

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
    ///     a contrast stretch around the land band so that mid-range differences translate
    ///     into coastline variety instead of concentric bands.
    /// </summary>
    /// <param name="continentalness">Continentalness field, updated in place.</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    /// <param name="terrain">Terrain generation options.</param>
    private static void NormalizeAndStretch(float[,] continentalness, int width, int height,
        TerrainOptions terrain)
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
        var oceanThreshold = terrain.Thresholds.Ocean;

        System.Threading.Tasks.Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                var normalized = (continentalness[x, y] - min) / range;
                continentalness[x, y] = ContrastStretch(normalized, oceanThreshold);
            }
        });
    }

    /// <summary>
    ///     Combines the layout mask into the normalized continentalness field additively, so
    ///     that band thresholds keep their meaning and strength interpolates between no
    ///     layout and mask-dominant.
    /// </summary>
    /// <param name="continentalness">Normalized continentalness field, updated in place.</param>
    /// <param name="mask">Layout mask in [0, 1].</param>
    /// <param name="strength">Layout strength in [0, 1].</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    private static void ApplyMask(float[,] continentalness, float[,] mask, float strength,
        int width, int height)
    {
        System.Threading.Tasks.Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                var value = continentalness[x, y] + strength * (mask[x, y] - 0.5f);
                continentalness[x, y] = System.Math.Clamp(value, 0f, 1f);
            }
        });
    }

    /// <summary>
    ///     Applies an edge falloff so that the footprint border always trends below sea level.
    ///     The falloff is applied after the layout mask so that no anchor can put land on the
    ///     map border regardless of strength.
    /// </summary>
    /// <param name="continentalness">Continentalness field, updated in place.</param>
    /// <param name="warpNoise">Noise source for the edge falloff.</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    /// <param name="maxDimension">Larger footprint dimension.</param>
    /// <param name="terrain">Terrain generation options.</param>
    private static void ApplyEdgeFalloff(float[,] continentalness, SeededNoise warpNoise,
        int width, int height, int maxDimension, TerrainOptions terrain)
    {
        var edgeWavelength = terrain.ContinentalnessWavelengthFactor * maxDimension;

        System.Threading.Tasks.Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                var falloff = EdgeFalloff(warpNoise, x, y, width, height, edgeWavelength);
                var compressed = continentalness[x, y] * (1f - falloff) - falloff;
                continentalness[x, y] = System.Math.Clamp(compressed, 0f, 1f);
            }
        });
    }

    /// <summary>
    ///     Applies a monotone smoothstep remap that spreads the land band above the ocean
    ///     threshold across more of [0, 1]. Below the threshold the ocean range is compressed
    ///     toward zero; above it, the range is re-anchored at
    ///     <see cref="StretchAnchorScale" /> times the threshold and eased with smoothstep so
    ///     the remap stays deterministic and C1-continuous.
    /// </summary>
    /// <param name="value">Normalized continentalness in [0, 1].</param>
    /// <param name="oceanThreshold">Ocean band threshold in (0, 1).</param>
    /// <returns>Contrast-stretched value in [0, 1].</returns>
    internal static float ContrastStretch(float value, float oceanThreshold)
    {
        var anchor = StretchAnchorScale * oceanThreshold;
        if (value < oceanThreshold)
        {
            return anchor * SmoothStep(value / oceanThreshold);
        }

        var t = (value - oceanThreshold) / (1f - oceanThreshold);
        return anchor + (1f - anchor) * SmoothStep(t);
    }

    /// <summary>
    ///     Computes the smoothstep easing of a value in [0, 1].
    /// </summary>
    /// <param name="t">Value in [0, 1].</param>
    /// <returns>Eased value in [0, 1].</returns>
    private static float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
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
