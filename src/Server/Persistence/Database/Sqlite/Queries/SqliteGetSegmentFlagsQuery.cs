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

using Microsoft.Data.Sqlite;
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     Sqlite implementation of IGetSegmentFlagsQuery.
/// </summary>
/// <param name="connection">Database connection.</param>
public class SqliteGetSegmentFlagsQuery(SqliteConnection connection) : IGetSegmentFlagsQuery
{
    private const string query = @"SELECT flags FROM WorldSegmentFlag WHERE x = @X AND y = @Y AND z = @Z";

    public WorldSegmentFlag GetFlags(GridPosition segmentIndex)
    {
        using var cmd = new SqliteCommand(query, connection);

        var pX = new SqliteParameter("X", SqliteType.Integer);
        pX.Value = segmentIndex.X;
        cmd.Parameters.Add(pX);

        var pY = new SqliteParameter("Y", SqliteType.Integer);
        pY.Value = segmentIndex.Y;
        cmd.Parameters.Add(pY);

        var pZ = new SqliteParameter("Z", SqliteType.Integer);
        pZ.Value = segmentIndex.Z;
        cmd.Parameters.Add(pZ);

        var result = cmd.ExecuteScalar();
        return result is null or System.DBNull
            ? WorldSegmentFlag.None
            : (WorldSegmentFlag)System.Convert.ToInt32(result);
    }
}
