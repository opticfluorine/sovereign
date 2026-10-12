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
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Sovereign.EngineCore.Main;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Xunit;

namespace TestPersistence.Database;

/// <summary>
///     Integration tests for the incremental migration runner.
/// </summary>
public sealed class TestMigrationRunner : IDisposable
{
    /// <summary>
    ///     Path to the real migrations directory relative to the test working directory.
    /// </summary>
    private const string MigrationsDirectory = @"../../../../../Server/Persistence/Migrations";

    /// <summary>
    ///     Path to the baseline migration script relative to the test working directory.
    /// </summary>
    private const string BaselineScript = MigrationsDirectory + "/0001-Baseline/0001-Baseline_sqlite.sql";

    private readonly string scratchDirectory;

    public TestMigrationRunner()
    {
        scratchDirectory = Path.Combine(Path.GetTempPath(), $"migration-runner-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(scratchDirectory);
    }

    public void Dispose()
    {
        Directory.Delete(scratchDirectory, true);
    }

    /// <summary>
    ///     Tests that a level 1 database is upgraded to level 2 and that a second run is a no-op.
    /// </summary>
    [Fact]
    public void UpgradesFromLevelOneAndIsIdempotent()
    {
        using var connection = OpenBaselineConnection();
        var query = new SqliteMigrationQuery(connection);
        Assert.Equal(1, query.GetLatestMigrationLevel());
        Assert.False(TableExists(connection, "WorldSegmentFlag"));

        var runner = CreateRunner(connection, query, MigrationsDirectory);
        runner.ApplyMigrations(2);

        Assert.Equal(2, query.GetLatestMigrationLevel());
        Assert.True(TableExists(connection, "WorldSegmentFlag"));
        Assert.Equal(1, CountMigrationLog(connection, 1, "Baseline"));
        Assert.Equal(1, CountMigrationLog(connection, 2, "SegmentFlags"));

        /* A second apply call must be a no-op (the level guard prevents double application). */
        runner.ApplyMigrations(2);

        Assert.Equal(2, query.GetLatestMigrationLevel());
        Assert.Equal(2, CountMigrationLogRows(connection));
    }

    /// <summary>
    ///     Tests that a failing script rolls back its transaction.
    /// </summary>
    [Fact]
    public void FailingScriptRollsBack()
    {
        using var connection = OpenBaselineConnection();
        var query = new SqliteMigrationQuery(connection);

        var brokenDirectory = Path.Combine(scratchDirectory, "0002-Broken");
        Directory.CreateDirectory(brokenDirectory);
        File.WriteAllText(Path.Combine(brokenDirectory, "0002-Broken_sqlite.sql"), """
            CREATE TABLE WorldSegmentFlag
            (
                x     INTEGER NOT NULL,
                y     INTEGER NOT NULL,
                z     INTEGER NOT NULL,
                flags INTEGER NOT NULL,
                PRIMARY KEY (x, y, z)
            );

            THIS IS NOT VALID SQL;
            """);

        var runner = CreateRunner(connection, query, scratchDirectory);

        Assert.Throws<FatalErrorException>(() => runner.ApplyMigrations(2));

        Assert.Equal(1, query.GetLatestMigrationLevel());
        Assert.False(TableExists(connection, "WorldSegmentFlag"));
        Assert.Equal(1, CountMigrationLogRows(connection));
    }

    /// <summary>
    ///     Opens an in-memory database initialized from the baseline migration only.
    /// </summary>
    /// <returns>Open connection at migration level 1.</returns>
    private static SqliteConnection OpenBaselineConnection()
    {
        var builder = new SqliteConnectionStringBuilder { Mode = SqliteOpenMode.Memory };
        var connection = new SqliteConnection(builder.ConnectionString);
        connection.Open();

        Execute(connection, File.ReadAllText(BaselineScript));

        return connection;
    }

    /// <summary>
    ///     Creates a migration runner for the given connection and migrations directory.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    /// <param name="query">Migration query.</param>
    /// <param name="migrationsDirectory">Directory containing migration subdirectories.</param>
    /// <returns>Migration runner.</returns>
    private static MigrationRunner CreateRunner(SqliteConnection connection, SqliteMigrationQuery query,
        string migrationsDirectory)
    {
        return new MigrationRunner(connection, query, new MigrationScriptLocator(migrationsDirectory),
            NullLogger.Instance);
    }

    /// <summary>
    ///     Determines whether a table exists in the database.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    /// <param name="name">Table name.</param>
    /// <returns>true if the table exists.</returns>
    private static bool TableExists(SqliteConnection connection, string name)
    {
        using var command = new SqliteCommand(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @Name", connection);
        command.Parameters.AddWithValue("@Name", name);
        return (long)command.ExecuteScalar()! > 0;
    }

    /// <summary>
    ///     Counts the rows in the migration log.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    /// <returns>Number of migration log rows.</returns>
    private static long CountMigrationLogRows(SqliteConnection connection)
    {
        using var command = new SqliteCommand("SELECT COUNT(*) FROM MigrationLog", connection);
        return (long)command.ExecuteScalar()!;
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
        using var command = new SqliteCommand(
            "SELECT COUNT(*) FROM MigrationLog WHERE id = @Id AND name = @Name", connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Name", name);
        return (long)command.ExecuteScalar()!;
    }

    /// <summary>
    ///     Executes a non-query command.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    /// <param name="sql">SQL to execute.</param>
    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
    }
}
