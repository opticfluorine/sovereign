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
using System.Numerics;
using System.Text;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     Reusable add query for Vector3-typed components.
/// </summary>
/// <remarks>
///     Do not pass user data to the constructor; it will not be sanitized.
/// </remarks>
public sealed class Vector3SqliteAddComponentQuery : IAddComponentQuery<Vector3>
{
    private readonly SqliteConnection connection;
    private readonly string sql;

    /// <summary>
    ///     Creates an add query.
    /// </summary>
    /// <param name="tableName">Table name.</param>
    /// <param name="connection">Database connection.</param>
    /// <remarks>
    ///     Do not pass user data as tableName. It will not be sanitized.
    /// </remarks>
    public Vector3SqliteAddComponentQuery(string tableName, SqliteConnection connection)
    {
        this.connection = connection;

        var sb = new StringBuilder();
        sb.Append("INSERT INTO ").Append(tableName)
            .Append(" (id, x, y, z) VALUES (@Id, @X, @Y, @Z)");
        sql = sb.ToString();
    }

    public void Add(ulong entityId, Vector3 value, IDbTransaction transaction)
    {
        using (var cmd = new SqliteCommand(sql, connection, (SqliteTransaction)transaction))
        {
            var paramId = new SqliteParameter("Id", entityId);
            paramId.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(paramId);

            var paramX = new SqliteParameter("X", value.X);
            paramX.SqliteType = SqliteType.Real;
            cmd.Parameters.Add(paramX);

            var paramY = new SqliteParameter("Y", value.Y);
            paramY.SqliteType = SqliteType.Real;
            cmd.Parameters.Add(paramY);

            var paramZ = new SqliteParameter("Z", value.Z);
            paramZ.SqliteType = SqliteType.Real;
            cmd.Parameters.Add(paramZ);

            cmd.ExecuteNonQuery();
        }
    }
}