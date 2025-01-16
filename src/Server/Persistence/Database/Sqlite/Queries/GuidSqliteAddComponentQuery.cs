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
    /// <param name="paramName">Database parameter name.</param>
    /// <param name="dbConnection">Database connection.</param>
    /// <remarks>
    ///     Do not pass user-supplied data for tableName or paramName; it
    ///     will not be sanitized.
    /// </remarks>
    public GuidSqliteAddComponentQuery(string paramName, SqliteConnection dbConnection)
    {
        innerQuery = new SimpleSqliteAddComponentQuery<byte[]>(paramName, SqliteType.Blob, dbConnection);
    }

    public void Add(ulong entityId, Guid value, IDbTransaction transaction)
    {
        var bytes = value.ToByteArray();
        innerQuery.Add(entityId, bytes, transaction);
    }
}