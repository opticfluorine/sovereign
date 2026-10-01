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
using System.IO;
using Sovereign.WorldGen;
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Layout;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests for the per-mass climate thresholds: the mountain-bias shift fields, the
///     shifted alpine/snowcap classification, its clamps, composition with the temperature
///     bias, and the end-to-end differential response of the alpine fraction.
/// </summary>
public class TestClimateBias
{
    /// <summary>
    ///     Synthetic map side used by the classification tests.
    /// </summary>
    private const int Size = 8;

    private readonly BiomeStage stage = new();

    /// <summary>
    ///     Builds a constant quarter-resolution shift field.
    /// </summary>
    /// <param name="width">Quarter-grid width.</param>
    /// <param name="height">Quarter-grid height.</param>
    /// <param name="value">Constant field value.</param>
    /// <returns>Shift field.</returns>
    private static float[,] ConstantQuarter(int width, int height, float value)
    {
        var field = new float[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                field[x, y] = value;
            }
        }

        return field;
    }

    /// <summary>
    ///     Builds classification thresholds for a synthetic mountain bias.
    /// </summary>
    /// <param name="snowcapShift">Snowcap line shift in Z.</param>
    /// <param name="alpineShift">Alpine line shift in Z.</param>
    /// <returns>Threshold context.</returns>
    private static ClimateThresholds Thresholds(float snowcapShift, float alpineShift)
    {
        return new ClimateThresholds
        {
            SnowcapZ = 24,
            AlpineZ = 20,
            SeaLevelZ = TestTerrainMaps.SeaLevelZ,
            QuarterWidth = (Size + 3) / 4,
            QuarterHeight = (Size + 3) / 4,
            SnowcapShift = ConstantQuarter((Size + 3) / 4, (Size + 3) / 4, snowcapShift),
            AlpineShift = ConstantQuarter((Size + 3) / 4, (Size + 3) / 4, alpineShift)
        };
    }

    /// <summary>
    ///     Classifies a constant plateau map with the given thresholds.
    /// </summary>
    /// <param name="height">Plateau height in Z.</param>
    /// <param name="temperature">Temperature band value.</param>
    /// <param name="moisture">Moisture band value.</param>
    /// <param name="thresholds">Threshold context, or null for the profile scalars.</param>
    /// <returns>Biome at the sampled cell.</returns>
    private BiomeId Classify(int height, float temperature, float moisture,
        ClimateThresholds? thresholds)
    {
        var map = TestTerrainMaps.Create(Size, Size);
        for (var y = 0; y < Size; ++y)
        {
            for (var x = 0; x < Size; ++x)
            {
                map.Heights[x, y] = height;
            }
        }

        var biomes = stage.Apply(map, TestTerrainMaps.AllLand(Size, Size),
            TestTerrainMaps.CreateBiomeOptions(), Table(),
            TestTerrainMaps.ConstantField(Size, Size, temperature),
            TestTerrainMaps.ConstantField(Size, Size, moisture), thresholds);
        return biomes.Biome[3, 3];
    }

    /// <summary>
    ///     Builds the resolved Whittaker table of the shared test options.
    /// </summary>
    /// <returns>Resolved table indexed [temperature, moisture].</returns>
    private static BiomeId[,] Table()
    {
        return new BiomeId[3, 3]
        {
            { BiomeId.Taiga, BiomeId.Forest, BiomeId.Forest },
            { BiomeId.Savanna, BiomeId.Grassland, BiomeId.Forest },
            { BiomeId.Desert, BiomeId.Savanna, BiomeId.Grassland }
        };
    }

    [Fact]
    public void ShiftFields_AreMountainBiasScaledByCoefficient()
    {
        var options = new LayoutOptions
        {
            Strength = 1f,
            Anchors = new List<LayoutAnchor>
            {
                new() { X = 0.5f, Y = 0.5f, Radius = 0.3f, Weight = 1f, MountainBias = 1f }
            }
        };
        var anchors = LayoutMaskStage.ResolveAnchors(options,
            SeedDerivation.DeriveSubSeed(12345, "Layout"));
        var fields = LayoutBiasStage.Build(anchors, 4, 4, 16, 16);

        for (var y = 0; y < 4; ++y)
        {
            for (var x = 0; x < 4; ++x)
            {
                Assert.Equal(fields.Mountain[x, y] * LayoutBiasStage.SnowcapBiasCoefficient,
                    fields.SnowcapShift[x, y], 5);
                Assert.Equal(fields.Mountain[x, y] * LayoutBiasStage.AlpineBiasCoefficient,
                    fields.AlpineShift[x, y], 5);
            }
        }

    }

    [Fact]
    public void ShiftFields_NoMountainBias_AreNullAndQuarterResolution()
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Layout = new LayoutOptions
        {
            Strength = 1f,
            Anchors = new List<LayoutAnchor>
            {
                new() { X = 0.5f, Y = 0.5f, Radius = 0.3f, Weight = 1f }
            }
        };
        var fields = new LayoutMaskStage().Build(profile.Width, profile.Height, profile.Layout,
            SeedDerivation.DeriveSubSeed(12345, "Layout"));

        Assert.Null(fields.QuarterSnowcapShift);
        Assert.Null(fields.QuarterAlpineShift);

        profile.Layout.Anchors![0].MountainBias = 0.5f;
        fields = new LayoutMaskStage().Build(profile.Width, profile.Height, profile.Layout,
            SeedDerivation.DeriveSubSeed(12345, "Layout"));

        var snowcap = Assert.IsType<float[,]>(fields.QuarterSnowcapShift);
        var alpine = Assert.IsType<float[,]>(fields.QuarterAlpineShift);
        Assert.Equal((profile.Width + 3) / 4, snowcap.GetLength(0));
        Assert.Equal((profile.Height + 3) / 4, snowcap.GetLength(1));
        Assert.Equal((profile.Width + 3) / 4, alpine.GetLength(0));
        Assert.Equal((profile.Height + 3) / 4, alpine.GetLength(1));
        Assert.Equal(snowcap[16, 16] * LayoutBiasStage.AlpineBiasCoefficient,
            alpine[16, 16] * LayoutBiasStage.SnowcapBiasCoefficient, 4);
        Assert.True(snowcap[16, 16] > 0f);
    }

    [Fact]
    public void ShiftFields_DoubleBuild_IsDeterministic()
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Layout = new LayoutOptions
        {
            Strength = 1f,
            Anchors = new List<LayoutAnchor>
            {
                new()
                {
                    X = 0.4f, Y = 0.6f, Radius = 0.25f, Weight = 1f, MountainBias = -1f,
                    Jitter = 0.02f
                }
            }
        };

        var first = new LayoutMaskStage().Build(profile.Width, profile.Height, profile.Layout,
            SeedDerivation.DeriveSubSeed(12345, "Layout"));
        var second = new LayoutMaskStage().Build(profile.Width, profile.Height, profile.Layout,
            SeedDerivation.DeriveSubSeed(12345, "Layout"));

        var firstSnowcap = Assert.IsType<float[,]>(first.QuarterSnowcapShift);
        var secondSnowcap = Assert.IsType<float[,]>(second.QuarterSnowcapShift);
        for (var y = 0; y < firstSnowcap.GetLength(1); ++y)
        {
            for (var x = 0; x < firstSnowcap.GetLength(0); ++x)
            {
                Assert.Equal(firstSnowcap[x, y], secondSnowcap[x, y]);
            }
        }
    }

    [Fact]
    public void Classification_PositiveBias_LowersLineToSnowcap()
    {
        // Plateau at Z 22: above the lowered snowcap line (24 - 4 = 20).
        Assert.Equal(BiomeId.Snowcap, Classify(22, 0.5f, 0.5f, Thresholds(4f, 3f)));
    }

    [Fact]
    public void Classification_NegativeBias_RaisesLineAbovePlateau()
    {
        // Same plateau: the raised alpine line (20 + 3 = 23) leaves it below the override.
        var biome = Classify(22, 0.5f, 0.5f, Thresholds(-4f, -3f));
        Assert.Equal(BiomeId.Grassland, biome);
    }

    [Fact]
    public void Classification_ZeroBias_MatchesProfileScalars()
    {
        // Control: the plateau sits between alpine (20) and snowcap (24).
        Assert.Equal(BiomeId.Alpine, Classify(22, 0.5f, 0.5f, null));
    }

    [Fact]
    public void Clamps_Thresholds_StayAboveFloorsAndBelowRaisedProfile()
    {
        var thresholds = new ClimateThresholds
        {
            SnowcapZ = 14,
            AlpineZ = 13,
            SeaLevelZ = TestTerrainMaps.SeaLevelZ,
            QuarterWidth = (Size + 3) / 4,
            QuarterHeight = (Size + 3) / 4,
            SnowcapShift = ConstantQuarter((Size + 3) / 4, (Size + 3) / 4, 4f),
            AlpineShift = ConstantQuarter((Size + 3) / 4, (Size + 3) / 4, 3f)
        };

        Assert.Equal(TestTerrainMaps.SeaLevelZ + ClimateThresholds.SnowcapFloorAboveSea,
            thresholds.EffectiveSnowcapZ(3, 3));
        Assert.Equal(TestTerrainMaps.SeaLevelZ + ClimateThresholds.AlpineFloorAboveSea,
            thresholds.EffectiveAlpineZ(3, 3));

        var negative = new ClimateThresholds
        {
            SnowcapZ = 24,
            AlpineZ = 20,
            SeaLevelZ = TestTerrainMaps.SeaLevelZ,
            QuarterWidth = (Size + 3) / 4,
            QuarterHeight = (Size + 3) / 4,
            SnowcapShift = ConstantQuarter((Size + 3) / 4, (Size + 3) / 4, -4f),
            AlpineShift = ConstantQuarter((Size + 3) / 4, (Size + 3) / 4, -3f)
        };

        Assert.Equal(24 + (int)LayoutBiasStage.SnowcapBiasCoefficient,
            negative.EffectiveSnowcapZ(3, 3));
        Assert.Equal(20 + (int)LayoutBiasStage.AlpineBiasCoefficient,
            negative.EffectiveAlpineZ(3, 3));
    }

    [Fact]
    public void Composition_HotMountainousContinent_DesertLowlandsAlpinePeaks()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        for (var y = 0; y < Size; ++y)
        {
            for (var x = 0; x < Size; ++x)
            {
                map.Heights[x, y] = x < Size / 2 ? 12 : 25;
            }
        }

        var biomes = stage.Apply(map, TestTerrainMaps.AllLand(Size, Size),
            TestTerrainMaps.CreateBiomeOptions(), Table(),
            TestTerrainMaps.ConstantField(Size, Size, 1f),
            TestTerrainMaps.ConstantField(Size, Size, 0f), Thresholds(4f, 3f));

        Assert.Equal(BiomeId.Desert, biomes.Biome[1, 1]);
        Assert.Equal(BiomeId.Snowcap, biomes.Biome[6, 6]);
    }

    [Fact]
    public void Classification_DoubleRun_IsDeterministic()
    {
        var first = Classify(22, 0.5f, 0.5f, Thresholds(1.2f, 0.8f));
        var second = Classify(22, 0.5f, 0.5f, Thresholds(1.2f, 0.8f));
        Assert.Equal(first, second);
    }

    /// <summary>
    ///     Builds a 256x256 three-continent biome profile with the given per-anchor mountain
    ///     biases.
    /// </summary>
    /// <param name="westBias">West anchor mountain bias.</param>
    /// <param name="middleBias">Middle anchor mountain bias.</param>
    /// <param name="eastBias">East anchor mountain bias.</param>
    /// <returns>Profile.</returns>
    private static WorldGenProfile DifferentialProfile(float westBias, float middleBias,
        float eastBias)
    {
        var profile = TestProfiles.CreateSmall128Biomes();
        profile.Width = 256;
        profile.Height = 256;
        profile.Layout = new LayoutOptions
        {
            Strength = 3f,
            Anchors = new List<LayoutAnchor>
            {
                new()
                {
                    X = 0.18f, Y = 0.50f, Radius = 0.12f, Weight = 1f,
                    MountainBias = westBias
                },
                new()
                {
                    X = 0.50f, Y = 0.50f, Radius = 0.08f, Weight = 1f,
                    MountainBias = middleBias
                },
                new()
                {
                    X = 0.82f, Y = 0.50f, Radius = 0.12f, Weight = 1f,
                    MountainBias = eastBias
                }
            }
        };
        return profile;
    }

    /// <summary>
    ///     Runs a profile and returns the alpine+snowcap fraction of its three largest
    ///     masses ordered west to east.
    /// </summary>
    /// <param name="profile">Profile to run.</param>
    /// <param name="seed">World seed.</param>
    /// <returns>Fractions and the significant masses ordered by centroid X.</returns>
    private static (double West, double Middle, double East,
        IReadOnlyList<MassAnalysisResult> Masses) AlpineFractions(WorldGenProfile profile,
        ulong seed)
    {
        var staging = Path.Combine(Path.GetTempPath(), "worldgen-test-staging",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        var preview = Path.Combine(Path.GetTempPath(), $"worldgen-climate-{Guid.NewGuid():N}.png");
        try
        {
            var plan = new WorldGenPipeline().Plan(profile, "climate", seed, 0, 0,
                preview, staging, TestResolvedTemplates.ForProfile(profile), null);
            var report = MassAnalysis.Analyze(plan.Terrain, null);
            var masses = new List<MassAnalysisResult>(report.SignificantMasses);
            masses.Sort((a, b) => a.CentroidX.CompareTo(b.CentroidX));
            Assert.True(masses.Count >= 3,
                $"Expected three significant masses, got {masses.Count}: "
                + string.Join(", ", masses.ConvertAll(m =>
                    $"({m.CentroidX:F2},{m.CentroidY:F2}) n={m.Size}")));
            return (Fraction(plan, report, masses[0]), Fraction(plan, report, masses[1]),
                Fraction(plan, report, masses[^1]), masses);
        }
        finally
        {
            if (File.Exists(preview)) File.Delete(preview);
            Directory.Delete(staging, true);
        }
    }

    [Fact]
    public void Plan_BiasedProfile_IsDeterministic()
    {
        var first = RunBiased();
        var second = RunBiased();

        try
        {
            for (var y = 0; y < first.Plan.Terrain.Height; ++y)
            {
                for (var x = 0; x < first.Plan.Terrain.Width; ++x)
                {
                    Assert.Equal(first.Plan.Terrain.Heights[x, y],
                        second.Plan.Terrain.Heights[x, y]);
                    Assert.Equal(first.Plan.Biomes!.Biome[x, y],
                        second.Plan.Biomes!.Biome[x, y]);
                }
            }
        }
        finally
        {
            Cleanup(first);
            Cleanup(second);
        }
    }

    /// <summary>
    ///     Runs the biased differential profile on a fresh staging directory.
    /// </summary>
    /// <returns>Plan and its temporary paths.</returns>
    private static (WorldGenPlan Plan, string Staging, string Preview) RunBiased()
    {
        var profile = DifferentialProfile(-1f, 1f, -1f);
        var staging = Path.Combine(Path.GetTempPath(), "worldgen-test-staging",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        var preview = Path.Combine(Path.GetTempPath(), $"worldgen-climate-{Guid.NewGuid():N}.png");
        var plan = new WorldGenPipeline().Plan(profile, "climate", 12345, 0, 0, preview, staging,
            TestResolvedTemplates.ForProfile(profile), null);
        return (plan, staging, preview);
    }

    /// <summary>
    ///     Removes the temporary paths of a run.
    /// </summary>
    /// <param name="run">Run to clean up.</param>
    private static void Cleanup((WorldGenPlan Plan, string Staging, string Preview) run)
    {
        if (File.Exists(run.Preview)) File.Delete(run.Preview);
        Directory.Delete(run.Staging, true);
    }

    /// <summary>
    ///     Computes the alpine+snowcap fraction of one mass.
    /// </summary>
    /// <param name="plan">Completed plan.</param>
    /// <param name="report">Mass analysis report.</param>
    /// <param name="mass">Mass to measure.</param>
    /// <returns>Fraction of the mass classified alpine or snowcap.</returns>
    private static double Fraction(WorldGenPlan plan, MassAnalysisReport report,
        MassAnalysisResult mass)
    {
        var alpine = 0L;
        var total = 0L;
        var biomes = plan.Biomes!;
        for (var y = 0; y < plan.Terrain.Height; ++y)
        {
            for (var x = 0; x < plan.Terrain.Width; ++x)
            {
                if (report.Labels[x, y] != mass.ComponentId) continue;
                ++total;
                if (biomes.Biome[x, y] is BiomeId.Alpine or BiomeId.Snowcap) ++alpine;
            }
        }

        return total == 0 ? 0.0 : (double)alpine / total;
    }

    [Theory]
    [InlineData(12UL)]
    [InlineData(21UL)]
    public void Differential_MountainBias_ShiftsPerMassAlpineFraction(ulong seed)
    {
        var control = AlpineFractions(DifferentialProfile(0f, 0f, 0f), seed);
        var biased = AlpineFractions(DifferentialProfile(-1f, 1f, -1f), seed);

        // The climate-line change is classification-only: land masses keep their exact
        // positions and sizes.
        Assert.Equal(control.Masses.Count, biased.Masses.Count);
        for (var i = 0; i < control.Masses.Count; ++i)
        {
            Assert.Equal(control.Masses[i].CentroidX, biased.Masses[i].CentroidX);
            Assert.Equal(control.Masses[i].CentroidY, biased.Masses[i].CentroidY);
            Assert.Equal(control.Masses[i].Size, biased.Masses[i].Size);
        }

        Assert.True(biased.Middle > control.Middle + 0.03,
            $"Middle alpine fraction must rise: control {control.Middle:P1}, biased {biased.Middle:P1}.");
        Assert.True(biased.West < control.West - 0.05,
            $"West alpine fraction must fall: control {control.West:P1}, biased {biased.West:P1}.");
        Assert.True(biased.East < control.East - 0.05,
            $"East alpine fraction must fall: control {control.East:P1}, biased {biased.East:P1}.");
        Assert.True(biased.Middle > biased.West && biased.Middle > biased.East,
            $"Middle must exceed the outers: west {biased.West:P1}, middle {biased.Middle:P1}, " +
            $"east {biased.East:P1}.");
    }
}
