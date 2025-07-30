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
using System.Numerics;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     Reusable component queries for Vector2-valued components.
/// </summary>
public class Vector2SqliteComponentQueries(string columnPrefix, SqliteConnection connection)
    : IAddComponentQuery<Vector2>, IModifyComponentQuery<Vector2>, IRemoveComponentQuery
{
    private readonly string addModifySql
        = $@"UPDATE Entity SET
                {columnPrefix}x = @X,
                {columnPrefix}y = @Y
             WHERE id = @Id";

    private readonly string removeSql
        = $@"UPDATE Entity SET
                {columnPrefix}x = NULL,
                {columnPrefix}y = NULL
             WHERE id = @Id";

    public void Add(ulong entityId, Vector2 value, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(addModifySql, connection, (SqliteTransaction)transaction);

        var pX = new SqliteParameter
        {
            ParameterName = "X",
            SqliteType = SqliteType.Real,
            Value = value.X
        };
        cmd.Parameters.Add(pX);

        var pY = new SqliteParameter
        {
            ParameterName = "Y",
            SqliteType = SqliteType.Real,
            Value = value.Y
        };
        cmd.Parameters.Add(pY);

        var pId = new SqliteParameter
        {
            ParameterName = "Id",
            SqliteType = SqliteType.Integer,
            Value = entityId
        };
        cmd.Parameters.Add(pId);

        cmd.ExecuteNonQuery();
    }

    public void Modify(ulong entityId, Vector2 value, IDbTransaction transaction)
    {
        Add(entityId, value, transaction);
    }

    public void Remove(ulong entityId, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(removeSql, connection, (SqliteTransaction)transaction);

        var pId = new SqliteParameter
        {
            ParameterName = "Id",
            SqliteType = SqliteType.Integer,
            Value = entityId
        };
        cmd.Parameters.Add(pId);

        cmd.ExecuteNonQuery();
    }
}