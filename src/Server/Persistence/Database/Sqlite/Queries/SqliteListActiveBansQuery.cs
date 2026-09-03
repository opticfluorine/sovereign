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
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Bans;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite implementation of IListActiveBansQuery.
/// </summary>
public class SqliteListActiveBansQuery : IListActiveBansQuery
{
    /// <summary>
    ///     SQL query.
    /// </summary>
    private const string Query =
        @"SELECT b.account_id, a.username, b.player_name, b.admin_name, b.created, b.duration_days
            FROM Ban b INNER JOIN Account a ON a.id = b.account_id
            WHERE b.deleted = FALSE
            ORDER BY b.created";

    private readonly SqliteConnection connection;

    public SqliteListActiveBansQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public List<BanInfo> ListActiveBans()
    {
        // Execute query and process results.
        using var cmd = new SqliteCommand(Query, connection);
        using var reader = cmd.ExecuteReader();
        var results = new List<BanInfo>();
        while (reader.Read())
        {
            results.Add(new BanInfo
            {
                AccountId = new Guid((byte[])reader.GetValue(0)),
                Username = reader.GetString(1),
                PlayerName = reader.GetString(2),
                AdminName = reader.GetString(3),
                CreatedUtc = reader.GetDateTime(4),
                DurationDays = reader.IsDBNull(5) ? null : reader.GetInt32(5)
            });
        }

        return results;
    }
}
