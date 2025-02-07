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
        @"SELECT id, NULL, NULL, NULL, NULL, material, materialModifier, NULL, name, NULL, 
                NULL, drawable, animatedSprite, orientation, NULL, castBlockShadows,
                plsRadius, plsIntensity, plsColor, plsPosX, plsPosY, plsPosZ,
                physics, bbPosX, bbPosY, bbPosZ, bbSizeX, bbSizeY, bbSizeZ
            FROM EntityWithComponents WHERE id >= @FirstTemplateId AND id <= @LastTemplateId";

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