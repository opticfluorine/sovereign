/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, ET.either version 3 of the License, ET.or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, ET.see <https://www.gnu.org/licenses/>.
 */

using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite implementation of IRetrieveEntityQuery.
/// </summary>
public sealed class SqliteRetrieveEntityQuery : IRetrieveEntityQuery
{
    /// <summary>
    ///     SQL query to execute.
    /// </summary>
    private const string query =
        @"WITH RECURSIVE EntityTree(id, template_id, x, y, z, material, materialModifier, playerCharacter, name, account, 
                parent, drawableX, drawableY, animatedSprite, orientation, admin, castBlockShadows, plsRadius, plsIntensity,
                plsColor, plsPosX, plsPosY, plsPosZ, physics, bbPosX, bbPosY, bbPosZ, bbSizeX, bbSizeY, bbSizeZ,
                shadowRadius, entityType, serverOnly)
	        AS (
		        SELECT id, template_id, x, y, z, material, materialModifier, playerCharacter, name, account, parent, 
                        drawableX, drawableY, animatedSprite, orientation, admin, castBlockShadows, plsRadius, plsIntensity,
                        plsColor, plsPosX, plsPosY, plsPosZ, physics, bbPosX, bbPosY, bbPosZ, bbSizeX, bbSizeY, bbSizeZ,
                        shadowRadius, entityType, serverOnly
			        FROM EntityWithComponents WHERE id = @Id
		        UNION ALL
            		SELECT ec.id, ec.template_id, ec.x, ec.y, ec.z, ec.material, ec.materialModifier, ec.playerCharacter,
                        ec.name, ec.account, ec.parent, ec.drawableX, ec.drawableY, ec.animatedSprite, ec.orientation, ec.admin,
                        ec.castBlockShadows, ec.plsRadius, ec.plsIntensity, ec.plsColor,
                        ec.plsPosX, ec.plsPosY, ec.plsPosZ, ec.physics, ec.bbPosX, ec.bbPosY, ec.bbPosZ,
                        ec.bbSizeX, ec.bbSizeY, ec.bbSizeZ, ec.shadowRadius, ec.entityType, ec.serverOnly
			        FROM EntityWithComponents ec, EntityTree et
        			WHERE ec.parent = et.id
	        )
            SELECT et.id, kv.key, kv.value, et.template_id, et.x, et.y, et.z, et.material, et.materialModifier, et.playerCharacter, et.name, 
                et.account, et.parent, et.drawableX, et.drawableY, et.animatedSprite, et.orientation, et.admin, et.castBlockShadows, et.plsRadius, 
                et.plsIntensity, et.plsColor, et.plsPosX, et.plsPosY, et.plsPosZ, et.physics, et.bbPosX, et.bbPosY, et.bbPosZ, 
                et.bbSizeX, et.bbSizeY, et.bbSizeZ, et.shadowRadius, et.entityType, et.serverOnly
            FROM EntityTree et
            LEFT JOIN EntityKeyValue kv ON kv.entity_id = et.id
            ORDER BY et.parent NULLS LAST";

    private readonly SqliteConnection dbConnection;

    public SqliteRetrieveEntityQuery(IDbConnection dbConnection)
    {
        this.dbConnection = (SqliteConnection)dbConnection;
    }

    public QueryReader RetrieveEntity(ulong entityId)
    {
        var cmd = PrepareCommand(entityId);
        return new QueryReader(cmd);
    }

    /// <summary>
    ///     Prepares the SQL command.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>SQL command.</returns>
    private SqliteCommand PrepareCommand(ulong entityId)
    {
        var cmd = new SqliteCommand(query, dbConnection);

        var param = new SqliteParameter("Id", entityId);
        param.SqliteType = SqliteType.Integer;
        param.Value = entityId;
        cmd.Parameters.Add(param);

        return cmd;
    }
}