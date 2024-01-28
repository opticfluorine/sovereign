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
///     IRetrieveAccountQuery for the SQLite persistence provider.
/// </summary>
public sealed class SqliteRetrieveAccountQuery : IRetrieveAccountQuery
{
    /// <summary>
    ///     SQL query.
    /// </summary>
    private const string Query =
        @"SELECT id FROM Account
              WHERE username = @Username";

    private readonly SqliteConnection dbConnection;

    public SqliteRetrieveAccountQuery(IDbConnection dbConnection)
    {
        this.dbConnection = (SqliteConnection)dbConnection;
    }

    public QueryReader RetrieveAccount(string username)
    {
        var cmd = new SqliteCommand(Query, dbConnection);
        var transaction = dbConnection.BeginTransaction(IsolationLevel.RepeatableRead);
        cmd.Transaction = transaction;

        var param = new SqliteParameter("Username", username);
        param.SqliteType = SqliteType.Text;
        cmd.Parameters.Add(param);

        return new QueryReader(cmd);
    }
}