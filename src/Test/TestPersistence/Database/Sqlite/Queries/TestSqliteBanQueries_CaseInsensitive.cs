// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries;

/// <summary>
///     Verifies that the ban-related Sqlite queries match player names
///     case-insensitively (COLLATE NOCASE) and only for live player characters.
/// </summary>
[Collection("Sqlite")]
public class TestSqliteBanQueries_CaseInsensitive
{
    private readonly SqliteTestFixture fixture;

    public TestSqliteBanQueries_CaseInsensitive(SqliteTestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void TryGetAccountForPlayer_MatchesCaseInsensitively()
    {
        const ulong entityId = 99101;
        var accountId = Guid.NewGuid();
        fixture.AddAccount(accountId, "CaseAcct1", new byte[16], new byte[16]);
        fixture.AddEntity(entityId);
        fixture.AddPlayerCharacter(entityId);
        fixture.AddName(entityId, "CasePlayer");
        fixture.AddAccountComponent(entityId, accountId);

        var query = new SqliteGetAccountForPlayerNameQuery(fixture.Connection);
        Assert.True(query.TryGetAccountForPlayer("CasePlayer", out var exactAccountId));
        Assert.Equal(accountId, exactAccountId);

        Assert.True(query.TryGetAccountForPlayer("caseplayer", out var lowerAccountId));
        Assert.Equal(accountId, lowerAccountId);

        Assert.True(query.TryGetAccountForPlayer("CASEPLAYER", out var upperAccountId));
        Assert.Equal(accountId, upperAccountId);
    }

    [Fact]
    public void TryGetAccountForPlayer_NonPlayerEntity_ReturnsFalse()
    {
        const ulong entityId = 99102;
        fixture.AddEntity(entityId);
        fixture.AddName(entityId, "CaseNonPlayer");

        var query = new SqliteGetAccountForPlayerNameQuery(fixture.Connection);
        Assert.False(query.TryGetAccountForPlayer("CaseNonPlayer", out _));
    }

    [Fact]
    public void TryGetAccountForPlayer_SoftDeletedPlayer_ReturnsFalse()
    {
        const ulong entityId = 99103;
        var accountId = Guid.NewGuid();
        fixture.AddAccount(accountId, "CaseAcct2", new byte[16], new byte[16]);
        fixture.AddEntity(entityId);
        fixture.AddPlayerCharacter(entityId);
        fixture.AddName(entityId, "CaseDeletedPlayer");
        fixture.AddAccountComponent(entityId, accountId);
        SoftDeletePlayer(entityId);

        var query = new SqliteGetAccountForPlayerNameQuery(fixture.Connection);
        Assert.False(query.TryGetAccountForPlayer("CaseDeletedPlayer", out _));
    }

    [Fact]
    public void TryGetAccountForPlayer_PlayerWithoutAccount_ReturnsFalse()
    {
        const ulong entityId = 99104;
        fixture.AddEntity(entityId);
        fixture.AddPlayerCharacter(entityId);
        fixture.AddName(entityId, "CaseNoAccount");

        var query = new SqliteGetAccountForPlayerNameQuery(fixture.Connection);
        Assert.False(query.TryGetAccountForPlayer("CaseNoAccount", out _));
    }

    [Fact]
    public void TryGetAccountForPlayer_UnknownName_ReturnsFalse()
    {
        const ulong entityId = 99105;
        var accountId = Guid.NewGuid();
        fixture.AddAccount(accountId, "CaseAcct3", new byte[16], new byte[16]);
        fixture.AddEntity(entityId);
        fixture.AddPlayerCharacter(entityId);
        fixture.AddName(entityId, "CaseExisting");
        fixture.AddAccountComponent(entityId, accountId);

        var query = new SqliteGetAccountForPlayerNameQuery(fixture.Connection);
        Assert.False(query.TryGetAccountForPlayer("CaseNobody", out _));
    }

    /// <summary>
    ///     Marks the given player character as soft deleted.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void SoftDeletePlayer(ulong entityId)
    {
        const string sql = @"UPDATE Entity SET player_char_deleted = TRUE WHERE id = @Id";
        using var cmd = new SqliteCommand(sql, fixture.Connection);
        var pId = new SqliteParameter("Id", entityId);
        pId.SqliteType = SqliteType.Integer;
        cmd.Parameters.Add(pId);
        cmd.ExecuteNonQuery();
    }
}
