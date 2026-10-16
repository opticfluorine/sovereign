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
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Xunit;

namespace TestPersistence.Database;

/// <summary>
///     Integration tests for the world segment key-value migration (0004).
/// </summary>
public sealed class TestSegmentKeyValueMigration
{
    private const string MigrationsDirectory = @"../../../../../Server/Persistence/Migrations";

    private const string BaselineScript =
        MigrationsDirectory + "/0001-Baseline/0001-Baseline_sqlite.sql";

    private const string SegmentFlagsScript =
        MigrationsDirectory + "/0002-SegmentFlags/0002-SegmentFlags_sqlite.sql";

    private const string TypedKeyValueScript =
        MigrationsDirectory + "/0003-TypedKeyValue/0003-TypedKeyValue_sqlite.sql";

    private const string FullScript = MigrationsDirectory + "/Full/Full_sqlite.sql";

    /// <summary>
    ///     Tests that migration 0004 adds the WorldSegmentKeyValue table to a level-3 database.
    /// </summary>
    [Fact]
    public void UpgradeFromLevelThree_AddsSegmentKeyValueTable()
    {
        using var connection = OpenInMemory();
        Execute(connection, File.ReadAllText(BaselineScript));
        Execute(connection, File.ReadAllText(SegmentFlagsScript));
        Execute(connection, File.ReadAllText(TypedKeyValueScript));

        var query = new SqliteMigrationQuery(connection);
        Assert.Equal(3, query.GetLatestMigrationLevel());

        var runner = new MigrationRunner(connection, query, new MigrationScriptLocator(MigrationsDirectory),
            NullLogger.Instance);
        runner.ApplyMigrations(4);

        Assert.Equal(4, query.GetLatestMigrationLevel());
        Assert.Equal(1L, CountMigrationLog(connection, 4, "SegmentKeyValues"));

        // The new table exists and accepts a typed row.
        Execute(connection,
            "INSERT INTO WorldSegmentKeyValue (x, y, z, key, value, type) VALUES (1, 2, 3, 'Owner', '5', 5)");
        Assert.Equal("5", ReadSegmentValue(connection, 1, 2, 3, "Owner"));
    }

    /// <summary>
    ///     Tests that a fresh database built from the full schema comes up at level 4.
    /// </summary>
    [Fact]
    public void FreshFullSchema_IsAtLevelFour()
    {
        using var connection = OpenInMemory();
        Execute(connection, File.ReadAllText(FullScript));

        var query = new SqliteMigrationQuery(connection);
        Assert.Equal(4, query.GetLatestMigrationLevel());
        Assert.Equal(1L, CountMigrationLog(connection, 4, "SegmentKeyValues"));

        // Applying migrations again is a no-op.
        var runner = new MigrationRunner(connection, query, new MigrationScriptLocator(MigrationsDirectory),
            NullLogger.Instance);
        runner.ApplyMigrations(4);
        Assert.Equal(4, CountMigrationLogRows(connection));
    }

    /// <summary>
    ///     Opens a fresh in-memory SQLite connection.
    /// </summary>
    /// <returns>Open connection.</returns>
    private static SqliteConnection OpenInMemory()
    {
        var builder = new SqliteConnectionStringBuilder { Mode = SqliteOpenMode.Memory };
        var connection = new SqliteConnection(builder.ConnectionString);
        connection.Open();
        return connection;
    }

    /// <summary>
    ///     Reads a world segment key-value pair.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    /// <param name="x">Segment x index.</param>
    /// <param name="y">Segment y index.</param>
    /// <param name="z">Segment z index.</param>
    /// <param name="key">Key.</param>
    /// <returns>Stored value text.</returns>
    private static string? ReadSegmentValue(SqliteConnection connection, int x, int y, int z, string key)
    {
        using var cmd = new SqliteCommand(
            "SELECT value FROM WorldSegmentKeyValue WHERE x = @X AND y = @Y AND z = @Z AND key = @Key",
            connection);
        cmd.Parameters.AddWithValue("@X", x);
        cmd.Parameters.AddWithValue("@Y", y);
        cmd.Parameters.AddWithValue("@Z", z);
        cmd.Parameters.AddWithValue("@Key", key);
        return cmd.ExecuteScalar() as string;
    }

    /// <summary>
    ///     Counts migration log rows matching the given id and name.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    /// <param name="id">Migration ID.</param>
    /// <param name="name">Migration name.</param>
    /// <returns>Number of matching rows.</returns>
    private static long CountMigrationLog(SqliteConnection connection, int id, string name)
    {
        using var cmd = new SqliteCommand(
            "SELECT COUNT(*) FROM MigrationLog WHERE id = @Id AND name = @Name", connection);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@Name", name);
        return (long)cmd.ExecuteScalar()!;
    }

    /// <summary>
    ///     Counts all migration log rows.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    /// <returns>Number of migration log rows.</returns>
    private static long CountMigrationLogRows(SqliteConnection connection)
    {
        using var cmd = new SqliteCommand("SELECT COUNT(*) FROM MigrationLog", connection);
        return (long)cmd.ExecuteScalar()!;
    }

    /// <summary>
    ///     Executes a non-query command.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    /// <param name="sql">SQL to execute.</param>
    private static void Execute(SqliteConnection connection, string sql)
    {
        using var cmd = new SqliteCommand(sql, connection);
        cmd.ExecuteNonQuery();
    }
}
