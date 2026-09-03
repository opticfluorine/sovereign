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

using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite implementation of ISoftDeleteExpiredBansQuery.
/// </summary>
public class SqliteSoftDeleteExpiredBansQuery : ISoftDeleteExpiredBansQuery
{
    /// <summary>
    ///     SQL query.
    /// </summary>
    private const string Query =
        @"UPDATE Ban SET deleted = TRUE
            WHERE deleted = FALSE AND duration_days IS NOT NULL
              AND julianday(created) + duration_days <= julianday('now')";

    private readonly SqliteConnection connection;

    public SqliteSoftDeleteExpiredBansQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public void SoftDeleteExpiredBans()
    {
        using var cmd = new SqliteCommand(Query, connection);
        cmd.ExecuteNonQuery();
    }
}
