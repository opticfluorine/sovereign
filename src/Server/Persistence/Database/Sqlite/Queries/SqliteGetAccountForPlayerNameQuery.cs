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
///     SQLite implementation of IGetAccountForPlayerNameQuery.
/// </summary>
public class SqliteGetAccountForPlayerNameQuery : IGetAccountForPlayerNameQuery
{
    /// <summary>
    ///     SQL query.
    /// </summary>
    private const string Query =
        @"SELECT account_id FROM Entity
            WHERE name = @Name COLLATE NOCASE
              AND player_char = TRUE AND player_char_deleted = FALSE
              AND account_id IS NOT NULL
            LIMIT 1";

    private readonly SqliteConnection connection;

    public SqliteGetAccountForPlayerNameQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public bool TryGetAccountForPlayer(string playerName, out Guid accountId)
    {
        // Prepare query.
        using var cmd = new SqliteCommand(Query, connection);

        var pName = new SqliteParameter("Name", playerName);
        pName.SqliteType = SqliteType.Text;
        cmd.Parameters.Add(pName);

        // Execute query and parse result if any.
        using var reader = cmd.ExecuteReader();
        accountId = Guid.Empty;
        if (!reader.Read()) return false;

        var accountIdBytes = (byte[])reader.GetValue(0);
        if (accountIdBytes.Length != 16) return false;

        accountId = new Guid(accountIdBytes);
        return true;
    }
}
