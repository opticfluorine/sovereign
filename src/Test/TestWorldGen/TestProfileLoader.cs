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
using System.Text.Json;
using System.Text.RegularExpressions;
using Sovereign.WorldGen;
using Sovereign.WorldGen.Layout;
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
  ""bedrockTemplate"": ""Bedrock"",
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
  ""bedrockTemplate"": ""Bedrock"",
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
  ""bedrockTemplate"": ""Bedrock"",
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
  ""bedrockTemplate"": ""Bedrock"",
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

    [Fact]
    public void Load_PartialCavesSection_FillsMissingKeysWithDefaults()
    {
        using var scope = new TempProfileDirectory();
        scope.WriteProfile("caves", @"{
  ""width"": 2048,
  ""height"": 2048,
  ""seaLevelZ"": 12,
  ""surfaceMaxZ"": 28,
  ""rockFloorZ"": -63,
  ""bedrockZ"": -64,
  ""bedrockTemplate"": ""Bedrock"",
  ""stoneBands"": [
    { ""fromZ"": -63, ""toZ"": -1, ""template"": ""Basalt"" }
  ],
  ""caveLevels"": [
    { ""floorZ"": -32, ""headroom"": 2 }
  ],
  ""caves"": {
    ""porosity"": 0.4
  }
}");
        var loader = new ProfileLoader(scope.DirectoryPath);

        var profile = loader.Load("caves");

        var caves = profile.Caves;
        Assert.NotNull(caves);
        Assert.Equal(0.4, caves!.Porosity);
        Assert.Equal(CaveOptions.DefaultMinTunnelWidth, caves.MinTunnelWidth);
        Assert.Equal(CaveOptions.DefaultMouthMinLandDistance, caves.MouthMinLandDistance);
        Assert.Equal(0, caves.ShaftsPerLevelPair);
        Assert.Equal(0, caves.SurfaceMouths);
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
  ""bedrockTemplate"": ""Bedrock"",
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
  ""bedrockTemplate"": ""Bedrock"",
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
        Assert.Contains("Oak Tree", profile.Biomes.Definitions["Grassland"].Decorations![0].Template);
    }

    [Fact]
    public void Load_ShippedThreeLandsProfile_ParsesLayoutAndValidatesCleanly()
    {
        var loader = new ProfileLoader(
            Path.Combine(AppContext.BaseDirectory, "Data", "Worldgen"));
        var validator = new ProfileValidator();

        var profile = loader.Load("threelands");
        var issues = validator.Validate(profile);

        Assert.Empty(issues);
        Assert.NotNull(profile.Layout);
        Assert.Equal(1.0f, profile.Layout!.Strength);
        Assert.Null(profile.Layout.Preset);
        var anchors = Assert.IsType<List<LayoutAnchor>>(profile.Layout.Anchors);
        Assert.Equal(3, anchors.Count);
        Assert.Equal(1.0f, anchors[1].MountainBias);
    }

    [Fact]
    public void Load_ShippedThreeLandsProfile_DefaultsConnectivityToNone()
    {
        var loader = new ProfileLoader(
            Path.Combine(AppContext.BaseDirectory, "Data", "Worldgen"));

        var profile = loader.Load("threelands");

        Assert.Equal(LayoutConnectivity.None, profile.Layout!.Connectivity);
    }

    [Fact]
    public void Load_LayoutConnectivity_ParsesCaseInsensitively()
    {
        using var scope = new TempProfileDirectory();
        scope.WriteProfile("separate", @"{
  ""width"": 2048,
  ""height"": 1024,
  ""seaLevelZ"": 12,
  ""surfaceMaxZ"": 28,
  ""rockFloorZ"": -63,
  ""bedrockZ"": -64,
  ""bedrockTemplate"": ""Bedrock"",
  ""stoneBands"": [
    { ""fromZ"": -63, ""toZ"": -1, ""template"": ""Basalt"" }
  ],
  ""layout"": {
    ""strength"": 1.8,
    ""connectivity"": ""Strict"",
    ""anchors"": [
      { ""x"": 0.5, ""y"": 0.5, ""radius"": 0.2, ""weight"": 1.0 }
    ]
  }
}");
        var loader = new ProfileLoader(scope.DirectoryPath);

        var profile = loader.Load("separate");

        Assert.Equal(LayoutConnectivity.Strict, profile.Layout!.Connectivity);
        Assert.Equal(1.8f, profile.Layout.Strength);
    }

    [Fact]
    public void Save_ValidProfile_RoundTripsThroughLoad()
    {
        using var scope = new TempProfileDirectory();
        var loader = new ProfileLoader(scope.DirectoryPath);
        var profile = TestProfiles.CreateSmall128Biomes();

        loader.Save("saved", profile);
        var loaded = loader.Load("saved");

        Assert.Equal(Serialize(profile), Serialize(loaded));
    }

    [Fact]
    public void Save_OverwritesExistingProfile()
    {
        using var scope = new TempProfileDirectory();
        var loader = new ProfileLoader(scope.DirectoryPath);

        loader.Save("profile", TestProfiles.CreateSmall128());
        var modified = TestProfiles.CreateSmall128();
        modified.SeaLevelZ = 5;
        loader.Save("profile", modified);

        var loaded = loader.Load("profile");
        Assert.Equal(5, loaded.SeaLevelZ);
    }

    [Fact]
    public void Save_CreatesMissingDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "nested");
        try
        {
            var loader = new ProfileLoader(directory);

            loader.Save("fresh", TestProfiles.CreateSmall128());

            Assert.True(File.Exists(Path.Combine(directory, "fresh.json")));
        }
        finally
        {
            Directory.Delete(Path.Combine(Path.GetTempPath(),
                Path.GetFileName(Path.GetDirectoryName(directory))!), true);
        }
    }

    [Theory]
    [InlineData("../evil")]
    [InlineData("sub/dir")]
    [InlineData("a\\\\b")]
    [InlineData("..")]
    [InlineData(".")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("a:b")]
    [InlineData("a?b")]
    [InlineData("a\nb")]
    public void Save_InvalidName_ThrowsAndWritesNothing(string name)
    {
        using var scope = new TempProfileDirectory();
        var loader = new ProfileLoader(scope.DirectoryPath);

        Assert.Throws<ProfileLoadException>(
            () => loader.Save(name, TestProfiles.CreateSmall128()));

        Assert.Empty(Directory.EnumerateFiles(scope.DirectoryPath, "*", SearchOption.AllDirectories));
    }

    [Fact]
    public void Save_TraversalName_DoesNotWriteOutsideDirectory()
    {
        var parent = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(parent);
        try
        {
            var loader = new ProfileLoader(Path.Combine(parent, "profiles"));

            Assert.Throws<ProfileLoadException>(
                () => loader.Save("../evil", TestProfiles.CreateSmall128()));

            Assert.Empty(Directory.GetFiles(parent, "evil.json", SearchOption.AllDirectories));
        }
        finally
        {
            Directory.Delete(parent, true);
        }
    }

    [Theory]
    [InlineData("../evil")]
    [InlineData("sub/dir")]
    [InlineData("..")]
    [InlineData("")]
    public void Load_InvalidName_Throws(string name)
    {
        using var scope = new TempProfileDirectory();
        var loader = new ProfileLoader(scope.DirectoryPath);

        Assert.Throws<ProfileLoadException>(() => loader.Load(name));
    }

    [Fact]
    public void Parse_MinimalProfile_ParsesToExpectedValues()
    {
        var loader = new ProfileLoader("unused");

        var profile = loader.Parse(MinimalProfileJson);

        Assert.Equal(2048, profile.Width);
        Assert.Equal("Bedrock", profile.BedrockTemplate);
    }

    [Fact]
    public void Parse_MalformedJson_Throws()
    {
        var loader = new ProfileLoader("unused");

        Assert.Throws<ProfileLoadException>(() => loader.Parse("{ not json"));
    }

    [Fact]
    public void Parse_UnknownProperty_Throws()
    {
        var loader = new ProfileLoader("unused");
        var json = MinimalProfileJson.Replace("\"seaLevelZ\": 12",
            "\"seaLevelZ\": 12, \"bogus\": 1");

        Assert.Throws<ProfileLoadException>(() => loader.Parse(json));
    }

    [Fact]
    public void Parse_MissingRequiredProperty_Throws()
    {
        var loader = new ProfileLoader("unused");
        var json = MinimalProfileJson.Replace("\"width\": 2048,", "");

        Assert.Throws<ProfileLoadException>(() => loader.Parse(json));
    }

    /// <summary>
    ///     Serializes a profile for deep comparison.
    /// </summary>
    /// <param name="profile">Profile to serialize.</param>
    /// <returns>Serialized profile.</returns>
    private static string Serialize(WorldGenProfile profile)
    {
        return JsonSerializer.Serialize(profile, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
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
