/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries;

/// <summary>
///     Unit tests for SqliteMigrationQuery.
/// </summary>
[Collection("Sqlite")]
public sealed class TestSqliteMigrationQuery
{
    private readonly SqliteTestFixture testFixture;

    public TestSqliteMigrationQuery(SqliteTestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    /// <summary>
    ///     Tests the function of the migration query.
    /// </summary>
    [Fact]
    public void TestMigrationQuery()
    {
        /* Set the migration level. */
        const int level = 32768;
        testFixture.AddMigrationLevel(level);

        /* Execute the query. */
        var query = new SqliteMigrationQuery(testFixture.Connection);
        Assert.True(query.IsMigrationLevelApplied(level));
        Assert.False(query.IsMigrationLevelApplied(level + 1));
    }

    /// <summary>
    ///     Tests that the migration level check is an equality check, not a greater-than-or-equal check.
    /// </summary>
    [Fact]
    public void IsMigrationLevelAppliedIsEquality()
    {
        using var connection = CreateConnectionWithMigrationLog();
        Execute(connection, "INSERT INTO MigrationLog (id, name) VALUES (1, 'one')");
        Execute(connection, "INSERT INTO MigrationLog (id, name) VALUES (2, 'two')");

        var query = new SqliteMigrationQuery(connection);
        Assert.True(query.IsMigrationLevelApplied(2));
        Assert.False(query.IsMigrationLevelApplied(1));
        Assert.False(query.IsMigrationLevelApplied(3));
    }

    /// <summary>
    ///     Tests that the latest migration level is 0 for an empty log.
    /// </summary>
    [Fact]
    public void GetLatestMigrationLevelEmpty()
    {
        using var connection = CreateConnectionWithMigrationLog();
        var query = new SqliteMigrationQuery(connection);

        Assert.Equal(0, query.GetLatestMigrationLevel());
    }

    /// <summary>
    ///     Tests that the latest migration level is the maximum logged level.
    /// </summary>
    [Fact]
    public void GetLatestMigrationLevelPopulated()
    {
        using var connection = CreateConnectionWithMigrationLog();
        Execute(connection, "INSERT INTO MigrationLog (id, name) VALUES (1, 'one')");
        Execute(connection, "INSERT INTO MigrationLog (id, name) VALUES (5, 'five')");
        Execute(connection, "INSERT INTO MigrationLog (id, name) VALUES (3, 'three')");

        var query = new SqliteMigrationQuery(connection);

        Assert.Equal(5, query.GetLatestMigrationLevel());
    }

    /// <summary>
    ///     Creates a fresh in-memory database containing only the MigrationLog table.
    /// </summary>
    /// <returns>Open connection.</returns>
    private static SqliteConnection CreateConnectionWithMigrationLog()
    {
        var builder = new SqliteConnectionStringBuilder { Mode = SqliteOpenMode.Memory };
        var connection = new SqliteConnection(builder.ConnectionString);
        connection.Open();

        Execute(connection, "CREATE TABLE MigrationLog (id INTEGER PRIMARY KEY, name VARCHAR(255) NOT NULL)");

        return connection;
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
