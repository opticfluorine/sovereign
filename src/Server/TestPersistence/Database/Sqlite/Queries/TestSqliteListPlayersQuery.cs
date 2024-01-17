// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries;

[Collection("Sqlite")]
public class TestSqliteListPlayersQuery
{
    private readonly SqliteTestFixture testFixture;

    public TestSqliteListPlayersQuery(SqliteTestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public void TestListPlayersQuery()
    {
        // Add two accounts.
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var username1 = "First";
        var username2 = "Second";
        var salt = new byte[16];
        var hash = new byte[16];

        testFixture.AddAccount(id1, username1, salt, hash);
        testFixture.AddAccount(id2, username2, salt, hash);

        // Add a player to each account.
        ulong firstPlayerId = 100;
        ulong secondPlayerId = 200;

        testFixture.AddEntity(firstPlayerId);
        testFixture.AddPlayerCharacter(firstPlayerId);
        testFixture.AddName(firstPlayerId, username1);
        testFixture.AddAccountComponent(firstPlayerId, id1);

        testFixture.AddEntity(secondPlayerId);
        testFixture.AddPlayerCharacter(secondPlayerId);
        testFixture.AddName(secondPlayerId, username2);
        testFixture.AddAccountComponent(secondPlayerId, id2);

        // List the players for the first account.
        // Verify that only the correct player is returned.
        var query = new SqliteListPlayersQuery(testFixture.Connection);
        var list1 = query.ListPlayersForAccount(id1);
        Assert.Single(list1);
        var p1 = list1[0];
        Assert.Equal(firstPlayerId, p1.Id);
        Assert.Equal(username1, p1.Name);

        // List the players for the second account.
        // Verify that only the correct player is returned.
        var list2 = query.ListPlayersForAccount(id2);
        Assert.Single(list2);
        var p2 = list2[0];
        Assert.Equal(secondPlayerId, p2.Id);
        Assert.Equal(username2, p2.Name);
    }
}