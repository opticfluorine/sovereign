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

using Microsoft.Data.Sqlite;
using Xunit;

namespace TestPersistence.Database.Sqlite;

/// <summary>
///     Tests for the full schema produced by Full_sqlite.sql.
/// </summary>
[Collection("Sqlite")]
public sealed class TestFullSchema
{
    private readonly SqliteTestFixture testFixture;

    public TestFullSchema(SqliteTestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    /// <summary>
    ///     Tests that the WorldSegmentFlag table exists and accepts a row.
    /// </summary>
    [Fact]
    public void WorldSegmentFlagAcceptsRow()
    {
        using var insert = new SqliteCommand(
            "INSERT INTO WorldSegmentFlag (x, y, z, flags) VALUES (1, 2, 3, 5)", testFixture.Connection);
        insert.ExecuteNonQuery();

        using var select = new SqliteCommand(
            "SELECT flags FROM WorldSegmentFlag WHERE x = 1 AND y = 2 AND z = 3", testFixture.Connection);
        Assert.Equal(5L, select.ExecuteScalar());
    }

    /// <summary>
    ///     Tests that the migration log records both the baseline and the segment flags migration.
    /// </summary>
    [Fact]
    public void MigrationLogContainsBaselineAndSegmentFlags()
    {
        Assert.Equal(1L, CountMigrationLog(1, "Baseline"));
        Assert.Equal(1L, CountMigrationLog(2, "SegmentFlags"));
    }

    /// <summary>
    ///     Tests that the default config constants include MaximumZ and leave MinimumZ unchanged.
    /// </summary>
    [Fact]
    public void GlobalKeyValueContainsMaximumZ()
    {
        Assert.Equal("1024", GetGlobalKeyValue("__Sovereign.Config.MaximumZ"));
        Assert.Equal("-1024", GetGlobalKeyValue("__Sovereign.Config.MinimumZ"));
    }

    /// <summary>
    ///     Counts rows in MigrationLog matching the given id and name.
    /// </summary>
    /// <param name="id">Migration ID.</param>
    /// <param name="name">Migration name.</param>
    /// <returns>Number of matching rows.</returns>
    private long CountMigrationLog(int id, string name)
    {
        using var cmd = new SqliteCommand("SELECT COUNT(*) FROM MigrationLog WHERE id = @Id AND name = @Name",
            testFixture.Connection);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@Name", name);
        return (long)cmd.ExecuteScalar()!;
    }

    /// <summary>
    ///     Reads a global key-value pair.
    /// </summary>
    /// <param name="key">Key to read.</param>
    /// <returns>Value associated with the key, or null if absent.</returns>
    private string? GetGlobalKeyValue(string key)
    {
        using var cmd = new SqliteCommand("SELECT value FROM GlobalKeyValue WHERE key = @Key",
            testFixture.Connection);
        cmd.Parameters.AddWithValue("@Key", key);
        return cmd.ExecuteScalar() as string;
    }
}
