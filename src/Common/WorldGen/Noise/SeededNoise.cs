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

namespace Sovereign.WorldGen.Noise;

/// <summary>
///     Deterministic hash and sub-seed derivation utilities shared by world generation stages.
/// </summary>
public static class SeedDerivation
{
    /// <summary>
    ///     Derives a stage sub-seed from the root world seed.
    /// </summary>
    /// <param name="rootSeed">Root world seed.</param>
    /// <param name="stageName">Name of the generation stage.</param>
    /// <returns>Sub-seed for the stage.</returns>
    public static ulong DeriveSubSeed(ulong rootSeed, string stageName)
    {
        return SplitMix64(rootSeed ^ SplitMix64(Fnv1a64(stageName)));
    }

    /// <summary>
    ///     Mixes a 64-bit value with the SplitMix64 finalizer.
    /// </summary>
    /// <param name="value">Value to mix.</param>
    /// <returns>Mixed value.</returns>
    public static ulong SplitMix64(ulong value)
    {
        value += 0x9E3779B97F4A7C15UL;
        value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
        value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
        return value ^ (value >> 31);
    }

    /// <summary>
    ///     Computes the FNV-1a 64-bit hash of a string.
    /// </summary>
    /// <param name="text">Text to hash.</param>
    /// <returns>Hash value.</returns>
    public static ulong Fnv1a64(string text)
    {
        const ulong offsetBasis = 0xCBF29CE484222325UL;
        const ulong prime = 0x100000001B3UL;

        var hash = offsetBasis;
        foreach (var c in text)
        {
            hash ^= (byte)c;
            hash *= prime;
        }

        return hash;
    }

    /// <summary>
    ///     Returns the next pseudo-random value in a SplitMix64 stream.
    /// </summary>
    /// <param name="state">Stream state; updated with the next state value.</param>
    /// <returns>Next pseudo-random 64-bit value.</returns>
    public static ulong Next(ref ulong state)
    {
        state += 0x9E3779B97F4A7C15UL;
        return SplitMix64(state);
    }
}

/// <summary>
///     Seeded two-dimensional simplex noise with fBm, ridged, and domain-warped variants.
/// </summary>
public sealed class SeededNoise
{
    /// <summary>
    ///     Multiplicative scale factor mapping raw simplex output to approximately [-1, 1].
    /// </summary>
    private const float OutputScale = 70f;

    /// <summary>
    ///     Constant offset applied to warp-field sample coordinates to decorrelate the
    ///     X and Y warp components.
    /// </summary>
    private const float WarpOffset = 31.416f;

    /// <summary>
    ///     Default magnitude of the inner domain warp in noise-space.
    /// </summary>
    private const float DefaultInnerWarpAmplitude = 0.35f;

    /// <summary>
    ///     Default magnitude of the outer domain warp in noise-space.
    /// </summary>
    private const float DefaultOuterWarpAmplitude = 0.40f;

    /// <summary>
    ///     Number of octaves in each warp-field fBm. Kept fine-grained so that warp shapes
    ///     coastlines rather than map-scale geometry.
    /// </summary>
    private const int WarpOctaves = 3;

    /// <summary>
    ///     Skew factor for the simplex grid.
    /// </summary>
    private const float F2 = 0.36602540378443865f;

    /// <summary>
    ///     Unskew factor for the simplex grid.
    /// </summary>
    private const float G2 = 0.21132486540518713f;

    /// <summary>
    ///     X components of the eight simplex gradient directions.
    /// </summary>
    private static readonly float[] GradientX = { 1f, -1f, 1f, -1f, 1f, -1f, 0f, 0f };

    /// <summary>
    ///     Y components of the eight simplex gradient directions.
    /// </summary>
    private static readonly float[] GradientY = { 1f, 1f, -1f, -1f, 0f, 0f, 1f, -1f };

    /// <summary>
    ///     Seeded permutation table, duplicated to avoid index wrapping.
    /// </summary>
    private readonly byte[] permutation = new byte[512];

    /// <summary>
    ///     Creates a noise source with a permutation table shuffled from the given seed.
    /// </summary>
    /// <param name="seed">Seed for the permutation table.</param>
    public SeededNoise(ulong seed)
    {
        for (var i = 0; i < 256; ++i)
        {
            permutation[i] = (byte)i;
        }

        var state = seed;
        for (var i = 255; i > 0; --i)
        {
            var j = (int)(SeedDerivation.Next(ref state) % (ulong)(i + 1));
            (permutation[i], permutation[j]) = (permutation[j], permutation[i]);
        }

        for (var i = 0; i < 256; ++i)
        {
            permutation[i + 256] = permutation[i];
        }
    }

    /// <summary>
    ///     Samples the raw simplex noise field.
    /// </summary>
    /// <param name="x">X coordinate in noise-space.</param>
    /// <param name="y">Y coordinate in noise-space.</param>
    /// <returns>Noise value in approximately [-1, 1].</returns>
    public float Sample(float x, float y)
    {
        var skew = (x + y) * F2;
        var i = MathF.Floor(x + skew);
        var j = MathF.Floor(y + skew);
        var unskew = (i + j) * G2;
        var x0 = x - (i - unskew);
        var y0 = y - (j - unskew);

        int i1, j1;
        if (x0 > y0)
        {
            i1 = 1;
            j1 = 0;
        }
        else
        {
            i1 = 0;
            j1 = 1;
        }

        var x1 = x0 - i1 + G2;
        var y1 = y0 - j1 + G2;
        var x2 = x0 - 1f + 2f * G2;
        var y2 = y0 - 1f + 2f * G2;

        var ii = (int)i & 255;
        var jj = (int)j & 255;

        var sum = Contribution(x0, y0, permutation[ii + permutation[jj]]);
        sum += Contribution(x1, y1, permutation[ii + i1 + permutation[jj + j1]]);
        sum += Contribution(x2, y2, permutation[ii + 1 + permutation[jj + 1]]);

        return sum * OutputScale;
    }

    /// <summary>
    ///     Samples fractal Brownian motion summed over octave doubling frequencies.
    /// </summary>
    /// <param name="x">X coordinate in blocks.</param>
    /// <param name="y">Y coordinate in blocks.</param>
    /// <param name="wavelength">Wavelength of the base octave in blocks.</param>
    /// <param name="octaves">Number of octaves to sum.</param>
    /// <returns>fBm value in approximately [-1, 1].</returns>
    public float Fbm(float x, float y, float wavelength, int octaves)
    {
        var amplitude = 1f;
        var total = 0f;
        var norm = 0f;
        var fx = x / wavelength;
        var fy = y / wavelength;
        for (var octave = 0; octave < octaves; ++octave)
        {
            total += amplitude * Sample(fx, fy);
            norm += amplitude;
            amplitude *= 0.5f;
            fx *= 2f;
            fy *= 2f;
        }

        return total / norm;
    }

    /// <summary>
    ///     Samples domain-warped fBm of the form
    ///     <c>fbm(p + k1 * fbm(p + k2 * fbm(p)))</c>.
    /// </summary>
    /// <param name="x">X coordinate in blocks.</param>
    /// <param name="y">Y coordinate in blocks.</param>
    /// <param name="wavelength">Wavelength of the base octave in blocks.</param>
    /// <param name="octaves">Number of octaves in the outer fBm.</param>
    /// <param name="warpAmplitudeInner">Magnitude of the inner warp in noise-space.</param>
    /// <param name="warpAmplitudeOuter">Magnitude of the outer warp in noise-space.</param>
    /// <returns>Warped fBm value in approximately [-1, 1].</returns>
    public float DomainWarpedFbm(float x, float y, float wavelength, int octaves,
        float warpAmplitudeInner = DefaultInnerWarpAmplitude,
        float warpAmplitudeOuter = DefaultOuterWarpAmplitude)
    {
        var nx = x / wavelength;
        var ny = y / wavelength;

        var qx = FbmNorm(nx, ny, WarpOctaves);
        var qy = FbmNorm(nx + WarpOffset, ny + WarpOffset, WarpOctaves);
        var wx = nx + warpAmplitudeInner * qx;
        var wy = ny + warpAmplitudeInner * qy;

        var rx = FbmNorm(wx + WarpOffset, wy, WarpOctaves);
        var ry = FbmNorm(wx, wy + WarpOffset, WarpOctaves);
        var vx = wx + warpAmplitudeOuter * rx;
        var vy = wy + warpAmplitudeOuter * ry;

        return FbmNorm(vx, vy, octaves);
    }

    /// <summary>
    ///     Samples a ridged transform of fBm; creases rise to 1 and flats fall to 0.
    /// </summary>
    /// <param name="x">X coordinate in blocks.</param>
    /// <param name="y">Y coordinate in blocks.</param>
    /// <param name="wavelength">Wavelength of the base octave in blocks.</param>
    /// <param name="octaves">Number of octaves to sum.</param>
    /// <returns>Ridged value in [0, 1].</returns>
    public float Ridged(float x, float y, float wavelength, int octaves)
    {
        var amplitude = 1f;
        var total = 0f;
        var norm = 0f;
        var fx = x / wavelength;
        var fy = y / wavelength;
        for (var octave = 0; octave < octaves; ++octave)
        {
            var ridge = 1f - MathF.Abs(Sample(fx, fy));
            total += amplitude * ridge * ridge;
            norm += amplitude;
            amplitude *= 0.5f;
            fx *= 2f;
            fy *= 2f;
        }

        return total / norm;
    }

    /// <summary>
    ///     Samples fBm over pre-normalized coordinates.
    /// </summary>
    /// <param name="nx">Normalized X coordinate.</param>
    /// <param name="ny">Normalized Y coordinate.</param>
    /// <param name="octaves">Number of octaves to sum.</param>
    /// <returns>fBm value in approximately [-1, 1].</returns>
    private float FbmNorm(float nx, float ny, int octaves)
    {
        var amplitude = 1f;
        var total = 0f;
        var norm = 0f;
        var fx = nx;
        var fy = ny;
        for (var octave = 0; octave < octaves; ++octave)
        {
            total += amplitude * Sample(fx, fy);
            norm += amplitude;
            amplitude *= 0.5f;
            fx *= 2f;
            fy *= 2f;
        }

        return total / norm;
    }

    /// <summary>
    ///     Computes the contribution of one simplex corner.
    /// </summary>
    /// <param name="dx">X offset from the corner.</param>
    /// <param name="dy">Y offset from the corner.</param>
    /// <param name="gradientIndex">Gradient index from the permutation table.</param>
    /// <returns>Corner contribution.</returns>
    private float Contribution(float dx, float dy, int gradientIndex)
    {
        var t = 0.5f - dx * dx - dy * dy;
        if (t <= 0f) return 0f;

        var g = gradientIndex & 7;
        t *= t;
        return t * t * (GradientX[g] * dx + GradientY[g] * dy);
    }
}
