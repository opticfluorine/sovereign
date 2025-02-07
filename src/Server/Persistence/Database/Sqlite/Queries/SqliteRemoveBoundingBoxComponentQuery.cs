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
///     SQLite query for removing a BoundingBox component.
/// </summary>
public class SqliteRemoveBoundingBoxComponentQuery : IRemoveComponentQuery
{
    private const string query = @"
        UPDATE Entity
        SET
            bb_pos_x = NULL,
            bb_pos_y = NULL,
            bb_pos_z = NULL,
            bb_size_x = NULL,
            bb_size_y = NULL,
            bb_size_z = NULL
        WHERE id = @Id";

    private readonly SqliteConnection connection;

    public SqliteRemoveBoundingBoxComponentQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public void Remove(ulong entityId, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(query, connection, (SqliteTransaction)transaction);

        var pId = new SqliteParameter("Id", SqliteType.Integer);
        pId.Value = entityId;
        cmd.Parameters.Add(pId);

        cmd.ExecuteNonQuery();
    }
}