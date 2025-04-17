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
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

public class SqliteCastShadowsComponentQueries : IAddComponentQuery<Shadow>, IModifyComponentQuery<Shadow>,
    IRemoveComponentQuery
{
    private const string updateSql = "UPDATE Entity SET shadow_radius = @Radius WHERE id = @Id";
    private const string removeSql = "UPDATE Entity SET shadow_radius = NULL WHERE id = @Id";
    private readonly SqliteConnection connection;

    public SqliteCastShadowsComponentQueries(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public void Add(ulong entityId, Shadow value, IDbTransaction transaction)
    {
        DoUpdate(entityId, value, transaction);
    }

    public void Modify(ulong entityId, Shadow value, IDbTransaction transaction)
    {
        DoUpdate(entityId, value, transaction);
    }

    public void Remove(ulong entityId, IDbTransaction transaction)
    {
        var cmd = new SqliteCommand(removeSql, connection, (SqliteTransaction)transaction);

        var pId = new SqliteParameter("Id", SqliteType.Integer);
        pId.Value = entityId;
        cmd.Parameters.Add(pId);

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     Updates the CastShadows columns for the given entity in the database.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="value">New shadow description.</param>
    /// <param name="transaction">Database transaction.</param>
    private void DoUpdate(ulong entityId, Shadow value, IDbTransaction transaction)
    {
        var cmd = new SqliteCommand(updateSql, connection, (SqliteTransaction)transaction);
        
        var pId = new SqliteParameter("Id", SqliteType.Integer);
        pId.Value = entityId;
        cmd.Parameters.Add(pId);

        var pRadius = new SqliteParameter("Radius", SqliteType.Real);
        pRadius.Value = value.Radius;
        cmd.Parameters.Add(pRadius);

        cmd.ExecuteNonQuery();
    }
}