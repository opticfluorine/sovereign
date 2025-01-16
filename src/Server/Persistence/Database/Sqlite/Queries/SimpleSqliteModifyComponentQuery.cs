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
///     Reusable SQLite query for modifying a single-valued component.
/// </summary>
/// <remarks>
///     Do not pass user-supplied data to the constructor; it will not
///     be sanitized.
/// </remarks>
public class SimpleSqliteModifyComponentQuery<T> : IModifyComponentQuery<T>
{
    private readonly SqliteType columnType;
    private readonly SqliteConnection dbConnection;
    private readonly string sql;

    /// <summary>
    ///     Creates the modify component query.
    /// </summary>
    /// <param name="columnName">Database column name.</param>
    /// <param name="columnType">Database column type.</param>
    /// <param name="dbConnection">Database connection.</param>
    /// <remarks>
    ///     Do not pass user-supplied data for or paramName; it
    ///     will not be sanitized.
    /// </remarks>
    public SimpleSqliteModifyComponentQuery(string columnName,
        SqliteType columnType, SqliteConnection dbConnection)
    {
        this.dbConnection = dbConnection;
        this.columnType = columnType;

        sql = $"UPDATE Entity SET {columnName} = @Val WHERE id = @Id";
    }

    public void Modify(ulong entityId, T value, IDbTransaction transaction)
    {
        using (var cmd = new SqliteCommand(sql, dbConnection, (SqliteTransaction)transaction))
        {
            var idParam = new SqliteParameter("Id", entityId);
            idParam.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(idParam);

            var valParam = new SqliteParameter("Val", value);
            valParam.SqliteType = columnType;
            cmd.Parameters.Add(valParam);

            cmd.ExecuteNonQuery();
        }
    }
}