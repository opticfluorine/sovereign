// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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
///     DeletePlayerQuery implementation for SQLite.
/// </summary>
public class SqliteDeletePlayerQuery : IDeletePlayerQuery
{
    /// <summary>
    ///     SQL query.
    /// </summary>
    private const string Query = @"UPDATE Entity SET player_char_deleted = TRUE WHERE id = @Id";

    private readonly SqliteConnection dbConnection;

    public SqliteDeletePlayerQuery(SqliteConnection dbConnection)
    {
        this.dbConnection = dbConnection;
    }

    public void DeletePlayer(ulong playerEntityId)
    {
        using var cmd = new SqliteCommand(Query, dbConnection);

        var param = new SqliteParameter("Id", playerEntityId);
        param.SqliteType = SqliteType.Integer;
        cmd.Parameters.Add(param);

        cmd.ExecuteScalar();
    }
}