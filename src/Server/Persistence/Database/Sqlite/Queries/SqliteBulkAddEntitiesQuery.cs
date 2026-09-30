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

    // Parameter names are cached by row position; formatting them per row would allocate.
    private static readonly string[] IdParameterNames = BuildParameterNames("@Id");
    private static readonly string[] TemplateIdParameterNames = BuildParameterNames("@TemplateId");
    private static readonly string[] XParameterNames = BuildParameterNames("@X");
    private static readonly string[] YParameterNames = BuildParameterNames("@Y");
    private static readonly string[] ZParameterNames = BuildParameterNames("@Z");
    private static readonly string[] TypeParameterNames = BuildParameterNames("@Type");

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
        var sql = new StringBuilder(sqlPrefix, sqlPrefix.Length + count * 64);
        var cmd = connection.CreateCommand();
        cmd.Transaction = (SqliteTransaction)transaction;

        try
        {
            for (var i = 0; i < count; ++i)
            {
                if (i > 0) sql.Append(',');

                sql.Append("(@Id").Append(i)
                    .Append(", @TemplateId").Append(i)
                    .Append(", @X").Append(i)
                    .Append(", @Y").Append(i)
                    .Append(", @Z").Append(i)
                    .Append(", @Type").Append(i)
                    .Append(')');

                var row = rows[offset + i];
                AddParameter(cmd, IdParameterNames[i], (long)row.EntityId);
                AddParameter(cmd, TemplateIdParameterNames[i], (long)row.TemplateEntityId);
                AddParameter(cmd, XParameterNames[i], row.X, SqliteType.Real);
                AddParameter(cmd, YParameterNames[i], row.Y, SqliteType.Real);
                AddParameter(cmd, ZParameterNames[i], row.Z, SqliteType.Real);
                AddParameter(cmd, TypeParameterNames[i], row.EntityType);
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
    ///     Builds the cached parameter names for every row position of a parameter.
    /// </summary>
    /// <param name="prefix">Parameter name prefix including the leading sigil.</param>
    /// <returns>Parameter names indexed by row position.</returns>
    private static string[] BuildParameterNames(string prefix)
    {
        var names = new string[RowsPerCommand];
        for (var i = 0; i < names.Length; ++i)
        {
            names[i] = prefix + i;
        }

        return names;
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
