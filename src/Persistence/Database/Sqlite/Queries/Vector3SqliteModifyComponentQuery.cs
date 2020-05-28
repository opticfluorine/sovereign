/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Text;

namespace Sovereign.Persistence.Database.Sqlite.Queries
{

    /// <summary>
    /// Reusable add query for Vector3-typed components.
    /// </summary>
    /// <remarks>
    /// Do not pass user data to the constructor; it will not be sanitized.
    /// </remarks>
    public sealed class Vector3SqliteModifyComponentQuery : IModifyComponentQuery<Vector3>
    {
        private readonly SqliteConnection connection;
        private readonly string sql;

        /// <summary>
        /// Creates an add query.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="connection">Database connection.</param>
        /// <remarks>
        /// Do not pass user data as tableName. It will not be sanitized.
        /// </remarks>
        public Vector3SqliteModifyComponentQuery(string tableName, SqliteConnection connection)
        {
            this.connection = connection;

            var sb = new StringBuilder();
            sb.Append("UPDATE ").Append(tableName)
                .Append(" SET x = @X, y = @Y, z = @Z WHERE id = @Id");
            sql = sb.ToString();
        }

        public void Modify(ulong entityId, Vector3 value, IDbTransaction transaction)
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

}
