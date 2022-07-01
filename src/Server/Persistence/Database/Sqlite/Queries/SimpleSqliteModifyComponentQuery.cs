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
using System.Text;

namespace Sovereign.Persistence.Database.Sqlite.Queries
{

    /// <summary>
    /// Reusable SQLite query for modifying a single-valued component.
    /// </summary>
    /// <remarks>
    /// Do not pass user-supplied data to the constructor; it will not
    /// be sanitized.
    /// </remarks>
    public class SimpleSqliteModifyComponentQuery<T> : IModifyComponentQuery<T>
        where T : unmanaged
    {
        private readonly SqliteConnection dbConnection;
        private readonly SqliteType paramType;
        private readonly string sql;

        /// <summary>
        /// Creates the modify component query.
        /// </summary>
        /// <param name="tableName">Database table name.</param>
        /// <param name="paramName">Database parameter name.</param>
        /// <param name="paramType">Database parameter type.</param>
        /// <param name="dbConnection">Database connection.</param>
        /// <remarks>
        /// Do not pass user-supplied data for tableName or paramName; it
        /// will not be sanitized.
        /// </remarks>
        public SimpleSqliteModifyComponentQuery(string tableName, string paramName,
            SqliteType paramType, SqliteConnection dbConnection)
        {
            this.dbConnection = dbConnection;
            this.paramType = paramType;

            var sb = new StringBuilder();
            sb.Append("UPDATE ").Append(tableName)
                .Append(" SET ").Append(paramName)
                .Append(" = @Val WHERE id = @Id");
            sql = sb.ToString();
        }

        public void Modify(ulong entityId, T value, IDbTransaction transaction)
        {
            using (var cmd = new SqliteCommand(sql, dbConnection, (SqliteTransaction)transaction))
            {
                var idParam = new SqliteParameter("Id", entityId);
                idParam.SqliteType = SqliteType.Integer;
                cmd.Parameters.Add(idParam);

                var valParam = new SqliteParameter("Val", value);
                valParam.SqliteType = paramType;
                cmd.Parameters.Add(valParam);

                cmd.ExecuteNonQuery();
            }
        }

    }

}
