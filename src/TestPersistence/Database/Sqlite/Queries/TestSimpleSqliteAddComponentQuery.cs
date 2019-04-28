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

    [Collection("Sqlite")]
    public sealed class TestSimpleSqliteAddComponentQuery
    {
        private readonly SqliteTestFixture testFixture;

        private const ulong baseEntityId = 0x7fff000000040000;

        public TestSimpleSqliteAddComponentQuery(SqliteTestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        /// <summary>
        /// Tests that a component can be added.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="testValue">Value of the component to add.</param>
        [Theory]
        [InlineData(baseEntityId + 0, int.MinValue)]
        [InlineData(baseEntityId + 1, int.MaxValue)]
        public void TestAdd(ulong entityId, int testValue)
        {
            /* Set up. */
            testFixture.AddEntity(entityId);

            /* Execute query. */
            var query = new SimpleSqliteAddComponentQuery<int>("Material", "material",
                SqliteType.Integer, testFixture.Connection);
            using (var transaction = testFixture.Connection.BeginTransaction())
            {
                query.Add(entityId, testValue, transaction);
                transaction.Commit();
            }

            /* Check that the component was added. */
            var sql = "SELECT COUNT(*) FROM Material WHERE id = @Id AND material = @Mat";
            using (var cmd = new SqliteCommand(sql, testFixture.Connection))
            {
                var paramId = new SqliteParameter("Id", entityId);
                paramId.SqliteType = SqliteType.Integer;
                cmd.Parameters.Add(paramId);

                var paramMat = new SqliteParameter("Mat", testValue);
                paramMat.SqliteType = SqliteType.Integer;
                cmd.Parameters.Add(paramMat);

                var result = (long)cmd.ExecuteScalar();
                Assert.Equal(1, result);
            }
        }

    }

}
