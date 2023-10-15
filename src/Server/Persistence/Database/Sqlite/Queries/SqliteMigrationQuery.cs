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
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite implementation of IMigrationQuery.
/// </summary>
public sealed class SqliteMigrationQuery : IMigrationQuery
{
    /// <summary>
    ///     SQL query to execute.
    /// </summary>
    private const string query =
        @"SELECT MAX(id) FROM MigrationLog";

    private readonly SqliteConnection dbConnection;

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