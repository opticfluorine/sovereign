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
using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     Reusable SQLite query for adding a GUID-valued component.
/// </summary>
public class GuidSqliteAddComponentQuery : IAddComponentQuery<Guid>
{
    private readonly SimpleSqliteAddComponentQuery<byte[]> innerQuery;

    /// <summary>
    ///     Creates the add component query.
    /// </summary>
    /// <param name="tableName">Database table name.</param>
    /// <param name="paramName">Database parameter name.</param>
    /// <param name="dbConnection">Database connection.</param>
    /// <remarks>
    ///     Do not pass user-supplied data for tableName or paramName; it
    ///     will not be sanitized.
    /// </remarks>
    public GuidSqliteAddComponentQuery(string tableName, string paramName, SqliteConnection dbConnection)
    {
        innerQuery = new SimpleSqliteAddComponentQuery<byte[]>(tableName, paramName, SqliteType.Blob, dbConnection);
    }

    public void Add(ulong entityId, Guid value, IDbTransaction transaction)
    {
        var bytes = value.ToByteArray();
        innerQuery.Add(entityId, bytes, transaction);
    }
}