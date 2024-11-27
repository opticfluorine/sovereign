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

using System;
using System.Data;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Main;
using Sovereign.Persistence.Configuration;
using Sovereign.Persistence.Database.Queries;
using Sovereign.Persistence.Database.Sqlite.Queries;

namespace Sovereign.Persistence.Database.Sqlite;

/// <summary>
///     SQLite persistence provider.
/// </summary>
public sealed class SqlitePersistenceProvider : IPersistenceProvider
{
    private const string PositionTableName = "Position";

    private const string MaterialTableName = "Material";
    private const string MaterialParamName = "material";
    private const SqliteType MaterialParamType = SqliteType.Integer;

    private const string MaterialModifierTableName = "MaterialModifier";
    private const string MaterialModifierParamName = "modifier";
    private const SqliteType MaterialModifierParamType = SqliteType.Integer;

    private const string PlayerCharacterTableName = "PlayerCharacter";
    private const string PlayerCharacterParamName = "value";
    private const SqliteType PlayerCharacterParamType = SqliteType.Integer;

    private const string NameTableName = "Name";
    private const string NameParamName = "value";
    private const SqliteType NameParamType = SqliteType.Text;

    private const string AccountComponentTableName = "AccountComponent";
    private const string AccountComponentParamName = "account_id";

    private const SqliteType ParentParamType = SqliteType.Integer;
    private const string ParentParamName = "parent_id";
    private const string ParentTableName = "Parent";

    private const SqliteType DrawableParamType = SqliteType.Integer;
    private const string DrawableTableName = "Drawable";
    private const string DrawableParamName = "value";

    private const SqliteType AnimatedSpriteParamType = SqliteType.Integer;
    private const string AnimatedSpriteTableName = "AnimatedSprite";
    private const string AnimatedSpriteParamName = "value";

    private const string OrientationTableName = "Orientation";
    private const string OrientationParamName = "value";
    private const SqliteType OrientationParamType = SqliteType.Integer;

    private const SqliteType AdminParamType = SqliteType.Integer;
    private const string AdminTableName = "Admin";
    private const string AdminParamName = "value";

    private const SqliteType CastBlockShadowsParamType = SqliteType.Integer;
    private const string CastBlockShadowsTableName = "CastBlockShadows";
    private const string CastBlockShadowsParamName = "value";

    private const string PointLightSourceTableName = "PointLightSource";

    private readonly IPersistenceConfiguration configuration;
    private readonly ILogger<SqlitePersistenceProvider> logger;

    public SqlitePersistenceProvider(IPersistenceConfiguration configuration,
        ILogger<SqlitePersistenceProvider> logger)
    {
        this.configuration = configuration;
        this.logger = logger;

        Connect();
        if (Connection == null)
        {
            logger.LogCritical("Connection unexpectedly null.");
            throw new FatalErrorException();
        }

        MigrationQuery = new SqliteMigrationQuery(Connection);
        NextPersistedIdQuery = new SqliteNextPersistedIdQuery(Connection);
        RetrieveAllTemplatesQuery = new SqliteRetrieveAllTemplatesQuery((SqliteConnection)Connection);

        AddAccountQuery = new SqliteAddAccountQuery(Connection);
        RetrieveAccountQuery = new SqliteRetrieveAccountQuery(Connection);
        RetrieveAccountWithAuthQuery = new SqliteRetrieveAccountWithAuthQuery(Connection);

        RetrieveEntityQuery = new SqliteRetrieveEntityQuery(Connection);
        RetrieveRangeQuery = new SqliteRetrieveRangeQuery(Connection);

        AddEntityQuery = new SqliteAddEntityQuery(Connection);
        RemoveEntityQuery = new SqliteRemoveEntityQuery(Connection);

        SetTemplateQuery = new SqliteSetTemplateQuery((SqliteConnection)Connection);

        PlayerExistsQuery = new SqlitePlayerExistsQuery((SqliteConnection)Connection);
        GetAccountForPlayerQuery = new SqliteGetAccountForPlayerQuery((SqliteConnection)Connection);
        ListPlayersQuery = new SqliteListPlayersQuery((SqliteConnection)Connection);
        DeletePlayerQuery = new SqliteDeletePlayerQuery((SqliteConnection)Connection);

        AddAdminRoleQuery = new SqliteAddAdminRoleQuery((SqliteConnection)Connection);
        RemoveAdminRoleQuery = new SqliteRemoveAdminRoleQuery((SqliteConnection)Connection);

        /* Position component. */
        AddPositionQuery = new SqliteAddPositionComponentQuery((SqliteConnection)Connection);
        ModifyPositionQuery = new SqliteModifyPositionComponentQuery((SqliteConnection)Connection);
        RemovePositionQuery = new SqliteRemoveComponentQuery(PositionTableName,
            (SqliteConnection)Connection);

        /* Material component. */
        AddMaterialQuery = new SimpleSqliteAddComponentQuery<int>(
            MaterialTableName, MaterialParamName, MaterialParamType,
            (SqliteConnection)Connection);
        ModifyMaterialQuery = new SimpleSqliteModifyComponentQuery<int>(
            MaterialTableName, MaterialParamName, MaterialParamType,
            (SqliteConnection)Connection);
        RemoveMaterialQuery = new SqliteRemoveComponentQuery(MaterialTableName,
            (SqliteConnection)Connection);

        /* MaterialModifier component. */
        AddMaterialModifierQuery = new SimpleSqliteAddComponentQuery<int>(
            MaterialModifierTableName, MaterialModifierParamName,
            MaterialModifierParamType, (SqliteConnection)Connection);
        ModifyMaterialModifierQuery = new SimpleSqliteModifyComponentQuery<int>(
            MaterialModifierTableName, MaterialModifierParamName,
            MaterialModifierParamType, (SqliteConnection)Connection);
        RemoveMaterialModifierQuery = new SqliteRemoveComponentQuery(MaterialModifierTableName,
            (SqliteConnection)Connection);

        /* PlayerCharacter tag. */
        AddPlayerCharacterQuery = new SimpleSqliteAddComponentQuery<bool>(PlayerCharacterTableName,
            PlayerCharacterParamName, PlayerCharacterParamType, (SqliteConnection)Connection);
        ModifyPlayerCharacterQuery = new SimpleSqliteModifyComponentQuery<bool>(PlayerCharacterTableName,
            PlayerCharacterParamName, PlayerCharacterParamType, (SqliteConnection)Connection);
        RemovePlayerCharacterQuery = new SqliteRemoveComponentQuery(PlayerCharacterTableName,
            (SqliteConnection)Connection);

        /* Name component. */
        AddNameQuery = new SimpleSqliteAddComponentQuery<string>(NameTableName,
            NameParamName, NameParamType, (SqliteConnection)Connection);
        ModifyNameQuery = new SimpleSqliteModifyComponentQuery<string>(NameTableName, NameParamName,
            NameParamType, (SqliteConnection)Connection);
        RemoveNameQuery = new SqliteRemoveComponentQuery(NameTableName, (SqliteConnection)Connection);

        /* Account component. */
        AddAccountComponentQuery = new GuidSqliteAddComponentQuery(AccountComponentTableName,
            AccountComponentParamName, (SqliteConnection)Connection);
        ModifyAccountComponentQuery = new GuidSqliteModifyComponentQuery(AccountComponentTableName,
            AccountComponentParamName, (SqliteConnection)Connection);
        RemoveAccountComponentQuery = new SqliteRemoveComponentQuery(AccountComponentTableName,
            (SqliteConnection)Connection);

        // Parent component.
        AddParentComponentQuery = new SimpleSqliteAddComponentQuery<ulong>(ParentTableName, ParentParamName,
            ParentParamType, (SqliteConnection)Connection);
        ModifyParentComponentQuery = new SimpleSqliteModifyComponentQuery<ulong>(ParentTableName, ParentParamName,
            ParentParamType, (SqliteConnection)Connection);
        RemoveParentComponentQuery = new SqliteRemoveComponentQuery(ParentTableName, (SqliteConnection)Connection);

        // Drawable component.
        AddDrawableComponentQuery = new SimpleSqliteAddComponentQuery<bool>(DrawableTableName, DrawableParamName,
            DrawableParamType, (SqliteConnection)Connection);
        ModifyDrawableComponentQuery = new SimpleSqliteModifyComponentQuery<bool>(DrawableTableName,
            DrawableParamName,
            DrawableParamType, (SqliteConnection)Connection);
        RemoveDrawableComponentQuery =
            new SqliteRemoveComponentQuery(DrawableTableName, (SqliteConnection)Connection);

        // AnimatedSprite component.
        AddAnimatedSpriteComponentQuery = new SimpleSqliteAddComponentQuery<int>(AnimatedSpriteTableName,
            AnimatedSpriteParamName, AnimatedSpriteParamType, (SqliteConnection)Connection);
        ModifyAnimatedSpriteComponentQuery = new SimpleSqliteModifyComponentQuery<int>(AnimatedSpriteTableName,
            AnimatedSpriteParamName, AnimatedSpriteParamType, (SqliteConnection)Connection);
        RemoveAnimatedSpriteComponentQuery = new SqliteRemoveComponentQuery(
            AnimatedSpriteTableName, (SqliteConnection)Connection);

        // Orientation component.
        AddOrientationComponentQuery = new SimpleSqliteAddComponentQuery<Orientation>(OrientationTableName,
            OrientationParamName, OrientationParamType, (SqliteConnection)Connection);
        ModifyOrientationComponentQuery = new SimpleSqliteModifyComponentQuery<Orientation>(OrientationTableName,
            OrientationParamName, OrientationParamType, (SqliteConnection)Connection);
        RemoveOrientationComponentQuery =
            new SqliteRemoveComponentQuery(OrientationTableName, (SqliteConnection)Connection);

        // Admin component.
        AddAdminComponentQuery = new SimpleSqliteAddComponentQuery<bool>(AdminTableName,
            AdminParamName, AdminParamType, (SqliteConnection)Connection);
        ModifyAdminComponentQuery = new SimpleSqliteModifyComponentQuery<bool>(AdminTableName,
            AdminParamName, AdminParamType, (SqliteConnection)Connection);
        RemoveAdminComponentQuery = new SqliteRemoveComponentQuery(AdminTableName, (SqliteConnection)Connection);

        GetWorldSegmentBlockDataQuery = new SqliteGetWorldSegmentBlockDataQuery((SqliteConnection)Connection);
        SetWorldSegmentBlockDataQuery = new SqliteSetWorldSegmentBlockDataQuery((SqliteConnection)Connection);

        // CastBlockShadows component.
        AddCastBlockShadowsComponentQuery = new SimpleSqliteAddComponentQuery<bool>(CastBlockShadowsTableName,
            CastBlockShadowsParamName, CastBlockShadowsParamType, (SqliteConnection)Connection);
        ModifyCastBlockShadowsComponentQuery = new SimpleSqliteModifyComponentQuery<bool>(CastBlockShadowsTableName,
            CastBlockShadowsParamName, CastBlockShadowsParamType, (SqliteConnection)Connection);
        RemoveCastBlockShadowsComponentQuery = new SqliteRemoveComponentQuery(CastBlockShadowsTableName,
            (SqliteConnection)Connection);

        // PointLightSource component.
        var pointLightSourceQuery = new SqliteAddModifyPointLightSourceComponentQuery((SqliteConnection)Connection);
        AddPointLightSourceComponentQuery = pointLightSourceQuery;
        ModifyPointLightSourceComponentQuery = pointLightSourceQuery;
        RemovePointLightSourceComponentQuery =
            new SqliteRemoveComponentQuery(PointLightSourceTableName, (SqliteConnection)Connection);
    }

    public ISetTemplateQuery SetTemplateQuery { get; }
    public IRemoveComponentQuery RemoveAdminComponentQuery { get; }
    public IAddComponentQuery<string> AddNameQuery { get; }
    public IModifyComponentQuery<string> ModifyNameQuery { get; }
    public IRemoveComponentQuery RemoveNameQuery { get; }

    public IMigrationQuery MigrationQuery { get; }

    public INextPersistedIdQuery NextPersistedIdQuery { get; }

    public IRetrieveAllTemplatesQuery RetrieveAllTemplatesQuery { get; }

    public IAddAccountQuery AddAccountQuery { get; }

    public IRetrieveAccountQuery RetrieveAccountQuery { get; }

    public IRetrieveAccountWithAuthQuery RetrieveAccountWithAuthQuery { get; }

    public IRetrieveEntityQuery RetrieveEntityQuery { get; }

    public IRetrieveRangeQuery RetrieveRangeQuery { get; }

    public IDbConnection Connection { get; private set; }

    public IAddEntityQuery AddEntityQuery { get; }

    public IRemoveEntityQuery RemoveEntityQuery { get; }
    public IAddComponentQuery<Kinematics> AddPositionQuery { get; }
    public IModifyComponentQuery<Kinematics> ModifyPositionQuery { get; }
    public IRemoveComponentQuery RemovePositionQuery { get; }
    public IAddComponentQuery<int> AddMaterialQuery { get; }
    public IModifyComponentQuery<int> ModifyMaterialQuery { get; }
    public IRemoveComponentQuery RemoveMaterialQuery { get; }
    public IAddComponentQuery<int> AddMaterialModifierQuery { get; }
    public IModifyComponentQuery<int> ModifyMaterialModifierQuery { get; }
    public IRemoveComponentQuery RemoveMaterialModifierQuery { get; }
    public IAddComponentQuery<bool> AddPlayerCharacterQuery { get; }
    public IModifyComponentQuery<bool> ModifyPlayerCharacterQuery { get; }
    public IRemoveComponentQuery RemovePlayerCharacterQuery { get; }
    public IAddComponentQuery<Guid> AddAccountComponentQuery { get; }
    public IModifyComponentQuery<Guid> ModifyAccountComponentQuery { get; }
    public IRemoveComponentQuery RemoveAccountComponentQuery { get; }
    public IAddComponentQuery<ulong> AddParentComponentQuery { get; }
    public IModifyComponentQuery<ulong> ModifyParentComponentQuery { get; }
    public IRemoveComponentQuery RemoveParentComponentQuery { get; }
    public IAddComponentQuery<bool> AddDrawableComponentQuery { get; }
    public IModifyComponentQuery<bool> ModifyDrawableComponentQuery { get; }
    public IRemoveComponentQuery RemoveDrawableComponentQuery { get; }
    public IAddComponentQuery<int> AddAnimatedSpriteComponentQuery { get; }
    public IModifyComponentQuery<int> ModifyAnimatedSpriteComponentQuery { get; }
    public IRemoveComponentQuery RemoveAnimatedSpriteComponentQuery { get; }
    public IAddComponentQuery<Orientation> AddOrientationComponentQuery { get; }
    public IModifyComponentQuery<Orientation> ModifyOrientationComponentQuery { get; }
    public IRemoveComponentQuery RemoveOrientationComponentQuery { get; }
    public IAddComponentQuery<bool> AddAdminComponentQuery { get; }
    public IModifyComponentQuery<bool> ModifyAdminComponentQuery { get; }
    public IAddComponentQuery<bool> AddCastBlockShadowsComponentQuery { get; }
    public IModifyComponentQuery<bool> ModifyCastBlockShadowsComponentQuery { get; }
    public IRemoveComponentQuery RemoveCastBlockShadowsComponentQuery { get; }
    public IPlayerExistsQuery PlayerExistsQuery { get; }
    public IGetAccountForPlayerQuery GetAccountForPlayerQuery { get; }
    public IListPlayersQuery ListPlayersQuery { get; }
    public IDeletePlayerQuery DeletePlayerQuery { get; }
    public IAddAdminRoleQuery AddAdminRoleQuery { get; }
    public IRemoveAdminRoleQuery RemoveAdminRoleQuery { get; }
    public IGetWorldSegmentBlockDataQuery GetWorldSegmentBlockDataQuery { get; }
    public ISetWorldSegmentBlockDataQuery SetWorldSegmentBlockDataQuery { get; }
    public IAddComponentQuery<PointLight> AddPointLightSourceComponentQuery { get; }
    public IModifyComponentQuery<PointLight> ModifyPointLightSourceComponentQuery { get; }
    public IRemoveComponentQuery RemovePointLightSourceComponentQuery { get; }

    public void Dispose()
    {
        logger.LogInformation("Closing the SQLite database if open.");

        Connection.Close();

        logger.LogInformation("SQLite database closed.");
    }

    /// <summary>
    ///     Opens the SQLite database.
    /// </summary>
    private void Connect()
    {
        logger.LogInformation("Opening the SQLite database at {0}.",
            configuration.Host);

        var connString = CreateConnectionString();
        try
        {
            Connection = new SqliteConnection(connString);
            Connection.Open();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Failed to open the SQLite database.");
            throw new FatalErrorException();
        }

        logger.LogInformation("SQLite database opened.");
    }

    /// <summary>
    ///     Creates the SQLite connection string.
    /// </summary>
    /// <returns>Connection string.</returns>
    private string CreateConnectionString()
    {
        var sb = new StringBuilder();

        sb.Append("Data Source=")
            .Append(configuration.Host);

        return sb.ToString();
    }
}