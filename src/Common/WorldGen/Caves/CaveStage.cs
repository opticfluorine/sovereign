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
using System.Threading;
using System.Threading.Tasks;
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Output;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen.Caves;

/// <summary>
///     Result of the cave stage.
/// </summary>
public sealed class CaveStageResult
{
    /// <summary>
    ///     Carved cave map of the plan.
    /// </summary>
    public required CaveMap Map { get; init; }

    /// <summary>
    ///     Cave statistics of the plan.
    /// </summary>
    public required CaveStats Stats { get; init; }
}

/// <summary>
///     Orchestrates per-level cave generation, staircase shafts, and surface mouths. A
///     level that exceeds its repair budget or falls far short of its target porosity is
///     resampled with derived sub-seeds; the best attempt is always accepted, so cave
///     generation never hard-fails the plan.
/// </summary>
public sealed class CaveStage
{
    /// <summary>
    ///     A level is rejected for resampling when its realized open fraction falls below
    ///     this fraction of the configured porosity.
    /// </summary>
    private const double MinAcceptablePorosityFraction = 0.6;

    /// <summary>
    ///     Number of resample attempts per failing level.
    /// </summary>
    private const int ResampleAttempts = 2;

    /// <summary>
    ///     Realized porosity outside this factor of the configured porosity raises a tuning
    ///     warning.
    /// </summary>
    private const double PorosityWarningFactor = 1.5;

    /// <summary>
    ///     Audit radius in cells for the water-proximity check reported in the statistics.
    /// </summary>
    private const int AuditWaterRadius = 3;

    /// <summary>
    ///     Runs the cave stage.
    /// </summary>
    /// <param name="profile">Validated world generation profile.</param>
    /// <param name="terrain">Terrain map with heights and water flags populated.</param>
    /// <param name="biomes">Classified biome map, or null when biomes are not configured.</param>
    /// <param name="seed">Root world seed.</param>
    /// <param name="cancellationToken">Token observed between levels.</param>
    /// <returns>The cave stage result.</returns>
    public CaveStageResult Apply(WorldGenProfile profile, TerrainMap terrain, BiomeMap? biomes,
        ulong seed, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var levels = profile.CaveLevels!;
        var options = profile.Caves ?? new CaveOptions();

        var levelMaps = new List<CaveLevelMap>(levels.Count);
        var levelResults = new List<CaveLevelResult>(levels.Count);
        var warnings = new List<string>();

        for (var i = 0; i < levels.Count; ++i)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = GenerateWithResample(levels[i], terrain, options, seed, i, warnings);
            levelMaps.Add(result.Map);
            levelResults.Add(result);
        }


        var shaftBuilder = new CaveShaftBuilder();
        var shafts = shaftBuilder.Build(levelMaps, terrain, biomes, options,
            SeedDerivation.DeriveSubSeed(seed, "Caves.Shafts"),
            SeedDerivation.DeriveSubSeed(seed, "Caves.Mouths"));

        var map = new CaveMap
        {
            Width = profile.Width,
            Height = profile.Height,
            Levels = levelMaps,
            Shafts = shafts.Shafts,
            MouthList = shafts.Mouths
        };
        map.CountOpenCells();

        return new CaveStageResult
        {
            Map = map,
            Stats = BuildStats(map, levelResults, options, warnings, terrain)
        };
    }

    /// <summary>
    ///     Generates one level, resampling with derived sub-seeds when the attempt exceeds
    ///     its repair budget or falls far short of its target porosity. The best attempt is
    ///     always accepted.
    /// </summary>
    /// <param name="level">Level configuration.</param>
    /// <param name="terrain">Terrain map.</param>
    /// <param name="options">Cave generation options.</param>
    /// <param name="seed">Root world seed.</param>
    /// <param name="levelIndex">Zero-based level index.</param>
    /// <param name="warnings">Warning list to append to.</param>
    /// <returns>The accepted attempt.</returns>
    private static CaveLevelResult GenerateWithResample(CaveLevel level, TerrainMap terrain,
        CaveOptions options, ulong seed, int levelIndex, List<string> warnings)
    {
        var generator = new CaveLevelGenerator();
        var best = GenerateAttempt(generator, level, terrain, options,
            SeedDerivation.DeriveSubSeed(seed, $"Caves.Level{levelIndex}"));

        for (var k = 1; k <= ResampleAttempts; ++k)
        {
            if (Acceptable(best, options.Porosity)) break;

            var attempt = GenerateAttempt(generator, level, terrain, options,
                SeedDerivation.DeriveSubSeed(seed, $"Caves.Level{levelIndex}.resample{k}"));
            if (attempt.RealizedOpenFraction > best.RealizedOpenFraction) best = attempt;
        }

        if (!Acceptable(best, options.Porosity))
        {
            warnings.Add(
                $"level {levelIndex + 1} stayed below its target porosity or exceeded its " +
                $"repair budget after {ResampleAttempts + 1} attempts " +
                $"({100.0 * best.RealizedOpenFraction:F1}% open); accepted the best attempt.");
        }

        return best;
    }

    /// <summary>
    ///     Runs a single generation attempt for a level.
    /// </summary>
    /// <param name="generator">Level generator.</param>
    /// <param name="level">Level configuration.</param>
    /// <param name="terrain">Terrain map.</param>
    /// <param name="options">Cave generation options.</param>
    /// <param name="attemptSeed">Sub-seed of the attempt.</param>
    /// <returns>The attempt result.</returns>
    private static CaveLevelResult GenerateAttempt(CaveLevelGenerator generator,
        CaveLevel level, TerrainMap terrain, CaveOptions options, ulong attemptSeed)
    {
        return generator.Generate(level, terrain, options, attemptSeed);
    }

    /// <summary>
    ///     Determines whether a level attempt satisfies the acceptance rule: within the
    ///     repair budget and at least the minimum fraction of the target porosity.
    /// </summary>
    /// <param name="result">Attempt result.</param>
    /// <param name="porosity">Configured porosity.</param>
    /// <returns>true if the attempt is acceptable, false otherwise.</returns>
    private static bool Acceptable(CaveLevelResult result, double porosity)
    {
        return !result.RepairBudgetExceeded
               && result.RealizedOpenFraction >= MinAcceptablePorosityFraction * porosity;
    }

    /// <summary>
    ///     Builds the cave statistics block, including the porosity tuning warnings and the
    ///     water-proximity audit.
    /// </summary>
    /// <param name="map">Completed cave map.</param>
    /// <param name="levelResults">Accepted per-level results.</param>
    /// <param name="options">Cave generation options.</param>
    /// <param name="warnings">Warning list accumulated during generation.</param>
    /// <param name="terrain">Terrain map.</param>
    /// <returns>Cave statistics.</returns>
    private static CaveStats BuildStats(CaveMap map, IReadOnlyList<CaveLevelResult> levelResults,
        CaveOptions options, List<string> warnings, TerrainMap terrain)
    {
        var shaftsPerLevel = new int[map.Levels.Count];
        foreach (var shaft in map.Shafts)
        {
            ++shaftsPerLevel[shaft.UpperLevel];
            ++shaftsPerLevel[shaft.LowerLevel];
        }

        var levelStats = new List<CaveLevelStats>(map.Levels.Count);
        for (var i = 0; i < map.Levels.Count; ++i)
        {
            var result = levelResults[i];
            var fraction = map.LevelOpenCounts[i] / (double)(map.Width * map.Height);
            if (fraction < options.Porosity / PorosityWarningFactor
                || fraction > options.Porosity * PorosityWarningFactor)
            {
                warnings.Add(
                    $"level {i + 1} realized {100.0 * fraction:F1}% open against a configured " +
                    $"porosity of {100.0 * options.Porosity:F1}%; tune caves.porosity or the " +
                    "water exclusion.");
            }

            levelStats.Add(new CaveLevelStats
            {
                Level = i + 1,
                BaseFloorZ = result.Map.BaseFloorZ,
                OpenCells = map.LevelOpenCounts[i],
                OpenFraction = fraction,
                ConfiguredPorosity = options.Porosity,
                RepairCorridorCells = result.RepairCorridorCells,
                ComponentsBeforeRepair = result.ComponentsBeforeRepair,
                ComponentsAfterRepair = result.ComponentsAfterRepair,
                ShaftCount = shaftsPerLevel[i],
                MouthCount = i == 0 ? map.MouthList.Count : 0
            });
        }

        return new CaveStats
        {
            Levels = levelStats,
            WaterProximityViolations = AuditWaterProximity(map, terrain),
            Warnings = warnings
        };
    }

    /// <summary>
    ///     Audits the completed cave map against the water-proximity rule: no open cell may
    ///     sit within the audit radius of a river or lake without the required ceiling
    ///     clearance, and no open cell may sit in a shallow column under a lake.
    /// </summary>
    /// <param name="map">Completed cave map.</param>
    /// <param name="terrain">Terrain map.</param>
    /// <returns>Number of violations found.</returns>
    private static int AuditWaterProximity(CaveMap map, TerrainMap terrain)
    {
        var nearWater = NearWaterMask(terrain, AuditWaterRadius);
        var violations = 0;
        for (var i = 0; i < map.Levels.Count; ++i)
        {
            var level = map.Levels[i];
            Parallel.For(0, map.Height, y =>
            {
                for (var x = 0; x < map.Width; ++x)
                {
                    if (!level.Open[x, y]) continue;

                    var shallowLake = terrain.IsLake[x, y]
                        && terrain.LakeSurfaceZ[x, y] - level.FloorZ[x, y] < 8;
                    var ceilingTooClose = nearWater[y * map.Width + x]
                        && terrain.Heights[x, y] - (level.FloorZ[x, y] + level.Headroom) < 3;
                    if (shallowLake || ceilingTooClose)
                    {
                        System.Threading.Interlocked.Increment(ref violations);
                    }
                }
            });
        }

        return violations;
    }

    /// <summary>
    ///     Builds a mask of cells within the given Manhattan dilation radius of a river or
    ///     lake cell.
    /// </summary>
    /// <param name="terrain">Terrain map.</param>
    /// <param name="radius">Dilation radius in cells.</param>
    /// <returns>Near-water mask.</returns>
    private static bool[] NearWaterMask(TerrainMap terrain, int radius)
    {
        var width = terrain.Width;
        var height = terrain.Height;
        var mask = new bool[width * height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                mask[y * width + x] = terrain.IsRiver[x, y] || terrain.IsLake[x, y];
            }
        }

        for (var pass = 0; pass < radius; ++pass)
        {
            var next = new bool[width * height];
            Parallel.For(0, height, y =>
            {
                for (var x = 0; x < width; ++x)
                {
                    var index = y * width + x;
                    next[index] = mask[index]
                                  || x > 0 && mask[index - 1]
                                  || x < width - 1 && mask[index + 1]
                                  || y > 0 && mask[index - width]
                                  || y < height - 1 && mask[index + width];
                }
            });
            mask = next;
        }

        return mask;
    }
}
