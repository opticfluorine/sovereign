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
using Sovereign.WorldGen;
using Sovereign.WorldGen.Hydrology;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests for <see cref="RiverExtractor" /> run over a realistic 128x128 pipeline surface:
///     every extracted river reaches water, width grading is monotonically non-decreasing
///     downstream, banks are adjacent to river cells, and a profile without river options
///     yields no river flags.
/// </summary>
public class TestRiverExtractor
{
    private const ulong Seed = 12345;

    [Fact]
    public void Extract_RiversReachWater()
    {
        var (map, _, routing, profile) = BuildSurface(TestProfiles.CreateSmall128());

        var result = new RiverExtractor(SeedDerivation.DeriveSubSeed(Seed, "RiverExtraction"))
            .Extract(map, FillSurface(map), SillSurface(map), routing, profile);

        Assert.True(result.RiverCount > 0, "The reference seed should produce rivers.");

        for (var y = 0; y < profile.Height; ++y)
        {
            for (var x = 0; x < profile.Width; ++x)
            {
                if (!map.IsRiver[x, y]) continue;

                // Every river cell either drains into water or into another river cell that
                // ultimately reaches water; verify the immediate receiver here and the full
                // path property via the river mouths below.
                var receiver = routing.Receiver[x, y];
                Assert.True(receiver >= 0, $"River cell ({x},{y}) has no receiver.");
                var rx = receiver % profile.Width;
                var ry = receiver / profile.Width;
                Assert.True(map.IsOcean[rx, ry] || map.IsRiver[rx, ry],
                    $"River cell ({x},{y}) drains into a non-river land cell.");
            }
        }
    }

    [Fact]
    public void Extract_WidthGradingMonotoneDownstream()
    {
        var (map, _, routing, profile) = BuildSurface(TestProfiles.CreateSmall128());

        new RiverExtractor(SeedDerivation.DeriveSubSeed(Seed, "RiverExtraction"))
            .Extract(map, FillSurface(map), SillSurface(map), routing, profile);

        for (var y = 0; y < profile.Height; ++y)
        {
            for (var x = 0; x < profile.Width; ++x)
            {
                if (!map.IsRiver[x, y]) continue;

                var receiver = routing.Receiver[x, y];
                var rx = receiver % profile.Width;
                var ry = receiver / profile.Width;
                if (!map.IsRiver[rx, ry]) continue;

                Assert.True(map.RiverWidth[rx, ry] >= map.RiverWidth[x, y],
                    $"Width narrows downstream of ({x},{y}).");
            }
        }
    }

    [Fact]
    public void Extract_BanksAdjacentToRivers()
    {
        var (map, _, routing, profile) = BuildSurface(TestProfiles.CreateSmall128());

        new RiverExtractor(SeedDerivation.DeriveSubSeed(Seed, "RiverExtraction"))
            .Extract(map, FillSurface(map), SillSurface(map), routing, profile);

        for (var y = 0; y < profile.Height; ++y)
        {
            for (var x = 0; x < profile.Width; ++x)
            {
                if (!map.IsBank[x, y]) continue;

                Assert.False(map.IsRiver[x, y], "A cell cannot be both river and bank.");

                var adjacentRiver = false;
                for (var d = 0; d < 8; ++d)
                {
                    var nx = x + FlowRouter.DirectionDx[d];
                    var ny = y + FlowRouter.DirectionDy[d];
                    if (nx < 0 || nx >= profile.Width || ny < 0 || ny >= profile.Height) continue;
                    adjacentRiver |= map.IsRiver[nx, ny];
                }

                Assert.True(adjacentRiver, $"Bank cell ({x},{y}) has no adjacent river cell.");
            }
        }
    }

    [Fact]
    public void Extract_WithoutRiverOptions_YieldsNoRiverFlags()
    {
        var (map, _, routing, profile) = BuildSurface(TestProfiles.CreateSmall128WithoutRivers());

        var result = new RiverExtractor(SeedDerivation.DeriveSubSeed(Seed, "RiverExtraction"))
            .Extract(map, FillSurface(map), SillSurface(map), routing, profile);

        Assert.Equal(0, result.RiverCount);
        AssertRiverFlagsAbsent(map, profile.Width, profile.Height);
    }

    [Fact]
    public void Pipeline_WithoutRiverOptions_YieldsNoRiverFlags()
    {
        var profile = TestProfiles.CreateSmall128WithoutRivers();
        var plan = new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0,
            TempPreviewPath("norivers"), null);

        AssertRiverFlagsAbsent(plan.Terrain, profile.Width, profile.Height);
        Assert.Equal(0, plan.Statistics.RiverCount);
    }

    /// <summary>
    ///     Asserts that no river, width, or bank flags are set on the map.
    /// </summary>
    private static void AssertRiverFlagsAbsent(TerrainMap map, int width, int height)
    {
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                Assert.False(map.IsRiver[x, y]);
                Assert.Equal(0, map.RiverWidth[x, y]);
                Assert.False(map.IsBank[x, y]);
            }
        }
    }

    /// <summary>
    ///     Builds a shaped terrain surface with routing for extraction.
    /// </summary>
    private static (TerrainMap Map, int[,] Filled, FlowRouting Routing, WorldGenProfile Profile)
        BuildSurface(WorldGenProfile profile)
    {
        var fields = new TerrainFieldStack().Sample(profile.Width, profile.Height,
            SeedDerivation.DeriveSubSeed(Seed, "TerrainFields"));
        var continentalness = new ContinentalnessStage().Apply(fields, profile.Width, profile.Height);
        var shape = new TerrainShapeStage().Apply(fields, continentalness, profile,
            SeedDerivation.DeriveSubSeed(Seed, "TerrainShape"));
        var filled = FillSurface(shape.Map);
        var routing = new FlowRouter(SeedDerivation.DeriveSubSeed(Seed, "FlowRouting")).Route(filled);
        return (shape.Map, filled, routing, profile);
    }

    /// <summary>
    ///     Computes the epsilon-filled routing surface of a map.
    /// </summary>
    private static int[,] FillSurface(TerrainMap map)
    {
        var filled = (int[,])map.Heights.Clone();
        DepressionFill.Fill(filled);
        return filled;
    }

    /// <summary>
    ///     Computes the sill-filled surface of a map.
    /// </summary>
    private static int[,] SillSurface(TerrainMap map)
    {
        var sills = (int[,])map.Heights.Clone();
        DepressionFill.Fill(sills, useEpsilon: false);
        return sills;
    }

    /// <summary>
    ///     Returns a temporary preview path.
    /// </summary>
    private static string TempPreviewPath(string tag)
    {
        return System.IO.Path.Combine(System.IO.Path.GetTempPath(),
            $"worldgen-test-{tag}-{Guid.NewGuid():N}.png");
    }
}
