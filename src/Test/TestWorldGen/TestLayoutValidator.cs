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

using System.Collections.Generic;
using Sovereign.WorldGen.Layout;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for <see cref="LayoutValidator" />.
/// </summary>
public class TestLayoutValidator
{
    private const int Size = 100;

    [Fact]
    public void Validate_MatchingAnchor_ReportsMatch()
    {
        var map = MapWithIsland(15, 35, 40, 60);
        var anchors = new List<ResolvedAnchor> { Anchor(0, 0.25f, 0.5f, 0.1f, 1f) };

        var result = new LayoutValidator().Validate(map, anchors, new List<string>());

        Assert.True(result.Success);
        Assert.Equal(1, result.Report.MassCount);
        Assert.Equal(1, result.MatchedCount);
        Assert.True(result.Report.Anchors[0].Matched);
        Assert.Equal(0.25f, result.Report.Anchors[0].CentroidX, 3);
        Assert.Equal(0.5f, result.Report.Anchors[0].CentroidY, 3);
    }

    [Fact]
    public void Validate_UnmatchedAnchor_ReportsFailure()
    {
        var map = MapWithIsland(15, 35, 40, 60);
        var anchors = new List<ResolvedAnchor>
        {
            Anchor(0, 0.25f, 0.5f, 0.1f, 1f),
            Anchor(1, 0.75f, 0.5f, 0.1f, 1f)
        };

        var result = new LayoutValidator().Validate(map, anchors, new List<string>());

        Assert.False(result.Success);
        Assert.Equal(1, result.Report.MassCount);
        Assert.Equal(1, result.MatchedCount);
        Assert.Equal(2, result.Report.SignificantAnchorCount);
        Assert.True(result.Report.Anchors[0].Matched);
        Assert.False(result.Report.Anchors[1].Matched);
    }

    [Fact]
    public void Validate_InsignificantAnchor_DoesNotBlockSuccess()
    {
        var map = MapWithIsland(15, 35, 40, 60);
        var anchors = new List<ResolvedAnchor>
        {
            Anchor(0, 0.25f, 0.5f, 0.1f, 1f),
            Anchor(1, 0.75f, 0.5f, 0.1f, 0.3f)
        };

        var result = new LayoutValidator().Validate(map, anchors, new List<string>());

        Assert.True(result.Success);
        Assert.Equal(1, result.Report.SignificantAnchorCount);
        Assert.Equal(1, result.MatchedCount);
    }

    [Fact]
    public void Validate_Report_FormatsWarnings()
    {
        var map = MapWithIsland(15, 35, 40, 60);
        var anchors = new List<ResolvedAnchor> { Anchor(0, 0.75f, 0.5f, 0.1f, 1f) };
        var warnings = new List<string> { "layout best effort." };

        var result = new LayoutValidator().Validate(map, anchors, warnings);

        var formatted = result.Report.Format();
        Assert.Contains("Layout:", formatted);
        Assert.Contains("unmatched", formatted);
        Assert.Contains("layout best effort.", formatted);
    }

    [Fact]
    public void Validate_None_SharedMass_Passes()
    {
        var map = MapWithIsland(15, 35, 40, 60);
        var anchors = new List<ResolvedAnchor>
        {
            Anchor(0, 0.25f, 0.5f, 0.1f, 1f),
            Anchor(1, 0.30f, 0.5f, 0.1f, 1f)
        };

        var result = new LayoutValidator().Validate(map, anchors, new List<string>(),
            LayoutConnectivity.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.MatchedCount);
        Assert.Equal(1, result.DistinctMatchedCount);
        Assert.Empty(result.Report.SharedMassGroups);
        Assert.Equal(LayoutFailureKind.None, result.FailureKind);
    }

    [Fact]
    public void Validate_Separate_SharedMass_Fails()
    {
        var map = MapWithIsland(15, 35, 40, 60);
        var anchors = new List<ResolvedAnchor>
        {
            Anchor(0, 0.25f, 0.5f, 0.1f, 1f),
            Anchor(1, 0.30f, 0.5f, 0.1f, 1f)
        };

        var result = new LayoutValidator().Validate(map, anchors, new List<string>(),
            LayoutConnectivity.Separate);

        Assert.False(result.Success);
        Assert.Equal(0, result.MatchedCount);
        Assert.Equal(0, result.DistinctMatchedCount);
        Assert.Equal(LayoutFailureKind.SharedMass, result.FailureKind);
        var group = Assert.Single(result.Report.SharedMassGroups);
        Assert.Equal(new[] { 0, 1 }, group);
        Assert.True(result.Report.Anchors[0].SharedMass);
        Assert.True(result.Report.Anchors[1].SharedMass);
        Assert.False(result.Report.Anchors[0].Matched);
        Assert.Contains("shared mass", result.Report.Format());
    }

    [Fact]
    public void Validate_Separate_DistinctMasses_Passes()
    {
        var map = MapWithIslands((5, 25, 40, 60), (40, 60, 40, 60), (75, 95, 40, 60));
        var anchors = new List<ResolvedAnchor>
        {
            Anchor(0, 0.15f, 0.5f, 0.1f, 1f),
            Anchor(1, 0.50f, 0.5f, 0.1f, 1f),
            Anchor(2, 0.85f, 0.5f, 0.1f, 1f)
        };

        var result = new LayoutValidator().Validate(map, anchors, new List<string>(),
            LayoutConnectivity.Separate);

        Assert.True(result.Success);
        Assert.Equal(3, result.MatchedCount);
        Assert.Equal(3, result.DistinctMatchedCount);
        Assert.Equal(3, result.Report.MassCount);
        Assert.Equal(LayoutFailureKind.None, result.FailureKind);
    }

    [Fact]
    public void CompareAttempts_DistinctBeatsShared()
    {
        var twoIslands = MapWithIslands((5, 25, 40, 60), (75, 95, 40, 60));
        var twoDistinct = new List<ResolvedAnchor>
        {
            Anchor(0, 0.15f, 0.5f, 0.1f, 1f),
            Anchor(1, 0.85f, 0.5f, 0.1f, 1f),
            Anchor(2, 0.50f, 0.5f, 0.1f, 1f)
        };
        var first = new LayoutValidator().Validate(twoIslands, twoDistinct,
            new List<string>(), LayoutConnectivity.Separate);

        var oneIsland = MapWithIsland(15, 35, 40, 60);
        var shared = new List<ResolvedAnchor>
        {
            Anchor(0, 0.25f, 0.5f, 0.1f, 1f),
            Anchor(1, 0.28f, 0.5f, 0.1f, 1f),
            Anchor(2, 0.30f, 0.5f, 0.1f, 1f)
        };
        var second = new LayoutValidator().Validate(oneIsland, shared,
            new List<string>(), LayoutConnectivity.Separate);

        Assert.Equal(2, first.DistinctMatchedCount);
        Assert.Equal(LayoutFailureKind.SharedMass, second.FailureKind);
        Assert.True(LayoutValidator.CompareAttempts(first, second) > 0,
            "The two-distinct attempt must rank above the shared-mass attempt.");
        Assert.True(LayoutValidator.CompareAttempts(second, first) < 0);
    }

    /// <summary>
    ///     Creates a map with one rectangular island and ocean elsewhere.
    /// </summary>
    /// <param name="minX">Inclusive island minimum X.</param>
    /// <param name="maxX">Inclusive island maximum X.</param>
    /// <param name="minY">Inclusive island minimum Y.</param>
    /// <param name="maxY">Inclusive island maximum Y.</param>
    /// <returns>Terrain map.</returns>
    private static TerrainMap MapWithIsland(int minX, int maxX, int minY, int maxY)
    {
        return MapWithIslands((minX, maxX, minY, maxY));
    }

    /// <summary>
    ///     Creates a map with the given rectangular islands and ocean elsewhere.
    /// </summary>
    /// <param name="islands">Island bounds, each inclusive.</param>
    /// <returns>Terrain map.</returns>
    private static TerrainMap MapWithIslands(
        params (int MinX, int MaxX, int MinY, int MaxY)[] islands)
    {
        var map = TestTerrainMaps.Create(Size, Size);
        for (var y = 0; y < Size; ++y)
        {
            for (var x = 0; x < Size; ++x)
            {
                map.IsOcean[x, y] = true;
            }
        }

        foreach (var (minX, maxX, minY, maxY) in islands)
        {
            for (var y = minY; y <= maxY; ++y)
            {
                for (var x = minX; x <= maxX; ++x)
                {
                    map.IsOcean[x, y] = false;
                }
            }
        }

        return map;
    }

    /// <summary>
    ///     Creates a resolved anchor.
    /// </summary>
    /// <param name="index">Anchor index.</param>
    /// <param name="x">Anchor X.</param>
    /// <param name="y">Anchor Y.</param>
    /// <param name="radius">Anchor radius.</param>
    /// <param name="weight">Anchor weight.</param>
    /// <returns>Resolved anchor.</returns>
    private static ResolvedAnchor Anchor(int index, float x, float y, float radius, float weight)
    {
        return new ResolvedAnchor
        {
            Index = index,
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
