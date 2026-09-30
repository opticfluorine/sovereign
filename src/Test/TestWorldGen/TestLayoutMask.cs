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
using Sovereign.WorldGen;
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Layout;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Runs the terrain stages for layout tests, resolving the layout fields exactly as the
///     pipeline does for the initial attempt.
/// </summary>
internal static class TestLayoutRunner
{
    /// <summary>
    ///     Runs terrain sampling, banding, and shaping for a profile.
    /// </summary>
    /// <param name="profile">Profile to run.</param>
    /// <param name="seed">Root seed.</param>
    /// <returns>Fields, continentalness, shaped map, and resolved layout fields.</returns>
    public static (TerrainFields Fields, ContinentalnessResult Continentalness, TerrainMap Map,
        LayoutFields? Layout) Run(WorldGenProfile profile, ulong seed)
    {
        LayoutFields? layout = null;
        if (profile.Layout is { IsActive: true } options)
        {
            layout = new LayoutMaskStage().Build(profile.Width, profile.Height, options,
                SeedDerivation.DeriveSubSeed(seed, "Layout"));
        }

        var fields = new TerrainFieldStack().Sample(profile.Width, profile.Height,
            SeedDerivation.DeriveSubSeed(seed, "TerrainFields"), profile.Terrain, layout);
        var continentalness = new ContinentalnessStage().Apply(fields, profile.Width,
            profile.Height, profile.Terrain);
        var map = new TerrainShapeStage().Apply(fields, continentalness, profile,
            SeedDerivation.DeriveSubSeed(seed, "TerrainShape"), layout).Map;
        return (fields, continentalness, map, layout);
    }
}

/// <summary>
///     Unit tests for the layout mask and bias fields.
/// </summary>
public class TestLayoutMask
{
    /// <summary>
    ///     Fixed seed used by the layout tests.
    /// </summary>
    private const ulong Seed = 12345;

    [Fact]
    public void Mask_SingleAnchor_CentersLandMassAtAnchor()
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Layout = SingleAnchor(1f, 0.25f);

        var (_, _, map, layout) = TestLayoutRunner.Run(profile, Seed);

        Assert.NotNull(layout);
        var (centroidX, centroidY, size) = LargestLandCentroid(map);
        Assert.True(size > 0, "The masked plan should contain land.");
        Assert.InRange(centroidX, 0.45f, 0.55f);
        Assert.InRange(centroidY, 0.45f, 0.55f);
    }

    [Fact]
    public void Mask_SmoothstepFalloff_IsMonotoneBeyondPeak()
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Layout = SingleAnchor(1f, 0.40f);
        var fields = new LayoutMaskStage().Build(profile.Width, profile.Height, profile.Layout,
            SeedDerivation.DeriveSubSeed(Seed, "Layout"));
        var quarter = Assert.IsType<float[,]>(fields.QuarterMask);

        var quarterY = fields.QuarterHeight / 2;
        var previous = float.MaxValue;
        for (var quarterX = fields.QuarterWidth / 2; quarterX < fields.QuarterWidth; ++quarterX)
        {
            var value = quarter[quarterX, quarterY];
            Assert.True(value <= previous + 1e-6f,
                $"Mask rose along the radius at qx={quarterX}: {previous} -> {value}.");
            previous = value;
        }
    }

    [Fact]
    public void Mask_QuarterResolution_UpsampleMatchesDirectSample()
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Layout = SingleAnchor(1f, 0.30f);
        var fields = new LayoutMaskStage().Build(profile.Width, profile.Height, profile.Layout,
            SeedDerivation.DeriveSubSeed(Seed, "Layout"));
        var mask = Assert.IsType<float[,]>(fields.Mask);

        Assert.Equal((profile.Width + 3) / 4, fields.QuarterWidth);
        Assert.Equal((profile.Height + 3) / 4, fields.QuarterHeight);
        Assert.NotNull(fields.QuarterMask);

        var maxDifference = 0f;
        for (var y = 0; y < profile.Height; ++y)
        {
            for (var x = 0; x < profile.Width; ++x)
            {
                var u = (x + 0.5f) / profile.Width;
                var v = (y + 0.5f) / profile.Height;
                var direct = 0f;
                foreach (var anchor in fields.Anchors) direct += LayoutMaskStage.BumpAt(anchor, u, v);
                maxDifference = MathF.Max(maxDifference, MathF.Abs(direct - mask[x, y]));
            }
        }

        Assert.InRange(maxDifference, 0f, 0.1f);
    }

    [Fact]
    public void Mask_StrengthInterpolation_RaisesContinentalnessAtAnchor()
    {
        var center0 = ContinentalnessAtCenter(0f);
        var centerHalf = ContinentalnessAtCenter(0.5f);
        var center1 = ContinentalnessAtCenter(1f);

        Assert.True(centerHalf > center0,
            $"Half strength ({centerHalf}) should exceed zero ({center0}).");
        Assert.True(center1 > centerHalf,
            $"Full strength ({center1}) should exceed half ({centerHalf}).");
    }

    [Fact]
    public void Mask_ZeroStrength_IsByteIdenticalToNoLayout()
    {
        var plain = TestProfiles.CreateSmall128();
        var (_, _, plainMap, _) = TestLayoutRunner.Run(plain, Seed);

        var zero = TestProfiles.CreateSmall128();
        zero.Layout = SingleAnchor(0f, 0.25f);
        var (_, _, zeroMap, _) = TestLayoutRunner.Run(zero, Seed);

        for (var y = 0; y < plain.Height; ++y)
        {
            for (var x = 0; x < plain.Width; ++x)
            {
                Assert.Equal(plainMap.Heights[x, y], zeroMap.Heights[x, y]);
            }
        }
    }

    [Fact]
    public void Mask_ZeroStrengthWithAnchor_IsInactive()
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Layout = SingleAnchor(0f, 0.30f);
        var fields = new LayoutMaskStage().Build(profile.Width, profile.Height, profile.Layout,
            SeedDerivation.DeriveSubSeed(Seed, "Layout"));

        Assert.Null(fields.Mask);
        Assert.Null(fields.QuarterMask);
    }

    [Fact]
    public void Bias_Absent_FieldsAreNull()
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Layout = SingleAnchor(1f, 0.30f);
        var fields = new LayoutMaskStage().Build(profile.Width, profile.Height, profile.Layout,
            SeedDerivation.DeriveSubSeed(Seed, "Layout"));

        Assert.NotNull(fields.Mask);
        Assert.Null(fields.MountainBias);
        Assert.Null(fields.TemperatureBias);
        Assert.Null(fields.MoistureBias);
        Assert.Null(fields.RoughnessBias);
    }

    [Fact]
    public void Bias_MountainBias_RaisesMountainCoverage()
    {
        var profile = TestProfiles.CreateSmall128();
        var (fields, continentalness, _, _) = TestLayoutRunner.Run(profile, Seed);

        var biasProfile = TestProfiles.CreateSmall128();
        biasProfile.Layout = SingleAnchor(0f, 0.45f, mountainBias: 1f);
        var layout = new LayoutMaskStage().Build(biasProfile.Width, biasProfile.Height,
            biasProfile.Layout, SeedDerivation.DeriveSubSeed(Seed, "Layout"));
        var bias = Assert.IsType<float[,]>(layout.MountainBias);

        var plain = TerrainShapeStage.BuildMountainMask(fields, continentalness,
            profile.Width, profile.Height, null);
        var biased = TerrainShapeStage.BuildMountainMask(fields, continentalness,
            profile.Width, profile.Height, bias);

        Assert.True(Sum(biased) > Sum(plain),
            "A positive mountain bias should mask in more mountain cells.");
    }

    [Fact]
    public void Bias_TemperatureBias_ShiftsWhittakerBand()
    {
        var plain = CountBiome(0f, BiomeId.Savanna);
        var biased = CountBiome(1f, BiomeId.Savanna);

        Assert.True(biased > plain,
            $"A positive temperature bias should increase the hot biome count ({plain} -> {biased}).");
    }

    [Fact]
    public void Bias_RoughnessBias_RaisesLocalHeights()
    {
        var profile = TestProfiles.CreateSmall128();
        var (_, _, plainMap, _) = TestLayoutRunner.Run(profile, Seed);

        var biasProfile = TestProfiles.CreateSmall128();
        biasProfile.Layout = SingleAnchor(0f, 0.45f, roughnessBias: 1f);
        var (_, _, biasedMap, _) = TestLayoutRunner.Run(biasProfile, Seed);

        var plainMean = MeanHeightNearAnchor(plainMap);
        var biasedMean = MeanHeightNearAnchor(biasedMap);

        Assert.True(biasedMean > plainMean,
            $"A positive roughness bias should raise mean height near the anchor " +
            $"({plainMean} -> {biasedMean}).");
    }

    /// <summary>
    ///     Computes the mean land height within 0.3 normalized units of the footprint center.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <returns>Mean height.</returns>
    private static double MeanHeightNearAnchor(TerrainMap map)
    {
        var sum = 0.0;
        var count = 0L;
        for (var y = 0; y < map.Height; ++y)
        {
            var v = (y + 0.5f) / map.Height - 0.5f;
            for (var x = 0; x < map.Width; ++x)
            {
                if (map.IsOcean[x, y]) continue;
                var u = (x + 0.5f) / map.Width - 0.5f;
                if (u * u + v * v > 0.3f * 0.3f) continue;

                sum += map.Heights[x, y];
                ++count;
            }
        }

        return count == 0 ? 0.0 : sum / count;
    }

    /// <summary>
    ///     Builds a layout with one anchor and the given strength.
    /// </summary>
    /// <param name="strength">Layout strength.</param>
    /// <param name="radius">Anchor radius.</param>
    /// <param name="mountainBias">Mountain bias.</param>
    /// <param name="roughnessBias">Roughness bias.</param>
    /// <returns>Layout options.</returns>
    private static LayoutOptions SingleAnchor(float strength, float radius,
        float mountainBias = 0f, float roughnessBias = 0f)
    {
        return new LayoutOptions
        {
            Strength = strength,
            Anchors = new List<LayoutAnchor>
            {
                new()
                {
                    X = 0.5f, Y = 0.5f, Radius = radius, Weight = 1f,
                    MountainBias = mountainBias, RoughnessBias = roughnessBias
                }
            }
        };
    }

    /// <summary>
    ///     Returns the normalized continentalness at the footprint center for the baseline
    ///     profile at the given layout strength.
    /// </summary>
    /// <param name="strength">Layout strength.</param>
    /// <returns>Center continentalness.</returns>
    private static float ContinentalnessAtCenter(float strength)
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Layout = SingleAnchor(strength, 0.25f);
        var (fields, _, _, _) = TestLayoutRunner.Run(profile, Seed);
        return fields.Continentalness[profile.Width / 2, profile.Height / 2];
    }

    /// <summary>
    ///     Counts cells classified as the given biome with a temperature bias.
    /// </summary>
    /// <param name="temperatureBias">Temperature bias.</param>
    /// <param name="biome">Biome to count.</param>
    /// <returns>Cell count.</returns>
    private static int CountBiome(float temperatureBias, BiomeId biome)
    {
        var profile = TestProfiles.CreateSmall128Biomes();
        profile.Layout = new LayoutOptions
        {
            Strength = 0f,
            Anchors = new List<LayoutAnchor>
            {
                new()
                {
                    X = 0.5f, Y = 0.5f, Radius = 0.45f, Weight = 1f,
                    TemperatureBias = temperatureBias
                }
            }
        };

        var (_, continentalness, map, layout) = TestLayoutRunner.Run(profile, Seed);
        var biomeMap = new BiomeStage().Apply(map, continentalness, profile,
            SeedDerivation.DeriveSubSeed(Seed, "Biomes"), layout);

        var count = 0;
        for (var y = 0; y < map.Height; ++y)
        {
            for (var x = 0; x < map.Width; ++x)
            {
                if (biomeMap.Biome[x, y] == biome) ++count;
            }
        }

        return count;
    }

    /// <summary>
    ///     Sums a field.
    /// </summary>
    /// <param name="field">Field to sum.</param>
    /// <returns>Sum of all values.</returns>
    private static double Sum(float[,] field)
    {
        var sum = 0.0;
        for (var y = 0; y < field.GetLength(1); ++y)
        {
            for (var x = 0; x < field.GetLength(0); ++x)
            {
                sum += field[x, y];
            }
        }

        return sum;
    }

    /// <summary>
    ///     Finds the largest 4-connected land mass and returns its normalized centroid and
    ///     size.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <returns>Normalized centroid and mass size.</returns>
    private static (float X, float Y, int Size) LargestLandCentroid(TerrainMap map)
    {
        var width = map.Width;
        var height = map.Height;
        var labels = new int[width, height];
        var queue = new Queue<(int X, int Y)>();
        var bestSize = 0;
        var bestSumX = 0L;
        var bestSumY = 0L;

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (map.IsOcean[x, y] || labels[x, y] != 0) continue;

                var id = x * height + y + 1;
                labels[x, y] = id;
                queue.Enqueue((x, y));
                var size = 0;
                long sumX = 0;
                long sumY = 0;
                while (queue.Count > 0)
                {
                    var (cx, cy) = queue.Dequeue();
                    ++size;
                    sumX += cx;
                    sumY += cy;
                    Enqueue(map, labels, queue, cx - 1, cy, width, height, id);
                    Enqueue(map, labels, queue, cx + 1, cy, width, height, id);
                    Enqueue(map, labels, queue, cx, cy - 1, width, height, id);
                    Enqueue(map, labels, queue, cx, cy + 1, width, height, id);
                }

                if (size > bestSize)
                {
                    bestSize = size;
                    bestSumX = sumX;
                    bestSumY = sumY;
                }
            }
        }

        if (bestSize == 0) return (float.NaN, float.NaN, 0);
        return ((float)(bestSumX / (double)bestSize / width),
            (float)(bestSumY / (double)bestSize / height), bestSize);
    }

    /// <summary>
    ///     Enqueues a land neighbor for the flood fill.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="labels">Component labels.</param>
    /// <param name="queue">Flood-fill queue.</param>
    /// <param name="x">Neighbor X coordinate.</param>
    /// <param name="y">Neighbor Y coordinate.</param>
    /// <param name="width">Footprint width.</param>
    /// <param name="height">Footprint height.</param>
    /// <param name="id">Component label.</param>
    private static void Enqueue(TerrainMap map, int[,] labels, Queue<(int X, int Y)> queue,
        int x, int y, int width, int height, int id)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (map.IsOcean[x, y] || labels[x, y] != 0) return;

        labels[x, y] = id;
        queue.Enqueue((x, y));
    }
}
