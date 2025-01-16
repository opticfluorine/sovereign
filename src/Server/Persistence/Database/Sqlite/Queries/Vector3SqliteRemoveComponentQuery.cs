// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     Reusable SQLite-backend IRemoveComponentQuery for Vector3-typed components.
/// </summary>
public class Vector3SqliteRemoveComponentQuery : IRemoveComponentQuery
{
    private readonly SqliteConnection connection;
    private readonly string sql;

    /// <summary>
    ///     Creates a query to remove the given component from an entity.
    /// </summary>
    /// <param name="columnPrefix">Column prefix in Entity table.</param>
    /// <param name="connection">Database connection.</param>
    public Vector3SqliteRemoveComponentQuery(string columnPrefix, SqliteConnection connection)
    {
        this.connection = connection;

        sql = $@"UPDATE Entity SET
                    {columnPrefix}x = NULL,
                    {columnPrefix}y = NULL,
                    {columnPrefix}z = NULL
                    WHERE id = @Id";
    }

    public void Remove(ulong entityId, IDbTransaction transaction)
    {
        using (var cmd = new SqliteCommand(sql, connection, (SqliteTransaction)transaction))
        {
            var param = new SqliteParameter("Id", entityId);
            param.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(param);

            cmd.ExecuteNonQuery();
        }
    }
}