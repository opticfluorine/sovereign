// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

/// <summary>
///     SQLite queries for BlockTile component.
/// </summary>
public sealed class SqliteBlockTileQuery(SqliteConnection conn) : IAddComponentQuery<BlockTile>,
    IModifyComponentQuery<BlockTile>,
    IRemoveComponentQuery
{
    private const string sql = @"UPDATE Entity SET 
                  front_tile_id = @FrontId,
                  top_tile_id = @TopId
                  WHERE id = @Id";

    private const string removeSql = @"UPDATE Entity SET 
                  front_tile_id = NULL,
                  top_tile_id = NULL
                  WHERE id = @Id";

    public void Add(ulong entityId, BlockTile value, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(sql, conn, (SqliteTransaction)transaction);

        var pId = new SqliteParameter("Id", SqliteType.Integer) { Value = entityId };
        cmd.Parameters.Add(pId);

        var pFrontId = new SqliteParameter("FrontId", SqliteType.Integer) { Value = value.FrontFaceId };
        cmd.Parameters.Add(pFrontId);

        var pTopId = new SqliteParameter("TopId", SqliteType.Integer) { Value = value.TopFaceId };
        cmd.Parameters.Add(pTopId);

        cmd.ExecuteNonQuery();
    }

    public void Modify(ulong entityId, BlockTile value, IDbTransaction transaction)
    {
        Add(entityId, value, transaction);
    }

    public void Remove(ulong entityId, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(removeSql, conn, (SqliteTransaction)transaction);

        var pId = new SqliteParameter("Id", SqliteType.Integer) { Value = entityId };
        cmd.Parameters.Add(pId);

        cmd.ExecuteNonQuery();
    }
}