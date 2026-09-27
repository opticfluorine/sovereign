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
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for <see cref="BiomeStage" />.
/// </summary>
public class TestBiomeStage
{
    /// <summary>
    ///     Synthetic map side used by the tests.
    /// </summary>
    private const int Size = 8;

    private readonly BiomeStage stage = new();

    /// <summary>
    ///     Builds the resolved Whittaker table of the shared test options.
    /// </summary>
    /// <returns>Resolved table indexed [temperature, moisture].</returns>
    private static BiomeId[,] Table()
    {
        return new BiomeId[3, 3]
        {
            { BiomeId.Taiga, BiomeId.Forest, BiomeId.Forest },
            { BiomeId.Savanna, BiomeId.Grassland, BiomeId.Forest },
            { BiomeId.Desert, BiomeId.Savanna, BiomeId.Grassland }
        };
    }

    /// <summary>
    ///     Classifies a synthetic all-land map with the given climate fields.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="temperature">Temperature field.</param>
    /// <param name="moisture">Moisture field.</param>
    /// <returns>Biome map.</returns>
    private BiomeMap Classify(TerrainMap map, float[,] temperature, float[,] moisture)
    {
        return stage.Apply(map, TestTerrainMaps.AllLand(map.Width, map.Height),
            TestTerrainMaps.CreateBiomeOptions(), Table(), temperature, moisture);
    }

    [Fact]
    public void Apply_ColdDryCell_ClassifiesTaiga()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        var biomes = Classify(map, TestTerrainMaps.ConstantField(Size, Size, 0f),
            TestTerrainMaps.ConstantField(Size, Size, 0f));

        Assert.Equal(BiomeId.Taiga, biomes.Biome[3, 3]);
    }

    [Fact]
    public void Apply_HotDryCell_ClassifiesDesert()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        var biomes = Classify(map, TestTerrainMaps.ConstantField(Size, Size, 1f),
            TestTerrainMaps.ConstantField(Size, Size, 0f));

        Assert.Equal(BiomeId.Desert, biomes.Biome[3, 3]);
    }

    [Fact]
    public void Apply_MildTemperateCell_ClassifiesGrassland()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        var biomes = Classify(map, TestTerrainMaps.ConstantField(Size, Size, 0.5f),
            TestTerrainMaps.ConstantField(Size, Size, 0.5f));

        Assert.Equal(BiomeId.Grassland, biomes.Biome[3, 3]);
    }

    [Fact]
    public void Apply_SnowcapHeight_OverridesTableBiome()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.Heights[3, 3] = TestTerrainMaps.SeaLevelZ + 13;
        var biomes = Classify(map, TestTerrainMaps.ConstantField(Size, Size, 0.5f),
            TestTerrainMaps.ConstantField(Size, Size, 0.5f));

        Assert.Equal(BiomeId.Snowcap, biomes.Biome[3, 3]);
    }

    [Fact]
    public void Apply_AlpineHeight_OverridesTableBiome()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.Heights[3, 3] = TestTerrainMaps.SeaLevelZ + 9;
        var biomes = Classify(map, TestTerrainMaps.ConstantField(Size, Size, 0.5f),
            TestTerrainMaps.ConstantField(Size, Size, 0.5f));

        Assert.Equal(BiomeId.Alpine, biomes.Biome[3, 3]);
    }

    [Fact]
    public void Apply_BeachFlag_OverridesLandBiome()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.IsBeach[3, 3] = true;
        var biomes = Classify(map, TestTerrainMaps.ConstantField(Size, Size, 0.5f),
            TestTerrainMaps.ConstantField(Size, Size, 0.5f));

        Assert.Equal(BiomeId.Beach, biomes.Biome[3, 3]);
    }

    [Fact]
    public void Apply_WaterFlags_ClassifyWaterBiomes()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.IsLake[1, 1] = true;
        map.IsRiver[2, 2] = true;
        map.IsOcean[3, 3] = true;
        map.IsOcean[4, 4] = true;

        var continentalness = TestTerrainMaps.AllLand(Size, Size);
        continentalness.Classes[3, 3] = ContinentalClass.DeepOcean;
        continentalness.Classes[4, 4] = ContinentalClass.Shelf;

        var biomes = stage.Apply(map, continentalness, TestTerrainMaps.CreateBiomeOptions(),
            Table(), TestTerrainMaps.ConstantField(Size, Size, 0.5f),
            TestTerrainMaps.ConstantField(Size, Size, 0.5f));

        Assert.Equal(BiomeId.Lake, biomes.Biome[1, 1]);
        Assert.Equal(BiomeId.River, biomes.Biome[2, 2]);
        Assert.Equal(BiomeId.Ocean, biomes.Biome[3, 3]);
        Assert.Equal(BiomeId.Shelf, biomes.Biome[4, 4]);
    }

    [Fact]
    public void Apply_BankCellBelowMaxHeight_ClassifiesSwamp()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.IsBank[3, 3] = true;
        map.Heights[3, 3] = 10;
        var biomes = Classify(map, TestTerrainMaps.ConstantField(Size, Size, 0.5f),
            TestTerrainMaps.ConstantField(Size, Size, 0.5f));

        Assert.Equal(BiomeId.Swamp, biomes.Biome[3, 3]);
    }

    [Fact]
    public void Apply_WaterWithinThreeBlocks_ClassifiesSwamp()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.IsRiver[3, 3 + 3] = true;
        var biomes = Classify(map, TestTerrainMaps.ConstantField(Size, Size, 0.5f),
            TestTerrainMaps.ConstantField(Size, Size, 0.5f));

        Assert.Equal(BiomeId.Swamp, biomes.Biome[3, 3]);
    }

    [Fact]
    public void Apply_WaterFourBlocksAway_DoesNotClassifySwamp()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.IsRiver[3, 3 + 4] = true;
        var biomes = Classify(map, TestTerrainMaps.ConstantField(Size, Size, 0.5f),
            TestTerrainMaps.ConstantField(Size, Size, 0.5f));

        Assert.Equal(BiomeId.Grassland, biomes.Biome[3, 3]);
    }

    [Fact]
    public void Apply_DesertTableBiome_SuppressesSwamp()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.IsBank[3, 3] = true;
        map.Heights[3, 3] = 10;
        var biomes = Classify(map, TestTerrainMaps.ConstantField(Size, Size, 1f),
            TestTerrainMaps.ConstantField(Size, Size, 0f));

        Assert.Equal(BiomeId.Desert, biomes.Biome[3, 3]);
    }

    [Fact]
    public void Apply_CellAboveMaxHeight_DoesNotClassifySwamp()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.IsBank[3, 3] = true;
        map.Heights[3, 3] = TestTerrainMaps.SwampMaxHeightZ + 1;
        var biomes = Classify(map, TestTerrainMaps.ConstantField(Size, Size, 0.5f),
            TestTerrainMaps.ConstantField(Size, Size, 0.5f));

        Assert.Equal(BiomeId.Grassland, biomes.Biome[3, 3]);
    }    [Fact]
    public void Apply_AbsentSwampOverride_NeverClassifiesSwamp()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.IsBank[3, 3] = true;
        var options = TestTerrainMaps.CreateBiomeOptions();
        options.Swamp = null;

        var biomes = stage.Apply(map, TestTerrainMaps.AllLand(Size, Size), options, Table(),
            TestTerrainMaps.ConstantField(Size, Size, 0.5f),
            TestTerrainMaps.ConstantField(Size, Size, 0.5f));

        Assert.Equal(BiomeId.Grassland, biomes.Biome[3, 3]);
    }

    [Fact]
    public void Apply_AllCellsClassified_AndDeterministicAcrossRuns()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        for (var x = 0; x < Size; ++x)
        {
            map.Heights[x, x] = TestTerrainMaps.SeaLevelZ + x;
            if (x % 3 == 0) map.IsRiver[x, (x + 4) % Size] = true;
            if (x % 4 == 0) map.IsOcean[x, 0] = true;
        }

        var temperature = NoisyField(Size);
        var moisture = NoisyField(Size);

        var first = Classify(map, temperature, moisture);
        var second = Classify(map, temperature, moisture);

        for (var y = 0; y < Size; ++y)
        {
            for (var x = 0; x < Size; ++x)
            {
                Assert.True(Enum.IsDefined(first.Biome[x, y]),
                    $"Cell ({x}, {y}) has an unclassified biome.");
                Assert.Equal(first.Biome[x, y], second.Biome[x, y]);
            }
        }
    }

    /// <summary>
    ///     Builds a field with deterministic per-cell variation spanning all three bands.
    /// </summary>
    /// <param name="size">Field size.</param>
    /// <returns>Field in [0, 1].</returns>
    private static float[,] NoisyField(int size)
    {
        var field = new float[size, size];
        for (var y = 0; y < size; ++y)
        {
            for (var x = 0; x < size; ++x)
            {
                field[x, y] = (x + 2 * y) % size / (float)(size - 1);
            }
        }

        return field;
    }
}
