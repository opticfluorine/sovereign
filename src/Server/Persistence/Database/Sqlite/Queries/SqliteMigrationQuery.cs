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
    /// SQLite implementation of IMigrationQuery.
    /// </summary>
    public sealed class SqliteMigrationQuery : IMigrationQuery
    {

        private readonly SqliteConnection dbConnection;

        /// <summary>
        /// SQL query to execute.
        /// </summary>
        private const string query =
            @"SELECT MAX(id) FROM MigrationLog";

        public SqliteMigrationQuery(IDbConnection dbConnection)
        {
            this.dbConnection = (SqliteConnection)dbConnection;
        }

        public bool IsMigrationLevelApplied(int migrationLevel)
        {
            using (var cmd = new SqliteCommand(query, dbConnection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    // This particular query should always produce one row, so the call to Read() should always 
                    // return true. If not, just let the exception bubble up.
                    reader.Read();
                    var latestLevel = reader.GetInt32(0);
                    return latestLevel == migrationLevel;
                }
            }
        }

    }

}
