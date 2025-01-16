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

using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     Sqlite implementation of IAddAdminRoleQuery.
/// </summary>
public class SqliteAddAdminRoleQuery : IAddAdminRoleQuery
{
    /// <summary>
    ///     SQL query.
    /// </summary>
    private const string Query =
        @"UPDATE Entity
            SET admin = TRUE
            WHERE Name.value = @Name AND player_char = TRUE AND player_char_deleted = FALSE";

    private readonly SqliteConnection connection;

    public SqliteAddAdminRoleQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public bool TryAddAdminRole(string playerName)
    {
        using var cmd = new SqliteCommand(Query, connection);

        var pName = new SqliteParameter("Name", playerName);
        pName.SqliteType = SqliteType.Text;
        cmd.Parameters.Add(pName);

        return cmd.ExecuteNonQuery() > 0;
    }
}