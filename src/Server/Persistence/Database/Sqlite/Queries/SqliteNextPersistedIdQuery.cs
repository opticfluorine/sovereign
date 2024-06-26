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

using System;
using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.EngineCore.Entities;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite implementation of INextPersistedIdQuery.
/// </summary>
public sealed class SqliteNextPersistedIdQuery : INextPersistedIdQuery
{
    /// <summary>
    ///     SQL query to execute.
    /// </summary>
    private const string query =
        @"SELECT MAX(id) + 1 FROM (
                SELECT MAX(id) AS id FROM Entity
                UNION SELECT @FirstPersistedId - 1)";

    private readonly SqliteConnection dbConnection;

    public SqliteNextPersistedIdQuery(IDbConnection dbConnection)
    {
        this.dbConnection = (SqliteConnection)dbConnection;
    }

    public ulong GetNextPersistedEntityId()
    {
        using var cmd = new SqliteCommand(query, dbConnection);

        var pFirstPersistedId = new SqliteParameter("FirstPersistedId", SqliteType.Integer);
        pFirstPersistedId.Value = EntityConstants.FirstPersistedEntityId;
        cmd.Parameters.Add(pFirstPersistedId);

        var result = cmd.ExecuteScalar();
        if (result == null) throw new Exception("Database is in an invalid state.");
        return (ulong)(long)result;
    }
}