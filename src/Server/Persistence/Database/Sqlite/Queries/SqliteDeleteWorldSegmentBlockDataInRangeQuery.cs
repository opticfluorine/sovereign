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
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     Sqlite implementation of IDeleteWorldSegmentBlockDataInRangeQuery.
/// </summary>
/// <param name="connection">Database connection.</param>
public sealed class SqliteDeleteWorldSegmentBlockDataInRangeQuery(SqliteConnection connection)
    : IDeleteWorldSegmentBlockDataInRangeQuery
{
    private const string query =
        @"DELETE FROM WorldSegmentBlockData
          WHERE x >= @MinX AND x < @MaxX
            AND y >= @MinY AND y < @MaxY
            AND z >= @MinZ AND z < @MaxZ";

    public void DeleteInRange(int minX, int maxX, int minY, int maxY, int minZ, int maxZ,
        IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(query, connection, (SqliteTransaction)transaction);
        AddParameter(cmd, "MinX", minX);
        AddParameter(cmd, "MaxX", maxX);
        AddParameter(cmd, "MinY", minY);
        AddParameter(cmd, "MaxY", maxY);
        AddParameter(cmd, "MinZ", minZ);
        AddParameter(cmd, "MaxZ", maxZ);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     Adds one bound integer parameter to a command.
    /// </summary>
    /// <param name="cmd">Command.</param>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    private static void AddParameter(SqliteCommand cmd, string name, int value)
    {
        var param = new SqliteParameter(name, SqliteType.Integer) { Value = value };
        cmd.Parameters.Add(param);
    }
}
