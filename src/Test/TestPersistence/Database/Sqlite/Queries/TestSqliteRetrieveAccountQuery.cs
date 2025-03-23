/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries;

/// <summary>
///     Unit tests for SqliteRetrieveAccountQuery.
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
            while (reader.Reader.Read()) count++;
            Assert.Equal(expected, count);
        }
    }
}