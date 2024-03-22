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
using System.Numerics;
using System.Text;
using Castle.Core.Logging;
using Microsoft.Data.Sqlite;
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
    private const string POSITION_TABLE_NAME = "Position";

    private const string MATERIAL_TABLE_NAME = "Material";
    private const string MATERIAL_PARAM_NAME = "material";
    private const SqliteType MATERIAL_PARAM_TYPE = SqliteType.Integer;

    private const string MATERIAL_MODIFIER_TABLE_NAME = "MaterialModifier";
    private const string MATERIAL_MODIFIER_PARAM_NAME = "modifier";
    private const SqliteType MATERIAL_MODIFIER_PARAM_TYPE = SqliteType.Integer;

    private const string PLAYER_CHARACTER_TABLE_NAME = "PlayerCharacter";
    private const string PLAYER_CHARACTER_PARAM_NAME = "value";
    private const SqliteType PLAYER_CHARACTER_PARAM_TYPE = SqliteType.Integer;

    private const string NAME_TABLE_NAME = "Name";
    private const string NAME_PARAM_NAME = "value";
    private const SqliteType NAME_PARAM_TYPE = SqliteType.Text;

    private const string ACCOUNT_COMPONENT_TABLE_NAME = "AccountComponent";
    private const string ACCOUNT_COMPONENT_PARAM_NAME = "account_id";

    /// <summary>
    ///     Database value type for Parent component.
    /// </summary>
    private const SqliteType PARENT_PARAM_TYPE = SqliteType.Integer;

    /// <summary>
    ///     Database column name for Parent component value.
    /// </summary>
    private const string PARENT_PARAM_NAME = "parent_id";

    /// <summary>
    ///     Database table name for Parent component.
    /// </summary>
    private const string PARENT_TABLE_NAME = "Parent";

    private const SqliteType DRAWABLE_PARAM_TYPE = SqliteType.Integer;
    private const string DRAWABLE_TABLE_NAME = "Drawable";
    private const string DRAWABLE_PARAM_NAME = "value";

    private const SqliteType ANIMATEDSPRITE_PARAM_TYPE = SqliteType.Integer;
    private const string ANIMATEDSPRITE_TABLE_NAME = "AnimatedSprite";
    private const string ANIMATEDSPRITE_PARAM_NAME = "value";

    private const string ORIENTATION_TABLE_NAME = "Orientation";
    private const string ORIENTATION_PARAM_NAME = "value";
    private const SqliteType ORIENTATION_PARAM_TYPE = SqliteType.Integer;

    private const SqliteType ADMIN_PARAM_TYPE = SqliteType.Integer;
    private const string ADMIN_TABLE_NAME = "Admin";
    private const string ADMIN_PARAM_NAME = "value";

    private readonly IPersistenceConfiguration configuration;

    public SqlitePersistenceProvider(IPersistenceConfiguration configuration)
    {
        this.configuration = configuration;

        Connect();
        if (Connection == null)
        {
            Logger.Fatal("Connection unexpectedly null.");
            throw new FatalErrorException();
        }

        MigrationQuery = new SqliteMigrationQuery(Connection);
        NextPersistedIdQuery = new SqliteNextPersistedIdQuery(Connection);

        AddAccountQuery = new SqliteAddAccountQuery(Connection);
        RetrieveAccountQuery = new SqliteRetrieveAccountQuery(Connection);
        RetrieveAccountWithAuthQuery = new SqliteRetrieveAccountWithAuthQuery(Connection);

        RetrieveEntityQuery = new SqliteRetrieveEntityQuery(Connection);
        RetrieveRangeQuery = new SqliteRetrieveRangeQuery(Connection);

        AddEntityQuery = new SqliteAddEntityQuery(Connection);
        RemoveEntityQuery = new SqliteRemoveEntityQuery(Connection);

        PlayerExistsQuery = new SqlitePlayerExistsQuery((SqliteConnection)Connection);
        GetAccountForPlayerQuery = new SqliteGetAccountForPlayerQuery((SqliteConnection)Connection);
        ListPlayersQuery = new SqliteListPlayersQuery((SqliteConnection)Connection);
        DeletePlayerQuery = new SqliteDeletePlayerQuery((SqliteConnection)Connection);

        AddAdminRoleQuery = new SqliteAddAdminRoleQuery((SqliteConnection)Connection);

        /* Position component. */
        AddPositionQuery = new Vector3SqliteAddComponentQuery(POSITION_TABLE_NAME,
            (SqliteConnection)Connection);
        ModifyPositionQuery = new Vector3SqliteModifyComponentQuery(POSITION_TABLE_NAME,
            (SqliteConnection)Connection);
        RemovePositionQuery = new SqliteRemoveComponentQuery(POSITION_TABLE_NAME,
            (SqliteConnection)Connection);

        /* Material component. */
        AddMaterialQuery = new SimpleSqliteAddComponentQuery<int>(
            MATERIAL_TABLE_NAME, MATERIAL_PARAM_NAME, MATERIAL_PARAM_TYPE,
            (SqliteConnection)Connection);
        ModifyMaterialQuery = new SimpleSqliteModifyComponentQuery<int>(
            MATERIAL_TABLE_NAME, MATERIAL_PARAM_NAME, MATERIAL_PARAM_TYPE,
            (SqliteConnection)Connection);
        RemoveMaterialQuery = new SqliteRemoveComponentQuery(MATERIAL_TABLE_NAME,
            (SqliteConnection)Connection);

        /* MaterialModifier component. */
        AddMaterialModifierQuery = new SimpleSqliteAddComponentQuery<int>(
            MATERIAL_MODIFIER_TABLE_NAME, MATERIAL_MODIFIER_PARAM_NAME,
            MATERIAL_MODIFIER_PARAM_TYPE, (SqliteConnection)Connection);
        ModifyMaterialModifierQuery = new SimpleSqliteModifyComponentQuery<int>(
            MATERIAL_MODIFIER_TABLE_NAME, MATERIAL_MODIFIER_PARAM_NAME,
            MATERIAL_MODIFIER_PARAM_TYPE, (SqliteConnection)Connection);
        RemoveMaterialModifierQuery = new SqliteRemoveComponentQuery(MATERIAL_MODIFIER_TABLE_NAME,
            (SqliteConnection)Connection);

        /* PlayerCharacter tag. */
        AddPlayerCharacterQuery = new SimpleSqliteAddComponentQuery<bool>(PLAYER_CHARACTER_TABLE_NAME,
            PLAYER_CHARACTER_PARAM_NAME, PLAYER_CHARACTER_PARAM_TYPE, (SqliteConnection)Connection);
        ModifyPlayerCharacterQuery = new SimpleSqliteModifyComponentQuery<bool>(PLAYER_CHARACTER_TABLE_NAME,
            PLAYER_CHARACTER_PARAM_NAME, PLAYER_CHARACTER_PARAM_TYPE, (SqliteConnection)Connection);
        RemovePlayerCharacterQuery = new SqliteRemoveComponentQuery(PLAYER_CHARACTER_TABLE_NAME,
            (SqliteConnection)Connection);

        /* Name component. */
        AddNameQuery = new SimpleSqliteAddComponentQuery<string>(NAME_TABLE_NAME,
            NAME_PARAM_NAME, NAME_PARAM_TYPE, (SqliteConnection)Connection);
        ModifyNameQuery = new SimpleSqliteModifyComponentQuery<string>(NAME_TABLE_NAME, NAME_PARAM_NAME,
            NAME_PARAM_TYPE, (SqliteConnection)Connection);
        RemoveNameQuery = new SqliteRemoveComponentQuery(NAME_TABLE_NAME, (SqliteConnection)Connection);

        /* Account component. */
        AddAccountComponentQuery = new GuidSqliteAddComponentQuery(ACCOUNT_COMPONENT_TABLE_NAME,
            ACCOUNT_COMPONENT_PARAM_NAME, (SqliteConnection)Connection);
        ModifyAccountComponentQuery = new GuidSqliteModifyComponentQuery(ACCOUNT_COMPONENT_TABLE_NAME,
            ACCOUNT_COMPONENT_PARAM_NAME, (SqliteConnection)Connection);
        RemoveAccountComponentQuery = new SqliteRemoveComponentQuery(ACCOUNT_COMPONENT_TABLE_NAME,
            (SqliteConnection)Connection);

        // Parent component.
        AddParentComponentQuery = new SimpleSqliteAddComponentQuery<ulong>(PARENT_TABLE_NAME, PARENT_PARAM_NAME,
            PARENT_PARAM_TYPE, (SqliteConnection)Connection);
        ModifyParentComponentQuery = new SimpleSqliteModifyComponentQuery<ulong>(PARENT_TABLE_NAME, PARENT_PARAM_NAME,
            PARENT_PARAM_TYPE, (SqliteConnection)Connection);
        RemoveParentComponentQuery = new SqliteRemoveComponentQuery(PARENT_TABLE_NAME, (SqliteConnection)Connection);

        // Drawable component.
        AddDrawableComponentQuery = new SimpleSqliteAddComponentQuery<bool>(DRAWABLE_TABLE_NAME, DRAWABLE_PARAM_NAME,
            DRAWABLE_PARAM_TYPE, (SqliteConnection)Connection);
        ModifyDrawableComponentQuery = new SimpleSqliteModifyComponentQuery<bool>(DRAWABLE_TABLE_NAME,
            DRAWABLE_PARAM_NAME,
            DRAWABLE_PARAM_TYPE, (SqliteConnection)Connection);
        RemoveDrawableComponentQuery =
            new SqliteRemoveComponentQuery(DRAWABLE_TABLE_NAME, (SqliteConnection)Connection);

        // AnimatedSprite component.
        AddAnimatedSpriteComponentQuery = new SimpleSqliteAddComponentQuery<int>(ANIMATEDSPRITE_TABLE_NAME,
            ANIMATEDSPRITE_PARAM_NAME, ANIMATEDSPRITE_PARAM_TYPE, (SqliteConnection)Connection);
        ModifyAnimatedSpriteComponentQuery = new SimpleSqliteModifyComponentQuery<int>(ANIMATEDSPRITE_TABLE_NAME,
            ANIMATEDSPRITE_PARAM_NAME, ANIMATEDSPRITE_PARAM_TYPE, (SqliteConnection)Connection);
        RemoveAnimatedSpriteComponentQuery = new SqliteRemoveComponentQuery(
            ANIMATEDSPRITE_TABLE_NAME, (SqliteConnection)Connection);

        // Orientation component.
        AddOrientationComponentQuery = new SimpleSqliteAddComponentQuery<Orientation>(ORIENTATION_TABLE_NAME,
            ORIENTATION_PARAM_NAME, ORIENTATION_PARAM_TYPE, (SqliteConnection)Connection);
        ModifyOrientationComponentQuery = new SimpleSqliteModifyComponentQuery<Orientation>(ORIENTATION_TABLE_NAME,
            ORIENTATION_PARAM_NAME, ORIENTATION_PARAM_TYPE, (SqliteConnection)Connection);
        RemoveOrientationComponentQuery =
            new SqliteRemoveComponentQuery(ORIENTATION_TABLE_NAME, (SqliteConnection)Connection);

        // Admin component.
        AddAdminComponentQuery = new SimpleSqliteAddComponentQuery<bool>(ADMIN_TABLE_NAME,
            ADMIN_PARAM_NAME, ADMIN_PARAM_TYPE, (SqliteConnection)Connection);
        ModifyAdminComponentQuery = new SimpleSqliteModifyComponentQuery<bool>(ADMIN_TABLE_NAME,
            ADMIN_PARAM_NAME, ADMIN_PARAM_TYPE, (SqliteConnection)Connection);
        RemoveAdminComponentQuery = new SqliteRemoveComponentQuery(ADMIN_TABLE_NAME, (SqliteConnection)Connection);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;
    public IRemoveComponentQuery RemoveAdminComponentQuery { get; }
    public IAddComponentQuery<string> AddNameQuery { get; }
    public IModifyComponentQuery<string> ModifyNameQuery { get; }
    public IRemoveComponentQuery RemoveNameQuery { get; }

    public IMigrationQuery MigrationQuery { get; }

    public INextPersistedIdQuery NextPersistedIdQuery { get; }

    public IAddAccountQuery AddAccountQuery { get; }

    public IRetrieveAccountQuery RetrieveAccountQuery { get; }

    public IRetrieveAccountWithAuthQuery RetrieveAccountWithAuthQuery { get; }

    public IRetrieveEntityQuery RetrieveEntityQuery { get; }

    public IRetrieveRangeQuery RetrieveRangeQuery { get; }

    public IDbConnection Connection { get; private set; }

    public IAddEntityQuery AddEntityQuery { get; }

    public IRemoveEntityQuery RemoveEntityQuery { get; }
    public IAddComponentQuery<Vector3> AddPositionQuery { get; }
    public IModifyComponentQuery<Vector3> ModifyPositionQuery { get; }
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
    public IPlayerExistsQuery PlayerExistsQuery { get; }
    public IGetAccountForPlayerQuery GetAccountForPlayerQuery { get; }
    public IListPlayersQuery ListPlayersQuery { get; }
    public IDeletePlayerQuery DeletePlayerQuery { get; }
    public IAddAdminRoleQuery AddAdminRoleQuery { get; }

    public void Dispose()
    {
        Logger.InfoFormat("Closing the SQLite database if open.");

        Connection.Close();

        Logger.InfoFormat("SQLite database closed.");
    }

    /// <summary>
    ///     Opens the SQLite database.
    /// </summary>
    private void Connect()
    {
        Logger.InfoFormat("Opening the SQLite database at {0}.",
            configuration.Host);

        var connString = CreateConnectionString();
        try
        {
            Connection = new SqliteConnection(connString);
            Connection.Open();
        }
        catch (Exception e)
        {
            Logger.Fatal("Failed to open the SQLite database.", e);
            throw new FatalErrorException();
        }

        Logger.Info("SQLite database opened.");
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