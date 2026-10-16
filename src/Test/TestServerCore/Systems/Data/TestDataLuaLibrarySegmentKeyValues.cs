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
using Sovereign.EngineCore.Components.Types;
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
///     Tests for the world segment key-value API exposed by DataLuaLibrary, driven through a real
///     Lua host with a recording controller fake.
/// </summary>
public class TestDataLuaLibrarySegmentKeyValues : IDisposable
{
    private const string SegmentLiteral = "{ X = 1, Y = 2, Z = 3 }";

    private static readonly GridPosition Segment = new(1, 2, 3);

    private readonly FakeEventSender eventSender = new();
    private readonly RecordingLogger<DataLuaLibrary> logger = new();
    private readonly PersistenceController persistenceController = new();
    private readonly ScriptManager scriptManager;
    private readonly SegmentFlagManager segmentFlagManager = new();
    private readonly RecordingSegmentKeyValueController segmentKeyValueController = new();
    private readonly WorldSegmentResolver segmentResolver = new();

    private readonly LuaHost host;

    private readonly DataLuaLibrary library;

    public TestDataLuaLibrarySegmentKeyValues()
    {
        scriptManager = new ScriptManager(NullLogger<ScriptManager>.Instance);
        var scriptingServices = new ScriptingServices(scriptManager);

        library = new DataLuaLibrary(null!, scriptingServices, logger, new FakeDataServices(),
            segmentFlagManager, segmentKeyValueController, segmentResolver, persistenceController, eventSender);

        host = new LuaHost(NullLogger.Instance, ".");
        library.Install(host);
    }

    public void Dispose()
    {
        host.Dispose();
        library.Dispose();
        scriptManager.Dispose();
    }

    [Fact]
    public void GetSegmentData_ReturnsNativeTypes()
    {
        segmentKeyValueController.Seed(Segment, "s", DataValue.FromString("x"));
        segmentKeyValueController.Seed(Segment, "i", DataValue.FromInt(7));
        segmentKeyValueController.Seed(Segment, "f", DataValue.FromFloat(1.25));
        segmentKeyValueController.Seed(Segment, "b", DataValue.FromBool(true));

        Eval($"Data.GetSegmentData({SegmentLiteral})[\"s\"]");
        Assert.Equal(LuaType.String, GetResultType());
        Assert.Equal("x", GetResultString());

        Eval($"Data.GetSegmentData({SegmentLiteral})[\"i\"]");
        Assert.Equal(7L, GetResultInteger());

        Eval($"Data.GetSegmentData({SegmentLiteral})[\"f\"]");
        Assert.Equal(1.25, GetResultNumber());

        Eval($"Data.GetSegmentData({SegmentLiteral})[\"b\"]");
        Assert.Equal(LuaType.Boolean, GetResultType());
    }

    [Fact]
    public void GetSegmentData_Vector3_ReturnsTableWithUppercaseFields()
    {
        segmentKeyValueController.Seed(Segment, "v", DataValue.FromVector3(new Vector3(1.0f, 2.0f, 3.0f)));

        Eval($"Data.GetSegmentData({SegmentLiteral})[\"v\"]");

        Assert.Equal(LuaType.Table, GetResultType());
        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), GetResultVector3());
    }

    [Fact]
    public void GetSegmentData_EntityId_ReturnsLightUserData()
    {
        segmentKeyValueController.Seed(Segment, "owner", DataValue.FromEntityId(0x1234));

        Eval($"Data.GetSegmentData({SegmentLiteral})[\"owner\"]");

        Assert.Equal(LuaType.LightUserData, GetResultType());
        Assert.Equal((IntPtr)0x1234, GetResultUserData());
    }

    [Fact]
    public void GetSegmentData_MissingKey_ReturnsNil()
    {
        segmentKeyValueController.MarkLoaded(Segment);

        Eval($"Data.GetSegmentData({SegmentLiteral})[\"missing\"]");

        Assert.Equal(LuaType.Nil, GetResultType());
    }

    [Fact]
    public void GetSegmentData_UnloadedSegment_ReturnsNilAndWarns()
    {
        Eval($"Data.GetSegmentData({SegmentLiteral})[\"Owner\"]");

        Assert.Equal(LuaType.Nil, GetResultType());
        Assert.Contains(logger.Messages, message => message.Contains("unloaded segment"));
    }

    [Theory]
    [InlineData("\"hi\"", DataValueType.String)]
    [InlineData("5", DataValueType.Int)]
    [InlineData("5.5", DataValueType.Float)]
    [InlineData("true", DataValueType.Bool)]
    public void SetSegmentData_InfersScalarTag(string luaValue, DataValueType expectedType)
    {
        Run($"Data.GetSegmentData({SegmentLiteral})[\"k\"] = {luaValue}");

        var (segment, key, value) = Assert.Single(segmentKeyValueController.SetValues);
        Assert.Equal(Segment, segment);
        Assert.Equal("k", key);
        Assert.Equal(expectedType, value.Type);
    }

    [Fact]
    public void SetSegmentData_Vector3Table_InfersVector3Tag()
    {
        Run($"Data.GetSegmentData({SegmentLiteral})[\"k\"] = {{ X = 1, Y = 2, Z = 3 }}");

        var (_, _, value) = Assert.Single(segmentKeyValueController.SetValues);
        Assert.Equal(DataValueType.Vector3, value.Type);
        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), value.AsVector3());
    }

    [Fact]
    public void SetSegmentData_NilRemovesKey()
    {
        Run($"Data.GetSegmentData({SegmentLiteral})[\"k\"] = nil");

        Assert.Empty(segmentKeyValueController.SetValues);
        var (segment, key) = Assert.Single(segmentKeyValueController.RemovedKeys);
        Assert.Equal(Segment, segment);
        Assert.Equal("k", key);
    }

    [Fact]
    public void GetSegmentData_FractionalComponent_LogsErrorAndReturnsNoTable()
    {
        Eval("Data.GetSegmentData({ X = 1.5, Y = 2, Z = 3 })");

        Assert.Equal(LuaType.Nil, GetResultType());
        Assert.Contains(logger.Messages, message => message.Contains("integral"));
    }

    [Fact]
    public void GetSegmentData_NonTableArgument_LogsErrorAndReturnsNoTable()
    {
        Eval("Data.GetSegmentData(5)");

        Assert.Equal(LuaType.Nil, GetResultType());
        Assert.Contains(logger.Messages, message => message.Contains("Vector3"));
    }

    [Fact]
    public void SetSegmentData_ReadOnlyKey_LogsErrorAndDoesNotWrite()
    {
        Run($"Data.GetSegmentData({SegmentLiteral})[\"__reserved\"] = 5");

        Assert.Empty(segmentKeyValueController.SetValues);
        Assert.Contains(logger.Messages, message => message.Contains("read-only"));
    }

    [Fact]
    public void SetSegmentData_TableWithoutVectorFields_LogsErrorAndDoesNotWrite()
    {
        Run($"Data.GetSegmentData({SegmentLiteral})[\"k\"] = {{ foo = 1 }}");

        Assert.Empty(segmentKeyValueController.SetValues);
        Assert.Contains(logger.Messages, message => message.Contains("numeric X, Y, and Z"));
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
