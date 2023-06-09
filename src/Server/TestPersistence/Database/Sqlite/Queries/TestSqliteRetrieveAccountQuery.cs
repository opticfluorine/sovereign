﻿/*
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

using Sovereign.Persistence.Database.Sqlite.Queries;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries
{

    /// <summary>
    /// Unit tests for SqliteRetrieveAccountQuery.
    /// </summary>
    [Collection("Sqlite")]
    public sealed class TestSqliteRetrieveAccountQuery
    {
        private readonly SqliteTestFixture fixture;

        public TestSqliteRetrieveAccountQuery(SqliteTestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData(true, "UserExists", 1)]
        [InlineData(false, "UserDoesNotExist", 0)]
        public void TestRetrieve(bool insert, string username, int expected)
        {
            /* Set up - add user if needed. */
            if (insert)
            {
                var id = Guid.NewGuid();
                var salt = new byte[16];
                var hash = new byte[16];
                fixture.AddAccount(id, username, salt, hash);
            }

            /* Query the account. */
            var query = new SqliteRetrieveAccountQuery(fixture.Connection);
            using (var reader = query.RetrieveAccount(username))
            {
                // Check that the correct number of accounts are returned.
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
