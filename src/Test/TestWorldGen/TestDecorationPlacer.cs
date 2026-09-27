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
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Decorations;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for <see cref="DecorationPlacer" />.
/// </summary>
public class TestDecorationPlacer
{
    /// <summary>
    ///     Synthetic map side used by the tests.
    /// </summary>
    private const int Size = 32;

    /// <summary>
    ///     Surface height of the flat synthetic maps.
    /// </summary>
    private const int Height = 20;

    /// <summary>
    ///     Decoration sub-seed used by the tests.
    /// </summary>
    private const ulong Seed = 42;

    /// <summary>
    ///     Creates a single-entry decoration pool with the given parameters.
    /// </summary>
    /// <param name="template">Template name.</param>
    /// <param name="weight">Weight.</param>
    /// <param name="minSpacing">Minimum spacing.</param>
    /// <param name="maxSlope">Maximum slope.</param>
    /// <returns>Decoration options list.</returns>
    private static List<DecorationOptions> Pool(string template = "OakTree", double weight = 1.0,
        int minSpacing = 5, int maxSlope = 1)
    {
        return new List<DecorationOptions>
        {
            new() { Template = template, Weight = weight, MinSpacing = minSpacing, MaxSlope = maxSlope }
        };
    }

    /// <summary>
    ///     Creates biome options whose named biome carries the given pool.
    /// </summary>
    /// <param name="pool">Decoration pool.</param>
    /// <param name="biome">Biome name to attach the pool to.</param>
    /// <returns>Biome options.</returns>
    private static BiomeOptions Options(List<DecorationOptions> pool, string biome = "Grassland")
    {
        var options = TestTerrainMaps.CreateBiomeOptions();
        options.Definitions[biome] = TestTerrainMaps.Definition("Grass", "Dirt", 4);
        options.Definitions[biome].Decorations = pool;
        return options;
    }

    /// <summary>
    ///     Creates a flat all-Grassland synthetic world.
    /// </summary>
    /// <param name="map">Terrain map to update in place.</param>
    /// <returns>Built biome map.</returns>
    private static BiomeMap FlatWorld(TerrainMap map)
    {
        for (var y = 0; y < Size; ++y)
        {
            for (var x = 0; x < Size; ++x)
            {
                map.Heights[x, y] = Height;
            }
        }

        return UniformBiomes(BiomeId.Grassland);
    }

    /// <summary>
    ///     Creates a biome map that assigns the given biome to every cell.
    /// </summary>
    /// <param name="biome">Biome to assign.</param>
    /// <returns>Biome map.</returns>
    private static BiomeMap UniformBiomes(BiomeId biome)
    {
        var biomes = new BiomeId[Size, Size];
        for (var y = 0; y < Size; ++y)
        {
            for (var x = 0; x < Size; ++x)
            {
                biomes[x, y] = biome;
            }
        }

        return new BiomeMap { Width = Size, Height = Size, Biome = biomes };
    }

    /// <summary>
    ///     Runs the placer over the given world.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="biomes">Biome map.</param>
    /// <param name="options">Biome options.</param>
    /// <returns>Placements.</returns>
    private static IReadOnlyList<DecorationPlacement> Place(TerrainMap map, BiomeMap biomes,
        BiomeOptions options)
    {
        return new DecorationPlacer(Seed).Apply(map, biomes, options).Placements;
    }

    [Fact]
    public void Apply_SameSeed_ProducesIdenticalPlacements()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        var biomes = FlatWorld(map);
        var options = Options(Pool(weight: 0.02, minSpacing: 1));

        var first = Place(map, biomes, options);
        var second = Place(map, biomes, options);

        Assert.Equal(first.Count, second.Count);
        for (var i = 0; i < first.Count; ++i)
        {
            Assert.Equal(first[i].TemplateName, second[i].TemplateName);
            Assert.Equal(first[i].X, second[i].X);
            Assert.Equal(first[i].Y, second[i].Y);
            Assert.Equal(first[i].Z, second[i].Z);
            Assert.Equal(first[i].Biome, second[i].Biome);
            Assert.Equal(first[i].PoolIndex, second[i].PoolIndex);
        }
    }

    [Fact]
    public void Apply_Placements_RespectMinimumSpacing()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        var biomes = FlatWorld(map);
        var options = Options(Pool(weight: 0.5, minSpacing: 6));

        var placements = Place(map, biomes, options);

        for (var i = 0; i < placements.Count; ++i)
        {
            for (var j = i + 1; j < placements.Count; ++j)
            {
                var distance = Math.Max(Math.Abs(placements[i].X - placements[j].X),
                    Math.Abs(placements[i].Y - placements[j].Y));
                Assert.True(distance >= 6,
                    $"Placements {i} and {j} violate the minimum spacing (distance {distance}).");
            }
        }
    }

    [Fact]
    public void Apply_WaterCliffAndSlopeCells_AreExcluded()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        var biomes = FlatWorld(map);
        for (var x = 0; x < Size; ++x)
        {
            map.Heights[x, 8] = Height;
            map.IsRiver[x, 8] = true;
            map.IsBank[x, 9] = true;
            map.IsCliff[x, 10] = true;
            map.IsLake[x, 11] = true;
            map.IsOcean[x, 12] = true;
        }

        // A slope of 2 exceeds maxSlope 1 on the cell at (5, 13).
        map.Heights[5, 14] = Height + 2;

        var placements = Place(map, biomes, Options(Pool(weight: 1.0, minSpacing: 1)));

        Assert.DoesNotContain(placements, p => p.Y == 8);
        Assert.DoesNotContain(placements, p => p.Y == 9);
        Assert.DoesNotContain(placements, p => p.Y == 10);
        Assert.DoesNotContain(placements, p => p.Y == 11);
        Assert.DoesNotContain(placements, p => p.Y == 12);
        Assert.DoesNotContain(placements, p => p.X == 5 && p.Y == 13);
    }

    [Fact]
    public void Apply_BeachBiome_IsExemptFromWaterFlagExclusion()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        for (var y = 0; y < Size; ++y)
        {
            for (var x = 0; x < Size; ++x)
            {
                map.Heights[x, y] = Height;
            }
        }

        map.IsBank[3, 3] = true;
        map.IsOcean[3, 3] = true;

        var biomes = UniformBiomes(BiomeId.Beach);
        var options = Options(Pool(weight: 1.0, minSpacing: 1), biome: "Beach");

        var placements = Place(map, biomes, options);

        Assert.Contains(placements, p => p.X == 3 && p.Y == 3);
    }

    [Fact]
    public void Apply_GrasslandBank_IsExcluded()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.IsBank[3, 3] = true;

        var biomes = UniformBiomes(BiomeId.Grassland);
        var options = Options(Pool(weight: 1.0, minSpacing: 1));

        var placements = Place(map, biomes, options);

        Assert.DoesNotContain(placements, p => p.X == 3 && p.Y == 3);
    }

    [Fact]
    public void Apply_AboveSnowcapWithoutSnowcapBiome_IsExcluded()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.Heights[6, 6] = Height;
        map.Heights[7, 7] = 30;

        var biomes = UniformBiomes(BiomeId.Grassland);
        var options = Options(Pool(weight: 1.0, minSpacing: 1));

        var placements = Place(map, biomes, options);

        Assert.DoesNotContain(placements, p => p.X == 7 && p.Y == 7);
    }

    [Fact]
    public void Apply_AtMostOneDecorationPerColumn()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        var biomes = FlatWorld(map);
        var options = Options(new List<DecorationOptions>
        {
            new() { Template = "OakTree", Weight = 1.0, MinSpacing = 1, MaxSlope = 1 },
            new() { Template = "PineTree", Weight = 1.0, MinSpacing = 1, MaxSlope = 1 }
        });

        var placements = Place(map, biomes, options);

        Assert.Equal(placements.Count,
            placements.Select(p => (p.X, p.Y)).Distinct().Count());
        Assert.DoesNotContain(placements, p => p.TemplateName == "PineTree");
    }

    [Fact]
    public void Apply_PlacementZ_IsSurfaceTop()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.Heights[4, 4] = Height + 1;
        var biomes = FlatWorld(map);
        var options = Options(Pool(weight: 1.0, minSpacing: 1));

        var placements = Place(map, biomes, options);

        foreach (var placement in placements)
        {
            Assert.Equal(map.Heights[placement.X, placement.Y] + 1, placement.Z);
        }
    }

    [Fact]
    public void Apply_RealizedCount_SanityBoundedByWeight()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        var biomes = FlatWorld(map);
        var options = Options(Pool(weight: 0.02, minSpacing: 1));

        var placements = Place(map, biomes, options);

        var candidates = (long)Size * Size * 0.02;
        Assert.True(placements.Count <= 5 * candidates,
            $"Realized {placements.Count} placements exceed the sanity bound of {5 * candidates}.");
        Assert.True(placements.Count > 0, "A nonzero density should place some decorations.");
    }

    [Fact]
    public void Apply_PlacementsCarryProvenance()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        var biomes = FlatWorld(map);
        var options = Options(Pool(weight: 1.0, minSpacing: 1));

        var placements = Place(map, biomes, options);

        Assert.All(placements, p =>
        {
            Assert.Equal(BiomeId.Grassland, p.Biome);
            Assert.Equal(0, p.PoolIndex);
            Assert.Equal("OakTree", p.TemplateName);
        });
    }
}
