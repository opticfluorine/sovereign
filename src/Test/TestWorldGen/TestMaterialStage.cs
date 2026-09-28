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
///     Unit tests for <see cref="MaterialStage" />.
/// </summary>
public class TestMaterialStage
{
    /// <summary>
    ///     Synthetic map side used by the tests.
    /// </summary>
    private const int Size = 8;

    /// <summary>
    ///     Jitter sub-seed used by the tests.
    /// </summary>
    private const ulong Seed = 987654321;

    private readonly MaterialStage stage = new();

    /// <summary>
    ///     Creates a biome map that assigns the given biome to every cell.
    /// </summary>
    /// <param name="biome">Biome to assign.</param>
    /// <returns>Biome map.</returns>
    private static BiomeMap UniformBiomes(BiomeId biome)
    {
        return UniformBiomes((_, _) => biome);
    }

    /// <summary>
    ///     Creates a biome map from a per-cell assignment function.
    /// </summary>
    /// <param name="biomeOf">Assignment function.</param>
    /// <returns>Biome map.</returns>
    private static BiomeMap UniformBiomes(Func<int, int, BiomeId> biomeOf)
    {
        var map = new BiomeId[Size, Size];
        for (var y = 0; y < Size; ++y)
        {
            for (var x = 0; x < Size; ++x)
            {
                map[x, y] = biomeOf(x, y);
            }
        }

        return new BiomeMap { Width = Size, Height = Size, Biome = map };
    }

    /// <summary>
    ///     Runs the material stage over an all-land synthetic map.
    /// </summary>
    /// <param name="biomes">Biome map.</param>
    /// <param name="map">Optional prebuilt terrain map.</param>
    /// <returns>Column materials.</returns>
    private ColumnMaterials Apply(BiomeMap biomes, TerrainMap? map = null)
    {
        map ??= TestTerrainMaps.Create(Size, Size);
        return stage.Apply(map, biomes, TestTerrainMaps.CreateBiomeOptions(), Seed);
    }

    [Fact]
    public void Apply_DesertColumn_UsesDefinitionDepthAndTemplates()
    {
        var materials = Apply(UniformBiomes(BiomeId.Desert));

        Assert.Equal("Sand", materials.SurfaceTemplate[3, 3]);
        Assert.Equal("Sandstone", materials.SubSurfaceTemplate[3, 3]);
        Assert.Equal(8, materials.SubSurfaceDepth[3, 3]);
    }

    [Fact]
    public void Apply_GrasslandColumn_UsesDefinitionMaterials()
    {
        var materials = Apply(UniformBiomes(BiomeId.Grassland));

        Assert.Equal("Grass", materials.SurfaceTemplate[3, 3]);
        Assert.Equal("Dirt", materials.SubSurfaceTemplate[3, 3]);
        Assert.Equal(4, materials.SubSurfaceDepth[3, 3]);
    }

    [Fact]
    public void Apply_CliffCell_SurfaceMatchesSubSurface()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.IsCliff[3, 3] = true;
        var materials = Apply(UniformBiomes(BiomeId.Grassland), map);

        Assert.Equal(materials.SubSurfaceTemplate[3, 3], materials.SurfaceTemplate[3, 3]);
        Assert.Equal("Dirt", materials.SurfaceTemplate[3, 3]);
    }

    [Fact]
    public void Apply_OceanCellAwayFromLand_UsesOceanFloorTemplate()
    {
        var map = TestTerrainMaps.Create(16, 16);
        var biomes = new BiomeMap { Width = 16, Height = 16, Biome = new BiomeId[16, 16] };
        for (var y = 0; y < 16; ++y)
        {
            for (var x = 0; x < 16; ++x)
            {
                if (x <= 7)
                {
                    map.IsOcean[x, y] = true;
                    biomes.Biome[x, y] = BiomeId.Ocean;
                }
                else
                {
                    biomes.Biome[x, y] = BiomeId.Grassland;
                }
            }
        }

        var materials = Apply(biomes, map);

        Assert.Equal("Gravel", materials.SurfaceTemplate[0, 8]);
        Assert.Equal("Gravel", materials.SubSurfaceTemplate[0, 8]);
        Assert.Equal(0, materials.SubSurfaceDepth[0, 8]);

        // Within the coast range the beach materials are used instead.
        Assert.Equal("Sand", materials.SurfaceTemplate[4, 8]);
        Assert.Equal(3, materials.SubSurfaceDepth[4, 8]);
    }

    [Fact]
    public void Apply_OceanCellNearCoast_UsesBeachMaterials()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.IsOcean[0, 0] = true;
        map.IsOcean[0, 1] = true;
        var biomes = UniformBiomes((x, y) => x == 0 && y <= 1 ? BiomeId.Shelf : BiomeId.Grassland);

        var materials = Apply(biomes, map);

        Assert.Equal("Sand", materials.SurfaceTemplate[0, 0]);
        Assert.Equal(3, materials.SubSurfaceDepth[0, 0]);
    }

    [Fact]
    public void Apply_LakeAndRiverCells_UseWaterFloorTemplate()
    {
        var map = TestTerrainMaps.Create(Size, Size);
        map.IsLake[1, 1] = true;
        map.IsRiver[2, 2] = true;
        var biomes = UniformBiomes((x, y) => (x, y) switch
        {
            (1, 1) => BiomeId.Lake,
            (2, 2) => BiomeId.River,
            _ => BiomeId.Grassland
        });

        var materials = Apply(biomes, map);

        Assert.Equal("Sand", materials.SurfaceTemplate[1, 1]);
        Assert.Equal("Sand", materials.SurfaceTemplate[2, 2]);
        Assert.Equal(0, materials.SubSurfaceDepth[1, 1]);
    }

    [Fact]
    public void Apply_BeachCell_UsesBeachDefinitionAtFixedDepth()
    {
        var materials = Apply(UniformBiomes(BiomeId.Beach));

        Assert.Equal("Sand", materials.SurfaceTemplate[3, 3]);
        Assert.Equal("Sand", materials.SubSurfaceTemplate[3, 3]);
        Assert.Equal(3, materials.SubSurfaceDepth[3, 3]);
    }

    [Fact]
    public void Apply_SwampCell_UsesSwampTemplateOverride()
    {
        var materials = Apply(UniformBiomes(BiomeId.Swamp));

        Assert.Equal("Mud", materials.SurfaceTemplate[3, 3]);
        Assert.Equal("Dirt", materials.SubSurfaceTemplate[3, 3]);
        Assert.Equal(2, materials.SubSurfaceDepth[3, 3]);
    }

    [Fact]
    public void Apply_SurfaceModifier_IsStableWithinRange()
    {
        var biomes = UniformBiomes(BiomeId.Grassland);
        var first = Apply(biomes);
        var second = Apply(biomes);

        for (var y = 0; y < Size; ++y)
        {
            for (var x = 0; x < Size; ++x)
            {
                Assert.InRange(first.SurfaceModifier[x, y], 0, 3);
                Assert.Equal(first.SurfaceModifier[x, y], second.SurfaceModifier[x, y]);
            }
        }
    }
}
