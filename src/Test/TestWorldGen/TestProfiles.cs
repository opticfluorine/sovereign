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

namespace TestWorldGen;

/// <summary>
///     Builder for valid baseline world generation profiles.
/// </summary>
internal static class TestProfiles
{
    /// <summary>
    ///     Creates a fully valid baseline profile.
    /// </summary>
    /// <returns>Profile.</returns>
    public static WorldGenProfile CreateValid()
    {
        return new WorldGenProfile
        {
            Width = 2048,
            Height = 2048,
            SeaLevelZ = 12,
            SurfaceMaxZ = 28,
            RockFloorZ = -63,
            BedrockZ = -64,
            StoneBands = new List<StoneBand>
            {
                new() { FromZ = -16, ToZ = -1, Template = "Shale" },
                new() { FromZ = -40, ToZ = -17, Template = "Granite" },
                new() { FromZ = -63, ToZ = -41, Template = "Basalt" }
            },
            CaveLevels = new List<CaveLevel>
            {
                new() { FloorZ = -16, Headroom = 2 },
                new() { FloorZ = -32, Headroom = 2 },
                new() { FloorZ = -48, Headroom = 2 }
            },
            Rivers = new RiverOptions { MaxCount = 40, MinLength = 64 },
            Caves = new CaveOptions { ShaftsPerLevelPair = 3, SurfaceMouths = 2 }
        };
    }

    /// <summary>
    ///     Creates the 128x128 baseline profile used by pipeline tests. Cave data is parsed
    ///     but unused until the cave generation card.
    /// </summary>
    /// <returns>Profile.</returns>
    public static WorldGenProfile CreateSmall128()
    {
        return new WorldGenProfile
        {
            Width = 128,
            Height = 128,
            SeaLevelZ = 12,
            SurfaceMaxZ = 28,
            RockFloorZ = -63,
            BedrockZ = -64,
            StoneBands = new List<StoneBand>
            {
                new() { FromZ = -16, ToZ = -1, Template = "Shale" },
                new() { FromZ = -40, ToZ = -17, Template = "Granite" },
                new() { FromZ = -63, ToZ = -41, Template = "Basalt" }
            },
            CaveLevels = new List<CaveLevel>
            {
                new() { FloorZ = -32, Headroom = 2 }
            },
            Rivers = new RiverOptions { MaxCount = 8, MinLength = 16 }
        };
    }

    /// <summary>
    ///     Creates the 128x128 baseline profile without river options; river stages are skipped.
    /// </summary>
    /// <returns>Profile.</returns>
    public static WorldGenProfile CreateSmall128WithoutRivers()
    {
        var profile = CreateSmall128();
        profile.Rivers = null;
        return profile;
    }

    /// <summary>
    ///     Creates the 128x128 baseline profile with a minimal biomes section: three table
    ///     biomes, ocean and beach handled by terrain flags, one swamp configuration, and
    ///     small decoration pools. Terrain parameters match <see cref="CreateSmall128" /> so
    ///     that the heights golden hash is shared.
    /// </summary>
    /// <returns>Profile.</returns>
    public static WorldGenProfile CreateSmall128Biomes()
    {
        var profile = CreateSmall128();
        profile.Biomes = new BiomeOptions
        {
            SnowcapZ = 26,
            AlpineZ = 23,
            OceanFloorTemplate = "Gravel",
            WaterFloorTemplate = "Sand",
            Table = new BiomeTableOptions
            {
                Cold = new BiomeTableRow { Dry = "Taiga", Temperate = "Taiga", Wet = "Taiga" },
                Mild = new BiomeTableRow { Dry = "Grassland", Temperate = "Grassland", Wet = "Grassland" },
                Hot = new BiomeTableRow { Dry = "Savanna", Temperate = "Grassland", Wet = "Grassland" }
            },
            Swamp = new SwampOptions { Template = "Grass", MaxHeightZ = 14 },
            Definitions = new Dictionary<string, BiomeDefinition>
            {
                ["Grassland"] = new BiomeDefinition
                {
                    SurfaceTemplate = "Grass",
                    SubSurfaceTemplate = "Dirt",
                    SubSurfaceDepth = 4,
                    Decorations = new List<DecorationOptions>
                    {
                        new() { Template = "OakTree", Weight = 0.02, MinSpacing = 5, MaxSlope = 1 },
                        new() { Template = "Boulder", Weight = 0.005, MinSpacing = 7, MaxSlope = 2 }
                    }
                },
                ["Taiga"] = new BiomeDefinition
                {
                    SurfaceTemplate = "Grass",
                    SubSurfaceTemplate = "Dirt",
                    SubSurfaceDepth = 2,
                    Decorations = new List<DecorationOptions>
                    {
                        new() { Template = "PineTree", Weight = 0.06, MinSpacing = 3, MaxSlope = 2 }
                    }
                },
                ["Savanna"] = new BiomeDefinition
                {
                    SurfaceTemplate = "Grass",
                    SubSurfaceTemplate = "Sand",
                    SubSurfaceDepth = 2,
                    Decorations = new List<DecorationOptions>
                    {
                        new() { Template = "AcaciaTree", Weight = 0.01, MinSpacing = 6, MaxSlope = 1 }
                    }
                },
                ["Beach"] = new BiomeDefinition
                {
                    SurfaceTemplate = "Sand",
                    SubSurfaceTemplate = "Sand",
                    SubSurfaceDepth = 3
                },
                ["Swamp"] = new BiomeDefinition
                {
                    SurfaceTemplate = "Grass",
                    SubSurfaceTemplate = "Dirt",
                    SubSurfaceDepth = 2,
                    Decorations = new List<DecorationOptions>
                    {
                        new() { Template = "DeadBush", Weight = 0.04, MinSpacing = 4, MaxSlope = 1 }
                    }
                },
                ["Alpine"] = new BiomeDefinition
                {
                    SurfaceTemplate = "Shale",
                    SubSurfaceTemplate = "Shale",
                    SubSurfaceDepth = 1
                },
                ["Snowcap"] = new BiomeDefinition
                {
                    SurfaceTemplate = "Snow",
                    SubSurfaceTemplate = "Shale",
                    SubSurfaceDepth = 1
                }
            }
        };
        return profile;
    }
}
