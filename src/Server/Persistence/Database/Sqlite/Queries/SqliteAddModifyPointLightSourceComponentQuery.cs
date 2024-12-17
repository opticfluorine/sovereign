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

using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite query for adding and modifying point light source components.
/// </summary>
public class SqliteAddModifyPointLightSourceComponentQuery : IAddComponentQuery<PointLight>,
    IModifyComponentQuery<PointLight>
{
    private const string query = @"
        INSERT INTO PointLightSource (id, radius, intensity, color, pos_x, pos_y, pos_z)
        VALUES (@Id, @Radius, @Intensity, @ColorR, @ColorG, @ColorB, @PosX, @PosY, @PosZ)
        ON CONFLICT(id) DO
        UPDATE PointLightSource SET
            radius = excluded.radius,
            intensity = excluded.intensity,
            color = excluded.color_r,
            pos_x = excluded.pos_x,
            pos_y = excluded.pos_y,
            pos_z = excluded.pos_z
            WHERE PointLightSource.id = excluded.id";

    private readonly SqliteConnection connection;

    public SqliteAddModifyPointLightSourceComponentQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public void Add(ulong entityId, PointLight value, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(query, connection, (SqliteTransaction)transaction);

        var pId = new SqliteParameter
        {
            ParameterName = "Id",
            Value = entityId,
            SqliteType = SqliteType.Integer
        };
        cmd.Parameters.Add(pId);

        var pRadius = new SqliteParameter
        {
            ParameterName = "Radius",
            Value = value.Radius,
            SqliteType = SqliteType.Real
        };
        cmd.Parameters.Add(pRadius);

        var pIntensity = new SqliteParameter
        {
            ParameterName = "Intensity",
            Value = value.Intensity,
            SqliteType = SqliteType.Real
        };
        cmd.Parameters.Add(pIntensity);

        var pColorR = new SqliteParameter
        {
            ParameterName = "Color",
            Value = value.Color,
            SqliteType = SqliteType.Integer
        };
        cmd.Parameters.Add(pColorR);

        var pPosX = new SqliteParameter
        {
            ParameterName = "PosX",
            Value = value.PositionOffset.X,
            SqliteType = SqliteType.Real
        };
        cmd.Parameters.Add(pPosX);

        var pPosY = new SqliteParameter
        {
            ParameterName = "PosY",
            Value = value.PositionOffset.Y,
            SqliteType = SqliteType.Real
        };
        cmd.Parameters.Add(pPosY);

        var pPosZ = new SqliteParameter
        {
            ParameterName = "PosZ",
            Value = value.PositionOffset.Z,
            SqliteType = SqliteType.Real
        };
        cmd.Parameters.Add(pPosZ);

        cmd.ExecuteNonQuery();
    }

    public void Modify(ulong entityId, PointLight value, IDbTransaction transaction)
    {
        Add(entityId, value, transaction);
    }
}