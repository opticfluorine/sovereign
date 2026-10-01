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
using System.Linq;
using Sovereign.WorldGen.Layout;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for <see cref="MassAnalysis" />, the shared definition of land-mass
///     significance.
/// </summary>
public class TestMassAnalysis
{
    /// <summary>
    ///     Footprint side used by the synthetic maps.
    /// </summary>
    private const int Side = 100;

    [Fact]
    public void Analyze_MassInsideAnchorFootprint_IsAnchorBacked()
    {
        // One 30x30 island centered on a weight-1 anchor with radius 0.2: every cell lies
        // well within Radius * 1.25, so the mass is anchor-backed.
        var map = MapWithIsland(35, 64, 35, 64);
        var anchors = new List<ResolvedAnchor> { Anchor(0.5f, 0.5f, 0.2f, 1f) };

        var report = MassAnalysis.Analyze(map, anchors);
        var mass = Assert.Single(report.Masses);

        Assert.True(mass.AnchorBacked);
        Assert.True(mass.Significant);
        Assert.Equal(MassSignificanceKind.AnchorBacked, mass.SignificanceKind);
    }

    [Fact]
    public void Analyze_MassStraddlingFootprint_IsBackedOnlyAboveHalf()
    {
        // A 30x30 island whose anchor only overlaps the right half: about half of the
        // cells lie within the extended footprint, which still qualifies as backed.
        var map = MapWithIsland(20, 49, 35, 64);
        var anchors = new List<ResolvedAnchor> { Anchor(0.5f, 0.5f, 0.05f, 1f) };

        var report = MassAnalysis.Analyze(map, anchors);
        var mass = Assert.Single(report.Masses);

        // 15 of 30 columns inside a 0.05*1.25 = 0.0625 normalized radius of x = 0.5: the
        // inside half is u in [0.455, 0.5) within reach, so a bit under half the island is
        // backed. The exact fraction must not matter below the threshold.
        Assert.False(mass.AnchorBacked);
        Assert.True(mass.Significant,
            "A 900-cell mass must stay significant through the floor.");
    }

    [Fact]
    public void Analyze_MassOverHalfInsideFootprint_IsBacked()
    {
        // A 20x20 island centered on the anchor: every cell lies within
        // Radius * 1.25 = 0.125 of the anchor center, so the mass is anchor-backed even
        // though it sits below the 400-cell floor (which requires strictly larger).
        var map = MapWithIsland(40, 59, 40, 59);
        var anchors = new List<ResolvedAnchor> { Anchor(0.5f, 0.5f, 0.1f, 1f) };

        var report = MassAnalysis.Analyze(map, anchors);
        var mass = Assert.Single(report.Masses);

        Assert.True(mass.AnchorBacked);
        Assert.True(mass.Significant);
        Assert.Equal(MassSignificanceKind.AnchorBacked, mass.SignificanceKind);
    }

    [Fact]
    public void Analyze_IslandSpray_BelowFloorAndUnbacked_IsDecoration()
    {
        // Two tiny islands with no anchors: both sit below the 400-cell floor and carry no
        // backing, so both are decoration.
        var map = MapWithIslands(new[] { (10, 29, 10, 29), (60, 74, 60, 74) }, Side, Side);
        var report = MassAnalysis.Analyze(map, null);

        Assert.Equal(2, report.Masses.Count);
        Assert.All(report.Masses, mass => Assert.False(mass.Significant));
        Assert.Empty(report.SignificantMasses);
    }

    [Fact]
    public void Analyze_LargeNonAnchorMass_IsSignificantViaFloor()
    {
        // One 45x45 = 2025-cell island with no anchors: above the 400-cell floor.
        var map = MapWithIsland(20, 64, 20, 64);
        var report = MassAnalysis.Analyze(map, null);

        var mass = Assert.Single(report.Masses);
        Assert.False(mass.AnchorBacked);
        Assert.True(mass.Significant);
        Assert.Equal(MassSignificanceKind.SizeFloor, mass.SignificanceKind);
    }

    [Fact]
    public void Analyze_FloorScalesWithTotalLand()
    {
        // Total land above 40,000 cells puts the 1% floor above the absolute 400-cell
        // floor: a 442-cell island is decoration while the dominant mass is significant.
        var map = MapWithIslands(new[] { (5, 17, 5, 38), (40, 289, 10, 189) }, 300, 200);
        var report = MassAnalysis.Analyze(map, null);

        var first = report.Masses[0];
        var second = report.Masses[1];

        Assert.Equal(13L * 34, first.Size);
        Assert.False(first.Significant,
            $"A {first.Size}-cell island under the 1% floor is decoration.");
        Assert.True(second.Significant);
    }

    [Fact]
    public void Analyze_PreviouslyDominantMass_StaysSignificant()
    {
        // A merged runaway mass above 3% of land remains significant: the floor keeps
        // everything the retired relative-dominance rule accepted.
        var map = MapWithIslands(new[] { (5, 34, 5, 34), (70, 94, 70, 94) }, Side, Side);
        var report = MassAnalysis.Analyze(map, null);

        var dominant = report.Masses[0];
        Assert.Equal(900L, dominant.Size);
        Assert.True(dominant.Significant);
    }

    [Fact]
    public void Analyze_LowWeightAnchor_DoesNotBack()
    {
        // An anchor below the significant weight never backs a mass.
        var map = MapWithIsland(35, 64, 35, 64);
        var anchors = new List<ResolvedAnchor> { Anchor(0.5f, 0.5f, 0.2f, 0.49f) };

        var report = MassAnalysis.Analyze(map, anchors);
        var mass = Assert.Single(report.Masses);

        Assert.False(mass.AnchorBacked);
        Assert.True(mass.Significant,
            "A 900-cell mass stays significant through the floor.");
    }

    [Fact]
    public void Analyze_TwoMasses_CentroidsMatchFloodFill()
    {
        var map = MapWithIslands(new[] { (10, 29, 10, 29), (60, 84, 60, 84) }, Side, Side);
        var report = MassAnalysis.Analyze(map, null);

        Assert.Equal(2, report.Masses.Count);
        Assert.Equal(400L, report.Masses[0].Size);
        Assert.Equal(625L, report.Masses[1].Size);
        Assert.InRange(report.Masses[0].CentroidX, 19.5 / Side - 0.001f, 19.5 / Side + 0.001f);
        Assert.InRange(report.Masses[1].CentroidX, 72.0 / Side - 0.001f, 72.0 / Side + 0.001f);
    }

    [Fact]
    public void Analyze_AllOcean_ReturnsEmpty()
    {
        var map = new TerrainMap
        {
            Width = Side,
            Height = Side,
            Heights = new int[Side, Side],
            IsOcean = new bool[Side, Side],
            IsCliff = new bool[Side, Side],
            IsBeach = new bool[Side, Side],
            IsRiver = new bool[Side, Side],
            RiverWidth = new int[Side, Side],
            IsBank = new bool[Side, Side],
            IsLake = new bool[Side, Side],
            LakeSurfaceZ = new int[Side, Side]
        };
        for (var y = 0; y < Side; ++y)
        {
            for (var x = 0; x < Side; ++x)
            {
                map.IsOcean[x, y] = true;
            }
        }

        var report = MassAnalysis.Analyze(map, null);
        Assert.Empty(report.Masses);
    }

    /// <summary>
    ///     Builds a map with a single rectangular island of land.
    /// </summary>
    /// <param name="minX">Inclusive X lower bound.</param>
    /// <param name="maxX">Inclusive X upper bound.</param>
    /// <param name="minY">Inclusive Y lower bound.</param>
    /// <param name="maxY">Inclusive Y upper bound.</param>
    /// <returns>Terrain map.</returns>
    private static TerrainMap MapWithIsland(int minX, int maxX, int minY, int maxY)
    {
        return MapWithIslands(new[] { (minX, maxX, minY, maxY) }, Side, Side);
    }

    /// <summary>
    ///     Builds a map with rectangular islands of land on an otherwise ocean footprint.
    /// </summary>
    /// <param name="islands">Inclusive island bounds.</param>
    /// <param name="width">Footprint width.</param>
    /// <param name="height">Footprint height.</param>
    /// <returns>Terrain map.</returns>
    private static TerrainMap MapWithIslands(
        IReadOnlyList<(int MinX, int MaxX, int MinY, int MaxY)> islands, int width, int height)
    {
        var map = new TerrainMap
        {
            Width = width,
            Height = height,
            Heights = new int[width, height],
            IsOcean = new bool[width, height],
            IsCliff = new bool[width, height],
            IsBeach = new bool[width, height],
            IsRiver = new bool[width, height],
            RiverWidth = new int[width, height],
            IsBank = new bool[width, height],
            IsLake = new bool[width, height],
            LakeSurfaceZ = new int[width, height]
        };

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                map.IsOcean[x, y] = islands.All(island =>
                    x < island.MinX || x > island.MaxX || y < island.MinY || y > island.MaxY);
            }
        }

        return map;
    }

    /// <summary>
    ///     Builds one resolved anchor.
    /// </summary>
    /// <param name="x">Normalized X coordinate.</param>
    /// <param name="y">Normalized Y coordinate.</param>
    /// <param name="radius">Anchor radius.</param>
    /// <param name="weight">Anchor weight.</param>
    /// <returns>Resolved anchor.</returns>
    private static ResolvedAnchor Anchor(float x, float y, float radius, float weight)
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
