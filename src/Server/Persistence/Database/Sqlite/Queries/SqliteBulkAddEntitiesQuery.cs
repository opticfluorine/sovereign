// Sovereign Engine
// Copyright (c) 2026 opticfluorine
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     Sqlite implementation of IBulkAddEntitiesQuery. Rows are written with multi-row
///     INSERT commands within the caller's transaction.
/// </summary>
/// <param name="connection">Database connection.</param>
public sealed class SqliteBulkAddEntitiesQuery(SqliteConnection connection) : IBulkAddEntitiesQuery
{
    /// <summary>
    ///     Maximum number of rows per INSERT command.
    /// </summary>
    private const int RowsPerCommand = 32;

    /// <summary>
    ///     Prefix of the shared INSERT statement.
    /// </summary>
    private const string sqlPrefix =
        "INSERT INTO Entity (id, template_id, pos_x, pos_y, pos_z, entity_type) VALUES ";

    public void AddEntities(IReadOnlyList<BulkEntityRow> rows, IDbTransaction transaction)
    {
        for (var offset = 0; offset < rows.Count; offset += RowsPerCommand)
        {
            var count = Math.Min(RowsPerCommand, rows.Count - offset);
            using var cmd = BuildCommand(rows, offset, count, transaction);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    ///     Builds the multi-row INSERT command for a range of rows.
    /// </summary>
    /// <param name="rows">Rows to insert.</param>
    /// <param name="offset">Offset of the first row.</param>
    /// <param name="count">Number of rows to insert.</param>
    /// <param name="transaction">Transaction.</param>
    /// <returns>Command with bound parameters.</returns>
    private SqliteCommand BuildCommand(IReadOnlyList<BulkEntityRow> rows, int offset, int count,
        IDbTransaction transaction)
    {
        var sql = new StringBuilder(sqlPrefix);
        var cmd = connection.CreateCommand();
        cmd.Transaction = (SqliteTransaction)transaction;

        try
        {
            for (var i = 0; i < count; ++i)
            {
                if (i > 0) sql.Append(',');

                var row = rows[offset + i];
                sql.Append($"(@Id{i}, @TemplateId{i}, @X{i}, @Y{i}, @Z{i}, @Type{i})");

                AddParameter(cmd, $"@Id{i}", (long)row.EntityId);
                AddParameter(cmd, $"@TemplateId{i}", (long)row.TemplateEntityId);
                AddParameter(cmd, $"@X{i}", row.X, SqliteType.Real);
                AddParameter(cmd, $"@Y{i}", row.Y, SqliteType.Real);
                AddParameter(cmd, $"@Z{i}", row.Z, SqliteType.Real);
                AddParameter(cmd, $"@Type{i}", row.EntityType);
            }

            cmd.CommandText = sql.ToString();
            return cmd;
        }
        catch
        {
            cmd.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     Adds one bound parameter to a command.
    /// </summary>
    /// <param name="cmd">Command.</param>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <param name="type">Sqlite parameter type, defaulting to integer.</param>
    private static void AddParameter(SqliteCommand cmd, string name, object value,
        SqliteType type = SqliteType.Integer)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        param.SqliteType = type;
        cmd.Parameters.Add(param);
    }
}
