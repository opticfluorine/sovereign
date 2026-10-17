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

using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.EngineCore.World;
using Sovereign.Scripting.Lua;
using Sovereign.ServerCore.Systems.Data;
using Sovereign.ServerCore.Systems.Persistence;
using Sovereign.ServerCore.Systems.Scripting;
using Sovereign.ServerCore.Systems.WorldManagement;
using Sovereign.ServerCore.TestSupport;
using Xunit;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.ServerCore.Systems.Data;

/// <summary>
///     Tests for the typed key-value API exposed by DataLuaLibrary, driven through a real Lua host.
/// </summary>
public class TestDataLuaLibraryKeyValues : IDisposable
{
    private const ulong EntityId = 0x1000;

    private readonly RecordingDataController dataController = new();
    private readonly FakeDataServices dataServices = new();
    private readonly FakeEventSender eventSender = new();
    private readonly RecordingLogger<DataLuaLibrary> logger = new();
    private readonly PersistenceController persistenceController = new();
    private readonly ScriptManager scriptManager;
    private readonly SegmentFlagManager segmentFlagManager = new();
    private readonly RecordingSegmentKeyValueController segmentKeyValueController = new();
    private readonly WorldSegmentResolver segmentResolver = new();

    private readonly LuaHost host;

    private readonly DataLuaLibrary library;

    public TestDataLuaLibraryKeyValues()
    {
        scriptManager = new ScriptManager(NullLogger<ScriptManager>.Instance);
        var scriptingServices = new ScriptingServices(scriptManager);

        library = new DataLuaLibrary(dataController, scriptingServices, logger, dataServices,
            segmentFlagManager, segmentKeyValueController, segmentResolver, persistenceController, eventSender);

        host = new LuaHost(NullLogger.Instance, ".");
        library.Install(host);

        // Publish a stable entity handle for entity-scoped tests, matching the callback push pattern.
        host.ExecuteOther(_ =>
        {
            lua_pushlightuserdata(host.LuaState, (IntPtr)EntityId);
            lua_setglobal(host.LuaState, "__test_entity");
        });
    }

    public void Dispose()
    {
        host.Dispose();
        library.Dispose();
        scriptManager.Dispose();
    }

    [Fact]
    public void GlobalGet_String_ReturnsString()
    {
        dataServices.SetGlobal("k", DataValue.FromString("hello"));

        Eval("Data.Global[\"k\"]");

        Assert.Equal(LuaType.String, GetResultType());
        Assert.Equal("hello", GetResultString());
    }

    [Fact]
    public void GlobalGet_Int_ReturnsInteger()
    {
        dataServices.SetGlobal("k", DataValue.FromInt(42));

        Eval("Data.Global[\"k\"]");

        Assert.Equal(LuaType.Number, GetResultType());
        Assert.Equal(42L, GetResultInteger());
    }

    [Fact]
    public void GlobalGet_Float_ReturnsNumber()
    {
        dataServices.SetGlobal("k", DataValue.FromFloat(3.5));

        Eval("Data.Global[\"k\"]");

        Assert.Equal(LuaType.Number, GetResultType());
        Assert.Equal(3.5, GetResultNumber());
    }

    [Fact]
    public void GlobalGet_Bool_ReturnsBoolean()
    {
        dataServices.SetGlobal("k", DataValue.FromBool(true));

        Eval("Data.Global[\"k\"]");

        Assert.Equal(LuaType.Boolean, GetResultType());
        Assert.True(GetResultBoolean());
    }

    [Fact]
    public void GlobalGet_Vector3_ReturnsTable()
    {
        dataServices.SetGlobal("k", DataValue.FromVector3(new Vector3(1.0f, 2.0f, 3.0f)));

        Eval("Data.Global[\"k\"]");

        Assert.Equal(LuaType.Table, GetResultType());
        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), GetResultVector3());
    }

    [Fact]
    public void GlobalGet_EntityId_ReturnsLightUserData()
    {
        dataServices.SetGlobal("k", DataValue.FromEntityId(0x1234));

        Eval("Data.Global[\"k\"]");

        Assert.Equal(LuaType.LightUserData, GetResultType());
        Assert.Equal((IntPtr)0x1234, GetResultUserData());
    }

    [Fact]
    public void EntityGet_ReturnsNativeTypes()
    {
        dataServices.SetEntityKeyValue(EntityId, "s", DataValue.FromString("x"));
        dataServices.SetEntityKeyValue(EntityId, "i", DataValue.FromInt(7));
        dataServices.SetEntityKeyValue(EntityId, "f", DataValue.FromFloat(1.25));
        dataServices.SetEntityKeyValue(EntityId, "b", DataValue.FromBool(true));

        Eval("Data.GetEntityData(__test_entity)[\"s\"]");
        Assert.Equal(LuaType.String, GetResultType());
        Assert.Equal("x", GetResultString());

        Eval("Data.GetEntityData(__test_entity)[\"i\"]");
        Assert.Equal(7L, GetResultInteger());

        Eval("Data.GetEntityData(__test_entity)[\"f\"]");
        Assert.Equal(1.25, GetResultNumber());

        Eval("Data.GetEntityData(__test_entity)[\"b\"]");
        Assert.Equal(LuaType.Boolean, GetResultType());
    }

    [Theory]
    [InlineData("Data.Global[\"k\"] = \"hi\"", DataValueType.String)]
    [InlineData("Data.Global[\"k\"] = 5", DataValueType.Int)]
    [InlineData("Data.Global[\"k\"] = 5.5", DataValueType.Float)]
    [InlineData("Data.Global[\"k\"] = true", DataValueType.Bool)]
    public void GlobalSet_InfersScalarTag(string script, DataValueType expectedType)
    {
        Run(script);

        var value = Assert.Single(dataController.SetValues);
        Assert.Equal(expectedType, value.Type);
    }

    [Fact]
    public void GlobalSet_Vector3Table_InfersVector3Tag()
    {
        Run("Data.Global[\"k\"] = { X = 1, Y = 2, Z = 3 }");

        var value = Assert.Single(dataController.SetValues);
        Assert.Equal(DataValueType.Vector3, value.Type);
        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), value.AsVector3());
    }

    [Fact]
    public void GlobalSet_EntityIdUserData_InfersEntityIdTag()
    {
        Run("Data.Global[\"k\"] = __test_entity");

        var value = Assert.Single(dataController.SetValues);
        Assert.Equal(DataValueType.EntityId, value.Type);
        Assert.Equal(EntityId, value.AsEntityId());
    }

    [Fact]
    public void GlobalSet_NilRemovesKey()
    {
        Run("Data.Global[\"k\"] = nil");

        Assert.Empty(dataController.SetValues);
        Assert.Equal("k", Assert.Single(dataController.RemovedKeys));
    }

    [Fact]
    public void EntitySet_InfersTagsAndRemoves()
    {
        Run("Data.GetEntityData(__test_entity)[\"k\"] = 1.5");
        var value = Assert.Single(dataController.SetEntityValues);
        Assert.Equal(DataValueType.Float, value.Type);

        dataController.SetEntityValues.Clear();
        Run("Data.GetEntityData(__test_entity)[\"k\"] = nil");
        Assert.Empty(dataController.SetEntityValues);
        Assert.Equal("k", Assert.Single(dataController.RemovedEntityKeys));
    }

    [Fact]
    public void GlobalSet_TableWithoutVectorFields_LogsErrorAndDoesNotWrite()
    {
        Run("Data.Global[\"k\"] = { foo = 1 }");

        Assert.Empty(dataController.SetValues);
        Assert.Contains(logger.Messages, message => message.Contains("numeric X, Y, and Z"));
    }

    [Fact]
    public void GlobalSet_Vector3TableWithLowercaseFields_LogsErrorAndDoesNotWrite()
    {
        Run("Data.Global[\"k\"] = { x = 1, y = 2, z = 3 }");

        Assert.Empty(dataController.SetValues);
        Assert.Contains(logger.Messages, message => message.Contains("numeric X, Y, and Z"));
    }

    [Fact]
    public void GlobalSet_UnsupportedType_LogsErrorAndDoesNotWrite()
    {
        Run("Data.Global[\"k\"] = function() end");

        Assert.Empty(dataController.SetValues);
        Assert.Contains(logger.Messages, message => message.Contains("Unsupported value type"));
    }

    [Fact]
    public void GlobalSet_ReadOnlyKey_LogsErrorAndDoesNotWrite()
    {
        Run("Data.Global[\"__reserved\"] = 5");

        Assert.Empty(dataController.SetValues);
        Assert.Contains(logger.Messages, message => message.Contains("read-only"));
    }

    [Fact]
    public void EntitySet_ReadOnlyKey_LogsErrorAndDoesNotWrite()
    {
        Run("Data.GetEntityData(__test_entity)[\"__reserved\"] = 5");

        Assert.Empty(dataController.SetEntityValues);
        Assert.Contains(logger.Messages, message => message.Contains("read-only"));
    }

    /// <summary>
    ///     Executes a Lua statement without reading a result.
    /// </summary>
    /// <param name="script">Lua code.</param>
    private void Run(string script)
    {
        host.LoadAndExecuteString(script);
    }

    /// <summary>
    ///     Evaluates a Lua expression into the shared test result global.
    /// </summary>
    /// <param name="expression">Lua expression.</param>
    private void Eval(string expression)
    {
        host.LoadAndExecuteString($"__test_result = {expression}");
    }

    /// <summary>
    ///     Gets the type of the current test result.
    /// </summary>
    /// <returns>Lua type.</returns>
    private LuaType GetResultType()
    {
        var result = LuaType.None;
        host.ExecuteOther(_ =>
        {
            lua_getglobal(host.LuaState, "__test_result");
            result = lua_type(host.LuaState, -1);
            lua_pop(host.LuaState, 1);
        });

        return result;
    }

    /// <summary>
    ///     Gets the string value of the current test result.
    /// </summary>
    /// <returns>String value.</returns>
    private string GetResultString()
    {
        var result = "";
        host.ExecuteOther(_ =>
        {
            lua_getglobal(host.LuaState, "__test_result");
            result = lua_tostring(host.LuaState, -1);
            lua_pop(host.LuaState, 1);
        });

        return result;
    }

    /// <summary>
    ///     Gets the integer value of the current test result.
    /// </summary>
    /// <returns>Integer value.</returns>
    private long GetResultInteger()
    {
        var result = 0L;
        host.ExecuteOther(_ =>
        {
            lua_getglobal(host.LuaState, "__test_result");
            result = lua_tointeger(host.LuaState, -1);
            lua_pop(host.LuaState, 1);
        });

        return result;
    }

    /// <summary>
    ///     Gets the number value of the current test result.
    /// </summary>
    /// <returns>Number value.</returns>
    private double GetResultNumber()
    {
        var result = 0.0;
        host.ExecuteOther(_ =>
        {
            lua_getglobal(host.LuaState, "__test_result");
            result = lua_tonumber(host.LuaState, -1);
            lua_pop(host.LuaState, 1);
        });

        return result;
    }

    /// <summary>
    ///     Gets the boolean value of the current test result.
    /// </summary>
    /// <returns>Boolean value.</returns>
    private bool GetResultBoolean()
    {
        var result = false;
        host.ExecuteOther(_ =>
        {
            lua_getglobal(host.LuaState, "__test_result");
            result = lua_toboolean(host.LuaState, -1);
            lua_pop(host.LuaState, 1);
        });

        return result;
    }

    /// <summary>
    ///     Gets the light userdata value of the current test result.
    /// </summary>
    /// <returns>Light userdata pointer.</returns>
    private IntPtr GetResultUserData()
    {
        var result = IntPtr.Zero;
        host.ExecuteOther(_ =>
        {
            lua_getglobal(host.LuaState, "__test_result");
            result = lua_touserdata(host.LuaState, -1);
            lua_pop(host.LuaState, 1);
        });

        return result;
    }

    /// <summary>
    ///     Gets the X/Y/Z table value of the current test result.
    /// </summary>
    /// <returns>Vector value.</returns>
    private Vector3 GetResultVector3()
    {
        var result = Vector3.Zero;
        host.ExecuteOther(_ =>
        {
            lua_getglobal(host.LuaState, "__test_result");
            lua_getfield(host.LuaState, -1, "X");
            var x = (float)lua_tonumber(host.LuaState, -1);
            lua_pop(host.LuaState, 1);
            lua_getfield(host.LuaState, -1, "Y");
            var y = (float)lua_tonumber(host.LuaState, -1);
            lua_pop(host.LuaState, 1);
            lua_getfield(host.LuaState, -1, "Z");
            var z = (float)lua_tonumber(host.LuaState, -1);
            lua_pop(host.LuaState, 1);
            result = new Vector3(x, y, z);
        });

        return result;
    }

    /// <summary>
    ///     Recording data controller that captures writes.
    /// </summary>
    private sealed class RecordingDataController : IDataController
    {
        /// <summary>
        ///     Removed entity keys, in order.
        /// </summary>
        public List<string> RemovedEntityKeys { get; } = new();

        /// <summary>
        ///     Removed global keys, in order.
        /// </summary>
        public List<string> RemovedKeys { get; } = new();

        /// <summary>
        ///     Entity values written, in order.
        /// </summary>
        public List<DataValue> SetEntityValues { get; } = new();

        /// <summary>
        ///     Global values written, in order.
        /// </summary>
        public List<DataValue> SetValues { get; } = new();

        public void SetGlobalSync<T>(string key, T value) where T : notnull
        {
            SetValues.Add(ToDataValue(value));
        }

        public void RemoveGlobalSync(string key)
        {
            RemovedKeys.Add(key);
        }

        public void SetEntityKeyValueSync<T>(ulong entityId, string key, T value) where T : notnull
        {
            SetEntityValues.Add(ToDataValue(value));
        }

        public void RemoveEntityKeyValueSync(ulong entityId, string key)
        {
            RemovedEntityKeys.Add(key);
        }

        public void ClearEntityKeyValuesSync(ulong entityId)
        {
        }

        /// <summary>
        ///     Requires that the library passes already-tagged values.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>Tagged value.</returns>
        private static DataValue ToDataValue(object value)
        {
            return value is DataValue dataValue
                ? dataValue
                : throw new InvalidCastException($"Expected a DataValue but got {value.GetType()}.");
        }
    }

    /// <summary>
    ///     Recording logger that captures formatted log messages.
    /// </summary>
    /// <typeparam name="T">Logger category type.</typeparam>
    private sealed class RecordingLogger<T> : ILogger<T>
    {
        /// <summary>
        ///     Captured log messages, in order.
        /// </summary>
        public List<string> Messages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
