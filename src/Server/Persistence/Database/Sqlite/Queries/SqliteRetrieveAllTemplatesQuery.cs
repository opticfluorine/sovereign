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

using Microsoft.Data.Sqlite;
using Sovereign.EngineCore.Entities;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite implementation of IRetrieveAllTemplatesQuery.
/// </summary>
public class SqliteRetrieveAllTemplatesQuery : IRetrieveAllTemplatesQuery
{
    /// <summary>
    ///     Query. Disallowed components for templates are nulled out to maintain compatibility
    ///     with EntityProcessor.
    /// </summary>
    private const string query =
        @"SELECT id, kv.key, kv.value, NULL, NULL, NULL, NULL, front_tile_id, top_tile_id, NULL, name, NULL, 
                NULL, drawable_x, drawable_y, animated_sprite, orientation, NULL, cast_block_shadows,
                pls_radius, pls_intensity, pls_color, pls_pos_x, pls_pos_y, pls_pos_z,
                physics, bb_pos_x, bb_pos_y, bb_pos_z, bb_size_x, bb_size_y, bb_size_z, shadow_radius, entity_type, server_only,
                stackable, quantity, item_use, npc_flags, use_range,
                health_value, health_max_value, health_change_rate, health_change_interval,
                stamina_value, stamina_max_value, stamina_change_rate, stamina_change_interval,
                mana_value, mana_max_value, mana_change_rate, mana_change_interval,
                stats_strength, stats_defense, stats_agility, stats_intelligence, stats_wisdom, stats_charisma, stats_luck,
                equipment_type, level, experience,
                radiant_category, radiant_function, radiant_param0, radiant_param1, radiant_param2, player_flags
            FROM Entity
            LEFT JOIN EntityKeyValue kv ON kv.entity_id = id
            WHERE id >= @FirstTemplateId AND id <= @LastTemplateId";

    private readonly SqliteConnection connection;

    public SqliteRetrieveAllTemplatesQuery(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public QueryReader RetrieveAllTemplates()
    {
        var cmd = new SqliteCommand(query, connection);

        var pFirstTemplateId = new SqliteParameter("FirstTemplateId", SqliteType.Integer);
        pFirstTemplateId.Value = EntityConstants.FirstTemplateEntityId;
        cmd.Parameters.Add(pFirstTemplateId);

        var pLastTemplateId = new SqliteParameter("LastTemplateId", SqliteType.Integer);
        pLastTemplateId.Value = EntityConstants.LastTemplateEntityId;
        cmd.Parameters.Add(pLastTemplateId);

        return new QueryReader(cmd);
    }
}
