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
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite implementation of IAddBanQuery.
/// </summary>
public class SqliteAddBanQuery : IAddBanQuery
{
    /// <summary>
    ///     SQL query.
    /// </summary>
    private const string Query =
        @"INSERT INTO Ban (account_id, player_name, admin_name, created, duration_days)
            VALUES (@AccountId, @PlayerName, @AdminName, @Created, @DurationDays)";

    private readonly SqliteConnection connection;

    public SqliteAddBanQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public void AddBan(Guid accountId, string playerName, string adminName,
        DateTime createdUtc, int? durationDays)
    {
        using var cmd = new SqliteCommand(Query, connection);

        var pAccountId = new SqliteParameter("AccountId", SqliteType.Blob);
        pAccountId.Value = accountId.ToByteArray();
        cmd.Parameters.Add(pAccountId);

        var pPlayerName = new SqliteParameter("PlayerName", SqliteType.Text);
        pPlayerName.Value = playerName;
        cmd.Parameters.Add(pPlayerName);

        var pAdminName = new SqliteParameter("AdminName", SqliteType.Text);
        pAdminName.Value = adminName;
        cmd.Parameters.Add(pAdminName);

        var pCreated = new SqliteParameter("Created", SqliteType.Text);
        pCreated.Value = createdUtc;
        cmd.Parameters.Add(pCreated);

        var pDuration = new SqliteParameter("DurationDays", SqliteType.Integer);
        pDuration.Value = (object?)durationDays ?? DBNull.Value;
        cmd.Parameters.Add(pDuration);

        cmd.ExecuteNonQuery();
    }
}
