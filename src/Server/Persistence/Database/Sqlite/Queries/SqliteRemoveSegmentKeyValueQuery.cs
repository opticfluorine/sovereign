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
///     Sqlite implementation of IRemoveSegmentKeyValueQuery.
/// </summary>
/// <param name="connection">Database connection.</param>
public class SqliteRemoveSegmentKeyValueQuery(SqliteConnection connection) : IRemoveSegmentKeyValueQuery
{
    private const string delete =
        @"DELETE FROM WorldSegmentKeyValue WHERE x = @X AND y = @Y AND z = @Z AND key = @Key";

    public void RemoveKeyValue(GridPosition segmentIndex, string key, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(delete, connection, (SqliteTransaction)transaction);

        cmd.Parameters.Add(CreateIntParameter("X", segmentIndex.X));
        cmd.Parameters.Add(CreateIntParameter("Y", segmentIndex.Y));
        cmd.Parameters.Add(CreateIntParameter("Z", segmentIndex.Z));

        var keyParameter = new SqliteParameter("Key", SqliteType.Text);
        keyParameter.Value = key;
        cmd.Parameters.Add(keyParameter);

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     Creates an integer parameter.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <returns>Parameter.</returns>
    private static SqliteParameter CreateIntParameter(string name, int value)
    {
        var parameter = new SqliteParameter(name, SqliteType.Integer);
        parameter.Value = value;
        return parameter;
    }
}
