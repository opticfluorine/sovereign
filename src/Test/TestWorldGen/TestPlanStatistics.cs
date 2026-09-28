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
using Sovereign.WorldGen.Output;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for <see cref="PlanStatistics" /> formatting.
/// </summary>
public class TestPlanStatistics
{
    /// <summary>
    ///     Creates statistics with the given optional biome data.
    /// </summary>
    /// <param name="biomePercentages">Biome fractions, or null for an unconfigured plan.</param>
    /// <param name="decorationCounts">Decoration counts, or null for an unconfigured plan.</param>
    /// <param name="decorationsTotal">Total decoration count.</param>
    /// <param name="caves">Cave statistics, or null for an unconfigured plan.</param>
    /// <returns>Plan statistics.</returns>
    private static PlanStatistics Statistics(
        IReadOnlyDictionary<BiomeId, double>? biomePercentages = null,
        IReadOnlyDictionary<string, int>? decorationCounts = null, int decorationsTotal = 0,
        CaveStats? caves = null)
    {
        return new PlanStatistics
        {
            Width = 128,
            Height = 128,
            LandCells = 8192,
            WaterCells = 8192,
            RiverCount = 4,
            LakeCount = 2,
            LongestStraightRiverRun = 37,
            TerrainMs = 1000,
            HydrologyMs = 500,
            BiomesMs = 250,
            CavesMs = caves is null ? 0 : 750,
            PreviewMs = 100,
            TotalMs = caves is null ? 1850 : 2600,
            BiomePercentages = biomePercentages,
            DecorationCounts = decorationCounts,
            DecorationsTotal = decorationsTotal,
            Caves = caves
        };
    }

    /// <summary>
    ///     Creates cave statistics for one level.
    /// </summary>
    /// <returns>Cave statistics.</returns>
    private static CaveStats CaveStatistics()
    {
        return new CaveStats
        {
            Levels = new List<CaveLevelStats>
            {
                new()
                {
                    Level = 1,
                    BaseFloorZ = -16,
                    OpenCells = 1399,
                    OpenFraction = 0.3416,
                    ConfiguredPorosity = 0.34,
                    RepairCorridorCells = 42,
                    ComponentsBeforeRepair = 5,
                    ComponentsAfterRepair = 1,
                    ShaftCount = 3,
                    MouthCount = 2
                }
            },
            WaterProximityViolations = 0,
            Warnings = new List<string> { "level 1 tuned hot." }
        };
    }

    [Fact]
    public void Format_WithoutBiomes_ReportsNotConfigured()
    {
        var text = Statistics().Format();

        Assert.Contains("Biomes: not configured", text);
        Assert.Contains("Decorations: not configured", text);
        Assert.Contains("LongestStraightRiverRun: 37", text);
        Assert.DoesNotContain("Biome ", text);
    }

    [Fact]
    public void Format_WithBiomes_ListsBiomeLinesAndDecorationTotals()
    {
        var text = Statistics(
            new SortedDictionary<BiomeId, double>
            {
                [BiomeId.Grassland] = 0.5,
                [BiomeId.Ocean] = 0.5
            },
            new SortedDictionary<string, int>
            {
                ["OakTree"] = 120,
                ["PineTree"] = 80
            },
            200).Format();

        Assert.Contains("Biome Grassland: 0.5000", text);
        Assert.Contains("Biome Ocean: 0.5000", text);
        Assert.Contains("Decorations: 200 total: OakTree 120, PineTree 80", text);
        Assert.DoesNotContain("not configured", text);
    }

    [Fact]
    public void Format_WithBiomesButNoDecorations_ReportsZeroTotal()
    {
        var text = Statistics(
            new SortedDictionary<BiomeId, double> { [BiomeId.Grassland] = 1.0 },
            new SortedDictionary<string, int>()).Format();

        Assert.Contains("Decorations: 0 total", text);
    }

    [Fact]
    public void Format_WithCaves_PrintsOneLinePerLevelPlusAuditAndWarnings()
    {
        var text = Statistics(caves: CaveStatistics()).Format();

        Assert.Contains("Cave level 1 (floor -16): 34.2% open (1399 cells), repair 42, " +
                        "components 5->1, shafts 3, mouths 2", text);
        Assert.Contains("Cave water-proximity audit: 0 violations", text);
        Assert.Contains("Cave warning: level 1 tuned hot.", text);
        Assert.Contains("Caves: 0.8 s", text);
    }

    [Fact]
    public void Format_WithoutCaves_PrintsNoCaveLines()
    {
        var text = Statistics().Format();

        Assert.DoesNotContain("Cave level", text);
        Assert.DoesNotContain("Cave warning", text);
        Assert.DoesNotContain("water-proximity audit", text);
    }
}
