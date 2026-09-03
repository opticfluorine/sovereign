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
///     Tests for the ban-related Sqlite queries.
/// </summary>
[Collection("Sqlite")]
public class TestSqliteBanQueries
{
    private readonly SqliteTestFixture fixture;

    public TestSqliteBanQueries(SqliteTestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void AddBan_GetActiveBansForAccount_RoundTrip()
    {
        var accountId = Guid.NewGuid();
        fixture.AddAccount(accountId, "BanRoundTrip", new byte[16], new byte[16]);

        const string playerName = "RoundTripPlayer";
        const string adminName = "RoundTripAdmin";
        var created = TruncateToSeconds(DateTime.UtcNow);
        const int durationDays = 7;

        var addQuery = new SqliteAddBanQuery(fixture.Connection);
        addQuery.AddBan(accountId, playerName, adminName, created, durationDays);

        var getQuery = new SqliteGetActiveBansForAccountQuery(fixture.Connection);
        var bans = getQuery.GetActiveBansForAccount(accountId);

        var ban = Assert.Single(bans);
        Assert.Equal(accountId, ban.AccountId);
        Assert.Equal("BanRoundTrip", ban.Username);
        Assert.Equal(playerName, ban.PlayerName);
        Assert.Equal(adminName, ban.AdminName);
        Assert.Equal(created, ban.CreatedUtc);
        Assert.Equal(durationDays, ban.DurationDays);
    }

    [Fact]
    public void AddBan_PermanentBan_NeverExpires()
    {
        var accountId = Guid.NewGuid();
        fixture.AddAccount(accountId, "BanPermanent", new byte[16], new byte[16]);

        var created = TruncateToSeconds(DateTime.UtcNow.AddDays(-30));
        var addQuery = new SqliteAddBanQuery(fixture.Connection);
        addQuery.AddBan(accountId, "PermanentPlayer", "AdminUser", created, null);

        var getQuery = new SqliteGetActiveBansForAccountQuery(fixture.Connection);
        var bans = getQuery.GetActiveBansForAccount(accountId);

        var ban = Assert.Single(bans);
        Assert.Null(ban.DurationDays);
        Assert.Equal(created, ban.CreatedUtc);
    }

    [Fact]
    public void GetActiveBansForAccount_ExpiredTimedBan_IsSoftDeleted()
    {
        var accountId = Guid.NewGuid();
        fixture.AddAccount(accountId, "BanExpired", new byte[16], new byte[16]);

        var created = TruncateToSeconds(DateTime.UtcNow.AddDays(-2));
        var addQuery = new SqliteAddBanQuery(fixture.Connection);
        addQuery.AddBan(accountId, "ExpiredPlayer", "AdminUser", created, 1);

        // The raw query reports the not-yet-purged ban.
        var getQuery = new SqliteGetActiveBansForAccountQuery(fixture.Connection);
        Assert.Single(getQuery.GetActiveBansForAccount(accountId));

        // Soft deleting expired bans (performed by PersistenceBanServices on
        // each lookup) removes it, and subsequent lookups stay empty.
        var purgeQuery = new SqliteSoftDeleteExpiredBansQuery(fixture.Connection);
        purgeQuery.SoftDeleteExpiredBans();

        Assert.Empty(getQuery.GetActiveBansForAccount(accountId));
        Assert.True(IsBanDeleted(accountId, "ExpiredPlayer"));

        // Subsequent lookups stay empty.
        Assert.Empty(getQuery.GetActiveBansForAccount(accountId));
    }

    [Fact]
    public void RemoveBansForAccount_RemovesOnlyTargetAccount()
    {
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        fixture.AddAccount(accountId1, "BanRemove1", new byte[16], new byte[16]);
        fixture.AddAccount(accountId2, "BanRemove2", new byte[16], new byte[16]);

        var addQuery = new SqliteAddBanQuery(fixture.Connection);
        addQuery.AddBan(accountId1, "RemovePlayer1", "AdminUser", DateTime.UtcNow, null);
        addQuery.AddBan(accountId1, "RemovePlayer2", "AdminUser", DateTime.UtcNow, 3);
        addQuery.AddBan(accountId2, "RemovePlayer3", "AdminUser", DateTime.UtcNow, null);

        var removeQuery = new SqliteRemoveBansForAccountQuery(fixture.Connection);
        Assert.Equal(2, removeQuery.RemoveBansForAccount(accountId1));

        var getQuery = new SqliteGetActiveBansForAccountQuery(fixture.Connection);
        Assert.Empty(getQuery.GetActiveBansForAccount(accountId1));
        var remaining = getQuery.GetActiveBansForAccount(accountId2);
        Assert.Equal("RemovePlayer3", Assert.Single(remaining).PlayerName);
    }

    [Fact]
    public void SoftDeleteExpiredBans_PurgesOnlyExpiredActiveBans()
    {
        var accountId = Guid.NewGuid();
        fixture.AddAccount(accountId, "BanPurge", new byte[16], new byte[16]);

        var addQuery = new SqliteAddBanQuery(fixture.Connection);
        // Expired timed ban: purged.
        addQuery.AddBan(accountId, "PurgeExpired", "AdminUser",
            TruncateToSeconds(DateTime.UtcNow.AddDays(-2)), 1);
        // Active timed ban: untouched.
        addQuery.AddBan(accountId, "PurgeActive", "AdminUser",
            TruncateToSeconds(DateTime.UtcNow), 1);
        // Permanent ban: untouched.
        addQuery.AddBan(accountId, "PurgePermanent", "AdminUser",
            TruncateToSeconds(DateTime.UtcNow.AddDays(-30)), null);

        // Already-deleted expired ban on a second account: stays deleted.
        var accountId2 = Guid.NewGuid();
        fixture.AddAccount(accountId2, "BanPurge2", new byte[16], new byte[16]);
        addQuery.AddBan(accountId2, "PurgePreDeleted", "AdminUser",
            TruncateToSeconds(DateTime.UtcNow.AddDays(-2)), 1);
        MarkBanDeleted(accountId2, "PurgePreDeleted");

        var purgeQuery = new SqliteSoftDeleteExpiredBansQuery(fixture.Connection);
        purgeQuery.SoftDeleteExpiredBans();

        Assert.True(IsBanDeleted(accountId, "PurgeExpired"));
        Assert.False(IsBanDeleted(accountId, "PurgeActive"));
        Assert.False(IsBanDeleted(accountId, "PurgePermanent"));
        Assert.True(IsBanDeleted(accountId2, "PurgePreDeleted"));
    }

    [Fact]
    public void ListActiveBans_ReturnsAllAccounts_ExcludesDeleted()
    {
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        fixture.AddAccount(accountId1, "BanList1", new byte[16], new byte[16]);
        fixture.AddAccount(accountId2, "BanList2", new byte[16], new byte[16]);

        var addQuery = new SqliteAddBanQuery(fixture.Connection);
        addQuery.AddBan(accountId1, "ListPlayer1", "AdminUser", DateTime.UtcNow, null);
        addQuery.AddBan(accountId2, "ListPlayer2", "AdminUser", DateTime.UtcNow, 2);
        addQuery.AddBan(accountId1, "ListPlayer3", "AdminUser", DateTime.UtcNow, null);
        MarkBanDeleted(accountId1, "ListPlayer3");

        var listQuery = new SqliteListActiveBansQuery(fixture.Connection);
        var bans = listQuery.ListActiveBans();

        Assert.Contains(bans, b => b.AccountId == accountId1 && b.Username == "BanList1"
                                   && b.PlayerName == "ListPlayer1");
        Assert.Contains(bans, b => b.AccountId == accountId2 && b.Username == "BanList2"
                                   && b.PlayerName == "ListPlayer2" && b.DurationDays == 2);
        Assert.DoesNotContain(bans, b => b.PlayerName == "ListPlayer3");
    }

    /// <summary>
    ///     Checks whether the ban for the given account and player name is soft deleted.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <param name="playerName">Player name of the ban.</param>
    /// <returns>true if the ban is soft deleted, false otherwise.</returns>
    private bool IsBanDeleted(Guid accountId, string playerName)
    {
        const string sql = @"SELECT deleted FROM Ban
                                   WHERE account_id = @AccountId AND player_name = @PlayerName";
        using var cmd = new SqliteCommand(sql, fixture.Connection);

        var pAccountId = new SqliteParameter("AccountId", accountId.ToByteArray());
        pAccountId.SqliteType = SqliteType.Blob;
        cmd.Parameters.Add(pAccountId);

        var pPlayerName = new SqliteParameter("PlayerName", playerName);
        pPlayerName.SqliteType = SqliteType.Text;
        cmd.Parameters.Add(pPlayerName);

        var result = cmd.ExecuteScalar();
        return result != null && result != DBNull.Value && (long)result != 0;
    }

    /// <summary>
    ///     Marks the ban for the given account and player name as soft deleted.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <param name="playerName">Player name of the ban.</param>
    private void MarkBanDeleted(Guid accountId, string playerName)
    {
        const string sql = @"UPDATE Ban SET deleted = TRUE
                                   WHERE account_id = @AccountId AND player_name = @PlayerName";
        using var cmd = new SqliteCommand(sql, fixture.Connection);

        var pAccountId = new SqliteParameter("AccountId", accountId.ToByteArray());
        pAccountId.SqliteType = SqliteType.Blob;
        cmd.Parameters.Add(pAccountId);

        var pPlayerName = new SqliteParameter("PlayerName", playerName);
        pPlayerName.SqliteType = SqliteType.Text;
        cmd.Parameters.Add(pPlayerName);

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     Truncates a DateTime to whole seconds.
    /// </summary>
    /// <param name="dateTime">DateTime to truncate.</param>
    /// <returns>Truncated DateTime.</returns>
    private static DateTime TruncateToSeconds(DateTime dateTime)
    {
        var ticks = dateTime.Ticks - dateTime.Ticks % TimeSpan.TicksPerSecond;
        return new DateTime(ticks, DateTimeKind.Utc);
    }
}
