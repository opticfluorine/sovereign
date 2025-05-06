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

using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite implementation of the query to retrieve all global key-value pairs.
/// </summary>
public class SqliteGetGlobalKeyValuePairsQuery : IGetGlobalKeyValuePairsQuery
{
    private const string Sql = "SELECT key, value FROM GlobalKeyValue";
    private readonly SqliteConnection connection;

    public SqliteGetGlobalKeyValuePairsQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public QueryReader GetGlobalKeyValuePairs()
    {
        var cmd = new SqliteCommand(Sql, connection);
        return new QueryReader(cmd, cmd.ExecuteReader());
    }
}