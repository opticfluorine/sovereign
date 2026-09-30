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
using System.Security.Cryptography;
using Sovereign.WorldGen;
using Sovereign.WorldGen.Caves;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests for the cave stage orchestration: the resample rule, the water-proximity
///     audit, and end-to-end determinism of the cave map.
/// </summary>
public class TestCaveStage
{
    /// <summary>
    ///     Synthetic map size used by the tests.
    /// </summary>
    private const int MapSize = 128;

    [Fact]
    public void Apply_LevelBelowTargetAfterAllAttempts_CompletesWithWarning()
    {
        // Every cell is a shallow lake column, so exclusion keeps the whole level solid
        // no matter how often it is resampled.
        var terrain = LakeWorldTerrain();
        var profile = SingleLevelProfile(porosity: 0.5);

        var result = new CaveStage().Apply(profile, terrain, null, 42UL);

        Assert.Equal(0, result.Map.LevelOpenCounts[0]);
        Assert.Contains(result.Stats.Warnings, w => w.Contains("level 1"));
        Assert.Contains("Cave warning", result.Stats.Format());
        Assert.Contains("level 1", result.Stats.Format());
    }

    [Fact]
    public void Apply_HealthyLevel_CompletesWithoutWarningsAndPassesAudit()
    {
        var terrain = DryTerrain();
        var profile = SingleLevelProfile(porosity: 0.34);

        var result = new CaveStage().Apply(profile, terrain, null, 42UL);

        Assert.Empty(result.Stats.Warnings);
        var levelStats = Assert.Single(result.Stats.Levels);
        Assert.True(levelStats.OpenCells > 0,
            "A healthy level should contain open cells.");
        Assert.Equal(1, levelStats.ComponentsAfterRepair);
        Assert.Equal(0, result.Stats.WaterProximityViolations);
        Assert.Contains("Cave level 1 (floor -16)", result.Stats.Format());
        Assert.Contains("water-proximity audit: 0 violations", result.Stats.Format());
    }

    [Fact]
    public void Apply_IsDeterministic()
    {
        var terrain = DryTerrain();
        var profile = ThreeLevelProfile();

        var first = new CaveStage().Apply(profile, terrain, null, 1234UL);
        var second = new CaveStage().Apply(profile, terrain, null, 1234UL);

        Assert.Equal(HashCaveMap(first.Map), HashCaveMap(second.Map));
        Assert.Equal(first.Stats.WaterProximityViolations, second.Stats.WaterProximityViolations);
    }

    [Fact]
    public void Apply_ReportsPerLevelStatsAndConsistentShafts()
    {
        var terrain = DryTerrain();
        var profile = ThreeLevelProfile();
        profile.Caves = new CaveOptions
        {
            ShaftsPerLevelPair = 2,
            SurfaceMouths = 0,
            Porosity = 0.34
        };

        var result = new CaveStage().Apply(profile, terrain, null, 777UL);

        Assert.Equal(3, result.Stats.Levels.Count);
        for (var i = 0; i < 3; ++i)
        {
            Assert.Equal(i + 1, result.Stats.Levels[i].Level);
            Assert.Equal(-16 - 16 * i, result.Stats.Levels[i].BaseFloorZ);
            Assert.Equal(1, result.Stats.Levels[i].ComponentsAfterRepair);
            Assert.Equal(0, result.Stats.Levels[i].MouthCount);
        }

        // Shaft counts in the stat block agree with the placed shafts, and every shaft
        // joins adjacent levels.
        var attached = 0;
        foreach (var shaft in result.Map.Shafts)
        {
            Assert.Equal(shaft.UpperLevel + 1, shaft.LowerLevel);
            ++attached;
            Assert.Equal(shaft.LowerFloorZ + 1, shaft.Columns.Min(c => c.StepZs.Min()));
            Assert.Equal(shaft.UpperFloorZ, shaft.Columns.Max(c => c.StepZs.Max()));
        }

        Assert.Equal(2 * attached, result.Stats.Levels.Sum(l => l.ShaftCount));
    }

    /// <summary>
    ///     Creates a profile with one cave level and the given porosity.
    /// </summary>
    /// <param name="porosity">Configured porosity.</param>
    /// <returns>Profile.</returns>
    private static WorldGenProfile SingleLevelProfile(double porosity)
    {
        return new WorldGenProfile
        {
            Width = MapSize,
            Height = MapSize,
            SeaLevelZ = 12,
            SurfaceMaxZ = 28,
            RockFloorZ = -63,
            BedrockZ = -64,
            BedrockTemplate = "Bedrock",
            StoneBands = new List<StoneBand>
            {
                new() { FromZ = -63, ToZ = -1, Template = "Shale" }
            },
            CaveLevels = new List<CaveLevel> { new() { FloorZ = -16, Headroom = 2 } },
            Caves = new CaveOptions { Porosity = porosity }
        };
    }

    /// <summary>
    ///     Creates a three-level profile with default tunings.
    /// </summary>
    /// <returns>Profile.</returns>
    private static WorldGenProfile ThreeLevelProfile()
    {
        return new WorldGenProfile
        {
            Width = MapSize,
            Height = MapSize,
            SeaLevelZ = 12,
            SurfaceMaxZ = 28,
            RockFloorZ = -63,
            BedrockZ = -64,
            BedrockTemplate = "Bedrock",
            StoneBands = new List<StoneBand>
            {
                new() { FromZ = -63, ToZ = -1, Template = "Shale" }
            },
            CaveLevels = new List<CaveLevel>
            {
                new() { FloorZ = -16, Headroom = 2 },
                new() { FloorZ = -32, Headroom = 2 },
                new() { FloorZ = -48, Headroom = 2 }
            },
            Caves = new CaveOptions()
        };
    }

    /// <summary>
    ///     Creates dry synthetic terrain: all land at height 28, no water.
    /// </summary>
    /// <returns>Terrain map.</returns>
    private static TerrainMap DryTerrain()
    {
        var terrain = TestTerrainMaps.Create(MapSize, MapSize);
        for (var y = 0; y < MapSize; ++y)
        {
            for (var x = 0; x < MapSize; ++x)
            {
                terrain.Heights[x, y] = 28;
            }
        }

        return terrain;
    }

    /// <summary>
    ///     Creates terrain where every column is a lake whose surface lies only six blocks
    ///     above the cave floor, so the entire level is water-excluded.
    /// </summary>
    /// <returns>Terrain map.</returns>
    private static TerrainMap LakeWorldTerrain()
    {
        var terrain = TestTerrainMaps.Create(MapSize, MapSize);
        for (var y = 0; y < MapSize; ++y)
        {
            for (var x = 0; x < MapSize; ++x)
            {
                terrain.Heights[x, y] = 28;
                terrain.IsLake[x, y] = true;
                terrain.LakeSurfaceZ[x, y] = -12;
            }
        }

        return terrain;
    }

    /// <summary>
    ///     Serializes the cave map open, floor, and carve arrays row-major and hashes them.
    /// </summary>
    /// <param name="map">Cave map to hash.</param>
    /// <returns>Hex hash string.</returns>
    private static string HashCaveMap(CaveMap map)
    {
        using var sha = SHA256.Create();
        foreach (var level in map.Levels)
        {
            var openBytes = new byte[map.Width * map.Height];
            var floorBytes = new byte[map.Width * map.Height * sizeof(int)];
            var carveBytes = new byte[map.Width * map.Height];
            for (var y = 0; y < map.Height; ++y)
            {
                for (var x = 0; x < map.Width; ++x)
                {
                    var index = y * map.Width + x;
                    openBytes[index] = level.Open[x, y] ? (byte)1 : (byte)0;
                    carveBytes[index] = level.CarveHeight[x, y];
                    var value = level.FloorZ[x, y];
                    floorBytes[index * sizeof(int)] = (byte)value;
                    floorBytes[index * sizeof(int) + 1] = (byte)(value >> 8);
                    floorBytes[index * sizeof(int) + 2] = (byte)(value >> 16);
                    floorBytes[index * sizeof(int) + 3] = (byte)(value >> 24);
                }
            }

            sha.TransformBlock(openBytes, 0, openBytes.Length, null, 0);
            sha.TransformBlock(floorBytes, 0, floorBytes.Length, null, 0);
            sha.TransformBlock(carveBytes, 0, carveBytes.Length, null, 0);
        }

        sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return Convert.ToHexString(sha.Hash!);
    }
}
