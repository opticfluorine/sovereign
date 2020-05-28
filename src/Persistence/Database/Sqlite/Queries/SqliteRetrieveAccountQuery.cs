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
    /// IRetrieveAccountQuery for the SQLite persistence provider.
    /// </summary>
    public sealed class SqliteRetrieveAccountQuery : IRetrieveAccountQuery
    {

        private readonly SqliteConnection dbConnection;

        /// <summary>
        /// SQL query.
        /// </summary>
        private const string Query =
            @"SELECT id FROM Account
              WHERE username = @Username";

        public SqliteRetrieveAccountQuery(IDbConnection dbConnection)
        {
            this.dbConnection = (SqliteConnection)dbConnection;
        }

        public QueryReader RetrieveAccount(string username)
        {
            var cmd = new SqliteCommand(Query, dbConnection);

            var param = new SqliteParameter("Username", username);
            param.SqliteType = SqliteType.Text;
            cmd.Parameters.Add(param);

            return new QueryReader(cmd);
        }

    }

}
