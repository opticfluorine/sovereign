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
using System.IO;
using System.Text.RegularExpressions;
using Sovereign.WorldGen;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for <see cref="ProfileLoader" />.
/// </summary>
public class TestProfileLoader
{
    /// <summary>
    ///     Valid minimal profile containing only the required fields.
    /// </summary>
    private const string MinimalProfileJson = @"{
  ""width"": 2048,
  ""height"": 1024,
  ""seaLevelZ"": 12,
  ""surfaceMaxZ"": 28,
  ""rockFloorZ"": -63,
  ""bedrockZ"": -64,
  ""stoneBands"": [
    { ""fromZ"": -63, ""toZ"": -1, ""template"": ""Basalt"" }
  ]
}";

    [Fact]
    public void Load_ValidMinimalProfile_ParsesToExpectedValues()
    {
        using var scope = new TempProfileDirectory();
        scope.WriteProfile("minimal", MinimalProfileJson);
        var loader = new ProfileLoader(scope.DirectoryPath);

        var profile = loader.Load("minimal");

        Assert.Equal(2048, profile.Width);
        Assert.Equal(1024, profile.Height);
        Assert.Equal(12, profile.SeaLevelZ);
        Assert.Equal(28, profile.SurfaceMaxZ);
        Assert.Equal(-63, profile.RockFloorZ);
        Assert.Equal(-64, profile.BedrockZ);
        var band = Assert.Single(profile.StoneBands);
        Assert.Equal(-63, band.FromZ);
        Assert.Equal(-1, band.ToZ);
        Assert.Equal("Basalt", band.Template);
        Assert.Null(profile.CaveLevels);
        Assert.Null(profile.Rivers);
        Assert.Null(profile.Caves);
        AssertTerrainDefaults(profile.Terrain);
    }

    [Fact]
    public void Load_TerrainSection_ParsesToExpectedValues()
    {
        using var scope = new TempProfileDirectory();
        scope.WriteProfile("terrain", @"{
  ""width"": 2048,
  ""height"": 1024,
  ""seaLevelZ"": 12,
  ""surfaceMaxZ"": 28,
  ""rockFloorZ"": -63,
  ""bedrockZ"": -64,
  ""stoneBands"": [
    { ""fromZ"": -63, ""toZ"": -1, ""template"": ""Basalt"" }
  ],
  ""terrain"": {
    ""continentalnessWavelengthFactor"": 2.5,
    ""continentalnessOctaves"": 7,
    ""warpAmplitudeInner"": 0.2,
    ""warpAmplitudeOuter"": 0.3,
    ""thresholds"": { ""ocean"": 0.35, ""coast"": 0.5, ""inland"": 0.7 },
    ""maxStraightRiverRun"": 64
  }
}");
        var loader = new ProfileLoader(scope.DirectoryPath);

        var profile = loader.Load("terrain");

        var terrain = profile.Terrain;
        Assert.Equal(2.5f, terrain.ContinentalnessWavelengthFactor);
        Assert.Equal(7, terrain.ContinentalnessOctaves);
        Assert.Equal(0.2f, terrain.WarpAmplitudeInner);
        Assert.Equal(0.3f, terrain.WarpAmplitudeOuter);
        Assert.Equal(0.35f, terrain.Thresholds.Ocean);
        Assert.Equal(0.5f, terrain.Thresholds.Coast);
        Assert.Equal(0.7f, terrain.Thresholds.Inland);
        Assert.Equal(64, terrain.MaxStraightRiverRun);
    }

    [Fact]
    public void Load_PartialTerrainSection_FillsMissingKeysWithDefaults()
    {
        using var scope = new TempProfileDirectory();
        scope.WriteProfile("partial", @"{
  ""width"": 2048,
  ""height"": 1024,
  ""seaLevelZ"": 12,
  ""surfaceMaxZ"": 28,
  ""rockFloorZ"": -63,
  ""bedrockZ"": -64,
  ""stoneBands"": [
    { ""fromZ"": -63, ""toZ"": -1, ""template"": ""Basalt"" }
  ],
  ""terrain"": {
    ""maxStraightRiverRun"": 128
  }
}");
        var loader = new ProfileLoader(scope.DirectoryPath);

        var profile = loader.Load("partial");

        Assert.Equal(128, profile.Terrain.MaxStraightRiverRun);
        AssertTerrainDefaultsExceptRun(profile.Terrain);
    }

    [Fact]
    public void Load_UnknownTerrainKey_IsRejected()
    {
        using var scope = new TempProfileDirectory();
        scope.WriteProfile("drifted", @"{
  ""width"": 2048,
  ""height"": 2048,
  ""seaLevelZ"": 12,
  ""surfaceMaxZ"": 28,
  ""rockFloorZ"": -63,
  ""bedrockZ"": -64,
  ""stoneBands"": [
    { ""fromZ"": -63, ""toZ"": -1, ""template"": ""Basalt"" }
  ],
  ""terrain"": {
    ""bogus"": 1
  }
}");
        var loader = new ProfileLoader(scope.DirectoryPath);

        var exception = Assert.Throws<ProfileLoadException>(() => loader.Load("drifted"));

        Assert.Contains("bogus", exception.Message);
    }

    /// <summary>
    ///     Asserts that the terrain options carry the shipped defaults.
    /// </summary>
    /// <param name="terrain">Terrain options to assert.</param>
    private static void AssertTerrainDefaults(TerrainOptions terrain)
    {
        Assert.Equal(0.9f, terrain.ContinentalnessWavelengthFactor);
        Assert.Equal(6, terrain.ContinentalnessOctaves);
        Assert.Equal(0.35f, terrain.WarpAmplitudeInner);
        Assert.Equal(0.40f, terrain.WarpAmplitudeOuter);
        Assert.Equal(0.32f, terrain.Thresholds.Ocean);
        Assert.Equal(0.40f, terrain.Thresholds.Coast);
        Assert.Equal(0.52f, terrain.Thresholds.Inland);
        Assert.Equal(96, terrain.MaxStraightRiverRun);
    }

    /// <summary>
    ///     Asserts the terrain defaults except the maximum straight river run.
    /// </summary>
    /// <param name="terrain">Terrain options to assert.</param>
    private static void AssertTerrainDefaultsExceptRun(TerrainOptions terrain)
    {
        Assert.Equal(0.9f, terrain.ContinentalnessWavelengthFactor);
        Assert.Equal(6, terrain.ContinentalnessOctaves);
        Assert.Equal(0.35f, terrain.WarpAmplitudeInner);
        Assert.Equal(0.40f, terrain.WarpAmplitudeOuter);
        Assert.Equal(0.32f, terrain.Thresholds.Ocean);
        Assert.Equal(0.40f, terrain.Thresholds.Coast);
        Assert.Equal(0.52f, terrain.Thresholds.Inland);
    }

    [Fact]
    public void Load_MalformedJson_ReportsPathAndPosition()
    {
        using var scope = new TempProfileDirectory();
        scope.WriteProfile("broken", @"{
  ""width"": 2048,
  ""height"": @@@
}");
        var loader = new ProfileLoader(scope.DirectoryPath);

        var exception = Assert.Throws<ProfileLoadException>(() => loader.Load("broken"));

        Assert.Contains("broken.json", exception.Message);
        Assert.Matches(new Regex(@"line 3, column \d+"), exception.Message);
    }

    [Fact]
    public void Load_UnknownProperty_IsRejected()
    {
        using var scope = new TempProfileDirectory();
        scope.WriteProfile("drifted", @"{
  ""width"": 2048,
  ""height"": 2048,
  ""seaLevelZ"": 12,
  ""surfaceMaxZ"": 28,
  ""rockFloorZ"": -63,
  ""bedrockZ"": -64,
  ""stoneBands"": [
    { ""fromZ"": -63, ""toZ"": -1, ""template"": ""Basalt"" }
  ],
  ""biomes"": []
}");
        var loader = new ProfileLoader(scope.DirectoryPath);

        var exception = Assert.Throws<ProfileLoadException>(() => loader.Load("drifted"));

        Assert.Contains("biomes", exception.Message);
    }

    [Fact]
    public void Load_MissingRequiredField_IsRejectedNamingField()
    {
        using var scope = new TempProfileDirectory();
        scope.WriteProfile("incomplete", @"{
  ""height"": 2048,
  ""seaLevelZ"": 12,
  ""surfaceMaxZ"": 28,
  ""rockFloorZ"": -63,
  ""bedrockZ"": -64,
  ""stoneBands"": [
    { ""fromZ"": -63, ""toZ"": -1, ""template"": ""Basalt"" }
  ]
}");
        var loader = new ProfileLoader(scope.DirectoryPath);

        var exception = Assert.Throws<ProfileLoadException>(() => loader.Load("incomplete"));

        Assert.Contains("width", exception.Message);
    }

    [Fact]
    public void Load_UnknownProfileName_ErrorsWithSearchDirectoryAndName()
    {
        using var scope = new TempProfileDirectory();
        var loader = new ProfileLoader(scope.DirectoryPath);

        var exception = Assert.Throws<ProfileLoadException>(() => loader.Load("nonexistent"));

        Assert.Contains("nonexistent", exception.Message);
        Assert.Contains(scope.DirectoryPath, exception.Message);
    }

    [Fact]
    public void Load_ShippedDefaultProfile_LoadsAndValidatesCleanly()
    {
        var loader = new ProfileLoader(
            Path.Combine(AppContext.BaseDirectory, "Data", "Worldgen"));
        var validator = new ProfileValidator();

        var profile = loader.Load("default");
        var issues = validator.Validate(profile);

        Assert.Empty(issues);
        Assert.Equal(2048, profile.Width);
        Assert.Equal(2048, profile.Height);
        Assert.Equal(12, profile.SeaLevelZ);
        Assert.Equal(-63, profile.RockFloorZ);
        Assert.Equal(-64, profile.BedrockZ);
        Assert.Equal(new[] { -16, -32, -48 },
            new[] { profile.CaveLevels![0].FloorZ, profile.CaveLevels[1].FloorZ, profile.CaveLevels[2].FloorZ });
        Assert.NotNull(profile.Biomes);
        Assert.Equal(
            new HashSet<string> { "Taiga", "Forest", "Savanna", "Grassland", "Desert" },
            ReferencedTableBiomes(profile.Biomes!));
        Assert.Equal(96, profile.Terrain.MaxStraightRiverRun);
        Assert.Contains("OakTree", profile.Biomes.Definitions["Grassland"].Decorations![0].Template);
    }

    /// <summary>
    ///     Collects the distinct biome names referenced by a table.
    /// </summary>
    /// <param name="options">Biome options.</param>
    /// <returns>Distinct referenced biome names.</returns>
    private static HashSet<string> ReferencedTableBiomes(BiomeOptions options)
    {
        return new HashSet<string>
        {
            options.Table.Cold.Dry, options.Table.Cold.Temperate, options.Table.Cold.Wet,
            options.Table.Mild.Dry, options.Table.Mild.Temperate, options.Table.Mild.Wet,
            options.Table.Hot.Dry, options.Table.Hot.Temperate, options.Table.Hot.Wet
        };
    }

    /// <summary>
    ///     Temporary directory for profile files, deleted on disposal.
    /// </summary>
    private sealed class TempProfileDirectory : IDisposable
    {
        public TempProfileDirectory()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(DirectoryPath);
        }

        /// <summary>
        ///     Path of the temporary directory.
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        ///     Writes a profile JSON file into the temporary directory.
        /// </summary>
        /// <param name="name">Profile name without extension.</param>
        /// <param name="json">Profile JSON content.</param>
        public void WriteProfile(string name, string json)
        {
            File.WriteAllText(Path.Combine(DirectoryPath, $"{name}.json"), json);
        }

        public void Dispose()
        {
            Directory.Delete(DirectoryPath, true);
        }
    }
}
