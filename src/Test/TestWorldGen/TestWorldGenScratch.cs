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

using System.IO;
using Microsoft.Extensions.Options;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests of the scratch directory service: staging directories and TTL cleanup.
/// </summary>
public class TestWorldGenScratch : IDisposable
{
    /// <summary>
    ///     Scratch directory override for the test.
    /// </summary>
    private readonly string scratchPath;

    public TestWorldGenScratch()
    {
        scratchPath = Path.Combine(Path.GetTempPath(), "worldgen-scratch-test",
            Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        if (Directory.Exists(scratchPath)) Directory.Delete(scratchPath, true);
    }

    /// <summary>
    ///     Creates a scratch service rooted at the test directory.
    /// </summary>
    /// <returns>Scratch service.</returns>
    private WorldGenScratch CreateScratch()
    {
        return new WorldGenScratch(Options.Create(new WorldGenOptions
        {
            ScratchDirectory = scratchPath
        }));
    }

    [Fact]
    public void CreateSessionDirectory_CreatesUniqueDirectories()
    {
        var scratch = CreateScratch();

        var first = scratch.CreateSessionDirectory(42);
        var second = scratch.CreateSessionDirectory(42);

        Assert.NotEqual(first, second);
        Assert.True(Directory.Exists(first));
        Assert.StartsWith(scratchPath, first, StringComparison.Ordinal);
        Assert.Contains(WorldGenScratch.SessionDirectoryPrefix, first, StringComparison.Ordinal);
    }

    [Fact]
    public void CleanupStaleDirectories_RemovesOldStagingDirectories()
    {
        var scratch = CreateScratch();
        var stale = scratch.CreateSessionDirectory(1);
        Directory.SetLastWriteTimeUtc(stale, DateTime.UtcNow - TimeSpan.FromDays(2));

        var removed = scratch.CleanupStaleDirectories();

        Assert.Equal(1, removed);
        Assert.False(Directory.Exists(stale));
    }

    [Fact]
    public void CleanupStaleDirectories_KeepsRecentDirectories()
    {
        var scratch = CreateScratch();
        var fresh = scratch.CreateSessionDirectory(2);

        var removed = scratch.CleanupStaleDirectories();

        Assert.Equal(0, removed);
        Assert.True(Directory.Exists(fresh), "Startup cleanup must not touch a fresh job's directory.");
    }

    [Fact]
    public void CleanupStaleDirectories_RemovesOldPreviews()
    {
        var scratch = CreateScratch();
        var stalePreview = Path.Combine(scratch.ResolveDirectory(), "preview_1_1.png");
        File.WriteAllBytes(stalePreview, new byte[] { 1, 2, 3 });
        File.SetLastWriteTimeUtc(stalePreview, DateTime.UtcNow - TimeSpan.FromDays(2));

        var removed = scratch.CleanupStaleDirectories();

        Assert.Equal(1, removed);
        Assert.False(File.Exists(stalePreview));
    }

    [Fact]
    public void DeleteSessionDirectory_RemovesStagingDirectory()
    {
        var scratch = CreateScratch();
        var staging = scratch.CreateSessionDirectory(3);
        Directory.CreateDirectory(Path.Combine(staging, "segments"));

        scratch.DeleteSessionDirectory(staging);

        Assert.False(Directory.Exists(staging));
    }
}
