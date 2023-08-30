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

using System;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite implementation of IGetAccountForPlayerQuery.
/// </summary>
public class SqliteGetAccountForPlayerQuery : IGetAccountForPlayerQuery
{
    /// <summary>
    ///     SQL query to execute.
    /// </summary>
    private const string query =
        @"SELECT AC.account_id FROM PlayerCharacter PC INNER JOIN AccountComponent AC ON PC.id = AC.id WHERE PC.id = @PlayerId";

    private readonly SqliteConnection connection;

    public SqliteGetAccountForPlayerQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public bool TryGetAccountForPlayer(ulong playerEntityId, out Guid accountId)
    {
        // Prepare query.
        var cmd = new SqliteCommand(query, connection);
        var param = new SqliteParameter("PlayerId", SqliteType.Integer);
        param.Value = playerEntityId;
        cmd.Parameters.Add(param);

        // Execute query and parse result if any.
        var reader = cmd.ExecuteReader();
        var result = false;
        accountId = Guid.Empty;
        if (reader.NextResult())
        {
            var accountIdBytes = new byte[16];
            var len = reader.GetBytes(0, 0, accountIdBytes, 0, accountIdBytes.Length);
            if (len == accountIdBytes.Length)
            {
                result = true;
                accountId = new Guid(accountIdBytes);
            }
        }

        return result;
    }
}