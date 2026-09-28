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
    /// <returns>Plan statistics.</returns>
    private static PlanStatistics Statistics(
        IReadOnlyDictionary<BiomeId, double>? biomePercentages = null,
        IReadOnlyDictionary<string, int>? decorationCounts = null, int decorationsTotal = 0)
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
            PreviewMs = 100,
            TotalMs = 1850,
            BiomePercentages = biomePercentages,
            DecorationCounts = decorationCounts,
            DecorationsTotal = decorationsTotal
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
}
