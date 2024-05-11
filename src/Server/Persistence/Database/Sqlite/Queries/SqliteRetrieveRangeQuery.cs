﻿/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Data;
using System.Numerics;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite implementation of IRetrieveRangeQuery.
/// </summary>
public sealed class SqliteRetrieveRangeQuery : IRetrieveRangeQuery
{
    /// <summary>
    ///     SQL query.
    /// </summary>
    private const string Query =
        @"WITH RECURSIVE 
            EntityTree(id, x, y, z, material, materialModifier, playerCharacter, name, account, parent, 
                drawable, animatedSprite, orientation, admin, blockX, blockY, blockZ)
	        AS (
	        	SELECT id, x, y, z, material, materialModifier, playerCharacter, name, account, parent, drawable,
                        animatedSprite, orientation, admin, NULL, NULL, NULL
	        		FROM EntityWithComponents
	        		WHERE x >= @X1 AND x < @X2
	        		  AND y >= @Y1 AND y < @Y2
	        		  AND z >= @Z1 AND z < @Z2
	        		  AND playerCharacter IS NULL
	        	UNION ALL
	        	SELECT ec.id, NULL, NULL, NULL, ec.material, ec.materialModifier, ec.playerCharacter, ec.name, 
                        ec.account, ec.parent, ec.drawable, ec.animatedSprite, ec.orientation, ec.admin,
                        NULL, NULL, NULL
	        		FROM EntityWithComponents ec, EntityTree et
	        		WHERE ec.parent = et.id 
                      AND ec.playerCharacter IS NULL
	        ),
            BlockEntityTree(id, x, y, z, material, materialModifier, playerCharacter, name, account, parent,
                drawable, animatedSprite, orientation, admin, blockX, blockY, blockZ)
            AS (
                SELECT id, NULL, NULL, NULL, material, materialModifier, playerCharacter, name, 
                        account, parent, drawable, animatedSprite, orientation, admin, blockX, blockY, blockZ
                    FROM EntityWithComponents
                    WHERE blockX >= @X1 AND blockX < @X2
                      AND blockY >= @Y1 AND blockY < @Y2
                      AND blockZ >= @Z1 AND blockZ < @Z2
                UNION ALL
                SELECT ec.id, NULL, NULL, NULL, ec.material, ec.materialModifier, ec.playerCharacter, ec.name,
                        ec.account, ec.parent, ec.drawable, ec.animatedSprite, ec.orientation, ec.admin,
                        ec.blockX, ec.blockY, ec.blockZ
                    FROM EntityWithComponents ec, BlockEntityTree et
                    WHERE ec.parent = et.id
            )
            SELECT id, x, y, z, material, materialModifier, playerCharacter, name, account, parent, drawable,
                animatedSprite, orientation, admin, blockX, blockY, blockZ 
            FROM EntityTree 
            UNION ALL
            SELECT id, x, y, z, material, materialModifier, playerCharacter, name, account, parent, drawable,
                animatedSprite, orientation, admin, blockX, blockY, blockZ
            FROM BlockEntityTree
            ORDER BY parent NULLS LAST";

    private readonly SqliteConnection dbConnection;

    public SqliteRetrieveRangeQuery(IDbConnection dbConnection)
    {
        this.dbConnection = (SqliteConnection)dbConnection;
    }

    public QueryReader RetrieveEntitiesInRange(Vector3 minPos, Vector3 maxPos)
    {
        var cmd = PrepareCommand(minPos, maxPos);
        return new QueryReader(cmd);
    }

    /// <summary>
    ///     Prepares the SQL command.
    /// </summary>
    /// <param name="minPos">Minimum position.</param>
    /// <param name="maxPos">Maximum position.</param>
    /// <returns>SQL command.</returns>
    private SqliteCommand PrepareCommand(Vector3 minPos, Vector3 maxPos)
    {
        var cmd = new SqliteCommand(Query, dbConnection);

        cmd.Parameters.Add(MakeParameter("X1", minPos.X));
        cmd.Parameters.Add(MakeParameter("X2", maxPos.X));
        cmd.Parameters.Add(MakeParameter("Y1", minPos.Y));
        cmd.Parameters.Add(MakeParameter("Y2", maxPos.Y));
        cmd.Parameters.Add(MakeParameter("Z1", minPos.Z));
        cmd.Parameters.Add(MakeParameter("Z2", maxPos.Z));

        return cmd;
    }

    /// <summary>
    ///     Creates a query parameter.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <returns>Parameter.</returns>
    private SqliteParameter MakeParameter(string name, float value)
    {
        var param = new SqliteParameter(name, value);
        param.SqliteType = SqliteType.Real;
        return param;
    }
}