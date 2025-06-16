// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
///     SQLite implementation of <see cref="IUpdateEntityKeyValueQuery" />.
/// </summary>
public class SqliteUpdateEntityKeyValueQuery : IUpdateEntityKeyValueQuery
{
    private const string Sql =
        @"INSERT INTO EntityKeyValue (entity_id, key, value)
    	    VALUES (@Id, @Key, @Value)
	        ON CONFLICT DO UPDATE SET entity_id=excluded.entity_id, key=excluded.key, value=excluded.value;";

    private readonly SqliteConnection connection;

    public SqliteUpdateEntityKeyValueQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public void UpdateEntityKeyValue(ulong entityId, string key, string value, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(Sql, connection, (SqliteTransaction)transaction);

        var pId = new SqliteParameter("Id", SqliteType.Integer);
        pId.Value = entityId;
        cmd.Parameters.Add(pId);

        var pKey = new SqliteParameter("Key", SqliteType.Text);
        pKey.Value = key;
        cmd.Parameters.Add(pKey);

        var pValue = new SqliteParameter("Value", SqliteType.Text);
        pValue.Value = value;
        cmd.Parameters.Add(pValue);

        cmd.ExecuteNonQuery();
    }
}