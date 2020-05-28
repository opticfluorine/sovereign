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
    /// Unit tests for SqliteAddEntityQuery.
    /// </summary>
    [Collection("Sqlite")]
    public sealed class TestSqliteAddEntityQuery
    {
        private readonly SqliteTestFixture testFixture;

        private const ulong baseEntityId = 0x7fff000000030000;

        public TestSqliteAddEntityQuery(SqliteTestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        /// <summary>
        /// Tests adding an entity to the database.
        /// </summary>
        [Fact]
        public void TestAdd()
        {
            /* Add the entity. */
            ulong entityId = baseEntityId;
            var query = new SqliteAddEntityQuery(testFixture.Connection);
            var transaction = testFixture.Connection.BeginTransaction();

            query.AddEntity(entityId, transaction);
            transaction.Commit();

            /* Verify that the entity is added. */
            const string sql = @"SELECT COUNT(*) FROM Entity WHERE id = @Id";
            using (var cmd = new SqliteCommand(sql, testFixture.Connection))
            {
                var param = new SqliteParameter("Id", entityId);
                cmd.Parameters.Add(param);

                var result = (ulong)(long)cmd.ExecuteScalar();
                Assert.Equal(1ul, result);
            }
        }

    }

}
