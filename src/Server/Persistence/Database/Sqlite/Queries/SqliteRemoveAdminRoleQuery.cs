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
///     SQLite implementation of IRemoveAdminRoleQuery.
/// </summary>
public class SqliteRemoveAdminRoleQuery : IRemoveAdminRoleQuery
{
    /// <summary>
    ///     SQL query.
    /// </summary>
    private const string Sql
        = @"DELETE FROM Admin WHERE id in
                (SELECT Name.id FROM Name
                    INNER JOIN PlayerCharacter PC ON PC.id = Name.id
                    WHERE Name.value = @Name AND PC.deleted = FALSE)";

    private readonly SqliteConnection connection;

    public SqliteRemoveAdminRoleQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public void RemoveAdminRole(string playerName)
    {
        using var cmd = new SqliteCommand(Sql, connection);

        var pName = new SqliteParameter("Name", playerName);
        pName.SqliteType = SqliteType.Text;
        cmd.Parameters.Add(pName);

        cmd.ExecuteNonQuery();
    }
}