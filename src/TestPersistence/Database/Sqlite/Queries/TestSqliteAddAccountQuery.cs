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
