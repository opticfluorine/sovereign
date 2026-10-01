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
using Sovereign.WorldGen.Layout;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for the per-mass mountain mask quotas in
///     <see cref="TerrainShapeStage.BuildMountainMask" />: per-mass cuts, bias-driven
///     coverage, small-mass damping, and decoration behavior.
/// </summary>
public class TestMountainMask
{
    /// <summary>
    ///     Footprint side of the synthetic three-mass terrain.
    /// </summary>
    private const int Side = 256;

    [Fact]
    public void PerMassCut_EachMassGetsItsOwnQuota()
    {
        // Three well-separated masses with very different ridge distributions: a global
        // cut would mask almost nothing on the weak mass and far too much on the strong
        // one, so any per-mass equality of the masked fraction is only reachable through
        // the per-mass cut.
        var (fields, classes, islands) = ThreeMassTerrain(RidgeStyle.Distinct);

        var mask = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side, null);

        foreach (var island in islands)
        {
            Assert.InRange(MaskedFraction(mask, island),
                TerrainShapeCoverage.BaseCoverage - 0.03f,
                TerrainShapeCoverage.BaseCoverage + 0.03f);
        }
    }

    [Fact]
    public void PerMassCut_BiasOnOneMass_IsolatesCoverage()
    {
        // +1 mountain bias on the second mass alone raises its coverage by the
        // sensitivity without touching the neighbors.
        var (fields, classes, islands) = ThreeMassTerrain(RidgeStyle.Distinct);
        var bias = BiasField(Side, Side, islands[1], 1f);

        var plain = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side, null);
        var biased = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side, bias);

        var plainFirst = MaskedFraction(plain, islands[0]);
        var biasedFirst = MaskedFraction(biased, islands[0]);
        var plainSecond = MaskedFraction(plain, islands[1]);
        var biasedSecond = MaskedFraction(biased, islands[1]);
        var plainThird = MaskedFraction(plain, islands[2]);
        var biasedThird = MaskedFraction(biased, islands[2]);

        Assert.InRange(biasedSecond - plainSecond,
            TerrainShapeCoverage.CoverageSensitivity - 0.03f,
            TerrainShapeCoverage.CoverageSensitivity + 0.03f);
        Assert.Equal(plainFirst, biasedFirst, 4);
        Assert.Equal(plainThird, biasedThird, 4);
    }

    [Fact]
    public void PerMassCut_NegativeBias_ReducesCoverage()
    {
        var (fields, classes, islands) = ThreeMassTerrain(RidgeStyle.Distinct);
        var bias = BiasField(Side, Side, islands[0], -1f);

        var plain = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side, null);
        var biased = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side, bias);

        Assert.InRange(MaskedFraction(plain, islands[0]) - MaskedFraction(biased, islands[0]),
            0f, 0.13f);
        Assert.True(MaskedFraction(biased, islands[0]) < MaskedFraction(plain, islands[0]),
            "A negative mountain bias must reduce the mass coverage.");
    }

    [Fact]
    public void PerMassCut_WeakRidge_DoesNotFabricateMountains()
    {
        // A uniformly weak ridge with full +1 bias is still capped at its coverage quota,
        // and the masked-in cells stay tiny: the mask value of the crest stays close to
        // the ridge values themselves, so no material mountains rise from nothing.
        var (fields, classes, islands) = ThreeMassTerrain(RidgeStyle.Weak);
        var bias = BiasField(Side, Side, islands[0], 1f);

        var masked = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side, bias);

        var island = islands[0];
        var fraction = MaskedFraction(masked, island);
        var quota = TerrainShapeCoverage.BaseCoverage + TerrainShapeCoverage.CoverageSensitivity;
        Assert.InRange(fraction, quota - 0.03f, quota + 0.03f);

        var maxMasked = 0f;
        for (var y = island.MinY; y <= island.MaxY; ++y)
        {
            for (var x = island.MinX; x <= island.MaxX; ++x)
            {
                maxMasked = MathF.Max(maxMasked, masked[x, y]);
            }
        }

        // The strongest crest of the weak field sits at ridge = 0.2 with a cut floor near
        // the quota's quantile, so the mask value stays far below one.
        Assert.True(maxMasked < 0.1f,
            $"A weak ridge must not fabricate mountains (max mask value {maxMasked}).");
    }

    [Fact]
    public void PerMassCut_SmallMassIslet_IsDampedToFlooredShare()
    {
        // A 300-cell anchor-backed islet: the quota is damped linearly toward the islet's
        // area share with an absolute 5% coverage floor, so it stays well below the full
        // base quota.
        var (fields, classes) = Fields(RidgeStyle.Distinct);
        var islet = (MinX: 10, MaxX: 34, MinY: 10, MaxY: 21);
        MarkIsland(classes, islet);
        var anchors = new List<ResolvedAnchor>
        {
            TestAnchor(22.5f / Side, 15.5f / Side, 0.06f, 1f)
        };

        var mask = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side, null,
            anchors);

        // Damped coverage: 0.125 * 300/2500 = 1.5%, floored to 5% coverage, so the islet
        // masks in about 5% of its cells - far below the full 12.5% quota.
        var fraction = MaskedFraction(mask, islet);
        Assert.InRange(fraction, 0.02f, 0.08f);
        Assert.True(fraction < TerrainShapeCoverage.BaseCoverage,
            "A damped 300-cell islet must not carry a full mountain quota.");
    }

    [Fact]
    public void PerMassCut_LargeMass_IsUndamped()
    {
        // A 3000-cell mass keeps its full quota.
        var (fields, classes) = Fields(RidgeStyle.Distinct);
        var mass = (MinX: 10, MaxX: 69, MinY: 10, MaxY: 59);
        MarkIsland(classes, mass);

        var mask = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side, null);

        Assert.InRange(MaskedFraction(mask, mass),
            TerrainShapeCoverage.BaseCoverage - 0.03f,
            TerrainShapeCoverage.BaseCoverage + 0.03f);
    }

    [Fact]
    public void DecorationIslet_WithoutAnchors_GoesToGlobalCut()
    {
        // Below-floor islets with no anchors all fall to one shared global cut at the base
        // coverage: the combined fraction is still about the base coverage.
        var (fields, classes) = Fields(RidgeStyle.Distinct);
        var first = (MinX: 10, MaxX: 21, MinY: 10, MaxY: 21);
        var second = (MinX: 100, MaxX: 114, MinY: 100, MaxY: 109);
        MarkIsland(classes, first);
        MarkIsland(classes, second);

        var mask = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side, null);

        var combined = MaskedFraction(mask, first) + MaskedFraction(mask, second);
        var combinedWeighted = (MaskedFraction(mask, first) * 144
                                + MaskedFraction(mask, second) * 150) / 294;
        Assert.InRange(combinedWeighted, TerrainShapeCoverage.BaseCoverage - 0.04f,
            TerrainShapeCoverage.BaseCoverage + 0.04f);
        Assert.True(combined >= 0f);
    }

    [Fact]
    public void Determinism_DoubleRun_IsByteIdentical()
    {
        // Per-mass cuts must not depend on scheduling or hash order: two runs over the
        // same inputs produce byte-identical masks.
        var (fields, classes, _) = ThreeMassTerrain(RidgeStyle.Distinct);
        var bias = BiasField(Side, Side, (MinX: 10, MaxX: 69, MinY: 10, MaxY: 69), 0.5f);

        var first = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side, bias);
        var second = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side, bias);

        for (var y = 0; y < Side; ++y)
        {
            for (var x = 0; x < Side; ++x)
            {
                Assert.Equal(first[x, y], second[x, y]);
            }
        }
    }

    [Fact]
    public void PerMassCut_MaskDoesNotMoveCoastlines()
    {
        // The mask only scales mountain amplitude on land: the water classes of the same
        // fields stay identical regardless of the mask settings.
        var (fields, classes, islands) = ThreeMassTerrain(RidgeStyle.Distinct);
        var biasNone = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side, null);
        var biasStrong = TerrainShapeStage.BuildMountainMask(fields, classes, Side, Side,
            BiasField(Side, Side, islands[1], 1f));

        // The land/ocean split comes from the classes, untouched by the mask; assert the
        // mask writes nothing on water cells in either configuration.
        Assert.All(new[] { biasNone, biasStrong }, mask =>
        {
            for (var y = 0; y < Side; ++y)
            {
                for (var x = 0; x < Side; ++x)
                {
                    if (classes.Classes[x, y] != ContinentalClass.Land)
                    {
                        Assert.Equal(0f, mask[x, y]);
                    }
                }
            }
        });
    }

    [Fact]
    public void Shaping_MaskBiasChange_DoesNotMoveCoastlines()
    {
        // The mask only scales mountain amplitude on land: shaping the same fields with a
        // zero-bias and a full-bias mask yields identical water classes and identical
        // significant mass centroids, while the heights differ.
        var (fields, classes, islands) = ThreeMassTerrain(RidgeStyle.Distinct);
        var bias = BiasField(Side, Side, islands[1], 1f);

        var stage = new TerrainShapeStage();
        var profile = TestProfiles.CreateSmall128();
        profile.Width = Side;
        profile.Height = Side;
        var zero = stage.Apply(fields, classes, profile, 7UL).Map;
        var one = stage.Apply(fields, classes, profile, 7UL,
            new LayoutFields
            {
                Width = Side,
                Height = Side,
                QuarterWidth = (Side + 3) / 4,
                QuarterHeight = (Side + 3) / 4,
                Strength = 1f,
                Anchors = Array.Empty<ResolvedAnchor>(),
                MountainBias = bias
            }).Map;

        for (var y = 0; y < Side; ++y)
        {
            for (var x = 0; x < Side; ++x)
            {
                Assert.Equal(zero.IsOcean[x, y], one.IsOcean[x, y]);
            }
        }

        var zeroMasses = SignificantMasses(zero);
        var oneMasses = SignificantMasses(one);
        Assert.Equal(zeroMasses.Count, oneMasses.Count);
        for (var i = 0; i < zeroMasses.Count; ++i)
        {
            Assert.Equal(zeroMasses[i].CentroidX, oneMasses[i].CentroidX, 3);
            Assert.Equal(zeroMasses[i].CentroidY, oneMasses[i].CentroidY, 3);
        }

        // The heights themselves must move: the biased island gains mountains.
        var zeroSum = 0L;
        var oneSum = 0L;
        for (var y = islands[1].MinY; y <= islands[1].MaxY; ++y)
        {
            for (var x = islands[1].MinX; x <= islands[1].MaxX; ++x)
            {
                zeroSum += zero.Heights[x, y];
                oneSum += one.Heights[x, y];
            }
        }

        Assert.True(oneSum > zeroSum,
            "A positive mountain bias must raise the biased island's heights.");
    }

    /// <summary>
    ///     Filters a mass analysis report down to its significant masses.
    /// </summary>
    /// <param name="map">Shaped terrain map.</param>
    /// <returns>Significant masses in scan order.</returns>
    private static List<MassAnalysisResult> SignificantMasses(TerrainMap map)
    {
        var report = MassAnalysis.Analyze(map, null);
        var masses = new List<MassAnalysisResult>();
        foreach (var mass in report.Masses)
        {
            if (mass.Significant) masses.Add(mass);
        }

        return masses;
    }

    /// <summary>
    ///     Coverage constants mirrored from <see cref="TerrainShapeStage" /> for test
    ///     expectations.
    /// </summary>
    private static class TerrainShapeCoverage
    {
        /// <summary>Base mountain coverage of the cut.</summary>
        public const float BaseCoverage = 0.125f;

        /// <summary>Coverage points of a full mountain bias.</summary>
        public const float CoverageSensitivity = 0.10f;
    }

    /// <summary>
    ///     Ridge distribution styles of the synthetic masses.
    /// </summary>
    private enum RidgeStyle
    {
        /// <summary>Deterministic hash-like distribution spanning [0, 1].</summary>
        Distinct,

        /// <summary>Uniformly weak distribution spanning [0, 0.2].</summary>
        Weak
    }

    /// <summary>
    ///     Builds three separated 60x50 islands with per-island ridge distributions.
    /// </summary>
    /// <param name="style">Ridge distribution style.</param>
    /// <returns>Fields, classification, and island bounds.</returns>
    private static (TerrainFields Fields, ContinentalnessResult Classes,
        List<(int MinX, int MaxX, int MinY, int MaxY)> Islands) ThreeMassTerrain(
        RidgeStyle style)
    {
        var islands = new List<(int MinX, int MaxX, int MinY, int MaxY)>
        {
            (10, 69, 10, 59),
            (110, 169, 10, 59),
            (10, 69, 110, 159)
        };
        var (fields, classes) = Fields(style);
        foreach (var island in islands) MarkIsland(classes, island);
        return (fields, classes, islands);
    }

    /// <summary>
    ///     Builds base terrain fields and an all-ocean classification over the side-sized
    ///     footprint.
    /// </summary>
    /// <param name="style">Ridge distribution style.</param>
    /// <returns>Fields and all-ocean classification.</returns>
    private static (TerrainFields Fields, ContinentalnessResult Classes) Fields(
        RidgeStyle style)
    {
        var ridge = new float[Side, Side];
        var continentalness = new float[Side, Side];
        var roughness = new float[Side, Side];
        for (var y = 0; y < Side; ++y)
        {
            for (var x = 0; x < Side; ++x)
            {
                ridge[x, y] = style == RidgeStyle.Weak
                    ? Hash01(x, y) * 0.2f
                    : Hash01(x, y);
                continentalness[x, y] = 0.3f;
                roughness[x, y] = 0.3f;
            }
        }

        var classes = new ContinentalClass[Side, Side];
        var result = new ContinentalnessResult
        {
            Classes = classes,
            ThresholdOcean = 0.4f,
            ThresholdCoast = 0.48f,
            ThresholdInland = 0.62f
        };
        return (new TerrainFields
        {
            Continentalness = continentalness,
            MountainRidge = ridge,
            Roughness = roughness
        }, result);
    }

    /// <summary>
    ///     Marks a rectangle of the classification as land.
    /// </summary>
    /// <param name="classes">Classification to update.</param>
    /// <param name="island">Inclusive island bounds.</param>
    private static void MarkIsland(ContinentalnessResult classes,
        (int MinX, int MaxX, int MinY, int MaxY) island)
    {
        for (var y = island.MinY; y <= island.MaxY; ++y)
        {
            for (var x = island.MinX; x <= island.MaxX; ++x)
            {
                classes.Classes[x, y] = ContinentalClass.Land;
            }
        }
    }

    /// <summary>
    ///     Builds a block-resolution bias field that is constant over one island and zero
    ///     elsewhere.
    /// </summary>
    /// <param name="width">Footprint width.</param>
    /// <param name="height">Footprint height.</param>
    /// <param name="island">Island receiving the bias.</param>
    /// <param name="value">Bias value in [-1, 1].</param>
    /// <returns>Bias field.</returns>
    private static float[,] BiasField(int width, int height,
        (int MinX, int MaxX, int MinY, int MaxY) island, float value)
    {
        var bias = new float[width, height];
        for (var y = island.MinY; y <= island.MaxY; ++y)
        {
            for (var x = island.MinX; x <= island.MaxX; ++x)
            {
                bias[x, y] = value;
            }
        }

        return bias;
    }

    /// <summary>
    ///     Computes the fraction of an island's cells masked in as mountains.
    /// </summary>
    /// <param name="mask">Mountain mask.</param>
    /// <param name="island">Inclusive island bounds.</param>
    /// <returns>Fraction of island cells with a nonzero mask value.</returns>
    private static float MaskedFraction(float[,] mask,
        (int MinX, int MaxX, int MinY, int MaxY) island)
    {
        var masked = 0;
        var total = 0;
        for (var y = island.MinY; y <= island.MaxY; ++y)
        {
            for (var x = island.MinX; x <= island.MaxX; ++x)
            {
                ++total;
                if (mask[x, y] > 0f) ++masked;
            }
        }

        return (float)masked / total;
    }

    /// <summary>
    ///     Deterministic per-cell pseudo-random value in [0, 1).
    /// </summary>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <returns>Pseudo-random value.</returns>
    private static float Hash01(int x, int y)
    {
        var hash = (uint)(x * 73856093) ^ (uint)(y * 19349663);
        hash ^= hash >> 13;
        hash *= 0x5BD1E995;
        hash ^= hash >> 15;
        return hash / (float)uint.MaxValue;
    }

    /// <summary>
    ///     Builds one resolved anchor for backing tests.
    /// </summary>
    /// <param name="x">Normalized X.</param>
    /// <param name="y">Normalized Y.</param>
    /// <param name="radius">Anchor radius.</param>
    /// <param name="weight">Anchor weight.</param>
    /// <returns>Resolved anchor.</returns>
    private static ResolvedAnchor TestAnchor(float x, float y, float radius, float weight)
    {
        return new ResolvedAnchor
        {
            Index = 0,
            X = x,
            Y = y,
            Radius = radius,
            Weight = weight,
            MountainBias = 0f,
            TemperatureBias = 0f,
            MoistureBias = 0f,
            RoughnessBias = 0f
        };
    }
}
