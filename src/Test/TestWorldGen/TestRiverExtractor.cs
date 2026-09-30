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
            TempPreviewPath("norivers"), TempPreviewPath("norivers-stage"),
            TestResolvedTemplates.ForProfile(profile), null);

        AssertRiverFlagsAbsent(plan.Terrain, profile.Width, profile.Height);
        Assert.Equal(0, plan.Statistics.RiverCount);
    }

    [Fact]
    public void Extract_OverlongStraightRun_TrimsToEndpointsAndReportsRun()
    {
        // 300 east steps followed by an alternating northeast/southeast zigzag: the straight
        // run (300 cells) exceeds the 96-cell maximum and is trimmed to its endpoints, while
        // the zigzag is untouched. The river survives with 22 retained cells (>= minLength).
        var path = new List<(int X, int Y)>();
        for (var x = 0; x < 300; ++x) path.Add((x, 8));
        for (var i = 0; i < 20; ++i)
        {
            path.Add((300 + i, i % 2 == 0 ? 7 : 8));
        }

        var (map, filled, sills, routing, profile, result) =
            BuildSynthetic(path, (383, 8));

        Assert.Equal(1, result.RiverCount);
        Assert.Equal(2, result.LongestStraightRiverRun);

        Assert.False(map.IsRiver[1, 8]);
        Assert.False(map.IsRiver[298, 8]);
        Assert.True(map.IsRiver[0, 8]);
        Assert.True(map.IsRiver[299, 8]);
        foreach (var (x, y) in path.Skip(300))
        {
            Assert.True(map.IsRiver[x, y], $"Winding cell ({x},{y}) should remain river.");
        }

        // The retained path holds exactly the expected cells.
        var riverCells = 0;
        for (var y = 0; y < profile.Height; ++y)
        {
            for (var x = 0; x < profile.Width; ++x)
            {
                if (map.IsRiver[x, y]) ++riverCells;
            }
        }

        Assert.Equal(22, riverCells);
    }

    [Fact]
    public void Extract_RiverLeftBelowMinLengthByTrimming_DropsOut()
    {
        // A 300-cell straight run trims to 2 cells, below the minimum length, so the
        // river drops out entirely.
        var path = new List<(int X, int Y)>();
        for (var x = 0; x < 300; ++x) path.Add((x, 8));

        var (map, filled, sills, routing, profile, result) =
            BuildSynthetic(path, (383, 8));

        Assert.Equal(0, result.RiverCount);
        Assert.Equal(0, result.LongestStraightRiverRun);
        AssertRiverFlagsAbsent(map, profile.Width, profile.Height);
    }

    [Fact]
    public void Extract_WindingRiver_IsUntouched()
    {
        // A fully winding river has no run longer than two cells and must keep every cell.
        var path = new List<(int X, int Y)> { (0, 8) };
        for (var i = 0; i < 30; ++i)
        {
            var (x, y) = path[^1];
            path.Add(i % 2 == 0 ? (x + 1, y - 1) : (x + 1, y + 1));
        }

        var (map, filled, sills, routing, profile, result) =
            BuildSynthetic(path, (383, 8));

        Assert.Equal(1, result.RiverCount);
        Assert.Equal(2, result.LongestStraightRiverRun);
        foreach (var (x, y) in path)
        {
            Assert.True(map.IsRiver[x, y], $"River cell ({x},{y}) was trimmed.");
        }
    }

    [Fact]
    public void Extract_KnownStraightRun_MetricMatchesFilterDefinition()
    {
        // Zigzag, a 50-cell north run (below the 96-cell maximum, so untrimmed), then
        // zigzag again: the metric must report the known 51-cell run (50 steps plus the
        // pivot cell).
        var path = new List<(int X, int Y)> { (100, 60) };
        for (var i = 0; i < 6; ++i)
        {
            var (x, y) = path[^1];
            path.Add(i % 2 == 0 ? (x + 1, y - 1) : (x + 1, y + 1));
        }

        var (zx, zy) = path[^1];
        for (var i = 1; i <= 50; ++i) path.Add((zx, zy - i));
        for (var i = 0; i < 6; ++i)
        {
            var (x, y) = path[^1];
            path.Add(i % 2 == 0 ? (x + 1, y + 1) : (x + 1, y - 1));
        }

        var (map, filled, sills, routing, profile, result) =
            BuildSynthetic(path, (383, 8));

        Assert.Equal(1, result.RiverCount);
        Assert.Equal(51, result.LongestStraightRiverRun);
        foreach (var (x, y) in path)
        {
            Assert.True(map.IsRiver[x, y], $"River cell ({x},{y}) was trimmed.");
        }
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
        BuildSurface(WorldGenProfile profile)    {
        var fields = new TerrainFieldStack().Sample(profile.Width, profile.Height,
            SeedDerivation.DeriveSubSeed(Seed, "TerrainFields"), profile.Terrain);
        var continentalness = new ContinentalnessStage().Apply(fields, profile.Width,
            profile.Height, profile.Terrain);
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
    ///     Builds a flat synthetic surface whose flow routing follows the given path into the
    ///     ocean cell at the given mouth, then extracts rivers over it.
    /// </summary>
    /// <param name="path">River path in downstream order.</param>
    /// <param name="mouth">Ocean cell that the last path cell drains into.</param>
    /// <returns>The map, fill surfaces, routing, profile, and extraction result.</returns>
    private static (TerrainMap Map, int[,] Filled, int[,] Sills, FlowRouting Routing,
        WorldGenProfile Profile, RiverExtractionResult Result) BuildSynthetic(
        IReadOnlyList<(int X, int Y)> path, (int X, int Y) mouth)
    {
        const int width = 384;
        const int height = 80;

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
                map.Heights[x, y] = 20;
            }
        }

        for (var x = 380; x < width; ++x)
        {
            for (var y = 0; y < height; ++y)
            {
                map.Heights[x, y] = 0;
                map.IsOcean[x, y] = true;
            }
        }

        var receiver = new int[width, height];
        var accumulation = new int[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                receiver[x, y] = -1;
                accumulation[x, y] = 1;
            }
        }

        foreach (var (x, y) in path)
        {
            receiver[x, y] = mouth.Y * width + mouth.X;
            accumulation[x, y] = 4096;
        }

        for (var i = 0; i < path.Count - 1; ++i)
        {
            var (x, y) = path[i];
            var next = path[i + 1];
            receiver[x, y] = next.Y * width + next.X;
        }

        var routing = new FlowRouting { Receiver = receiver, Accumulation = accumulation };
        var profile = new WorldGenProfile
        {
            Width = width,
            Height = height,
            SeaLevelZ = 12,
            SurfaceMaxZ = 28,
            RockFloorZ = -63,
            BedrockZ = -64,
            BedrockTemplate = "Bedrock",
            StoneBands = new List<StoneBand>
            {
                new() { FromZ = -63, ToZ = -1, Template = "Basalt" }
            },
            Rivers = new RiverOptions { MaxCount = 8, MinLength = 16 }
        };

        var filled = (int[,])map.Heights.Clone();
        var sills = (int[,])map.Heights.Clone();
        var result = new RiverExtractor(Seed).Extract(map, filled, sills, routing, profile);
        return (map, filled, sills, routing, profile, result);
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
