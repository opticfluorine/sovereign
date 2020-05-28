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
using System.Text;

namespace Sovereign.Persistence.Database.Sqlite.Queries
{

    /// <summary>
    /// SQLite implementation of IRemoveComponentQuery.
    /// </summary>
    public sealed class SqliteRemoveComponentQuery : IRemoveComponentQuery
    {
        private readonly SqliteConnection connection;
        private readonly string sql;

        public SqliteRemoveComponentQuery(string tableName, SqliteConnection connection)
        {
            this.connection = connection;

            var sb = new StringBuilder();
            sb.Append("DELETE FROM ").Append(tableName).Append(" WHERE id = @Id");
            sql = sb.ToString();
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

}
