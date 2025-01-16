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
///     SQLite query for removing a PointLightSource component from an entity.
/// </summary>
public class SqliteRemovePointLightSourceComponentQuery : IRemoveComponentQuery
{
    private const string query =
        @"UPDATE Entity SET
            pls_radius = NULL,
            pls_intensity = NULL,
            pls_color = NULL,
            pls_pos_x = NULL,
            pls_pos_y = NULL,
            pls_pos_z = NULL
            WHERE id = @Id";

    private readonly SqliteConnection connection;

    public SqliteRemovePointLightSourceComponentQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public void Remove(ulong entityId, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(query, connection, (SqliteTransaction)transaction);

        var pId = new SqliteParameter
        {
            ParameterName = "Id",
            Value = entityId,
            SqliteType = SqliteType.Integer
        };
        cmd.Parameters.Add(pId);

        cmd.ExecuteNonQuery();
    }
}