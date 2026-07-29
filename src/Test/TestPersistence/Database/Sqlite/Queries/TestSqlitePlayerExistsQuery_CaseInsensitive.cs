// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using Sovereign.Persistence.Database.Sqlite.Queries;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries;

/// <summary>
///     Verifies that SqlitePlayerExistsQuery matches player names case-insensitively.
/// </summary>
[Collection("Sqlite")]
public class TestSqlitePlayerExistsQuery_CaseInsensitive
{
    private readonly SqliteTestFixture fixture;

    public TestSqlitePlayerExistsQuery_CaseInsensitive(SqliteTestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void PlayerExists_MatchesCaseInsensitively()
    {
        const ulong entityId = 99001;
        fixture.AddEntity(entityId);
        fixture.AddPlayerCharacter(entityId);
        fixture.AddName(entityId, "Alice");

        var query = new SqlitePlayerExistsQuery(fixture.Connection);
        Assert.True(query.PlayerExists("alice"));
        Assert.True(query.PlayerExists("ALICE"));
        Assert.True(query.PlayerExists("Alice"));
        Assert.False(query.PlayerExists("bob"));
    }
}
