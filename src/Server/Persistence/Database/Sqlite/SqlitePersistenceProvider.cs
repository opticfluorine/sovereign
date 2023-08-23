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
    private const string positionTableName = "Position";

    private const string materialTableName = "Material";
    private const string materialParamName = "material";
    private const SqliteType materialParamType = SqliteType.Integer;

    private const string materialModifierTableName = "MaterialModifier";
    private const string materialModifierParamName = "modifier";
    private const SqliteType materialModifierParamType = SqliteType.Integer;

    private const string playerCharacterTableName = "PlayerCharacter";
    private const string playerCharacterParamName = "value";
    private const SqliteType playerCharacterParamType = SqliteType.Integer;

    private readonly IPersistenceConfiguration configuration;

    public SqlitePersistenceProvider(IPersistenceConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

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

        /* Position component. */
        AddPositionQuery = new Vector3SqliteAddComponentQuery(positionTableName,
            (SqliteConnection)Connection);
        ModifyPositionQuery = new Vector3SqliteModifyComponentQuery(positionTableName,
            (SqliteConnection)Connection);
        RemovePositionQuery = new SqliteRemoveComponentQuery(positionTableName,
            (SqliteConnection)Connection);

        /* Material component. */
        AddMaterialQuery = new SimpleSqliteAddComponentQuery<int>(
            materialTableName, materialParamName, materialParamType,
            (SqliteConnection)Connection);
        ModifyMaterialQuery = new SimpleSqliteModifyComponentQuery<int>(
            materialTableName, materialParamName, materialParamType,
            (SqliteConnection)Connection);
        RemoveMaterialQuery = new SqliteRemoveComponentQuery(materialTableName,
            (SqliteConnection)Connection);

        /* MaterialModifier component. */
        AddMaterialModifierQuery = new SimpleSqliteAddComponentQuery<int>(
            materialModifierTableName, materialModifierParamName,
            materialModifierParamType, (SqliteConnection)Connection);
        ModifyMaterialModifierQuery = new SimpleSqliteModifyComponentQuery<int>(
            materialModifierTableName, materialModifierParamName,
            materialModifierParamType, (SqliteConnection)Connection);
        RemoveMaterialModifierQuery = new SqliteRemoveComponentQuery(materialModifierTableName,
            (SqliteConnection)Connection);

        /* PlayerCharacter tag. */
        AddPlayerCharacterQuery = new SimpleSqliteAddComponentQuery<bool>(playerCharacterTableName,
            playerCharacterParamName, playerCharacterParamType, (SqliteConnection)Connection);
        ModifyPlayerCharacterQuery = new SimpleSqliteModifyComponentQuery<bool>(playerCharacterTableName,
            playerCharacterParamName, playerCharacterParamType, (SqliteConnection)Connection);
        RemovePlayerCharacterQuery = new SqliteRemoveComponentQuery(playerCharacterTableName,
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