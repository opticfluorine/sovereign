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
///     Unit tests for SqliteRemoveComponentQuery.
/// </summary>
[Collection("Sqlite")]
public sealed class TestSqliteRemoveComponentQuery
{
    private const ulong baseEntityId = 0x7fff000000080000;
    private readonly SqliteTestFixture testFixture;

    public TestSqliteRemoveComponentQuery(SqliteTestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    /// <summary>
    ///     Tests removing a component.
    /// </summary>
    [Fact]
    public void TestRemove()
    {
        /* Set up, two entities. */
        var entityId1 = baseEntityId;
        var entityId2 = entityId1 + 1;
        testFixture.AddEntity(entityId1);
        testFixture.AddEntity(entityId2);
        testFixture.AddMaterial(entityId1, 1);
        testFixture.AddMaterial(entityId2, 1);

        /* Remove the material from the first entity. */
        var query = new SqliteRemoveComponentQuery("Material", testFixture.Connection);
        using (var transaction = testFixture.Connection.BeginTransaction())
        {
            query.Remove(entityId1, transaction);
            transaction.Commit();
        }

        /* Confirm that the first entity's component was removed. */
        var sql = @"SELECT COUNT(*) FROM Material WHERE id = @Id";
        using (var cmd = new SqliteCommand(sql, testFixture.Connection))
        {
            var param = new SqliteParameter("Id", entityId1);
            param.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(param);

            var scalar = cmd.ExecuteScalar();
            Assert.NotNull(scalar);
            var count = (long)scalar;
            Assert.Equal(0, count);
        }

        /* Confirm that the second entity's component was not removed. */
        using (var cmd = new SqliteCommand(sql, testFixture.Connection))
        {
            var param = new SqliteParameter("Id", entityId2);
            param.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(param);

            var scalar = cmd.ExecuteScalar();
            Assert.NotNull(scalar);
            var count = (long)scalar;
            Assert.Equal(1, count);
        }
    }
}