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

using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite query for adding and modifying RadiantData components.
/// </summary>
public class SqliteAddModifyRadiantDataComponentQuery : IAddComponentQuery<RadiantData>,
    IModifyComponentQuery<RadiantData>
{
    private const string query =
        @"UPDATE Entity SET
            radiant_category = @Category,
            radiant_function = @Function,
            radiant_param0 = @Param0,
            radiant_param1 = @Param1,
            radiant_param2 = @Param2
            WHERE id = @Id";

    private readonly SqliteConnection connection;

    public SqliteAddModifyRadiantDataComponentQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public void Add(ulong entityId, RadiantData value, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(query, connection, (SqliteTransaction)transaction);

        var pId = new SqliteParameter
        {
            ParameterName = "Id",
            Value = entityId,
            SqliteType = SqliteType.Integer
        };
        cmd.Parameters.Add(pId);

        var pCategory = new SqliteParameter
        {
            ParameterName = "Category",
            Value = (int)value.Category,
            SqliteType = SqliteType.Integer
        };
        cmd.Parameters.Add(pCategory);

        var pFunction = new SqliteParameter
        {
            ParameterName = "Function",
            Value = (int)value.Function,
            SqliteType = SqliteType.Integer
        };
        cmd.Parameters.Add(pFunction);

        var pParam0 = new SqliteParameter
        {
            ParameterName = "Param0",
            Value = value.Param0,
            SqliteType = SqliteType.Real
        };
        cmd.Parameters.Add(pParam0);

        var pParam1 = new SqliteParameter
        {
            ParameterName = "Param1",
            Value = value.Param1,
            SqliteType = SqliteType.Real
        };
        cmd.Parameters.Add(pParam1);

        var pParam2 = new SqliteParameter
        {
            ParameterName = "Param2",
            Value = value.Param2,
            SqliteType = SqliteType.Real
        };
        cmd.Parameters.Add(pParam2);

        cmd.ExecuteNonQuery();
    }

    public void Modify(ulong entityId, RadiantData value, IDbTransaction transaction)
    {
        Add(entityId, value, transaction);
    }
}
