/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite query to remove an entity ID from the database.
/// </summary>
public sealed class SqliteRemoveEntityQuery : IRemoveEntityQuery
{
    /// <summary>
    ///     Query string.
    /// </summary>
    private const string sql = @"DELETE FROM Entity WHERE id = @Id";

    private readonly SqliteConnection connection;

    public SqliteRemoveEntityQuery(IDbConnection connection)
    {
        this.connection = (SqliteConnection)connection;
    }

    public void RemoveEntityId(ulong entityId, IDbTransaction transaction)
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