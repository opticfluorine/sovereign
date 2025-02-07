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

/// <summary>
///     SQLite query for adding or modifying BoundingBox components.
/// </summary>
public class SqliteAddModifyBoundingBoxComponentQuery : IAddComponentQuery<BoundingBox>,
    IModifyComponentQuery<BoundingBox>
{
    private const string query = @"
        UPDATE Entity
        SET
            bb_pos_x = @BbPosX,
            bb_pos_y = @BbPosY,
            bb_pos_z = @BbPosZ,
            bb_size_x = @BbSizeX,
            bb_size_y = @BbSizeY,
            bb_size_z = @BbSizeZ
        WHERE id = @Id";

    private readonly SqliteConnection connection;

    public SqliteAddModifyBoundingBoxComponentQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public void Add(ulong entityId, BoundingBox value, IDbTransaction transaction)
    {
        RunQuery(entityId, value, transaction);
    }

    public void Modify(ulong entityId, BoundingBox value, IDbTransaction transaction)
    {
        RunQuery(entityId, value, transaction);
    }

    /// <summary>
    ///     Runs the query for the given component.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="value">Value.</param>
    /// <param name="transaction">Transaction.</param>
    private void RunQuery(ulong entityId, BoundingBox value, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(query, connection, (SqliteTransaction)transaction);

        var pId = new SqliteParameter("Id", SqliteType.Integer);
        pId.Value = entityId;
        cmd.Parameters.Add(pId);

        var pBbPosX = new SqliteParameter("BbPosX", SqliteType.Real);
        pBbPosX.Value = value.Position.X;
        cmd.Parameters.Add(pBbPosX);

        var pBbPosY = new SqliteParameter("BbPosY", SqliteType.Real);
        pBbPosY.Value = value.Position.Y;
        cmd.Parameters.Add(pBbPosY);

        var pBbPosZ = new SqliteParameter("BbPosZ", SqliteType.Real);
        pBbPosZ.Value = value.Position.Z;
        cmd.Parameters.Add(pBbPosZ);

        var pBbSizeX = new SqliteParameter("BbSizeX", SqliteType.Real);
        pBbSizeX.Value = value.Size.X;
        cmd.Parameters.Add(pBbSizeX);

        var pBbSizeY = new SqliteParameter("BbSizeY", SqliteType.Real);
        pBbSizeY.Value = value.Size.Y;
        cmd.Parameters.Add(pBbSizeY);

        var pBbSizeZ = new SqliteParameter("BbSizeZ", SqliteType.Real);
        pBbSizeZ.Value = value.Size.Z;
        cmd.Parameters.Add(pBbSizeZ);

        cmd.ExecuteNonQuery();
    }
}