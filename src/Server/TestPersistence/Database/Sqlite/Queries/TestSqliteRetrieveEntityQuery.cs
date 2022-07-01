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
    /// Unit tests for SqliteRetrieveEntityQuery.
    /// </summary>
    [Collection("Sqlite")]
    public sealed class TestSqliteRetrieveEntityQuery
    {
        private readonly SqliteTestFixture fixture;

        private const ulong baseEntityId = 0x7fff000000010000;

        public TestSqliteRetrieveEntityQuery(SqliteTestFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Tests retrieving an entity that exists.
        /// </summary>
        [Theory]
        [InlineData(true, baseEntityId, 1)]
        [InlineData(false, baseEntityId + 1, 0)]
        public void TestRetrieve(bool insert, ulong testId, int expected)
        {
            /* Set up - create entity. */
            if (insert)
            {
                fixture.AddEntity(testId);
            }

            /* Attempt to retrieve entity. */
            var query = new SqliteRetrieveEntityQuery(fixture.Connection);
            using (var reader = query.RetrieveEntity(testId))
            {
                /* Check that the entity exists. */
                var count = 0;
                while (reader.Reader.Read())
                {
                    count++;
                }
                Assert.Equal(expected, count);
            }
        }

    }

}
