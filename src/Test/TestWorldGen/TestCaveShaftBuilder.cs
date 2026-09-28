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
using Sovereign.WorldGen;
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Caves;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests for the staircase shaft and surface mouth builder.
/// </summary>
public class TestCaveShaftBuilder
{
    /// <summary>
    ///     Synthetic map size used by the tests.
    /// </summary>
    private const int MapSize = 256;

    [Fact]
    public void BuildShafts_StepsAscendExactlyOneZPerStep()
    {
        var levels = DryLevels();
        var shafts = BuildShafts(levels, shaftsPerPair: 2);

        Assert.NotEmpty(shafts);
        foreach (var shaft in shafts)
        {
            var lowerFloorZ = shaft.LowerFloorZ;
            var gap = shaft.UpperFloorZ - shaft.LowerFloorZ;
            Assert.InRange(gap, 12, 20);

            var allSteps = new List<int>();
            foreach (var column in shaft.Columns)
            {
                Assert.InRange(column.X, shaft.CenterX - 1, shaft.CenterX + 1);
                Assert.InRange(column.Y, shaft.CenterY - 1, shaft.CenterY + 1);
                Assert.NotEqual((shaft.CenterX, shaft.CenterY), (column.X, column.Y));

                // Within one column, steps are one full ring turn apart, far more than the
                // headroom, so the block below each step is solid by construction.
                for (var i = 1; i < column.StepZs.Count; ++i)
                {
                    Assert.Equal(8, column.StepZs[i] - column.StepZs[i - 1]);
                }

                allSteps.AddRange(column.StepZs);
            }

            // The steps cover the gap exactly once each, ascending one Z at a time.
            allSteps.Sort();
            Assert.Equal(gap, allSteps.Count);
            for (var i = 0; i < allSteps.Count; ++i)
            {
                Assert.Equal(lowerFloorZ + i + 1, allSteps[i]);
            }
        }
    }

    [Fact]
    public void BuildShafts_SitesAreSpacedApart()
    {
        var levels = DryLevels();
        var shafts = BuildShafts(levels, shaftsPerPair: 3);

        for (var i = 0; i < shafts.Count; ++i)
        {
            for (var j = i + 1; j < shafts.Count; ++j)
            {
                var distance = System.Math.Max(
                    System.Math.Abs(shafts[i].CenterX - shafts[j].CenterX),
                    System.Math.Abs(shafts[i].CenterY - shafts[j].CenterY));
                Assert.True(distance >= 8,
                    $"Shafts {i} and {j} are only {distance} cells apart.");
            }
        }
    }

    [Fact]
    public void BuildShafts_RingIsRecordedOutsideFootprint()
    {
        var levels = DryLevels();
        var shafts = BuildShafts(levels, shaftsPerPair: 1);

        Assert.NotEmpty(shafts);
        foreach (var shaft in shafts)
        {
            Assert.True(shaft.Ring.Count >= 8,
                "The re-solidified ring around the 3x3 footprint should be recorded.");
            foreach (var (x, y) in shaft.Ring)
            {
                var dx = x - shaft.CenterX;
                var dy = y - shaft.CenterY;
                Assert.Equal(2, System.Math.Max(System.Math.Abs(dx), System.Math.Abs(dy)));
            }

            Assert.Equal(shaft.UpperFloorZ + shaft.Headroom, shaft.RingTopZ);
        }
    }

    [Fact]
    public void BuildShafts_RespectsPerPairLimit()
    {
        var levels = DryLevels();
        var shafts = BuildShafts(levels, shaftsPerPair: 2);

        Assert.True(shafts.Count <= 2,
            "A single adjacent pair must receive at most the configured shaft count.");
    }

    [Fact]
    public void BuildMouths_ValidSiteProducesOpeningWithStairs()
    {
        var levels = DryLevels();
        var terrain = FlatMouthTerrain();
        var biomes = GrasslandBiomes();

        var options = new CaveOptions { SurfaceMouths = 1 };
        var result = new CaveShaftBuilder().Build(levels, terrain, biomes, options,
            SeedDerivation.DeriveSubSeed(42UL, "Caves.Shafts"),
            SeedDerivation.DeriveSubSeed(42UL, "Caves.Mouths"));

        var mouth = Assert.Single(result.Mouths);
        Assert.Equal(-4, mouth.SurfaceZ);
        Assert.InRange(mouth.FloorZ, -18, -14);

        // The 2x2 opening is forced open on the shallowest level.
        foreach (var (x, y) in new[] { (mouth.X, mouth.Y), (mouth.X + 1, mouth.Y),
                     (mouth.X, mouth.Y + 1), (mouth.X + 1, mouth.Y + 1) })
        {
            Assert.True(levels[0].Open[x, y], $"Mouth column ({x},{y}) should be open.");
        }

        // Interior stairs ascend one block per step to the surface.
        var allSteps = new List<int>();
        foreach (var column in mouth.Columns)
        {
            for (var i = 1; i < column.StepZs.Count; ++i)
            {
                Assert.Equal(4, column.StepZs[i] - column.StepZs[i - 1]);
            }

            allSteps.AddRange(column.StepZs);
        }

        allSteps.Sort();
        Assert.Equal(mouth.SurfaceZ - mouth.FloorZ, allSteps.Count);
        for (var i = 0; i < allSteps.Count; ++i)
        {
            Assert.Equal(mouth.FloorZ + i + 1, allSteps[i]);
        }
    }

    [Fact]
    public void BuildMouths_WaterNearby_SiteIsRejected()
    {
        var levels = DryLevels();
        var terrain = FlatMouthTerrain();
        var biomes = GrasslandBiomes();

        // A river row through every jittered scan site: any site is within the exclusion
        // radius of its own river row.
        foreach (var (sx, sy) in ScanSites())
        {
            for (var dx = -16; dx <= 16; ++dx)
            {
                var x = sx + dx;
                if (x < 0 || x >= MapSize) continue;
                terrain.IsRiver[x, sy] = true;
            }
        }

        var options = new CaveOptions { SurfaceMouths = 1 };
        var result = new CaveShaftBuilder().Build(levels, terrain, biomes, options,
            SeedDerivation.DeriveSubSeed(42UL, "Caves.Shafts"),
            SeedDerivation.DeriveSubSeed(42UL, "Caves.Mouths"));

        Assert.Empty(result.Mouths);
    }

    [Fact]
    public void BuildMouths_BeachBiome_SiteIsRejected()
    {
        var levels = DryLevels();
        var terrain = FlatMouthTerrain();
        var biomes = GrasslandBiomes();
        foreach (var (sx, sy) in ScanSites())
        {
            biomes.Biome[sx, sy] = BiomeId.Beach;
            biomes.Biome[sx + 1, sy] = BiomeId.Beach;
            biomes.Biome[sx, sy + 1] = BiomeId.Beach;
            biomes.Biome[sx + 1, sy + 1] = BiomeId.Beach;
        }

        var options = new CaveOptions { SurfaceMouths = 1 };
        var result = new CaveShaftBuilder().Build(levels, terrain, biomes, options,
            SeedDerivation.DeriveSubSeed(42UL, "Caves.Shafts"),
            SeedDerivation.DeriveSubSeed(42UL, "Caves.Mouths"));

        Assert.Empty(result.Mouths);
    }

    /// <summary>
    ///     Creates two dry levels with a 16-block base gap and open cells over the whole
    ///     map, standing in for post-repair level maps.
    /// </summary>
    /// <returns>Level maps.</returns>
    private static List<CaveLevelMap> DryLevels()
    {
        var levels = new List<CaveLevelMap>
        {
            MakeOpenLevel(-16),
            MakeOpenLevel(-32)
        };
        return levels;
    }

    /// <summary>
    ///     Creates a fully open level map with a constant floor.
    /// </summary>
    /// <param name="baseFloorZ">Base floor Z.</param>
    /// <returns>Level map.</returns>
    private static CaveLevelMap MakeOpenLevel(int baseFloorZ)
    {
        var open = new bool[MapSize, MapSize];
        var floorZ = new int[MapSize, MapSize];
        var carveHeight = new byte[MapSize, MapSize];
        var worley = new byte[MapSize, MapSize];
        var waterExcluded = new bool[MapSize, MapSize];
        for (var y = 1; y < MapSize - 1; ++y)
        {
            for (var x = 1; x < MapSize - 1; ++x)
            {
                open[x, y] = true;
                floorZ[x, y] = baseFloorZ;
                carveHeight[x, y] = 2;
            }
        }

        return new CaveLevelMap
        {
            Width = MapSize,
            Height = MapSize,
            BaseFloorZ = baseFloorZ,
            Headroom = 2,
            Open = open,
            FloorZ = floorZ,
            CarveHeight = carveHeight,
            Worley = worley,
            WaterExcluded = waterExcluded
        };
    }

    /// <summary>
    ///     Creates flat terrain at height -4: low enough for a mouth within the 12-block
    ///     depth limit above a floor of -16, with slope zero and no water.
    /// </summary>
    /// <returns>Terrain map.</returns>
    private static TerrainMap FlatMouthTerrain()
    {
        var terrain = TestTerrainMaps.Create(MapSize, MapSize);
        for (var y = 0; y < MapSize; ++y)
        {
            for (var x = 0; x < MapSize; ++x)
            {
                terrain.Heights[x, y] = -4;
            }
        }

        return terrain;
    }

    /// <summary>
    ///     Creates a biome map that classifies every cell as grassland.
    /// </summary>
    /// <returns>Biome map.</returns>
    private static BiomeMap GrasslandBiomes()
    {
        var biomes = new BiomeId[MapSize, MapSize];
        for (var y = 0; y < MapSize; ++y)
        {
            for (var x = 0; x < MapSize; ++x)
            {
                biomes[x, y] = BiomeId.Grassland;
            }
        }

        return new BiomeMap { Width = MapSize, Height = MapSize, Biome = biomes };
    }

    /// <summary>
    ///     Replays the jittered mouth site scan to expose the site coordinates to tests.
    /// </summary>
    /// <returns>Site coordinates in scan order.</returns>
    private static List<(int X, int Y)> ScanSites()
    {
        var state = SeedDerivation.DeriveSubSeed(42UL, "Caves.Mouths");
        var sites = new List<(int X, int Y)>();
        for (var gy = 0; gy * 64 < MapSize; ++gy)
        {
            for (var gx = 0; gx * 64 < MapSize; ++gx)
            {
                var jx = (int)(SeedDerivation.Next(ref state) % 33) - 16;
                var jy = (int)(SeedDerivation.Next(ref state) % 33) - 16;
                sites.Add((
                    System.Math.Clamp(gx * 64 + jx, 1, MapSize - 2),
                    System.Math.Clamp(gy * 64 + jy, 1, MapSize - 2)));
            }
        }

        return sites;
    }

    /// <summary>
    ///     Builds shafts between the two dry levels.
    /// </summary>
    /// <param name="levels">Level maps.</param>
    /// <param name="shaftsPerPair">Configured shafts per level pair.</param>
    /// <returns>Placed shafts.</returns>
    private static IReadOnlyList<CaveShaft> BuildShafts(List<CaveLevelMap> levels,
        int shaftsPerPair)
    {
        var options = new CaveOptions { ShaftsPerLevelPair = shaftsPerPair };
        var result = new CaveShaftBuilder().Build(levels, TestTerrainMaps.Create(MapSize, MapSize),
            null, options, SeedDerivation.DeriveSubSeed(42UL, "Caves.Shafts"),
            SeedDerivation.DeriveSubSeed(42UL, "Caves.Mouths"));
        return result.Shafts;
    }
}
