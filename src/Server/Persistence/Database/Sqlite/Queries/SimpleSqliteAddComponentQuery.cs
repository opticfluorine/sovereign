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
///     Reusable SQLite query for adding a single-valued component.
/// </summary>
/// <remarks>
///     Do not pass user-supplied data to the constructor; it will not
///     be sanitized.
/// </remarks>
public class SimpleSqliteAddComponentQuery<T> : IAddComponentQuery<T>
{
    private readonly SqliteConnection dbConnection;
    private readonly SqliteType paramType;
    private readonly string sql;

    /// <summary>
    ///     Creates the add component query.
    /// </summary>
    /// <param name="paramName">Database parameter name.</param>
    /// <param name="paramType">Database parameter type.</param>
    /// <param name="dbConnection">Database connection.</param>
    /// <remarks>
    ///     Do not pass user-supplied data for tableName or paramName; it
    ///     will not be sanitized.
    /// </remarks>
    public SimpleSqliteAddComponentQuery(string paramName,
        SqliteType paramType, SqliteConnection dbConnection)
    {
        this.dbConnection = dbConnection;
        this.paramType = paramType;

        sql = $@"UPDATE Entity SET {paramName} = @Val WHERE id = @Id";
    }

    public void Add(ulong entityId, T value, IDbTransaction transaction)
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