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
///     Integration tests for the typed key-value migration (0003).
/// </summary>
public sealed class TestTypedKeyValueMigration
{
    private const string MigrationsDirectory = @"../../../../../Server/Persistence/Migrations";

    private const string BaselineScript =
        MigrationsDirectory + "/0001-Baseline/0001-Baseline_sqlite.sql";

    private const string SegmentFlagsScript =
        MigrationsDirectory + "/0002-SegmentFlags/0002-SegmentFlags_sqlite.sql";

    private const string FullScript = MigrationsDirectory + "/Full/Full_sqlite.sql";

    /// <summary>
    ///     Tests that migration 0003 retypes legacy keys and defaults everything else to String.
    /// </summary>
    [Fact]
    public void UpgradeFromLevelTwo_RetypesLegacyRows()
    {
        using var connection = OpenInMemory();
        Execute(connection, File.ReadAllText(BaselineScript));
        Execute(connection, File.ReadAllText(SegmentFlagsScript));

        // Legacy rows written before the type column existed.
        Execute(connection, "INSERT INTO GlobalKeyValue (key, value) VALUES ('__Sovereign.Clock', '3600')");
        Execute(connection, "INSERT INTO GlobalKeyValue (key, value) VALUES ('userKey', 'userValue')");

        var query = new SqliteMigrationQuery(connection);
        Assert.Equal(2, query.GetLatestMigrationLevel());

        var runner = new MigrationRunner(connection, query, new MigrationScriptLocator(MigrationsDirectory),
            NullLogger.Instance);
        runner.ApplyMigrations(3);

        Assert.Equal(3, query.GetLatestMigrationLevel());

        /* Float config keys. */
        Assert.Equal(3, ReadType(connection, "__Sovereign.Config.MinimumZ"));
        Assert.Equal(3, ReadType(connection, "__Sovereign.Config.MaximumZ"));

        /* Clock retyped to Int. */
        Assert.Equal(1, ReadType(connection, "__Sovereign.Clock"));

        /* Unknown rows default to String. */
        Assert.Equal(0, ReadType(connection, "userKey"));

        /* Entity key-value rows also gain a String-typed column by default. */
        Execute(connection, "INSERT INTO Entity (id) VALUES (1)");
        Execute(connection, "INSERT INTO EntityKeyValue (entity_id, key, value) VALUES (1, 'k', 'v')");
        Assert.Equal(0, ReadEntityType(connection, 1, "k"));
    }

    /// <summary>
    ///     Tests that a fresh database built from the full schema comes up at the latest level with
    ///     typed seeds.
    /// </summary>
    [Fact]
    public void FreshFullSchema_IsAtLatestLevelWithTypedSeeds()
    {
        using var connection = OpenInMemory();
        Execute(connection, File.ReadAllText(FullScript));

        var query = new SqliteMigrationQuery(connection);
        Assert.Equal(4, query.GetLatestMigrationLevel());

        Assert.Equal(3, ReadType(connection, "__Sovereign.Config.MinimumZ"));
        Assert.Equal(3, ReadType(connection, "__Sovereign.Config.MaximumZ"));
        Assert.Equal(1L, CountMigrationLog(connection, 3, "TypedKeyValue"));

        /* Applying migrations again is a no-op. */
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
    ///     Reads the type tag for a global key.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    /// <param name="key">Global key.</param>
    /// <returns>Type tag.</returns>
    private static long ReadType(SqliteConnection connection, string key)
    {
        using var cmd = new SqliteCommand("SELECT type FROM GlobalKeyValue WHERE key = @Key", connection);
        cmd.Parameters.AddWithValue("@Key", key);
        return (long)cmd.ExecuteScalar()!;
    }

    /// <summary>
    ///     Reads the type tag for an entity key.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <returns>Type tag.</returns>
    private static long ReadEntityType(SqliteConnection connection, ulong entityId, string key)
    {
        using var cmd = new SqliteCommand("SELECT type FROM EntityKeyValue WHERE entity_id = @Id AND key = @Key",
            connection);
        cmd.Parameters.AddWithValue("@Id", entityId);
        cmd.Parameters.AddWithValue("@Key", key);
        return (long)cmd.ExecuteScalar()!;
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
