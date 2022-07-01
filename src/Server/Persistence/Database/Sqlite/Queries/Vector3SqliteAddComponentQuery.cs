/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
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
    public sealed class Vector3SqliteAddComponentQuery : IAddComponentQuery<Vector3>
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

}
