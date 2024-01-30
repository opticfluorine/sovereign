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

using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

public class SqlitePlayerExistsQuery : IPlayerExistsQuery
{
    /// <summary>
    ///     SQL query to execute
    /// </summary>
    private const string query =
        @"SELECT EXISTS(SELECT 1 FROM Name INNER JOIN PlayerCharacter PC ON Name.id = PC.id WHERE Name.value = @Name)";

    private readonly SqliteConnection dbConnection;

    public SqlitePlayerExistsQuery(SqliteConnection dbConnection)
    {
        this.dbConnection = dbConnection;
    }

    public bool PlayerExists(string name)
    {
        using var cmd = new SqliteCommand(query, dbConnection);

        var param = new SqliteParameter("Name", name);
        param.SqliteType = SqliteType.Text;
        cmd.Parameters.Add(param);

        var exists = false;
        using var reader = new QueryReader(cmd).Reader;
        if (reader.Read()) exists = reader.GetBoolean(0);

        return exists;
    }
}