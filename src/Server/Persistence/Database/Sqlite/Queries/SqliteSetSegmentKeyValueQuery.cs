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
using Sovereign.EngineCore.Systems.Data;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     Sqlite implementation of ISetSegmentKeyValueQuery.
/// </summary>
/// <param name="connection">Database connection.</param>
public class SqliteSetSegmentKeyValueQuery(SqliteConnection connection) : ISetSegmentKeyValueQuery
{
    private const string upsert = @"INSERT INTO WorldSegmentKeyValue (x, y, z, key, value, type)
                                    VALUES (@X, @Y, @Z, @Key, @Value, @Type)
                                    ON CONFLICT(x, y, z, key) DO UPDATE SET value = excluded.value, type = excluded.type";

    public void SetKeyValue(GridPosition segmentIndex, string key, DataValue value, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(upsert, connection, (SqliteTransaction)transaction);

        cmd.Parameters.Add(CreateIntParameter("X", segmentIndex.X));
        cmd.Parameters.Add(CreateIntParameter("Y", segmentIndex.Y));
        cmd.Parameters.Add(CreateIntParameter("Z", segmentIndex.Z));

        var keyParameter = new SqliteParameter("Key", SqliteType.Text);
        keyParameter.Value = key;
        cmd.Parameters.Add(keyParameter);

        var valueParameter = new SqliteParameter("Value", SqliteType.Text);
        valueParameter.Value = DataValueCodec.Encode(value);
        cmd.Parameters.Add(valueParameter);

        cmd.Parameters.Add(CreateIntParameter("Type", (int)value.Type));

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
