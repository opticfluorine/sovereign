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
///     Sqlite implementation of IDeleteWorldGenDecorationsInRangeQuery.
/// </summary>
/// <param name="connection">Database connection.</param>
public sealed class SqliteDeleteWorldGenDecorationsInRangeQuery(SqliteConnection connection)
    : IDeleteWorldGenDecorationsInRangeQuery
{
    /// <summary>
    ///     Query. Only templated NPC rows with a position are eligible; player characters
    ///     and positionless entities are never deleted.
    /// </summary>
    private const string query =
        @"DELETE FROM Entity
          WHERE entity_type = 0
            AND template_id IS NOT NULL
            AND player_char IS NULL
            AND pos_x IS NOT NULL AND pos_y IS NOT NULL AND pos_z IS NOT NULL
            AND pos_x >= @MinX AND pos_x < @MaxX
            AND pos_y >= @MinY AND pos_y < @MaxY
            AND pos_z >= @MinZ AND pos_z < @MaxZ";

    public int DeleteInRange(float minX, float maxX, float minY, float maxY, float minZ,
        float maxZ, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(query, connection, (SqliteTransaction)transaction);

        AddParameter(cmd, "MinX", minX);
        AddParameter(cmd, "MaxX", maxX);
        AddParameter(cmd, "MinY", minY);
        AddParameter(cmd, "MaxY", maxY);
        AddParameter(cmd, "MinZ", minZ);
        AddParameter(cmd, "MaxZ", maxZ);

        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     Adds one bound real parameter to a command.
    /// </summary>
    /// <param name="cmd">Command.</param>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    private static void AddParameter(SqliteCommand cmd, string name, float value)
    {
        var param = new SqliteParameter(name, SqliteType.Real) { Value = value };
        cmd.Parameters.Add(param);
    }
}
