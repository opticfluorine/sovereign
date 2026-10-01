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
using Sovereign.WorldGen.Noise;

namespace Sovereign.WorldGen.Layout;

/// <summary>
///     Resolved layout fields for a plan: the full-resolution continentalness mask, the
///     optional regional bias fields, and the anchors that produced them. Null mask and
///     bias arrays mean the corresponding path is inactive and consumers keep their
///     pre-layout behavior.
/// </summary>
public sealed class LayoutFields
{
    /// <summary>
    ///     Footprint width in blocks.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    ///     Footprint height in blocks.
    /// </summary>
    public required int Height { get; init; }

    /// <summary>
    ///     Quarter-resolution grid width.
    /// </summary>
    public required int QuarterWidth { get; init; }

    /// <summary>
    ///     Quarter-resolution grid height.
    /// </summary>
    public required int QuarterHeight { get; init; }

    /// <summary>
    ///     Configured layout strength.
    /// </summary>
    public required float Strength { get; init; }

    /// <summary>
    ///     Resolved anchors after jitter, in resolution order.
    /// </summary>
    public required IReadOnlyList<ResolvedAnchor> Anchors { get; init; }

    /// <summary>
    ///     Continentalness mask at block resolution, or null when the mask is off.
    /// </summary>
    public float[,]? Mask { get; init; }

    /// <summary>
    ///     Mountain bias field at block resolution, or null when no anchor biases mountains.
    /// </summary>
    public float[,]? MountainBias { get; init; }

    /// <summary>
    ///     Temperature bias field at block resolution, or null when no anchor biases it.
    /// </summary>
    public float[,]? TemperatureBias { get; init; }

    /// <summary>
    ///     Moisture bias field at block resolution, or null when no anchor biases it.
    /// </summary>
    public float[,]? MoistureBias { get; init; }

    /// <summary>
    ///     Roughness bias field at block resolution, or null when no anchor biases it.
    /// </summary>
    public float[,]? RoughnessBias { get; init; }

    /// <summary>
    ///     Continentalness mask on the quarter-resolution grid, or null when the mask is off.
    /// </summary>
    public float[,]? QuarterMask { get; init; }

    /// <summary>
    ///     Snowcap climate-line shift on the quarter-resolution grid, or null when no anchor
    ///     biases mountains.
    /// </summary>
    public float[,]? QuarterSnowcapShift { get; init; }

    /// <summary>
    ///     Alpine climate-line shift on the quarter-resolution grid, or null when no anchor
    ///     biases mountains.
    /// </summary>
    public float[,]? QuarterAlpineShift { get; init; }
}

/// <summary>
///     Builds the layout mask and its companion bias fields from a profile layout section.
///     The mask is accumulated as smoothstep bumps at quarter resolution and bilinearly
///     upsampled to block resolution, bounding mask memory to about a sixteenth of a full
///     field. The mask is zero-centered: an empty mask is neutral 0.5 and anchors add land
///     rather than taxing everything else.
/// </summary>
public sealed class LayoutMaskStage
{
    /// <summary>
    ///     Block cells per mask grid cell.
    /// </summary>
    public const int DownsampleFactor = 4;

    /// <summary>
    ///     Minimum anchor center coordinate, keeping centers off the border falloff.
    /// </summary>
    public const float MinAnchorCoordinate = 0.02f;

    /// <summary>
    ///     Maximum anchor center coordinate, keeping centers off the border falloff.
    /// </summary>
    public const float MaxAnchorCoordinate = 0.98f;

    /// <summary>
    ///     Builds the layout fields for a plan.
    /// </summary>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <param name="options">Layout options.</param>
    /// <param name="seed">Layout sub-seed.</param>
    /// <param name="forcedJitter">Additional per-anchor jitter in normalized coordinates
    ///     applied on resample attempts, independent of the profile jitter; zero disables it.</param>
    /// <returns>Resolved layout fields.</returns>
    public LayoutFields Build(int width, int height, LayoutOptions options, ulong seed,
        float forcedJitter = 0f)
    {
        var anchors = ResolveAnchors(options, seed, forcedJitter);
        var hasMask = options.Strength > 0f && anchors.Count > 0;
        var hasMountain = HasBias(anchors, a => a.MountainBias);
        var hasTemperature = HasBias(anchors, a => a.TemperatureBias);
        var hasMoisture = HasBias(anchors, a => a.MoistureBias);
        var hasRoughness = HasBias(anchors, a => a.RoughnessBias);

        var quarterWidth = (width + DownsampleFactor - 1) / DownsampleFactor;
        var quarterHeight = (height + DownsampleFactor - 1) / DownsampleFactor;

        var quarterMask = new float[quarterWidth, quarterHeight];
        if (hasMask)
        {
            for (var qy = 0; qy < quarterHeight; ++qy)
            {
                var v = (DownsampleFactor * qy + 1.5f) / height;
                for (var qx = 0; qx < quarterWidth; ++qx)
                {
                    var u = (DownsampleFactor * qx + 1.5f) / width;
                    var sum = 0f;
                    foreach (var anchor in anchors)
                    {
                        sum += BumpAt(anchor, u, v);
                    }

                    quarterMask[qx, qy] = Math.Clamp(0.5f + sum, 0f, 1f);
                }
            }
        }

        float[,]? mask = null;
        float[,]? mountain = null;
        float[,]? temperature = null;
        float[,]? moisture = null;
        float[,]? roughness = null;
        float[,]? snowcapShift = null;
        float[,]? alpineShift = null;
        if (hasMask || hasMountain || hasTemperature || hasMoisture || hasRoughness)
        {
            var biasFields = LayoutBiasStage.Build(anchors, quarterWidth, quarterHeight, width, height);
            if (hasMask) mask = Upsample(quarterMask, quarterWidth, quarterHeight, width, height);
            if (hasMountain)
            {
                mountain = Upsample(biasFields.Mountain, quarterWidth, quarterHeight, width, height);
                snowcapShift = biasFields.SnowcapShift;
                alpineShift = biasFields.AlpineShift;
            }

            if (hasTemperature)
                temperature = Upsample(biasFields.Temperature, quarterWidth, quarterHeight, width, height);
            if (hasMoisture)
                moisture = Upsample(biasFields.Moisture, quarterWidth, quarterHeight, width, height);
            if (hasRoughness)
                roughness = Upsample(biasFields.Roughness, quarterWidth, quarterHeight, width, height);
        }

        return new LayoutFields
        {
            Width = width,
            Height = height,
            QuarterWidth = quarterWidth,
            QuarterHeight = quarterHeight,
            Strength = options.Strength,
            Anchors = anchors,
            Mask = mask,
            MountainBias = mountain,
            TemperatureBias = temperature,
            MoistureBias = moisture,
            RoughnessBias = roughness,
            QuarterMask = hasMask ? quarterMask : null,
            QuarterSnowcapShift = snowcapShift,
            QuarterAlpineShift = alpineShift
        };
    }

    /// <summary>
    ///     Resolves the layout anchors, applying a preset if configured and jittering each
    ///     center deterministically from the layout sub-seed.
    /// </summary>
    /// <param name="options">Layout options.</param>
    /// <param name="seed">Layout sub-seed.</param>
    /// <param name="forcedJitter">Additional per-anchor jitter in normalized coordinates
    ///     applied independently of the profile jitter; zero disables it.</param>
    /// <returns>Resolved anchors.</returns>
    public static List<ResolvedAnchor> ResolveAnchors(LayoutOptions options, ulong seed,
        float forcedJitter = 0f)
    {
        IReadOnlyList<LayoutAnchor> source = options.Preset is { } preset
            ? LayoutPresets.Resolve(preset, options.AnchorCount, seed)
            : options.Anchors ?? new List<LayoutAnchor>();

        var state = SeedDerivation.DeriveSubSeed(seed, "Layout.Jitter");
        var forcedState = SeedDerivation.DeriveSubSeed(seed, "Layout.ForcedJitter");
        var resolved = new List<ResolvedAnchor>(source.Count);
        for (var i = 0; i < source.Count; ++i)
        {
            var anchor = source[i];
            var x = anchor.X;
            var y = anchor.Y;
            if (anchor.Jitter > 0f)
            {
                x = anchor.X + (2f * NextUnit0(ref state) - 1f) * anchor.Jitter;
                y = anchor.Y + (2f * NextUnit0(ref state) - 1f) * anchor.Jitter;
            }

            if (forcedJitter > 0f)
            {
                x += (2f * NextUnit0(ref forcedState) - 1f) * forcedJitter;
                y += (2f * NextUnit0(ref forcedState) - 1f) * forcedJitter;
            }

            resolved.Add(new ResolvedAnchor
            {
                Index = i,
                X = Math.Clamp(x, MinAnchorCoordinate, MaxAnchorCoordinate),
                Y = Math.Clamp(y, MinAnchorCoordinate, MaxAnchorCoordinate),
                Radius = anchor.Radius,
                Weight = anchor.Weight,
                MountainBias = anchor.MountainBias,
                TemperatureBias = anchor.TemperatureBias,
                MoistureBias = anchor.MoistureBias,
                RoughnessBias = anchor.RoughnessBias
            });
        }

        return resolved;
    }

    /// <summary>
    ///     Determines whether two resolved anchor lists occupy identical positions. Used to
    ///     detect resample attempts whose forced jitter produced the same anchors as the
    ///     previous attempt, and therefore identical terrain.
    /// </summary>
    /// <param name="a">First anchor list.</param>
    /// <param name="b">Second anchor list, or null.</param>
    /// <returns>true when both lists have the same length and anchor centers.</returns>
    public static bool AnchorsEquivalent(IReadOnlyList<ResolvedAnchor> a,
        IReadOnlyList<ResolvedAnchor>? b)
    {
        if (b is null || a.Count != b.Count) return false;
        for (var i = 0; i < a.Count; ++i)
        {
            if (a[i].X != b[i].X || a[i].Y != b[i].Y) return false;
        }

        return true;
    }

    /// <summary>
    ///     Computes the smoothstep bump contribution of one anchor at a normalized point.
    /// </summary>
    /// <param name="anchor">Anchor to evaluate.</param>
    /// <param name="u">Normalized X coordinate.</param>
    /// <param name="v">Normalized Y coordinate.</param>
    /// <returns>Bump value in [0, weight].</returns>
    public static float BumpAt(ResolvedAnchor anchor, float u, float v)
    {
        if (anchor.Radius <= 0f) return 0f;

        var dx = u - anchor.X;
        var dy = v - anchor.Y;
        var distance = MathF.Sqrt(dx * dx + dy * dy);
        if (distance >= anchor.Radius) return 0f;

        var t = 1f - distance / anchor.Radius;
        return anchor.Weight * t * t * (3f - 2f * t);
    }

    /// <summary>
    ///     Bilinearly upsamples a quarter-resolution field to block resolution.
    /// </summary>
    /// <param name="quarter">Quarter-resolution field.</param>
    /// <param name="quarterWidth">Quarter-grid width.</param>
    /// <param name="quarterHeight">Quarter-grid height.</param>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <returns>Upsampled block-resolution field.</returns>
    public static float[,] Upsample(float[,] quarter, int quarterWidth, int quarterHeight,
        int width, int height)
    {
        var result = new float[width, height];
        for (var y = 0; y < height; ++y)
        {
            var qy = (y - 1.5f) / DownsampleFactor;
            for (var x = 0; x < width; ++x)
            {
                var qx = (x - 1.5f) / DownsampleFactor;
                result[x, y] = SampleBilinear(quarter, quarterWidth, quarterHeight, qx, qy);
            }
        }

        return result;
    }

    /// <summary>
    ///     Bilinearly samples a quarter-resolution field at one block cell using the same
    ///     half-cell mapping as <see cref="Upsample" />.
    /// </summary>
    /// <param name="quarter">Quarter-resolution field.</param>
    /// <param name="quarterWidth">Quarter-grid width.</param>
    /// <param name="quarterHeight">Quarter-grid height.</param>
    /// <param name="x">Block cell X coordinate.</param>
    /// <param name="y">Block cell Y coordinate.</param>
    /// <returns>Interpolated value.</returns>
    public static float SampleQuarter(float[,] quarter, int quarterWidth, int quarterHeight,
        int x, int y)
    {
        var qx = (x - 1.5f) / DownsampleFactor;
        var qy = (y - 1.5f) / DownsampleFactor;
        return SampleBilinear(quarter, quarterWidth, quarterHeight, qx, qy);
    }

    /// <summary>
    ///     Computes the value of a quarter-resolution field by bilinear interpolation.
    /// </summary>
    /// <param name="quarter">Quarter-resolution field.</param>
    /// <param name="quarterWidth">Quarter-grid width.</param>
    /// <param name="quarterHeight">Quarter-grid height.</param>
    /// <param name="qx">Fractional quarter-grid X coordinate.</param>
    /// <param name="qy">Fractional quarter-grid Y coordinate.</param>
    /// <returns>Interpolated value.</returns>
    private static float SampleBilinear(float[,] quarter, int quarterWidth, int quarterHeight,
        float qx, float qy)
    {
        var x0 = Math.Clamp((int)MathF.Floor(qx), 0, quarterWidth - 1);
        var x1 = Math.Clamp(x0 + 1, 0, quarterWidth - 1);
        var y0 = Math.Clamp((int)MathF.Floor(qy), 0, quarterHeight - 1);
        var y1 = Math.Clamp(y0 + 1, 0, quarterHeight - 1);
        var tx = Math.Clamp(qx - x0, 0f, 1f);
        var ty = Math.Clamp(qy - y0, 0f, 1f);

        var top = quarter[x0, y0] * (1f - tx) + quarter[x1, y0] * tx;
        var bottom = quarter[x0, y1] * (1f - tx) + quarter[x1, y1] * tx;
        return top * (1f - ty) + bottom * ty;
    }

    /// <summary>
    ///     Determines whether any resolved anchor carries a nonzero value for the given bias.
    /// </summary>
    /// <param name="anchors">Resolved anchors.</param>
    /// <param name="selector">Bias selector.</param>
    /// <returns>true if any bias is nonzero, false otherwise.</returns>
    private static bool HasBias(IReadOnlyList<ResolvedAnchor> anchors, Func<ResolvedAnchor, float> selector)
    {
        foreach (var anchor in anchors)
        {
            if (selector(anchor) != 0f) return true;
        }

        return false;
    }

    /// <summary>
    ///     Samples a uniform float in [0, 1) from the jitter stream.
    /// </summary>
    /// <param name="state">Random stream state.</param>
    /// <returns>Sample in [0, 1).</returns>
    private static float NextUnit0(ref ulong state)
    {
        const float scale = 1f / (1UL << 24);
        return (SeedDerivation.Next(ref state) >> 40) * scale;
    }
}
