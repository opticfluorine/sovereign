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
using System.Threading;
using System.Threading.Tasks;
using Sovereign.WorldGen.Layout;

namespace Sovereign.WorldGen.Terrain;

/// <summary>
///     Shaped terrain surface for a plan.
/// </summary>
public sealed class TerrainShapeResult
{
    /// <summary>
    ///     Terrain map with heights, water classes, cliff flags, and beach flags populated.
    /// </summary>
    public required TerrainMap Map { get; init; }
}

/// <summary>
///     Combines the sampled terrain fields into integer surface heights, relaxes slopes,
///     and marks cliffs and beaches.
/// </summary>
public sealed class TerrainShapeStage
{
    /// <summary>
    ///    Number of histogram bins used to threshold the mountain ridge field.
    /// </summary>
    private const int RidgeHistogramBins = 4096;

    /// <summary>
    ///     Fraction of a mass's land cells below its mountain mask threshold; the remaining
    ///     ridge crest is masked in as mountains. The same constant the global cut uses.
    /// </summary>
    private const float BaseCoverage = 0.125f;

    /// <summary>
    ///     Coverage points a full mountain bias adds to a mass's quota. The bias shifts the
    ///     coverage before the cut, making bias a coverage control: +1 adds ~10 points of
    ///     mountain coverage, -1 removes ~10. Sign convention preserved: positive bias is
    ///     more mountains.
    /// </summary>
    private const float CoverageSensitivity = 0.10f;

    /// <summary>
    ///     Minimum clipped mass coverage.
    /// </summary>
    private const float MinCoverage = 0f;

    /// <summary>
    ///     Maximum clipped mass coverage.
    /// </summary>
    private const float MaxCoverage = 0.5f;

    /// <summary>
    ///     Mass area in cells below which the per-mass coverage is damped linearly. Roughly
    ///     a 50x50 island; smaller islets carry a proportional share of the quota instead of
    ///     statutorily keeping a full quota of alpine blocks. Applies only to the per-mass
    ///     path; the global cut is undamped.
    /// </summary>
    private const long MinQuotaArea = 2500;

    /// <summary>
    ///     Damped-coverage floor: a damped mass keeps at least this absolute coverage, so
    ///     tiny islets still carry a little mountain texture.
    /// </summary>
    private const float MinQuotaCoverageFloor = 0.05f;

    /// <summary>
    ///     Mountain amplitude in blocks above the continental base.
    /// </summary>
    private const int MountainAmplitude = 12;

    /// <summary>
    ///     Roughness amplitude in blocks; must be at least 3.
    /// </summary>
    private const int RoughnessAmplitude = 3;

    /// <summary>
    ///     Fraction of the roughness range that a full layout roughness bias contributes.
    /// </summary>
    private const float RoughnessBiasCoefficient = 0.15f;

    /// <summary>
    ///     Depth in blocks below sea level at the boundary between deep ocean and shelf.
    /// </summary>
    private const int ShelfDepth = 6;

    /// <summary>
    ///     Half-width in blocks of the beach band around sea level.
    /// </summary>
    private const int BeachBandHalfWidth = 1;

    /// <summary>
    ///     Shapes the terrain surface from the sampled fields.
    /// </summary>
    /// <param name="fields">Sampled terrain fields.</param>
    /// <param name="continentalness">Banded continentalness classification.</param>
    /// <param name="profile">World generation profile.</param>
    /// <param name="seed">Sub-seed for terrain shaping.</param>
    /// <param name="layout">Resolved layout fields whose biases modulate the mountain mask
    /// and roughness, or null for the unbiased path.</param>
    /// <param name="anchors">Resolved layout anchors used for mass significance, or null when
    /// no anchors are available and the global cut applies to all land.</param>
    /// <param name="cancellationToken">Token observed at stage boundaries.</param>
    /// <returns>Shaped terrain result.</returns>
    public TerrainShapeResult Apply(TerrainFields fields, ContinentalnessResult continentalness,
        WorldGenProfile profile, ulong seed, LayoutFields? layout = null,
        IReadOnlyList<ResolvedAnchor>? anchors = null,
        CancellationToken cancellationToken = default)
    {
        var width = profile.Width;
        var height = profile.Height;
        cancellationToken.ThrowIfCancellationRequested();

        var mountainMask = BuildMountainMask(fields, continentalness, width, height,
            layout?.MountainBias, anchors, cancellationToken);
        var roughnessBias = layout?.RoughnessBias;

        var heights = new int[width, height];
        var minHeight = profile.RockFloorZ + 8;
        var isOcean = new bool[width, height];
        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                isOcean[x, y] = continentalness.Classes[x, y] != ContinentalClass.Land;
                var baseZ = BaseHeight(fields.Continentalness[x, y], continentalness, profile);
                var roughness = roughnessBias is null
                    ? fields.Roughness[x, y]
                    : Math.Clamp(fields.Roughness[x, y]
                                 + RoughnessBiasCoefficient * roughnessBias[x, y], 0f, 1f);
                var raw = baseZ
                          + fields.MountainRidge[x, y] * mountainMask[x, y] * MountainAmplitude
                          + roughness * RoughnessAmplitude;
                heights[x, y] = Math.Clamp((int)raw, minHeight, profile.SurfaceMaxZ);
            }
        });

        cancellationToken.ThrowIfCancellationRequested();

        // Only land cells participate in relaxation: the seafloor keeps its shape, and land
        // is not eroded toward the depth of the adjacent ocean floor.
        var movable = new bool[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                movable[x, y] = !isOcean[x, y];
            }
        }

        var cliffs = new bool[width, height];
        SlopeRelaxation.Relax(heights, cliffs, movable);

        cancellationToken.ThrowIfCancellationRequested();

        var isBeach = new bool[width, height];
        MarkBeaches(heights, isOcean, isBeach, width, height, profile);

        var map = new TerrainMap
        {
            Width = width,
            Height = height,
            Heights = heights,
            IsOcean = isOcean,
            IsCliff = cliffs,
            IsBeach = isBeach,
            IsRiver = new bool[width, height],
            RiverWidth = new int[width, height],
            IsBank = new bool[width, height],
            IsLake = new bool[width, height],
            LakeSurfaceZ = new int[width, height]
        };

        return new TerrainShapeResult { Map = map };
    }

    /// <summary>
    ///     Builds the mountain mask over every land cell. Significant masses cut their own
    ///     ridge histograms at their own coverage quotas; decoration land shares one global
    ///     histogram at the base coverage. Per-mass cuts are computed from histograms over
    ///     the mass's cells with stable tie-breaking by cell index, so results carry no
    ///     parallel-order or hash-order dependence.
    /// </summary>
    /// <param name="fields">Sampled terrain fields.</param>
    /// <param name="continentalness">Banded continentalness classification.</param>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <param name="mountainBias">Layout mountain bias field, or null for the unbiased path.</param>
    /// <param name="anchors">Resolved layout anchors for mass significance, or null to treat
    /// all land as decoration under the global cut.</param>
    /// <param name="cancellationToken">Token observed between per-mass cuts.</param>
    /// <returns>Mountain mask in [0, 1].</returns>
    internal static float[,] BuildMountainMask(TerrainFields fields,
        ContinentalnessResult continentalness, int width, int height, float[,]? mountainBias,
        IReadOnlyList<ResolvedAnchor>? anchors = null, CancellationToken cancellationToken = default)
    {
        var report = MassAnalysis.Analyze(
            SyntheticMap(continentalness, width, height), anchors);
        var labels = report.Labels;

        // Per-mass mask quota thresholds, keyed by component label. Decoration labels are
        // absent and fall to the global cut.
        var thresholds = new Dictionary<long, int>();
        foreach (var mass in report.Masses)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!mass.Significant) continue;

            var bias = mountainBias is null
                ? 0f
                : MeanMassBias(mountainBias, labels, mass);
            var coverage = Math.Clamp(BaseCoverage + bias * CoverageSensitivity,
                MinCoverage, MaxCoverage);
            if (mass.Size < MinQuotaArea)
            {
                coverage = Math.Max(coverage * mass.Size / (float)MinQuotaArea,
                    MinQuotaCoverageFloor);
            }

            thresholds[mass.ComponentId] = PerMassThresholdBin(fields, labels,
                width, height, mass.ComponentId, coverage);
        }

        var mask = new float[width, height];
        var globalThreshold = ThresholdBin(
            GlobalHistogram(fields, continentalness, width, height),
            CountLand(continentalness, width, height));

        Parallel.For(0, height, y =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var x = 0; x < width; ++x)
            {
                if (continentalness.Classes[x, y] != ContinentalClass.Land) continue;

                var label = labels[x, y];
                var effectiveBin = thresholds.TryGetValue(label, out var massThreshold)
                    ? massThreshold
                    : globalThreshold;

                if (BinOf(fields.MountainRidge[x, y]) < effectiveBin) continue;

                var floor = (float)effectiveBin / RidgeHistogramBins;
                mask[x, y] = (fields.MountainRidge[x, y] - floor) / (1f - floor);
            }
        });

        return mask;
    }

    /// <summary>
    ///     Computes the histogram bin of a ridge value.
    /// </summary>
    /// <param name="value">Ridge value in [0, 1].</param>
    /// <returns>Histogram bin index.</returns>
    private static int BinOf(float value)
    {
        return Math.Clamp((int)(value * RidgeHistogramBins), 0, RidgeHistogramBins - 1);
    }

    /// <summary>
    ///     Finds the histogram bin at which the configured mountain coverage is reached.
    /// </summary>
    /// <param name="histogram">Ridge value histogram over land cells.</param>
    /// <param name="landCells">Number of land cells histogrammed.</param>
    /// <returns>Threshold bin index.</returns>
    private static int ThresholdBin(int[] histogram, long landCells)
    {
        var target = (long)(landCells * (1.0 - BaseCoverage));
        var cumulative = 0L;
        for (var bin = 0; bin < RidgeHistogramBins; ++bin)
        {
            cumulative += histogram[bin];
            if (cumulative >= target) return bin;
        }

        return RidgeHistogramBins - 1;
    }

    /// <summary>
    ///     Counts the land cells of a classification.
    /// </summary>
    /// <param name="continentalness">Banded classification.</param>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <returns>Land cell count.</returns>
    private static long CountLand(ContinentalnessResult continentalness, int width, int height)
    {
        var count = 0L;
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (continentalness.Classes[x, y] == ContinentalClass.Land) ++count;
            }
        }

        return count;
    }

    /// <summary>
    ///     Builds a synthetic terrain map whose water classes mirror the classification, so
    ///     <see cref="MassAnalysis" /> and its per-mass cutoffs can read the land cells.
    /// </summary>
    /// <param name="continentalness">Banded classification.</param>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <returns>Map with IsOcean set to non-land.</returns>
    private static TerrainMap SyntheticMap(ContinentalnessResult continentalness, int width,
        int height)
    {
        var isOcean = new bool[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                isOcean[x, y] = continentalness.Classes[x, y] != ContinentalClass.Land;
            }
        }

        return new TerrainMap
        {
            Width = width,
            Height = height,
            Heights = new int[width, height],
            IsOcean = isOcean,
            IsCliff = new bool[width, height],
            IsBeach = new bool[width, height],
            IsRiver = new bool[width, height],
            RiverWidth = new int[width, height],
            IsBank = new bool[width, height],
            IsLake = new bool[width, height],
            LakeSurfaceZ = new int[width, height]
        };
    }

    /// <summary>
    ///     Computes the mean layout mountain bias over a mass's cells. The bias field is
    ///     provided at block resolution (the mask stage's caller upsamples the quarter-
    ///     resolution layout bias); the mean is a plain deterministic scan.
    /// </summary>
    /// <param name="mountainBias">Block-resolution bias field.</param>
    /// <param name="labels">Component labels.</param>
    /// <param name="mass">Analyzed mass.</param>
    /// <returns>Mean bias over the mass cells, or zero when the mass has no cells.</returns>
    private static float MeanMassBias(float[,] mountainBias, int[,] labels,
        MassAnalysisResult mass)
    {
        var sum = 0.0;
        var count = 0L;
        var width = labels.GetLength(0);
        var height = labels.GetLength(1);
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (labels[x, y] != mass.ComponentId) continue;
                sum += mountainBias[x, y];
                ++count;
            }
        }

        return count == 0 ? 0f : (float)(sum / count);
    }

    /// <summary>
    ///     Computes the histogram bin of a mass's ridge distribution at which the given
    ///     coverage fraction of the mass's own cells is masked in. The count of cells below
    ///     a bin depends only on the multiset of ridge values, so the cut is independent of
    ///     cell ordering and thread scheduling.
    /// </summary>
    /// <param name="fields">Sampled terrain fields.</param>
    /// <param name="labels">Component labels.</param>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <param name="componentId">Label of the mass under test.</param>
    /// <param name="coverage">Coverage fraction in [0, 1] of the mass's cells to mask in.</param>
    /// <returns>Threshold bin index.</returns>
    private static int PerMassThresholdBin(TerrainFields fields, int[,] labels, int width,
        int height, long componentId, float coverage)
    {
        var histogram = new int[RidgeHistogramBins];
        var cells = 0L;
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (labels[x, y] != componentId) continue;

                ++cells;
                ++histogram[BinOf(fields.MountainRidge[x, y])];
            }
        }

        if (cells == 0) return RidgeHistogramBins - 1;

        var target = (long)(cells * (1.0 - coverage));
        var cumulative = 0L;
        for (var bin = 0; bin < RidgeHistogramBins; ++bin)
        {
            cumulative += histogram[bin];
            if (cumulative >= target) return bin;
        }

        return RidgeHistogramBins - 1;
    }

    /// <summary>
    ///     Computes the ridge histogram over all land cells for the shared decorative cut.
    /// </summary>
    /// <param name="fields">Sampled terrain fields.</param>
    /// <param name="continentalness">Banded classification.</param>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <returns>Histogram of ridge bins over land.</returns>
    private static int[] GlobalHistogram(TerrainFields fields,
        ContinentalnessResult continentalness, int width, int height)
    {
        var histogram = new int[RidgeHistogramBins];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (continentalness.Classes[x, y] != ContinentalClass.Land) continue;

                ++histogram[BinOf(fields.MountainRidge[x, y])];
            }
        }

        return histogram;
    }

    /// <summary>
    ///     Maps normalized continentalness to a base surface height. The mapping is piecewise
    ///     linear through the deep-ocean, shelf, and coast thresholds so that ocean cells lie
    ///     below sea level, shelves just beneath it, and land above it.
    /// </summary>
    /// <param name="continentalness">Normalized continentalness in [0, 1].</param>
    /// <param name="bands">Banded classification thresholds.</param>
    /// <param name="profile">World generation profile.</param>
    /// <returns>Base surface height in blocks.</returns>
    private static float BaseHeight(float continentalness, ContinentalnessResult bands,
        WorldGenProfile profile)
    {
        var floorZ = profile.RockFloorZ + 8;
        var shelfZ = profile.SeaLevelZ - ShelfDepth;
        var coastZ = profile.SeaLevelZ;

        if (continentalness < bands.ThresholdOcean)
        {
            return MapRange(continentalness, 0f, bands.ThresholdOcean, floorZ, shelfZ);
        }

        if (continentalness < bands.ThresholdCoast)
        {
            return MapRange(continentalness, bands.ThresholdOcean, bands.ThresholdCoast, shelfZ, coastZ);
        }

        return MapRange(continentalness, bands.ThresholdCoast, 1f, coastZ, profile.SurfaceMaxZ);
    }

    /// <summary>
    ///     Maps a value from one range to another.
    /// </summary>
    /// <param name="value">Value to map.</param>
    /// <param name="fromMin">Source range minimum.</param>
    /// <param name="fromMax">Source range maximum.</param>
    /// <param name="toMin">Target range minimum.</param>
    /// <param name="toMax">Target range maximum.</param>
    /// <returns>Mapped value.</returns>
    private static float MapRange(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        var t = (value - fromMin) / (fromMax - fromMin);
        return toMin + t * (toMax - toMin);
    }

    /// <summary>
    ///     Marks land cells in the beach band around sea level that are adjacent to water.
    /// </summary>
    /// <param name="heights">Height field.</param>
    /// <param name="isOcean">Water flags.</param>
    /// <param name="isBeach">Beach flags, updated in place.</param>
    /// <param name="width">Footprint width.</param>
    /// <param name="height">Footprint height.</param>
    /// <param name="profile">World generation profile.</param>
    private static void MarkBeaches(int[,] heights, bool[,] isOcean, bool[,] isBeach,
        int width, int height, WorldGenProfile profile)
    {
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (isOcean[x, y]) continue;

                var h = heights[x, y];
                if (h < profile.SeaLevelZ - BeachBandHalfWidth
                    || h > profile.SeaLevelZ + BeachBandHalfWidth)
                {
                    continue;
                }

                if (IsAdjacentToWater(isOcean, x, y, width, height)) isBeach[x, y] = true;
            }
        }
    }

    /// <summary>
    ///     Determines whether a cell has a cardinal neighbor that is water.
    /// </summary>
    /// <param name="isOcean">Water flags.</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <param name="width">Footprint width.</param>
    /// <param name="height">Footprint height.</param>
    /// <returns>true if a cardinal neighbor is water, false otherwise.</returns>
    private static bool IsAdjacentToWater(bool[,] isOcean, int x, int y, int width, int height)
    {
        return x > 0 && isOcean[x - 1, y]
                      || y > 0 && isOcean[x, y - 1]
                      || x < width - 1 && isOcean[x + 1, y]
                      || y < height - 1 && isOcean[x, y + 1];
    }
}
