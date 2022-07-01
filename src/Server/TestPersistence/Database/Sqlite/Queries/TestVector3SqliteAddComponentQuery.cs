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
using System.Numerics;
using System.Text;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries
{

    [Collection("Sqlite")]
    public sealed class TestVector3SqliteAddComponentQuery
    {
        private readonly SqliteTestFixture testFixture;

        private const ulong baseEntityId = 0x7fff000000050000;

        public TestVector3SqliteAddComponentQuery(SqliteTestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        /// <summary>
        /// Tests adding a Vector3 component to an entity.
        /// </summary>
        [Fact]
        public void TestAdd()
        {
            /* Set up. */
            ulong entityId = baseEntityId;
            var position = Vector3.One;
            testFixture.AddEntity(entityId);

            /* Execute query. */
            var query = new Vector3SqliteAddComponentQuery("Position", testFixture.Connection);
            using (var transaction = testFixture.Connection.BeginTransaction())
            {
                query.Add(entityId, position, transaction);
                transaction.Commit();
            }

            /* Verify result. */
            var sql = @"SELECT x, y, z FROM Position WHERE id = @Id";
            using (var cmd = new SqliteCommand(sql, testFixture.Connection))
            {
                var param = new SqliteParameter("Id", entityId);
                param.SqliteType = SqliteType.Integer;
                cmd.Parameters.Add(param);

                using (var reader = cmd.ExecuteReader())
                {
                    int rowCount = 0;
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

}
