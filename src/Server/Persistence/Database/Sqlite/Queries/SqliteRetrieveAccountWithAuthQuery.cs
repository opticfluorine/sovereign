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
///     IRetrieveAccountWithAuthQuery for the SQLite persistence provider.
/// </summary>
public sealed class SqliteRetrieveAccountWithAuthQuery : IRetrieveAccountWithAuthQuery
{
    /// <summary>
    ///     SQL query.
    /// </summary>
    private const string Query =
        @"SELECT id, salt, hash, opslimit, memlimit
                FROM AccountWithAuthentication
                WHERE username = @Username";

    private readonly SqliteConnection dbConnection;

    public SqliteRetrieveAccountWithAuthQuery(IDbConnection dbConnection)
    {
        this.dbConnection = (SqliteConnection)dbConnection;
    }

    public QueryReader RetrieveAccountWithAuth(string username)
    {
        var cmd = new SqliteCommand(Query, dbConnection);
        cmd.Transaction = dbConnection.BeginTransaction(IsolationLevel.RepeatableRead);

        var param = new SqliteParameter("Username", username);
        param.SqliteType = SqliteType.Text;
        cmd.Parameters.Add(param);

        return new QueryReader(cmd);
    }
}