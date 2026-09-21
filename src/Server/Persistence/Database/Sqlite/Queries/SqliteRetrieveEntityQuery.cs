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
        @"WITH RECURSIVE EntityTree(id, template_id, x, y, z, frontTileId, topTileId, playerCharacter, name, account, 
                parent, drawableX, drawableY, animatedSprite, orientation, admin, castBlockShadows, plsRadius, plsIntensity,
                plsColor, plsPosX, plsPosY, plsPosZ, physics, bbPosX, bbPosY, bbPosZ, bbSizeX, bbSizeY, bbSizeZ,
                shadowRadius, entityType, serverOnly, stackable, quantity, itemUse, npcFlags, useRange,
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
		        FROM Entity WHERE id = @Id
	        UNION ALL
            	SELECT ec.id, ec.template_id, ec.pos_x, ec.pos_y, ec.pos_z, ec.front_tile_id, ec.top_tile_id, ec.player_char,
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
	        )
            SELECT et.id, kv.key, kv.value, et.template_id, et.x, et.y, et.z, et.frontTileId, et.topTileId, et.playerCharacter, et.name, 
                et.account, et.parent, et.drawableX, et.drawableY, et.animatedSprite, et.orientation, et.admin, et.castBlockShadows, et.plsRadius, 
                et.plsIntensity, et.plsColor, et.plsPosX, et.plsPosY, et.plsPosZ, et.physics, et.bbPosX, et.bbPosY, et.bbPosZ, 
                et.bbSizeX, et.bbSizeY, et.bbSizeZ, et.shadowRadius, et.entityType, et.serverOnly, et.stackable,
                et.quantity, et.itemUse, et.npcFlags, et.useRange,
                et.healthValue, et.healthMaxValue, et.healthChangeRate, et.healthChangeInterval,
                et.staminaValue, et.staminaMaxValue, et.staminaChangeRate, et.staminaChangeInterval,
                et.manaValue, et.manaMaxValue, et.manaChangeRate, et.manaChangeInterval,
                et.statsStrength, et.statsDefense, et.statsAgility, et.statsIntelligence, et.statsWisdom, et.statsCharisma, et.statsLuck,
                et.equipmentType, et.level, et.experience,
                et.radiantCategory, et.radiantFunction, et.radiantParam0, et.radiantParam1, et.radiantParam2, et.playerFlags
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
