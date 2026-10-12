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
using System.Linq;
using Sovereign.Persistence.Database;
using Xunit;

namespace TestPersistence.Database;

/// <summary>
///     Unit tests for MigrationScriptLocator.
/// </summary>
public sealed class TestMigrationScriptLocator : IDisposable
{
    private readonly string rootDirectory;

    public TestMigrationScriptLocator()
    {
        rootDirectory = Path.Combine(Path.GetTempPath(), $"migration-locator-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDirectory);

        CreateMigration("0001-Baseline");
        CreateMigration("0002-SegmentFlags");
        CreateMigration("0003-Later");
        CreateMigrationDirectoryOnly("0004-MissingScript");
        CreateDirectoryOnly("Full");
        CreateDirectoryOnly("not-a-migration");
    }

    public void Dispose()
    {
        Directory.Delete(rootDirectory, true);
    }

    /// <summary>
    ///     Tests that scripts are returned sorted by level.
    /// </summary>
    [Fact]
    public void ReturnsScriptsSortedByLevel()
    {
        var locator = new MigrationScriptLocator(rootDirectory);

        var scripts = locator.GetPendingScripts(0);

        Assert.Equal(new[] { 1, 2, 3 }, scripts.Select(s => s.Level));
    }

    /// <summary>
    ///     Tests that levels at or below the applied level are skipped.
    /// </summary>
    [Fact]
    public void SkipsAppliedLevels()
    {
        var locator = new MigrationScriptLocator(rootDirectory);

        Assert.Equal(new[] { 2, 3 }, locator.GetPendingScripts(1).Select(s => s.Level));
        Assert.Equal(new[] { 3 }, locator.GetPendingScripts(2).Select(s => s.Level));
        Assert.Empty(locator.GetPendingScripts(3));
    }

    /// <summary>
    ///     Tests that directory names are parsed into level numbers.
    /// </summary>
    [Fact]
    public void ParsesLevelFromDirectoryName()
    {
        var locator = new MigrationScriptLocator(rootDirectory);

        var script = locator.GetPendingScripts(1).First();

        Assert.Equal(2, script.Level);
        Assert.EndsWith("0002-SegmentFlags_sqlite.sql", script.Filename);
    }

    /// <summary>
    ///     Tests that non-matching directories are ignored.
    /// </summary>
    [Fact]
    public void SkipsNonMatchingDirectories()
    {
        var locator = new MigrationScriptLocator(rootDirectory);

        var scripts = locator.GetPendingScripts(0);

        Assert.DoesNotContain(scripts, s => s.Filename.Contains("Full"));
        Assert.DoesNotContain(scripts, s => s.Filename.Contains("not-a-migration"));
        Assert.DoesNotContain(scripts, s => s.Level == 4);
    }

    /// <summary>
    ///     Tests that a missing migrations directory yields no scripts.
    /// </summary>
    [Fact]
    public void MissingDirectoryYieldsNoScripts()
    {
        var locator = new MigrationScriptLocator(Path.Combine(rootDirectory, "does-not-exist"));

        Assert.Empty(locator.GetPendingScripts(0));
    }

    /// <summary>
    ///     Creates a migration directory and its matching script.
    /// </summary>
    /// <param name="name">Directory and script base name.</param>
    private void CreateMigration(string name)
    {
        var directory = Path.Combine(rootDirectory, name);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, $"{name}_sqlite.sql"), "-- test migration");
    }

    /// <summary>
    ///     Creates a matching migration directory with no script.
    /// </summary>
    /// <param name="name">Directory name.</param>
    private void CreateMigrationDirectoryOnly(string name)
    {
        Directory.CreateDirectory(Path.Combine(rootDirectory, name));
    }

    /// <summary>
    ///     Creates a directory that does not match the migration naming pattern.
    /// </summary>
    /// <param name="name">Directory name.</param>
    private void CreateDirectoryOnly(string name)
    {
        Directory.CreateDirectory(Path.Combine(rootDirectory, name));
    }
}
