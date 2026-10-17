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
using System.IO;
using System.Text.Json;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Sovereign.WorldGen;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for <see cref="WorldGenProfileStore" />.
/// </summary>
public class TestWorldGenProfileStore
{
    /// <summary>
    ///     JSON text of a semantically valid minimal profile.
    /// </summary>
    private const string ValidProfileJson = @"{
  ""width"": 128,
  ""height"": 128,
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
    public void TrySaveProfile_ValidProfile_SavesParseableFile()
    {
        using var scope = new TempDirectory();
        var store = CreateStore(scope.DirectoryPath);

        var saved = store.TrySaveProfile("rest", ValidProfileJson, out var error);

        Assert.True(saved, error);
        Assert.Null(error);
        var profile = new ProfileLoader(scope.DirectoryPath).Load("rest");
        Assert.Equal(128, profile.Width);
    }

    [Fact]
    public void TrySaveProfile_InvalidName_ReturnsFalseWithoutWriting()
    {
        using var scope = new TempDirectory();
        var store = CreateStore(scope.DirectoryPath);

        var saved = store.TrySaveProfile("../evil", ValidProfileJson, out var error);

        Assert.False(saved);
        Assert.NotNull(error);
        Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(scope.DirectoryPath)!, "evil.json"));
    }

    [Fact]
    public void TrySaveProfile_MalformedJson_ReturnsFalse()
    {
        using var scope = new TempDirectory();
        var store = CreateStore(scope.DirectoryPath);

        var saved = store.TrySaveProfile("broken", "{ not json", out var error);

        Assert.False(saved);
        Assert.NotNull(error);
    }

    [Fact]
    public void TrySaveProfile_UnknownProperty_ReturnsFalse()
    {
        using var scope = new TempDirectory();
        var store = CreateStore(scope.DirectoryPath);
        var json = ValidProfileJson.Replace("\"seaLevelZ\": 12",
            "\"seaLevelZ\": 12, \"bogus\": 1");

        var saved = store.TrySaveProfile("drifted", json, out var error);

        Assert.False(saved);
        Assert.Contains("bogus", error);
    }

    [Fact]
    public void TrySaveProfile_SemanticValidationError_ReturnsFalse()
    {
        using var scope = new TempDirectory();
        var store = CreateStore(scope.DirectoryPath);
        var json = ValidProfileJson.Replace("\"width\": 128", "\"width\": 100");

        var saved = store.TrySaveProfile("invalid", json, out var error);

        Assert.False(saved);
        Assert.Contains("width", error, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(Directory.GetFiles(scope.DirectoryPath));
    }

    [Fact]
    public void TrySaveProfile_OverwritesExistingProfile()
    {
        using var scope = new TempDirectory();
        var store = CreateStore(scope.DirectoryPath);

        Assert.True(store.TrySaveProfile("profile", ValidProfileJson, out _));
        var modified = ValidProfileJson.Replace("\"seaLevelZ\": 12", "\"seaLevelZ\": 3");
        Assert.True(store.TrySaveProfile("profile", modified, out var error));

        Assert.Null(error);
        var profile = new ProfileLoader(scope.DirectoryPath).Load("profile");
        Assert.Equal(3, profile.SeaLevelZ);
    }

    /// <summary>
    ///     Creates a profile store backed by the given profile directory.
    /// </summary>
    /// <param name="directoryPath">Profile directory path.</param>
    /// <returns>The store.</returns>
    private static WorldGenProfileStore CreateStore(string directoryPath)
    {
        return new WorldGenProfileStore(new ProfileLoader(directoryPath), new ProfileValidator());
    }

    /// <summary>
    ///     Temporary directory for profile files, deleted on disposal.
    /// </summary>
    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(DirectoryPath);
        }

        /// <summary>
        ///     Path of the temporary directory.
        /// </summary>
        public string DirectoryPath { get; }

        public void Dispose()
        {
            Directory.Delete(DirectoryPath, true);
        }
    }
}
