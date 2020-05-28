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

using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries
{

    /// <summary>
    /// SQLite implementation of INextPersistedIdQuery.
    /// </summary>
    public sealed class SqliteNextPersistedIdQuery : INextPersistedIdQuery
    {
        private readonly SqliteConnection dbConnection;

        /// <summary>
        /// SQL query to execute.
        /// </summary>
        private const string query =
            @"SELECT MAX(id) + 1 FROM (
                SELECT MAX(id) AS id FROM Entity
                UNION SELECT 0x7ffeffffffffffff)";

        public SqliteNextPersistedIdQuery(IDbConnection dbConnection)
        {
            this.dbConnection = (SqliteConnection)dbConnection;
        }

        public ulong GetNextPersistedEntityId()
        {
            using (var cmd = new SqliteCommand(query, dbConnection))
            {
                var result = (long)cmd.ExecuteScalar();
                return (ulong)result;
            }
        }

    }

}
