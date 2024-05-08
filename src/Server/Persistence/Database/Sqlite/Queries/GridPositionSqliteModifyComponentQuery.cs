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

using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     Reusable modify query for GridPosition-typed components.
/// </summary>
/// <remarks>
///     Do not pass user data to the constructor; it will not be sanitized.
/// </remarks>
public class GridPositionSqliteModifyComponentQuery : IModifyComponentQuery<GridPosition>
{
    private readonly SqliteConnection connection;
    private readonly string sql;

    public GridPositionSqliteModifyComponentQuery(string tableName, SqliteConnection connection)
    {
        this.connection = connection;
        sql = $"UPDATE {tableName} SET x = @X, y = @Y, z = @Z WHERE id = @Id";
    }

    public void Modify(ulong entityId, GridPosition value, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(sql, connection, (SqliteTransaction)transaction);

        var paramId = new SqliteParameter("Id", entityId);
        paramId.SqliteType = SqliteType.Integer;
        cmd.Parameters.Add(paramId);

        var paramX = new SqliteParameter("X", value.X);
        paramX.SqliteType = SqliteType.Integer;
        cmd.Parameters.Add(paramX);

        var paramY = new SqliteParameter("Y", value.Y);
        paramY.SqliteType = SqliteType.Integer;
        cmd.Parameters.Add(paramY);

        var paramZ = new SqliteParameter("Z", value.Z);
        paramZ.SqliteType = SqliteType.Integer;
        cmd.Parameters.Add(paramZ);

        cmd.ExecuteNonQuery();
    }
}