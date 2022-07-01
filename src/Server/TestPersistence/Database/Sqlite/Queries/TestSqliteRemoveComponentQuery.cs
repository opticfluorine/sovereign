/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
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
    /// Unit tests for SqliteRemoveComponentQuery.
    /// </summary>
    [Collection("Sqlite")]
    public sealed class TestSqliteRemoveComponentQuery
    {
        private readonly SqliteTestFixture testFixture;

        private const ulong baseEntityId = 0x7fff000000080000;

        public TestSqliteRemoveComponentQuery(SqliteTestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        /// <summary>
        /// Tests removing a component.
        /// </summary>
        [Fact]
        public void TestRemove()
        {
            /* Set up, two entities. */
            ulong entityId1 = baseEntityId;
            ulong entityId2 = entityId1 + 1;
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

                var count = (long)cmd.ExecuteScalar();
                Assert.Equal(0, count);
            }

            /* Confirm that the second entity's component was not removed. */
            using (var cmd = new SqliteCommand(sql, testFixture.Connection))
            {
                var param = new SqliteParameter("Id", entityId2);
                param.SqliteType = SqliteType.Integer;
                cmd.Parameters.Add(param);

                var count = (long)cmd.ExecuteScalar();
                Assert.Equal(1, count);
            }
        }

    }

}
