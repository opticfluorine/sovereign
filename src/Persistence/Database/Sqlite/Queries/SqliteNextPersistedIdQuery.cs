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
