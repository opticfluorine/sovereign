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

using Sovereign.Persistence.Database.Sqlite.Queries;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries;

/// <summary>
///     Unit tests for SqliteRetrieveEntityQuery.
/// </summary>
[Collection("Sqlite")]
public sealed class TestSqliteRetrieveEntityQuery
{
    private const ulong baseEntityId = 0x7fff000000010000;
    private readonly SqliteTestFixture fixture;

    public TestSqliteRetrieveEntityQuery(SqliteTestFixture fixture)
    {
        this.fixture = fixture;
    }

    /// <summary>
    ///     Tests retrieving an entity that exists.
    /// </summary>
    [Theory]
    [InlineData(true, baseEntityId, 1)]
    [InlineData(false, baseEntityId + 1, 0)]
    public void TestRetrieve(bool insert, ulong testId, int expected)
    {
        /* Set up - create entity. */
        if (insert) fixture.AddEntity(testId);

        /* Attempt to retrieve entity. */
        var query = new SqliteRetrieveEntityQuery(fixture.Connection);
        using (var reader = query.RetrieveEntity(testId))
        {
            /* Check that the entity exists. */
            var count = 0;
            while (reader.Reader.Read()) count++;
            Assert.Equal(expected, count);
        }
    }
}