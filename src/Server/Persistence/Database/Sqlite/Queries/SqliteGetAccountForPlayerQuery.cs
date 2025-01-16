// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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
///     SQLite implementation of IGetAccountForPlayerQuery.
/// </summary>
public class SqliteGetAccountForPlayerQuery : IGetAccountForPlayerQuery
{
    /// <summary>
    ///     SQL query to execute.
    /// </summary>
    private const string query =
        @"SELECT account_id FROM Entity WHERE id = @PlayerId";

    private readonly SqliteConnection connection;

    public SqliteGetAccountForPlayerQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public bool TryGetAccountForPlayer(ulong playerEntityId, out Guid accountId)
    {
        // Prepare query.
        using var cmd = new SqliteCommand(query, connection);

        var param = new SqliteParameter("PlayerId", SqliteType.Integer);
        param.Value = playerEntityId;
        cmd.Parameters.Add(param);

        // Execute query and parse result if any.
        using var reader = cmd.ExecuteReader();
        var result = false;
        accountId = Guid.Empty;
        if (reader.Read())
        {
            var accountIdBytes = new byte[16];
            var len = reader.GetBytes(0, 0, accountIdBytes, 0, accountIdBytes.Length);
            if (len == accountIdBytes.Length)
            {
                result = true;
                accountId = new Guid(accountIdBytes);
            }
        }

        return result;
    }
}