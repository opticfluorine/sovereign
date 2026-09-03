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
///     SQLite implementation of IRemoveBansForAccountQuery.
/// </summary>
public class SqliteRemoveBansForAccountQuery : IRemoveBansForAccountQuery
{
    /// <summary>
    ///     SQL query.
    /// </summary>
    private const string Query =
        @"UPDATE Ban SET deleted = TRUE
            WHERE account_id = @AccountId AND deleted = FALSE";

    private readonly SqliteConnection connection;

    public SqliteRemoveBansForAccountQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public int RemoveBansForAccount(Guid accountId)
    {
        using var cmd = new SqliteCommand(Query, connection);

        var pAccountId = new SqliteParameter("AccountId", SqliteType.Blob);
        pAccountId.Value = accountId.ToByteArray();
        cmd.Parameters.Add(pAccountId);

        return cmd.ExecuteNonQuery();
    }
}
