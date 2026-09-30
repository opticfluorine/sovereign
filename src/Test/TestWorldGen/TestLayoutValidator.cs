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
        var map = TestTerrainMaps.Create(Size, Size);
        for (var y = 0; y < Size; ++y)
        {
            for (var x = 0; x < Size; ++x)
            {
                map.IsOcean[x, y] = x < minX || x > maxX || y < minY || y > maxY;
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
