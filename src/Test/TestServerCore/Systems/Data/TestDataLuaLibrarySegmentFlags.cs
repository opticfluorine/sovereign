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
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
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
///     Tests for the world segment flag API exposed by DataLuaLibrary, driven through a real
///     Lua host.
/// </summary>
public class TestDataLuaLibrarySegmentFlags : IDisposable
{
    private const int SegmentLength = 32;

    private readonly RecordingLogger<DataLuaLibrary> logger = new();
    private readonly SegmentFlagManager segmentFlagManager = new();
    private readonly WorldSegmentResolver segmentResolver = new();
    private readonly PersistenceController persistenceController = new();
    private readonly FakeEventSender eventSender = new();
    private readonly ScriptManager scriptManager;

    private readonly LuaHost host;

    private readonly DataLuaLibrary library;

    public TestDataLuaLibrarySegmentFlags()
    {
        scriptManager = new ScriptManager(NullLogger<ScriptManager>.Instance);
        var scriptingServices = new ScriptingServices(scriptManager);

        library = new DataLuaLibrary(null!, scriptingServices, logger, new FakeDataServices(),
            segmentFlagManager, segmentResolver, persistenceController, eventSender);

        host = new LuaHost(NullLogger.Instance, ".");
        new Sovereign.EngineCore.Lua.EnumsLuaLibrary().Install(host);
        library.Install(host);
    }

    public void Dispose()
    {
        host.Dispose();
        library.Dispose();
        scriptManager.Dispose();
    }

    [Fact]
    public void GetSegmentFlags_UnflaggedSegment_ReturnsZero()
    {
        Assert.Equal(0, RunLuaInteger("Data.GetSegmentFlags({ X = 1, Y = 2, Z = 3 })"));
    }

    [Fact]
    public void GetSegmentFlags_FlaggedSegment_ReturnsCachedMask()
    {
        segmentFlagManager.SetFlagsInCache(new GridPosition(1, 2, 3), WorldSegmentFlag.Pvp);

        Assert.Equal((long)WorldSegmentFlag.Pvp,
            RunLuaInteger("Data.GetSegmentFlags({ X = 1, Y = 2, Z = 3 })"));
    }

    [Fact]
    public void GetSegmentFlagsForPosition_MapsPositivePosition()
    {
        var segment = new GridPosition(2, 3, 4);
        segmentFlagManager.SetFlagsInCache(segment, WorldSegmentFlag.Pvp);

        var position = $"{{ X = {segment.X * SegmentLength + 5}, Y = {segment.Y * SegmentLength + 5}, " +
                       $"Z = {segment.Z * SegmentLength + 5} }}";
        Assert.Equal((long)WorldSegmentFlag.Pvp,
            RunLuaInteger($"Data.GetSegmentFlagsForPosition({position})"));
    }

    [Fact]
    public void GetSegmentFlagsForPosition_MapsNegativePosition()
    {
        var segment = new GridPosition(-2, 0, 0);
        segmentFlagManager.SetFlagsInCache(segment, WorldSegmentFlag.Pvp);

        Assert.Equal((long)WorldSegmentFlag.Pvp,
            RunLuaInteger($"Data.GetSegmentFlagsForPosition({{ X = {-SegmentLength * 2 + 5}, Y = 0.0, Z = 0.0 }})"));
    }

    [Fact]
    public void GetSegmentFlagsForPosition_MapsSegmentBoundary()
    {
        var segment = new GridPosition(1, 0, 0);
        segmentFlagManager.SetFlagsInCache(segment, WorldSegmentFlag.Pvp);

        Assert.Equal((long)WorldSegmentFlag.Pvp,
            RunLuaInteger($"Data.GetSegmentFlagsForPosition({{ X = {SegmentLength}.0, Y = 0.0, Z = 0.0 }})"));
        Assert.Equal(0,
            RunLuaInteger($"Data.GetSegmentFlagsForPosition({{ X = {SegmentLength - 0.001}, Y = 0.0, Z = 0.0 }})"));
    }

    [Fact]
    public void SetSegmentFlag_SendsSingleSegmentEvent()
    {
        host.LoadAndExecuteString("Data.SetSegmentFlag({ X = 5, Y = 6, Z = 7 }, WorldSegmentFlag.Pvp)");

        var details = AssertSingleEvent();
        Assert.Equal(new GridPosition(5, 6, 7), details.MinSegment);
        Assert.Equal(new GridPosition(5, 6, 7), details.MaxSegment);
        Assert.Equal(WorldSegmentFlag.Pvp, details.Flag);
        Assert.True(details.Set);
    }

    [Fact]
    public void ClearSegmentFlag_SendsSingleSegmentEvent()
    {
        host.LoadAndExecuteString("Data.ClearSegmentFlag({ X = 5, Y = 6, Z = 7 }, WorldSegmentFlag.Pvp)");

        var details = AssertSingleEvent();
        Assert.Equal(new GridPosition(5, 6, 7), details.MinSegment);
        Assert.Equal(new GridPosition(5, 6, 7), details.MaxSegment);
        Assert.Equal(WorldSegmentFlag.Pvp, details.Flag);
        Assert.False(details.Set);
    }

    [Fact]
    public void SetSegmentFlagForPosition_SendsEventForContainingSegment()
    {
        host.LoadAndExecuteString(
            $"Data.SetSegmentFlagForPosition({{ X = {SegmentLength * 2 + 1}.0, Y = 0.5, Z = -0.5 }}, " +
            "WorldSegmentFlag.Pvp)");

        var details = AssertSingleEvent();
        Assert.Equal(new GridPosition(2, 0, -1), details.MinSegment);
        Assert.Equal(new GridPosition(2, 0, -1), details.MaxSegment);
        Assert.True(details.Set);
    }

    [Fact]
    public void ClearSegmentFlagForPosition_SendsEventForContainingSegment()
    {
        host.LoadAndExecuteString(
            $"Data.ClearSegmentFlagForPosition({{ X = {SegmentLength * 2 + 1}.0, Y = 0.5, Z = -0.5 }}, " +
            "WorldSegmentFlag.Pvp)");

        var details = AssertSingleEvent();
        Assert.Equal(new GridPosition(2, 0, -1), details.MinSegment);
        Assert.False(details.Set);
    }

    [Fact]
    public void SetSegmentFlagInBox_SendsNormalizedInclusiveBox()
    {
        host.LoadAndExecuteString(
            "Data.SetSegmentFlagInBox({ X = 3, Y = 2, Z = 1 }, { X = 1, Y = 4, Z = 2 }, WorldSegmentFlag.Pvp)");

        var details = AssertSingleEvent();
        Assert.Equal(new GridPosition(1, 2, 1), details.MinSegment);
        Assert.Equal(new GridPosition(3, 4, 2), details.MaxSegment);
        Assert.Equal(WorldSegmentFlag.Pvp, details.Flag);
        Assert.True(details.Set);
    }

    [Fact]
    public void ClearSegmentFlagInBox_SendsNormalizedInclusiveBox()
    {
        host.LoadAndExecuteString(
            "Data.ClearSegmentFlagInBox({ X = 1, Y = 4, Z = 2 }, { X = 3, Y = 2, Z = 1 }, WorldSegmentFlag.Pvp)");

        var details = AssertSingleEvent();
        Assert.Equal(new GridPosition(1, 2, 1), details.MinSegment);
        Assert.Equal(new GridPosition(3, 4, 2), details.MaxSegment);
        Assert.False(details.Set);
    }

    [Fact]
    public void Validation_NonTableArgument_LogsErrorAndSendsNoEvent()
    {
        Assert.Equal(0, RunLuaInteger("Data.GetSegmentFlags(5)"));

        host.LoadAndExecuteString("Data.SetSegmentFlag(5, WorldSegmentFlag.Pvp)");

        Assert.Empty(eventSender.SentEvents);
        Assert.Contains(logger.Messages, message => message.Contains("must be a Vector3"));
    }

    [Fact]
    public void SetSegmentFlag_TruncatesFractionalCoordinates()
    {
        host.LoadAndExecuteString("Data.SetSegmentFlag({ X = 1.5, Y = 2.9, Z = 3.0 }, WorldSegmentFlag.Pvp)");

        var details = AssertSingleEvent();
        Assert.Equal(new GridPosition(1, 2, 3), details.MinSegment);
        Assert.Equal(new GridPosition(1, 2, 3), details.MaxSegment);
    }

    [Fact]
    public void Validation_ZeroFlagBits_LogsErrorAndSendsNoEvent()
    {
        host.LoadAndExecuteString("Data.SetSegmentFlag({ X = 1, Y = 2, Z = 3 }, 0)");

        Assert.Empty(eventSender.SentEvents);
        Assert.Contains(logger.Messages, message => message.Contains("non-zero combination"));
    }

    [Fact]
    public void Validation_OutOfMaskFlagBits_LogsErrorAndSendsNoEvent()
    {
        host.LoadAndExecuteString("Data.SetSegmentFlag({ X = 1, Y = 2, Z = 3 }, 0x40)");

        Assert.Empty(eventSender.SentEvents);
        Assert.Contains(logger.Messages, message => message.Contains("non-zero combination"));
    }

    [Fact]
    public void Validation_OversizedBox_LogsErrorAndSendsNoEvent()
    {
        host.LoadAndExecuteString(
            "Data.SetSegmentFlagInBox({ X = 0, Y = 0, Z = 0 }, { X = 65536, Y = 0, Z = 0 }, WorldSegmentFlag.Pvp)");

        Assert.Empty(eventSender.SentEvents);
        Assert.Contains(logger.Messages, message => message.Contains("exceeds"));
    }

    [Fact]
    public void FlagConstants_ExposedAsIntegers()
    {
        Assert.Equal((long)WorldSegmentFlag.Pvp, RunLuaInteger("WorldSegmentFlag.Pvp"));
        Assert.Equal((long)WorldSegmentFlag.None, RunLuaInteger("WorldSegmentFlag.None"));
    }

    /// <summary>
    ///     Executes a Lua expression and returns its integer value.
    /// </summary>
    /// <param name="expression">Lua expression.</param>
    /// <returns>Integer value of the expression.</returns>
    private long RunLuaInteger(string expression)
    {
        host.LoadAndExecuteString($"__test_result = {expression}");

        var result = 0L;
        host.ExecuteOther(_ =>
        {
            luaL_checkstack(host.LuaState, 1, null);
            lua_getglobal(host.LuaState, "__test_result");
            result = lua_tointeger(host.LuaState, -1);
            lua_pop(host.LuaState, 1);
        });

        return result;
    }

    /// <summary>
    ///     Asserts that exactly one segment flag update event was sent and returns its details.
    /// </summary>
    /// <returns>Event details.</returns>
    private SegmentFlagUpdateEventDetails AssertSingleEvent()
    {
        var ev = Assert.Single(eventSender.SentEvents);
        Assert.Equal(Sovereign.EngineCore.Events.EventId.Server_Persistence_UpdateSegmentFlags, ev.EventId);
        return Assert.IsType<SegmentFlagUpdateEventDetails>(ev.EventDetails);
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
