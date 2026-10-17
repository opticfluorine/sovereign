// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.Persistence.Data;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Sovereign.Persistence.Entities;
using Xunit;

namespace TestPersistence.Database;

/// <summary>
///     Tests that legacy-shaped key-value rows load into the stores with their stored type tags.
/// </summary>
public sealed class TestKeyValueLoadPath
{
    private const string FullScript = @"../../../../../Server/Persistence/Migrations/Full/Full_sqlite.sql";

    /// <summary>
    ///     Tests that global rows load with their stored type tags.
    /// </summary>
    [Fact]
    public void GlobalKeyValueProcessor_DecodesStoredTypes()
    {
        using var connection = OpenInMemory();
        CreateValueTable(connection);
        Execute(connection,
            "INSERT INTO KV (key, value, type) VALUES ('int', '42', 1), ('float', '3.5', 3), ('str', 'hello', 0)");
        var controller = new RecordingDataController();

        var count = ProcessGlobals(connection, controller);

        Assert.Equal(3, count);
        Assert.Equal(DataValueType.Int, controller.Globals["int"].Type);
        Assert.Equal(42, controller.Globals["int"].AsInt());
        Assert.Equal(DataValueType.Float, controller.Globals["float"].Type);
        Assert.Equal(3.5, controller.Globals["float"].AsFloat());
        Assert.Equal(DataValueType.String, controller.Globals["str"].Type);
        Assert.Equal("hello", controller.Globals["str"].AsString());
    }

    /// <summary>
    ///     Tests that a malformed stored value is a fatal load error.
    /// </summary>
    [Fact]
    public void GlobalKeyValueProcessor_MalformedValue_ThrowsFatal()
    {
        using var connection = OpenInMemory();
        CreateValueTable(connection);
        Execute(connection, "INSERT INTO KV (key, value, type) VALUES ('bad', 'not-an-int', 1)");
        var controller = new RecordingDataController();

        Assert.Throws<FatalErrorException>(() => ProcessGlobals(connection, controller));
    }

    /// <summary>
    ///     Tests that a row with an unknown type tag is a fatal load error.
    /// </summary>
    [Fact]
    public void GlobalKeyValueProcessor_UnknownTag_ThrowsFatal()
    {
        using var connection = OpenInMemory();
        CreateValueTable(connection);
        Execute(connection, "INSERT INTO KV (key, value, type) VALUES ('bad', 'x', 99)");
        var controller = new RecordingDataController();

        Assert.Throws<FatalErrorException>(() => ProcessGlobals(connection, controller));
    }

    /// <summary>
    ///     Tests that entity key-value rows load with their stored type tags.
    /// </summary>
    [Fact]
    public void EntityProcessor_LoadsTypedKeyValues()
    {
        using var connection = OpenInMemory();
        Execute(connection, File.ReadAllText(FullScript));
        Execute(connection, "INSERT INTO Entity (id) VALUES (1000)");
        Execute(connection,
            "INSERT INTO EntityKeyValue (entity_id, key, value, type) VALUES (1000, 'int', '7', 1)");

        var controller = new RecordingDataController();
        var factory = new Mock<IEntityFactory>();
        factory.DefaultValue = DefaultValue.Mock;
        var builder = new Mock<IEntityBuilder>();
        builder.DefaultValue = DefaultValue.Mock;
        factory.Setup(f => f.GetBuilder(It.IsAny<ulong>(), true)).Returns(builder.Object);

        var processor = new EntityProcessor(factory.Object, new ExistingEntitySet(),
            NullLogger<EntityProcessor>.Instance, controller);

        var query = new SqliteRetrieveEntityQuery(connection);
        using var reader = query.RetrieveEntity(1000);
        var count = processor.ProcessFromReader(reader.Reader);

        Assert.Equal(1, count);
        Assert.True(controller.EntityKeyValues.ContainsKey((1000, "int")));
        var value = controller.EntityKeyValues[(1000, "int")];
        Assert.Equal(DataValueType.Int, value.Type);
        Assert.Equal(7, value.AsInt());
    }

    /// <summary>
    ///     Runs the global key-value processor against the KV table.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    /// <param name="controller">Recording controller.</param>
    /// <returns>Number of rows processed.</returns>
    private static int ProcessGlobals(SqliteConnection connection, RecordingDataController controller)
    {
        using var reader = new QueryReader(new SqliteCommand("SELECT key, value, type FROM KV", connection));
        var processor = new GlobalKeyValueProcessor(controller);
        return processor.Process(reader);
    }

    /// <summary>
    ///     Creates a KV table matching the global store shape.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    private static void CreateValueTable(SqliteConnection connection)
    {
        Execute(connection,
            "CREATE TABLE KV (key TEXT PRIMARY KEY NOT NULL, value TEXT NOT NULL, type INTEGER NOT NULL DEFAULT 0)");
    }

    /// <summary>
    ///     Opens a fresh in-memory SQLite connection.
    /// </summary>
    /// <returns>Open connection.</returns>
    private static SqliteConnection OpenInMemory()
    {
        var builder = new SqliteConnectionStringBuilder { Mode = SqliteOpenMode.Memory };
        var connection = new SqliteConnection(builder.ConnectionString);
        connection.Open();
        return connection;
    }

    /// <summary>
    ///     Executes a non-query command.
    /// </summary>
    /// <param name="connection">Open connection.</param>
    /// <param name="sql">SQL to execute.</param>
    private static void Execute(SqliteConnection connection, string sql)
    {
        using var cmd = new SqliteCommand(sql, connection);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     IDataController test double that records calls.
    /// </summary>
    private sealed class RecordingDataController : IDataController
    {
        /// <summary>
        ///     Recorded global writes.
        /// </summary>
        public Dictionary<string, DataValue> Globals { get; } = new();

        /// <summary>
        ///     Recorded entity writes.
        /// </summary>
        public Dictionary<(ulong, string), DataValue> EntityKeyValues { get; } = new();

        public void SetGlobalSync<T>(string key, T value) where T : notnull
        {
            Globals[key] = (DataValue)(object)value;
        }

        public void RemoveGlobalSync(string key)
        {
            Globals.Remove(key);
        }

        public void SetEntityKeyValueSync<T>(ulong entityId, string key, T value) where T : notnull
        {
            EntityKeyValues[(entityId, key)] = (DataValue)(object)value;
        }

        public void RemoveEntityKeyValueSync(ulong entityId, string key)
        {
            EntityKeyValues.Remove((entityId, key));
        }

        public void ClearEntityKeyValuesSync(ulong entityId)
        {
        }
    }
}
