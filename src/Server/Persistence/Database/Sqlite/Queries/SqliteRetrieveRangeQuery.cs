/*
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
            EntityTree(id, template_id, x, y, z, frontTileId, topTileId, playerCharacter, name, account, parent, 
                drawableX, drawableY, animatedSprite, orientation, admin, castBlockShadows, plsRadius, plsIntensity, plsColor,
                plsPosX, plsPosY, plsPosZ, physics, bbPosX, bbPosY, bbPosZ, bbSizeX, bbSizeY, bbSizeZ, shadowRadius,
                entityType, serverOnly, stackable, quantity, itemUse, npcFlags, useRange,
                healthValue, healthMaxValue, healthChangeRate, healthChangeInterval,
                staminaValue, staminaMaxValue, staminaChangeRate, staminaChangeInterval,
                manaValue, manaMaxValue, manaChangeRate, manaChangeInterval,
                statsStrength, statsDefense, statsAgility, statsIntelligence, statsWisdom, statsCharisma, statsLuck,
                equipmentType, level, experience,
                radiantCategory, radiantFunction, radiantParam0, radiantParam1, radiantParam2, playerFlags)
	        AS (
	        	SELECT id, template_id, pos_x AS x, pos_y AS y, pos_z AS z,
                        front_tile_id AS frontTileId, top_tile_id AS topTileId, player_char AS playerCharacter,
                        name, account_id AS account, parent_id AS parent,
                        drawable_x AS drawableX, drawable_y AS drawableY, animated_sprite AS animatedSprite, orientation, admin,
                        cast_block_shadows AS castBlockShadows, pls_radius AS plsRadius, pls_intensity AS plsIntensity,
                        pls_color AS plsColor, pls_pos_x AS plsPosX, pls_pos_y AS plsPosY, pls_pos_z AS plsPosZ, physics,
                        bb_pos_x AS bbPosX, bb_pos_y AS bbPosY, bb_pos_z AS bbPosZ,
                        bb_size_x AS bbSizeX, bb_size_y AS bbSizeY, bb_size_z AS bbSizeZ,
                        shadow_radius AS shadowRadius, entity_type AS entityType, server_only AS serverOnly,
                        stackable, quantity, item_use AS itemUse, npc_flags AS npcFlags, use_range AS useRange,
                        health_value AS healthValue, health_max_value AS healthMaxValue, 
                        health_change_rate AS healthChangeRate, health_change_interval AS healthChangeInterval,
                        stamina_value AS staminaValue, stamina_max_value AS staminaMaxValue, 
                        stamina_change_rate AS staminaChangeRate, stamina_change_interval AS staminaChangeInterval,
                        mana_value AS manaValue, mana_max_value AS manaMaxValue, 
                        mana_change_rate AS manaChangeRate, mana_change_interval AS manaChangeInterval,
                        stats_strength AS statsStrength, stats_defense AS statsDefense, stats_agility AS statsAgility, 
                        stats_intelligence AS statsIntelligence, stats_wisdom AS statsWisdom, stats_charisma AS statsCharisma, stats_luck AS statsLuck,
                        equipment_type AS equipmentType, level, experience,
                        radiant_category AS radiantCategory, radiant_function AS radiantFunction, 
                        radiant_param0 AS radiantParam0, radiant_param1 AS radiantParam1, radiant_param2 AS radiantParam2,
                        player_flags AS playerFlags
	        		FROM Entity
	        		WHERE pos_x >= @X1 AND pos_x < @X2
	        		  AND pos_y >= @Y1 AND pos_y < @Y2
	        		  AND pos_z >= @Z1 AND pos_z < @Z2
	        		  AND player_char IS NULL
	        UNION ALL
	        	SELECT ec.id, ec.template_id, NULL, NULL, NULL, ec.front_tile_id, ec.top_tile_id, ec.player_char, 
                        ec.name, ec.account_id, ec.parent_id, ec.drawable_x, ec.drawable_y, ec.animated_sprite, ec.orientation, ec.admin,
                        ec.cast_block_shadows, ec.pls_radius, ec.pls_intensity, ec.pls_color,
                        ec.pls_pos_x, ec.pls_pos_y, ec.pls_pos_z, ec.physics, ec.bb_pos_x, ec.bb_pos_y, ec.bb_pos_z,
                        ec.bb_size_x, ec.bb_size_y, ec.bb_size_z, ec.shadow_radius, ec.entity_type, ec.server_only,
                        ec.stackable, ec.quantity, ec.item_use, ec.npc_flags, ec.use_range,
                        ec.health_value, ec.health_max_value, ec.health_change_rate, ec.health_change_interval,
                        ec.stamina_value, ec.stamina_max_value, ec.stamina_change_rate, ec.stamina_change_interval,
                        ec.mana_value, ec.mana_max_value, ec.mana_change_rate, ec.mana_change_interval,
                        ec.stats_strength, ec.stats_defense, ec.stats_agility, ec.stats_intelligence, ec.stats_wisdom, ec.stats_charisma, ec.stats_luck,
                        ec.equipment_type, ec.level, ec.experience,
                        ec.radiant_category, ec.radiant_function, ec.radiant_param0, ec.radiant_param1, ec.radiant_param2, ec.player_flags
	        		FROM Entity ec, EntityTree et
	        		WHERE ec.parent_id = et.id 
                      AND ec.player_char IS NULL
	        )
            SELECT id, kv.key, kv.value, template_id, x, y, z, frontTileId, topTileId, playerCharacter, name, account, parent,
                drawableX, drawableY, animatedSprite, orientation, admin, castBlockShadows, plsRadius, plsIntensity, plsColor,
                plsPosX, plsPosY, plsPosZ, physics, bbPosX, bbPosY, bbPosZ, bbSizeX, bbSizeY, bbSizeZ, shadowRadius,
                entityType, serverOnly, stackable, quantity, itemUse, npcFlags, useRange,
                healthValue, healthMaxValue, healthChangeRate, healthChangeInterval,
                staminaValue, staminaMaxValue, staminaChangeRate, staminaChangeInterval,
                manaValue, manaMaxValue, manaChangeRate, manaChangeInterval,
                statsStrength, statsDefense, statsAgility, statsIntelligence, statsWisdom, statsCharisma, statsLuck,
                equipmentType, level, experience,
                radiantCategory, radiantFunction, radiantParam0, radiantParam1, radiantParam2, playerFlags
            FROM EntityTree 
            LEFT JOIN EntityKeyValue kv ON kv.entity_id = id
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
