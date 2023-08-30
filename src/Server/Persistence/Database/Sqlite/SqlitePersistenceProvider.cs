/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Data;
using System.Numerics;
using System.Text;
using Castle.Core.Logging;
using Microsoft.Data.Sqlite;
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
    private const SqliteType ACCOUNT_COMPONENT_PARAM_TYPE = SqliteType.Blob;

    private readonly IPersistenceConfiguration configuration;

    public SqlitePersistenceProvider(IPersistenceConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;
    public IAddComponentQuery<string> AddNameQuery { get; private set; }
    public IModifyComponentQuery<string> ModifyNameQuery { get; private set; }
    public IRemoveComponentQuery RemoveNameQuery { get; private set; }

    public IMigrationQuery MigrationQuery { get; private set; }

    public INextPersistedIdQuery NextPersistedIdQuery { get; private set; }

    public IAddAccountQuery AddAccountQuery { get; private set; }

    public IRetrieveAccountQuery RetrieveAccountQuery { get; private set; }

    public IRetrieveAccountWithAuthQuery RetrieveAccountWithAuthQuery { get; private set; }

    public IRetrieveEntityQuery RetrieveEntityQuery { get; private set; }

    public IRetrieveRangeQuery RetrieveRangeQuery { get; private set; }

    public IDbConnection Connection { get; private set; }

    public IAddEntityQuery AddEntityQuery { get; private set; }

    public IRemoveEntityQuery RemoveEntityQuery { get; private set; }
    public IAddComponentQuery<Vector3> AddPositionQuery { get; private set; }
    public IModifyComponentQuery<Vector3> ModifyPositionQuery { get; private set; }
    public IRemoveComponentQuery RemovePositionQuery { get; private set; }
    public IAddComponentQuery<int> AddMaterialQuery { get; private set; }
    public IModifyComponentQuery<int> ModifyMaterialQuery { get; private set; }
    public IRemoveComponentQuery RemoveMaterialQuery { get; private set; }
    public IAddComponentQuery<int> AddMaterialModifierQuery { get; private set; }
    public IModifyComponentQuery<int> ModifyMaterialModifierQuery { get; private set; }
    public IRemoveComponentQuery RemoveMaterialModifierQuery { get; private set; }
    public IAddComponentQuery<bool> AddPlayerCharacterQuery { get; private set; }
    public IModifyComponentQuery<bool> ModifyPlayerCharacterQuery { get; private set; }
    public IRemoveComponentQuery RemovePlayerCharacterQuery { get; private set; }
    public IAddComponentQuery<Guid> AddAccountComponentQuery { get; private set; }
    public IModifyComponentQuery<Guid> ModifyAccountComponentQuery { get; private set; }
    public IRemoveComponentQuery RemoveAccountComponentQuery { get; private set; }
    public IPlayerExistsQuery PlayerExistsQuery { get; private set; }
    public IGetAccountForPlayerQuery GetAccountForPlayerQuery { get; private set; }

    public void Initialize()
    {
        Connect();
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
    }

    public void Cleanup()
    {
        Logger.InfoFormat("Closing the SQLite database if open.");

        Connection?.Close();

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