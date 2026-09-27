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

using Sovereign.WorldGen;
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Terrain;

namespace TestWorldGen;

/// <summary>
///     Builder for synthetic terrain maps and biome options used by the biome stage tests.
/// </summary>
internal static class TestTerrainMaps
{
    /// <summary>
    ///     Sea level used by the synthetic maps.
    /// </summary>
    public const int SeaLevelZ = 12;

    /// <summary>
    ///     Surface maximum used by the synthetic maps.
    /// </summary>
    public const int SurfaceMaxZ = 28;

    /// <summary>
    ///     Maximum swamp height Z of the shared biome options.
    /// </summary>
    public const int SwampMaxHeightZ = 14;

    /// <summary>
    ///     Creates an empty terrain map: all land at sea level, no flags set.
    /// </summary>
    /// <param name="width">Map width in blocks.</param>
    /// <param name="height">Map height in blocks.</param>
    /// <returns>Terrain map.</returns>
    public static TerrainMap Create(int width, int height)
    {
        return new TerrainMap
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
    }

    /// <summary>
    ///     Creates a continentalness classification that marks every cell as land.
    /// </summary>
    /// <param name="width">Map width in blocks.</param>
    /// <param name="height">Map height in blocks.</param>
    /// <returns>Continentalness result.</returns>
    public static ContinentalnessResult AllLand(int width, int height)
    {
        var classes = new ContinentalClass[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                classes[x, y] = ContinentalClass.Land;
            }
        }

        return new ContinentalnessResult
        {
            Classes = classes,
            ThresholdOcean = 0.4f,
            ThresholdCoast = 0.48f,
            ThresholdInland = 0.62f
        };
    }

    /// <summary>
    ///     Creates a continentalness classification that marks every cell as deep ocean.
    /// </summary>
    /// <param name="width">Map width in blocks.</param>
    /// <param name="height">Map height in blocks.</param>
    /// <returns>Continentalness result.</returns>
    public static ContinentalnessResult AllDeepOcean(int width, int height)
    {
        var classes = new ContinentalClass[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                classes[x, y] = ContinentalClass.DeepOcean;
            }
        }

        return new ContinentalnessResult
        {
            Classes = classes,
            ThresholdOcean = 0.4f,
            ThresholdCoast = 0.48f,
            ThresholdInland = 0.62f
        };
    }

    /// <summary>
    ///     Creates a constant normalized climate field.
    /// </summary>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    /// <param name="value">Constant field value in [0, 1].</param>
    /// <returns>Climate field.</returns>
    public static float[,] ConstantField(int width, int height, float value)
    {
        var field = new float[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                field[x, y] = value;
            }
        }

        return field;
    }

    /// <summary>
    ///     Creates biome options with a Whittaker table that covers all nine band
    ///     combinations with distinct biomes.
    /// </summary>
    /// <returns>Biome options.</returns>
    public static BiomeOptions CreateBiomeOptions()
    {
        return new BiomeOptions
        {
            SnowcapZ = 24,
            AlpineZ = 20,
            OceanFloorTemplate = "Gravel",
            WaterFloorTemplate = "Sand",
            Table = new BiomeTableOptions
            {
                Cold = new BiomeTableRow { Dry = "Taiga", Temperate = "Forest", Wet = "Forest" },
                Mild = new BiomeTableRow { Dry = "Savanna", Temperate = "Grassland", Wet = "Forest" },
                Hot = new BiomeTableRow { Dry = "Desert", Temperate = "Savanna", Wet = "Grassland" }
            },
            Swamp = new SwampOptions { Template = "Mud", MaxHeightZ = 14 },
            Definitions = new Dictionary<string, BiomeDefinition>
            {
                ["Grassland"] = Definition("Grass", "Dirt", 4),
                ["Forest"] = Definition("Grass", "Dirt", 3),
                ["Taiga"] = Definition("Grass", "Dirt", 2),
                ["Desert"] = Definition("Sand", "Sandstone", 8),
                ["Savanna"] = Definition("Grass", "Sand", 2),
                ["Beach"] = Definition("Sand", "Sand", 3),
                ["Swamp"] = Definition("Mud", "Dirt", 2),
                ["Alpine"] = Definition("Shale", "Shale", 1),
                ["Snowcap"] = Definition("Snow", "Shale", 1)
            }
        };
    }

    /// <summary>
    ///     Creates a biome definition.
    /// </summary>
    /// <param name="surface">Surface template.</param>
    /// <param name="subSurface">Subsurface template.</param>
    /// <param name="depth">Subsurface depth.</param>
    /// <returns>Biome definition.</returns>
    public static BiomeDefinition Definition(string surface, string subSurface, int depth)
    {
        return new BiomeDefinition
        {
            SurfaceTemplate = surface,
            SubSurfaceTemplate = subSurface,
            SubSurfaceDepth = depth
        };
    }
}
