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

using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     Sqlite implementation of IGetSegmentFlagsInRangeQuery.
/// </summary>
/// <param name="connection">Database connection.</param>
public class SqliteGetSegmentFlagsInRangeQuery(SqliteConnection connection) : IGetSegmentFlagsInRangeQuery
{
    private const string query = @"SELECT x, y, z, flags FROM WorldSegmentFlag
                                   WHERE x BETWEEN @MinX AND @MaxX
                                     AND y BETWEEN @MinY AND @MaxY
                                     AND z BETWEEN @MinZ AND @MaxZ";

    public IDictionary<GridPosition, WorldSegmentFlag> GetFlagsInRange(GridPosition min, GridPosition max,
        IDbTransaction? transaction = null)
    {
        var result = new Dictionary<GridPosition, WorldSegmentFlag>();

        using var cmd = new SqliteCommand(query, connection, transaction as SqliteTransaction);
        AddParameter(cmd, "MinX", min.X);
        AddParameter(cmd, "MinY", min.Y);
        AddParameter(cmd, "MinZ", min.Z);
        AddParameter(cmd, "MaxX", max.X);
        AddParameter(cmd, "MaxY", max.Y);
        AddParameter(cmd, "MaxZ", max.Z);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var segment = new GridPosition(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2));
            result[segment] = (WorldSegmentFlag)reader.GetInt32(3);
        }

        return result;
    }

    /// <summary>
    ///     Adds an integer parameter to the given command.
    /// </summary>
    /// <param name="cmd">Command.</param>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    private static void AddParameter(SqliteCommand cmd, string name, int value)
    {
        var parameter = new SqliteParameter(name, SqliteType.Integer);
        parameter.Value = value;
        cmd.Parameters.Add(parameter);
    }
}
