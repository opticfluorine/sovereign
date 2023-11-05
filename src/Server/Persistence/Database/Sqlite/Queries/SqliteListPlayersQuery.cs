// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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
using Microsoft.Data.Sqlite;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite implementation of IListPlayersQuery.
/// </summary>
public class SqliteListPlayersQuery : IListPlayersQuery
{
    /// <summary>
    ///     SQL query.
    /// </summary>
    private const string query =
        @"SELECT PC.id, N.name
          FROM AccountComponent AC
          INNER JOIN Name N ON AC.id = N.id
          INNER JOIN PlayerCharacter PC ON AC.id = PC.id
          WHERE AC.id = @AccountId AND PC.value = TRUE";

    private readonly SqliteConnection connection;

    public SqliteListPlayersQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public IList<PlayerInfo> ListPlayersForAccount(Guid accountId)
    {
        // Prepare query.
        var cmd = new SqliteCommand(query, connection);
        var param = new SqliteParameter("AccountId", SqliteType.Blob);
        param.Value = accountId;
        cmd.Parameters.Add(param);

        // Execute query and process results.
        var reader = cmd.ExecuteReader();
        var results = new List<PlayerInfo>();
        while (reader.NextResult())
        {
            var player = new PlayerInfo
            {
                Id = (ulong)reader.GetInt64(0),
                Name = reader.GetString(1)
            };
            results.Add(player);
        }

        return results;
    }
}