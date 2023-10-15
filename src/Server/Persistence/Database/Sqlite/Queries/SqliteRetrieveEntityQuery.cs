﻿/*
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
///     SQLite implementation of IRetrieveEntityQuery.
/// </summary>
public sealed class SqliteRetrieveEntityQuery : IRetrieveEntityQuery
{
    /// <summary>
    ///     SQL query to execute.
    /// </summary>
    private const string query =
        @"SELECT * FROM EntityWithComponents WHERE id = @Id";

    private readonly SqliteConnection dbConnection;

    public SqliteRetrieveEntityQuery(IDbConnection dbConnection)
    {
        this.dbConnection = (SqliteConnection)dbConnection;
    }

    public QueryReader RetrieveEntity(ulong entityId)
    {
        var cmd = PrepareCommand(entityId);
        return new QueryReader(cmd);
    }

    /// <summary>
    ///     Prepares the SQL command.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>SQL command.</returns>
    private SqliteCommand PrepareCommand(ulong entityId)
    {
        var cmd = new SqliteCommand(query, dbConnection);

        var param = new SqliteParameter("Id", entityId);
        param.SqliteType = SqliteType.Integer;
        param.Value = entityId;
        cmd.Parameters.Add(param);

        return cmd;
    }
}