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
using Sovereign.Persistence.Database.Queries;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.Persistence.Database.Sqlite;

/// <summary>
///     SQLite persistence provider.
/// </summary>
public sealed class SqlitePersistenceProvider : IPersistenceProvider
{
    private const string MaterialParamName = "material";
    private const SqliteType MaterialParamType = SqliteType.Integer;

    private const string MaterialModifierParamName = "material_mod";
    private const SqliteType MaterialModifierParamType = SqliteType.Integer;

    private const string PlayerCharacterParamName = "player_char";
    private const SqliteType PlayerCharacterParamType = SqliteType.Integer;

    private const string NameParamName = "name";
    private const SqliteType NameParamType = SqliteType.Text;

    private const string AccountComponentParamName = "account_id";

    private const SqliteType ParentParamType = SqliteType.Integer;
    private const string ParentParamName = "parent_id";

    private const SqliteType DrawableParamType = SqliteType.Integer;
    private const string DrawableParamName = "drawable";

    private const SqliteType AnimatedSpriteParamType = SqliteType.Integer;
    private const string AnimatedSpriteParamName = "animated_sprite";

    private const string OrientationParamName = "orientation";
    private const SqliteType OrientationParamType = SqliteType.Integer;

    private const SqliteType AdminParamType = SqliteType.Integer;
    private const string AdminParamName = "admin";

    private const SqliteType CastBlockShadowsParamType = SqliteType.Integer;
    private const string CastBlockShadowsParamName = "cast_block_shadows";
    private const string PositionColumnPrefix = "pos_";

    private const SqliteType PhysicsParamType = SqliteType.Integer;
    private const string PhysicsParamName = "physics";

    private readonly DatabaseOptions configuration;
    private readonly ILogger<SqlitePersistenceProvider> logger;

    public SqlitePersistenceProvider(DatabaseOptions configuration,
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
        RemovePositionQuery = new Vector3SqliteRemoveComponentQuery(PositionColumnPrefix, (SqliteConnection)Connection);

        /* Material component. */
        AddMaterialQuery = new SimpleSqliteAddComponentQuery<int>(
            MaterialParamName, MaterialParamType,
            (SqliteConnection)Connection);
        ModifyMaterialQuery = new SimpleSqliteModifyComponentQuery<int>(
            MaterialParamName, MaterialParamType,
            (SqliteConnection)Connection);
        RemoveMaterialQuery = new SimpleSqliteRemoveComponentQuery(MaterialParamName,
            (SqliteConnection)Connection);

        /* MaterialModifier component. */
        AddMaterialModifierQuery = new SimpleSqliteAddComponentQuery<int>(
            MaterialModifierParamName,
            MaterialModifierParamType, (SqliteConnection)Connection);
        ModifyMaterialModifierQuery = new SimpleSqliteModifyComponentQuery<int>(
            MaterialModifierParamName,
            MaterialModifierParamType, (SqliteConnection)Connection);
        RemoveMaterialModifierQuery = new SimpleSqliteRemoveComponentQuery(MaterialModifierParamName,
            (SqliteConnection)Connection);

        /* PlayerCharacter tag. */
        AddPlayerCharacterQuery = new SimpleSqliteAddComponentQuery<bool>(
            PlayerCharacterParamName, PlayerCharacterParamType, (SqliteConnection)Connection);
        ModifyPlayerCharacterQuery = new SimpleSqliteModifyComponentQuery<bool>(
            PlayerCharacterParamName, PlayerCharacterParamType, (SqliteConnection)Connection);
        RemovePlayerCharacterQuery = new SimpleSqliteRemoveComponentQuery(PlayerCharacterParamName,
            (SqliteConnection)Connection);

        /* Name component. */
        AddNameQuery = new SimpleSqliteAddComponentQuery<string>(
            NameParamName, NameParamType, (SqliteConnection)Connection);
        ModifyNameQuery = new SimpleSqliteModifyComponentQuery<string>(NameParamName,
            NameParamType, (SqliteConnection)Connection);
        RemoveNameQuery = new SimpleSqliteRemoveComponentQuery(NameParamName, (SqliteConnection)Connection);

        /* Account component. */
        AddAccountComponentQuery = new GuidSqliteAddComponentQuery(
            AccountComponentParamName, (SqliteConnection)Connection);
        ModifyAccountComponentQuery = new GuidSqliteModifyComponentQuery(
            AccountComponentParamName, (SqliteConnection)Connection);
        RemoveAccountComponentQuery = new SimpleSqliteRemoveComponentQuery(AccountComponentParamName,
            (SqliteConnection)Connection);

        // Parent component.
        AddParentComponentQuery = new SimpleSqliteAddComponentQuery<ulong>(ParentParamName,
            ParentParamType, (SqliteConnection)Connection);
        ModifyParentComponentQuery = new SimpleSqliteModifyComponentQuery<ulong>(ParentParamName,
            ParentParamType, (SqliteConnection)Connection);
        RemoveParentComponentQuery =
            new SimpleSqliteRemoveComponentQuery(ParentParamName, (SqliteConnection)Connection);

        // Drawable component.
        AddDrawableComponentQuery = new SimpleSqliteAddComponentQuery<bool>(DrawableParamName,
            DrawableParamType, (SqliteConnection)Connection);
        ModifyDrawableComponentQuery = new SimpleSqliteModifyComponentQuery<bool>(
            DrawableParamName,
            DrawableParamType, (SqliteConnection)Connection);
        RemoveDrawableComponentQuery =
            new SimpleSqliteRemoveComponentQuery(DrawableParamName, (SqliteConnection)Connection);

        // AnimatedSprite component.
        AddAnimatedSpriteComponentQuery = new SimpleSqliteAddComponentQuery<int>(
            AnimatedSpriteParamName, AnimatedSpriteParamType, (SqliteConnection)Connection);
        ModifyAnimatedSpriteComponentQuery = new SimpleSqliteModifyComponentQuery<int>(
            AnimatedSpriteParamName, AnimatedSpriteParamType, (SqliteConnection)Connection);
        RemoveAnimatedSpriteComponentQuery = new SimpleSqliteRemoveComponentQuery(
            AnimatedSpriteParamName, (SqliteConnection)Connection);

        // Orientation component.
        AddOrientationComponentQuery = new SimpleSqliteAddComponentQuery<Orientation>(
            OrientationParamName, OrientationParamType, (SqliteConnection)Connection);
        ModifyOrientationComponentQuery = new SimpleSqliteModifyComponentQuery<Orientation>(
            OrientationParamName, OrientationParamType, (SqliteConnection)Connection);
        RemoveOrientationComponentQuery =
            new SimpleSqliteRemoveComponentQuery(OrientationParamName, (SqliteConnection)Connection);

        // Admin component.
        AddAdminComponentQuery = new SimpleSqliteAddComponentQuery<bool>(
            AdminParamName, AdminParamType, (SqliteConnection)Connection);
        ModifyAdminComponentQuery = new SimpleSqliteModifyComponentQuery<bool>(
            AdminParamName, AdminParamType, (SqliteConnection)Connection);
        RemoveAdminComponentQuery = new SimpleSqliteRemoveComponentQuery(AdminParamName, (SqliteConnection)Connection);

        GetWorldSegmentBlockDataQuery = new SqliteGetWorldSegmentBlockDataQuery((SqliteConnection)Connection);
        SetWorldSegmentBlockDataQuery = new SqliteSetWorldSegmentBlockDataQuery((SqliteConnection)Connection);

        // CastBlockShadows component.
        AddCastBlockShadowsComponentQuery = new SimpleSqliteAddComponentQuery<bool>(
            CastBlockShadowsParamName, CastBlockShadowsParamType, (SqliteConnection)Connection);
        ModifyCastBlockShadowsComponentQuery = new SimpleSqliteModifyComponentQuery<bool>(
            CastBlockShadowsParamName, CastBlockShadowsParamType, (SqliteConnection)Connection);
        RemoveCastBlockShadowsComponentQuery = new SimpleSqliteRemoveComponentQuery(CastBlockShadowsParamName,
            (SqliteConnection)Connection);

        // PointLightSource component.
        var pointLightSourceQuery = new SqliteAddModifyPointLightSourceComponentQuery((SqliteConnection)Connection);
        AddPointLightSourceComponentQuery = pointLightSourceQuery;
        ModifyPointLightSourceComponentQuery = pointLightSourceQuery;
        RemovePointLightSourceComponentQuery =
            new SqliteRemovePointLightSourceComponentQuery((SqliteConnection)Connection);

        // Physics component.
        AddPhysicsComponentQuery = new SimpleSqliteAddComponentQuery<bool>(PhysicsParamName, PhysicsParamType,
            (SqliteConnection)Connection);
        ModifyPhysicsComponentQuery = new SimpleSqliteModifyComponentQuery<bool>(PhysicsParamName, PhysicsParamType,
            (SqliteConnection)Connection);
        RemovePhysicsComponentQuery =
            new SimpleSqliteRemoveComponentQuery(PhysicsParamName, (SqliteConnection)Connection);

        // BoundingBox component.
        var boundingBoxQuery = new SqliteAddModifyBoundingBoxComponentQuery((SqliteConnection)Connection);
        AddBoundingBoxComponentQuery = boundingBoxQuery;
        ModifyBoundingBoxComponentQuery = boundingBoxQuery;
        RemoveBoundingBoxComponentQuery = new SqliteRemoveBoundingBoxComponentQuery((SqliteConnection)Connection);

        // CastShadows component.
        var castShadowsQueries = new SqliteCastShadowsComponentQueries((SqliteConnection)Connection);
        AddCastShadowsComponentQuery = castShadowsQueries;
        ModifyCastShadowsComponentQuery = castShadowsQueries;
        RemoveCastShadowsComponentQuery = castShadowsQueries;
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
    public IAddComponentQuery<bool> AddPhysicsComponentQuery { get; }
    public IModifyComponentQuery<bool> ModifyPhysicsComponentQuery { get; }
    public IRemoveComponentQuery RemovePhysicsComponentQuery { get; }
    public IAddComponentQuery<BoundingBox> AddBoundingBoxComponentQuery { get; }
    public IModifyComponentQuery<BoundingBox> ModifyBoundingBoxComponentQuery { get; }
    public IRemoveComponentQuery RemoveBoundingBoxComponentQuery { get; }
    public IAddComponentQuery<Shadow> AddCastShadowsComponentQuery { get; }
    public IModifyComponentQuery<Shadow> ModifyCastShadowsComponentQuery { get; }
    public IRemoveComponentQuery RemoveCastShadowsComponentQuery { get; }

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
        logger.LogInformation("Opening the SQLite database at {Host}.",
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