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
using System.IO;
using System.Numerics;
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
    private const string FullMigrationFilename = "Migrations/Full/Full_sqlite.sql";

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

    private const string DrawableColumnPrefix = "drawable_";

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

    private const string EntityTypeName = "entity_type";
    private const SqliteType EntityTypeType = SqliteType.Integer;

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

        var conn = (SqliteConnection)Connection;

        MigrationQuery = new SqliteMigrationQuery(Connection);
        NextPersistedIdQuery = new SqliteNextPersistedIdQuery(Connection);
        RetrieveAllTemplatesQuery = new SqliteRetrieveAllTemplatesQuery(conn);

        AddAccountQuery = new SqliteAddAccountQuery(Connection);
        RetrieveAccountQuery = new SqliteRetrieveAccountQuery(Connection);
        RetrieveAccountWithAuthQuery = new SqliteRetrieveAccountWithAuthQuery(Connection);

        RetrieveEntityQuery = new SqliteRetrieveEntityQuery(Connection);
        RetrieveRangeQuery = new SqliteRetrieveRangeQuery(Connection);

        AddEntityQuery = new SqliteAddEntityQuery(Connection);
        RemoveEntityQuery = new SqliteRemoveEntityQuery(Connection);

        SetTemplateQuery = new SqliteSetTemplateQuery(conn);

        PlayerExistsQuery = new SqlitePlayerExistsQuery(conn);
        GetAccountForPlayerQuery = new SqliteGetAccountForPlayerQuery(conn);
        ListPlayersQuery = new SqliteListPlayersQuery(conn);
        DeletePlayerQuery = new SqliteDeletePlayerQuery(conn);

        AddAdminRoleQuery = new SqliteAddAdminRoleQuery(conn);
        RemoveAdminRoleQuery = new SqliteRemoveAdminRoleQuery(conn);

        GetGlobalKeyValuePairsQuery = new SqliteGetGlobalKeyValuePairsQuery(conn);
        UpdateGlobalKeyValuePairQuery = new SqliteUpdateGlobalKeyValuePairQuery(conn);
        RemoveGlobalKeyValuePairQuery = new SqliteRemoveGlobalKeyValuePairQuery(conn);
        UpdateEntityKeyValueQuery = new SqliteUpdateEntityKeyValueQuery(conn);
        RemoveEntityKeyValueQuery = new SqliteRemoveEntityKeyValueQuery(conn);

        /* Position component. */
        AddPositionQuery = new SqliteAddPositionComponentQuery(conn);
        ModifyPositionQuery = new SqliteModifyPositionComponentQuery(conn);
        RemovePositionQuery = new Vector3SqliteRemoveComponentQuery(PositionColumnPrefix, conn);

        /* Material component. */
        AddMaterialQuery = new SimpleSqliteAddComponentQuery<int>(
            MaterialParamName, MaterialParamType,
            conn);
        ModifyMaterialQuery = new SimpleSqliteModifyComponentQuery<int>(
            MaterialParamName, MaterialParamType,
            conn);
        RemoveMaterialQuery = new SimpleSqliteRemoveComponentQuery(MaterialParamName,
            conn);

        /* MaterialModifier component. */
        AddMaterialModifierQuery = new SimpleSqliteAddComponentQuery<int>(
            MaterialModifierParamName,
            MaterialModifierParamType, conn);
        ModifyMaterialModifierQuery = new SimpleSqliteModifyComponentQuery<int>(
            MaterialModifierParamName,
            MaterialModifierParamType, conn);
        RemoveMaterialModifierQuery = new SimpleSqliteRemoveComponentQuery(MaterialModifierParamName,
            conn);

        /* PlayerCharacter tag. */
        AddPlayerCharacterQuery = new SimpleSqliteAddComponentQuery<bool>(
            PlayerCharacterParamName, PlayerCharacterParamType, conn);
        ModifyPlayerCharacterQuery = new SimpleSqliteModifyComponentQuery<bool>(
            PlayerCharacterParamName, PlayerCharacterParamType, conn);
        RemovePlayerCharacterQuery = new SimpleSqliteRemoveComponentQuery(PlayerCharacterParamName,
            conn);

        /* Name component. */
        AddNameQuery = new SimpleSqliteAddComponentQuery<string>(
            NameParamName, NameParamType, conn);
        ModifyNameQuery = new SimpleSqliteModifyComponentQuery<string>(NameParamName,
            NameParamType, conn);
        RemoveNameQuery = new SimpleSqliteRemoveComponentQuery(NameParamName, conn);

        /* Account component. */
        AddAccountComponentQuery = new GuidSqliteAddComponentQuery(
            AccountComponentParamName, conn);
        ModifyAccountComponentQuery = new GuidSqliteModifyComponentQuery(
            AccountComponentParamName, conn);
        RemoveAccountComponentQuery = new SimpleSqliteRemoveComponentQuery(AccountComponentParamName,
            conn);

        // Parent component.
        AddParentComponentQuery = new SimpleSqliteAddComponentQuery<ulong>(ParentParamName,
            ParentParamType, conn);
        ModifyParentComponentQuery = new SimpleSqliteModifyComponentQuery<ulong>(ParentParamName,
            ParentParamType, conn);
        RemoveParentComponentQuery =
            new SimpleSqliteRemoveComponentQuery(ParentParamName, conn);

        // Drawable component.
        var drawableQueries = new Vector2SqliteComponentQueries(DrawableColumnPrefix, conn);
        AddDrawableComponentQuery = drawableQueries;
        ModifyDrawableComponentQuery = drawableQueries;
        RemoveDrawableComponentQuery = drawableQueries;

        // AnimatedSprite component.
        AddAnimatedSpriteComponentQuery = new SimpleSqliteAddComponentQuery<int>(
            AnimatedSpriteParamName, AnimatedSpriteParamType, conn);
        ModifyAnimatedSpriteComponentQuery = new SimpleSqliteModifyComponentQuery<int>(
            AnimatedSpriteParamName, AnimatedSpriteParamType, conn);
        RemoveAnimatedSpriteComponentQuery = new SimpleSqliteRemoveComponentQuery(
            AnimatedSpriteParamName, conn);

        // Orientation component.
        AddOrientationComponentQuery = new SimpleSqliteAddComponentQuery<Orientation>(
            OrientationParamName, OrientationParamType, conn);
        ModifyOrientationComponentQuery = new SimpleSqliteModifyComponentQuery<Orientation>(
            OrientationParamName, OrientationParamType, conn);
        RemoveOrientationComponentQuery =
            new SimpleSqliteRemoveComponentQuery(OrientationParamName, conn);

        // Admin component.
        AddAdminComponentQuery = new SimpleSqliteAddComponentQuery<bool>(
            AdminParamName, AdminParamType, conn);
        ModifyAdminComponentQuery = new SimpleSqliteModifyComponentQuery<bool>(
            AdminParamName, AdminParamType, conn);
        RemoveAdminComponentQuery = new SimpleSqliteRemoveComponentQuery(AdminParamName, conn);

        GetWorldSegmentBlockDataQuery = new SqliteGetWorldSegmentBlockDataQuery(conn);
        SetWorldSegmentBlockDataQuery = new SqliteSetWorldSegmentBlockDataQuery(conn);

        // CastBlockShadows component.
        AddCastBlockShadowsComponentQuery = new SimpleSqliteAddComponentQuery<bool>(
            CastBlockShadowsParamName, CastBlockShadowsParamType, conn);
        ModifyCastBlockShadowsComponentQuery = new SimpleSqliteModifyComponentQuery<bool>(
            CastBlockShadowsParamName, CastBlockShadowsParamType, conn);
        RemoveCastBlockShadowsComponentQuery = new SimpleSqliteRemoveComponentQuery(CastBlockShadowsParamName,
            conn);

        // PointLightSource component.
        var pointLightSourceQuery = new SqliteAddModifyPointLightSourceComponentQuery(conn);
        AddPointLightSourceComponentQuery = pointLightSourceQuery;
        ModifyPointLightSourceComponentQuery = pointLightSourceQuery;
        RemovePointLightSourceComponentQuery =
            new SqliteRemovePointLightSourceComponentQuery(conn);

        // Physics component.
        AddPhysicsComponentQuery = new SimpleSqliteAddComponentQuery<bool>(PhysicsParamName, PhysicsParamType,
            conn);
        ModifyPhysicsComponentQuery = new SimpleSqliteModifyComponentQuery<bool>(PhysicsParamName, PhysicsParamType,
            conn);
        RemovePhysicsComponentQuery =
            new SimpleSqliteRemoveComponentQuery(PhysicsParamName, conn);

        // BoundingBox component.
        var boundingBoxQuery = new SqliteAddModifyBoundingBoxComponentQuery(conn);
        AddBoundingBoxComponentQuery = boundingBoxQuery;
        ModifyBoundingBoxComponentQuery = boundingBoxQuery;
        RemoveBoundingBoxComponentQuery = new SqliteRemoveBoundingBoxComponentQuery(conn);

        // CastShadows component.
        var castShadowsQueries = new SqliteCastShadowsComponentQueries(conn);
        AddCastShadowsComponentQuery = castShadowsQueries;
        ModifyCastShadowsComponentQuery = castShadowsQueries;
        RemoveCastShadowsComponentQuery = castShadowsQueries;

        // EntityType component.
        AddEntityTypeComponentQuery = new SimpleSqliteAddComponentQuery<EntityType>(
            EntityTypeName, EntityTypeType, conn);
        ModifyEntityTypeComponentQuery = new SimpleSqliteModifyComponentQuery<EntityType>(
            EntityTypeName, EntityTypeType, conn);
        RemoveEntityTypeComponentQuery = new SimpleSqliteRemoveComponentQuery(EntityTypeName, conn);
    }

    public ITransactionLock TransactionLock { get; } = new SingleWriterTransactionLock();

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
    public IAddComponentQuery<Vector2> AddDrawableComponentQuery { get; }
    public IModifyComponentQuery<Vector2> ModifyDrawableComponentQuery { get; }
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
    public IRemoveComponentQuery RemoveEntityTypeComponentQuery { get; }
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
    public IAddComponentQuery<EntityType> AddEntityTypeComponentQuery { get; }
    public IModifyComponentQuery<EntityType> ModifyEntityTypeComponentQuery { get; }
    public IGetGlobalKeyValuePairsQuery GetGlobalKeyValuePairsQuery { get; }
    public IUpdateGlobalKeyValuePairQuery UpdateGlobalKeyValuePairQuery { get; }
    public IRemoveGlobalKeyValuePairQuery RemoveGlobalKeyValuePairQuery { get; }
    public IUpdateEntityKeyValueQuery UpdateEntityKeyValueQuery { get; }
    public IRemoveEntityKeyValueQuery RemoveEntityKeyValueQuery { get; }

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

        if (configuration.CreateIfMissing) CreateIfMissing();

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

    /// <summary>
    ///     Creates the database if it is missing.
    /// </summary>
    private void CreateIfMissing()
    {
        var dbFilename = configuration.Host;
        if (File.Exists(dbFilename)) return;

        logger.LogInformation("Database {dbFilename} not found; creating new database.", dbFilename);

        try
        {
            var sql = File.ReadAllText(FullMigrationFilename);

            using (var connection = new SqliteConnection(CreateConnectionString()))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }

            logger.LogInformation("Database created.");
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Failed to create the SQLite database.");
            throw new FatalErrorException();
        }
    }
}