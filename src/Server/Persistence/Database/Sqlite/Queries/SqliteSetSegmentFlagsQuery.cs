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
///     Sqlite implementation of ISetSegmentFlagsQuery.
/// </summary>
/// <param name="connection">Database connection.</param>
public class SqliteSetSegmentFlagsQuery(SqliteConnection connection) : ISetSegmentFlagsQuery
{
    private const string upsert = @"INSERT INTO WorldSegmentFlag (x, y, z, flags)
                                    VALUES (@X, @Y, @Z, @Flags)
                                    ON CONFLICT(x, y, z) DO UPDATE SET flags = excluded.flags";

    private const string delete = @"DELETE FROM WorldSegmentFlag WHERE x = @X AND y = @Y AND z = @Z";

    public void SetFlags(GridPosition segmentIndex, WorldSegmentFlag flags, IDbTransaction transaction)
    {
        var sql = flags == WorldSegmentFlag.None ? delete : upsert;
        using var cmd = new SqliteCommand(sql, connection, (SqliteTransaction)transaction);

        cmd.Parameters.Add(CreateParameter("X", segmentIndex.X));
        cmd.Parameters.Add(CreateParameter("Y", segmentIndex.Y));
        cmd.Parameters.Add(CreateParameter("Z", segmentIndex.Z));

        if (flags != WorldSegmentFlag.None) cmd.Parameters.Add(CreateParameter("Flags", (int)flags));

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     Creates an integer parameter.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <returns>Parameter.</returns>
    private static SqliteParameter CreateParameter(string name, int value)
    {
        var parameter = new SqliteParameter(name, SqliteType.Integer);
        parameter.Value = value;
        return parameter;
    }
}
