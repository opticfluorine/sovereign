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

using System.Numerics;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries;

[Collection("Sqlite")]
public sealed class TestVector3SqliteAddComponentQuery
{
    private const ulong baseEntityId = 0x7fff000000050000;
    private readonly SqliteTestFixture testFixture;

    public TestVector3SqliteAddComponentQuery(SqliteTestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    /// <summary>
    ///     Tests adding a Vector3 component to an entity.
    /// </summary>
    [Fact]
    public void TestAdd()
    {
        /* Set up. */
        var entityId = baseEntityId;
        var position = Vector3.One;
        testFixture.AddEntity(entityId);

        /* Execute query. */
        var query = new Vector3SqliteAddComponentQuery("pos_", testFixture.Connection);
        using (var transaction = testFixture.Connection.BeginTransaction())
        {
            query.Add(entityId, position, transaction);
            transaction.Commit();
        }

        /* Verify result. */
        var sql = @"SELECT pos_x, pos_y, pos_z FROM Entity WHERE id = @Id";
        using (var cmd = new SqliteCommand(sql, testFixture.Connection))
        {
            var param = new SqliteParameter("Id", entityId);
            param.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(param);

            using (var reader = cmd.ExecuteReader())
            {
                var rowCount = 0;
                while (reader.Read())
                {
                    rowCount++;
                    var x = reader.GetFloat(0);
                    var y = reader.GetFloat(1);
                    var z = reader.GetFloat(2);
                    Assert.Equal(position.X, x, 7);
                    Assert.Equal(position.Y, y, 7);
                    Assert.Equal(position.Z, z, 7);
                }

                Assert.Equal(1, rowCount);
            }
        }
    }
}