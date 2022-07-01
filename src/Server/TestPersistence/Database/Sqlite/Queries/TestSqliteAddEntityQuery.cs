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
