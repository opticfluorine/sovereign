/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Sqlite.Queries;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries
{

    /// <summary>
    /// Unit tests for SqliteRemoveEntityQuery.
    /// </summary>
    [Collection("Sqlite")]
    public sealed class TestSqliteRemoveEntityQuery
    {
        private readonly SqliteTestFixture testFixture;

        /// <summary>
        /// Base entity ID for the tests.
        /// </summary>
        private const ulong baseEntityId = 0x7fff000000090000;

        public TestSqliteRemoveEntityQuery(SqliteTestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        /// <summary>
        /// Tests removing an entity ID from the database.
        /// </summary>
        [Fact]
        public void TestRemoveEntityId()
        {
            /* Add entity to the database. */
            var entityId = baseEntityId;
            testFixture.AddEntity(entityId);

            /* Remove the entity. */
            var query = new SqliteRemoveEntityQuery(testFixture.Connection);
            using (var transaction = testFixture.Connection.BeginTransaction())
            {
                query.RemoveEntityId(entityId, transaction);
                transaction.Commit();
            }

            /* Verify that the entity was removed. */
            var sql = @"SELECT COUNT(*) FROM Entity WHERE id = @Id";
            using (var cmd = new SqliteCommand(sql, testFixture.Connection))
            {
                var param = new SqliteParameter("Id", entityId);
                param.SqliteType = SqliteType.Integer;
                cmd.Parameters.Add(param);

                var count = (long)cmd.ExecuteScalar();
                Assert.Equal(0, count);
            }
        }

    }

}
