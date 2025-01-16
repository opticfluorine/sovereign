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
///     Unit tests for SimpleSqliteModifyComponentQuery.
/// </summary>
[Collection("Sqlite")]
public sealed class TestSimpleSqliteModifyComponentQuery
{
    private const ulong baseEntityId = 0x7fff000000060000;
    private readonly SqliteTestFixture testFixture;

    public TestSimpleSqliteModifyComponentQuery(SqliteTestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    /// <summary>
    ///     Tests that a component can be modified.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="testValue">Value of the component to add.</param>
    /// <param name="original">Original value.</param>
    [Theory]
    [InlineData(baseEntityId + 0, int.MinValue, 0)]
    [InlineData(baseEntityId + 1, int.MaxValue, 0)]
    public void TestModify(ulong entityId, int testValue, int original)
    {
        /* Set up. */
        testFixture.AddEntity(entityId);
        testFixture.AddMaterial(entityId, original);

        /* Execute query. */
        var query = new SimpleSqliteModifyComponentQuery<int>("material",
            SqliteType.Integer, testFixture.Connection);
        using (var transaction = testFixture.Connection.BeginTransaction())
        {
            query.Modify(entityId, testValue, transaction);
            transaction.Commit();
        }

        /* Check that the component was added. */
        var sql = "SELECT COUNT(*) FROM Entity WHERE id = @Id AND material = @Mat";
        using (var cmd = new SqliteCommand(sql, testFixture.Connection))
        {
            var paramId = new SqliteParameter("Id", entityId);
            paramId.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(paramId);

            var paramMat = new SqliteParameter("Mat", testValue);
            paramMat.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(paramMat);

            var scalar = cmd.ExecuteScalar();
            Assert.NotNull(scalar);
            var result = (long)scalar;
            Assert.Equal(1, result);
        }
    }
}