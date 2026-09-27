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
    ///     Fraction of land cells below the mountain mask threshold; the remaining ridge crest
    ///     is masked in as mountains.
    /// </summary>
    private const float MountainCoverage = 0.125f;

    /// <summary>
    ///     Mountain amplitude in blocks above the continental base.
    /// </summary>
    private const int MountainAmplitude = 12;

    /// <summary>
    ///     Roughness amplitude in blocks; must be at least 3.
    /// </summary>
    private const int RoughnessAmplitude = 3;

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
    /// <returns>Shaped terrain result.</returns>
    public TerrainShapeResult Apply(TerrainFields fields, ContinentalnessResult continentalness,
        WorldGenProfile profile, ulong seed)
    {
        var width = profile.Width;
        var height = profile.Height;

        var mountainMask = BuildMountainMask(fields, continentalness, width, height);

        var heights = new int[width, height];
        var minHeight = profile.RockFloorZ + 8;
        var isOcean = new bool[width, height];
        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                isOcean[x, y] = continentalness.Classes[x, y] != ContinentalClass.Land;
                var baseZ = BaseHeight(fields.Continentalness[x, y], continentalness, profile);
                var raw = baseZ
                          + fields.MountainRidge[x, y] * mountainMask[x, y] * MountainAmplitude
                          + fields.Roughness[x, y] * RoughnessAmplitude;
                heights[x, y] = Math.Clamp((int)raw, minHeight, profile.SurfaceMaxZ);
            }
        });

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
    ///     Builds the mountain mask by thresholding the ridge field so that mountains cover
    ///     roughly the configured fraction of land cells.
    /// </summary>
    /// <param name="fields">Sampled terrain fields.</param>
    /// <param name="continentalness">Banded continentalness classification.</param>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <returns>Mountain mask in [0, 1].</returns>
    private static float[,] BuildMountainMask(TerrainFields fields, ContinentalnessResult continentalness,
        int width, int height)
    {
        var histogram = new int[RidgeHistogramBins];
        var landCells = 0L;
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (continentalness.Classes[x, y] != ContinentalClass.Land) continue;

                ++landCells;
                var bin = BinOf(fields.MountainRidge[x, y]);
                ++histogram[bin];
            }
        }

        var thresholdBin = ThresholdBin(histogram, landCells);

        var mask = new float[width, height];
        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                if (continentalness.Classes[x, y] != ContinentalClass.Land
                    || BinOf(fields.MountainRidge[x, y]) < thresholdBin)
                {
                    continue;
                }

                mask[x, y] = (fields.MountainRidge[x, y] - (float)thresholdBin / RidgeHistogramBins)
                             / (1f - (float)thresholdBin / RidgeHistogramBins);
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
        var target = (long)(landCells * (1.0 - MountainCoverage));
        var cumulative = 0L;
        for (var bin = 0; bin < RidgeHistogramBins; ++bin)
        {
            cumulative += histogram[bin];
            if (cumulative >= target) return bin;
        }

        return RidgeHistogramBins - 1;
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
