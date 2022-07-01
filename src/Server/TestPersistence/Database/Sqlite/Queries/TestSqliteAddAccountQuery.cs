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

using Sovereign.Persistence.Database.Sqlite.Queries;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries
{

    /// <summary>
    /// Unit tests for SqliteAddAccountQuery.
    /// </summary>
    [Collection("Sqlite")]
    public sealed class TestSqliteAddAccountQuery
    {
        private readonly SqliteTestFixture fixture;

        public TestSqliteAddAccountQuery(SqliteTestFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Tests adding accounts.
        /// </summary>
        [Fact]
        public void TestAddAccount()
        {
            // Add an account.
            // This should be accepted.
            Guid id = Guid.NewGuid();
            string username = "TestUser";
            byte[] salt = new byte[16];
            byte[] hash = new byte[16];
            ulong opslimit = 0;
            ulong memlimit = 0;

            var query = new SqliteAddAccountQuery(fixture.Connection);
            var result = query.AddAccount(id, username, salt, hash, opslimit, memlimit);
            Assert.True(result);

            // Now switch the ID and try to reuse the same username.
            // This should be rejected.
            Guid id2 = Guid.NewGuid();
            var result2 = query.AddAccount(id2, username, salt, hash, opslimit, memlimit);
            Assert.False(result2);
        }

    }

}
