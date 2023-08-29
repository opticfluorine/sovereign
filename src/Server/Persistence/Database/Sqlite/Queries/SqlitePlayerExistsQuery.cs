// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

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
        var cmd = new SqliteCommand(query, dbConnection);

        var param = new SqliteParameter("Name", name);
        param.SqliteType = SqliteType.Text;
        cmd.Parameters.Add(param);

        var exists = false;
        var reader = new QueryReader(cmd).Reader;
        if (reader.Read()) exists = reader.GetBoolean(0);

        return exists;
    }
}