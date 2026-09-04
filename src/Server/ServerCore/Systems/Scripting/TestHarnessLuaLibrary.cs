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
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.Scripting.Lua;
using Sovereign.ServerCore.Configuration;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Lua library that provides the Test harness API for script test suites.
/// </summary>
/// <remarks>
///     This library is a no-op unless the test harness is enabled via configuration.
/// </remarks>
public class TestHarnessLuaLibrary(
    ScriptManager scriptManager,
    TestHarnessResultsCollector collector,
    IOptions<TestHarnessOptions> options,
    ILogger<TestHarnessLuaLibrary> logger,
    ScriptingServices scriptingServices)
    : ILuaLibrary
{
    /// <summary>
    ///     Lua chunk defining the assertion helpers. These are implemented in Lua so that
    ///     they can raise ordinary Lua errors which unwind into the protected call made by
    ///     Test.Case or Test.Step.
    /// </summary>
    private const string AssertionHelpers =
        @"
        function Test.AssertFailed(message)
            error(message or ""assertion failed"", 2)
        end

        function Test.AssertTrue(condition, message)
            if not condition then
                error(message or ""expected condition to be true"", 2)
            end
        end

        function Test.AssertNil(value, message)
            if value ~= nil then
                error(message or string.format(""expected nil but got %s"", tostring(value)), 2)
            end
        end

        function Test.AssertEqual(expected, actual, message)
            if expected ~= actual then
                local detail = string.format(""expected %s but got %s"",
                    tostring(expected), tostring(actual))
                error(message and (message .. "" ("" .. detail .. "")"") or detail, 2)
            end
        end

        function Test.AssertNear(expected, actual, epsilon, message)
            if math.abs(expected - actual) > epsilon then
                local detail = string.format(""expected %s to be within %s of %s"",
                    tostring(actual), tostring(epsilon), tostring(expected))
                error(message and (message .. "" ("" .. detail .. "")"") or detail, 2)
            end
        end
        ";

    public void Install(LuaHost luaHost)
    {
        if (!options.Value.Enabled) return;

        try
        {
            luaHost.BeginLibrary("Test");
            luaHost.AddLibraryFunction(nameof(Case), Case);
            luaHost.AddLibraryFunction(nameof(Async), Async);
            luaHost.AddLibraryFunction(nameof(Step), Step);
            luaHost.AddLibraryFunction(nameof(Pass), Pass);
            luaHost.AddLibraryFunction(nameof(Fail), Fail);
        }
        finally
        {
            luaHost.EndLibrary();
        }

        luaHost.LoadAndExecuteString(AssertionHelpers);
    }

    /// <summary>
    ///     Implements Test.Case(name, fn), which registers a synchronous test and runs it immediately.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of results pushed onto the Lua stack.</returns>
    private int Case(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        if (!ResolveSuite(mainState, "Test.Case", out var suite)) return 0;

        if (lua_gettop(luaState) != 2 || !lua_isstring(luaState, 1) || !lua_isfunction(luaState, 2))
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError(luaState, "Test.Case requires a string name and a function.");
            return 0;
        }

        var name = lua_tostring(luaState, 1) ?? "";
        if (!collector.RegisterTest(suite, name))
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogWarning("Duplicate test {Name}; not executed.", name);
            return 0;
        }

        var startTimestamp = Stopwatch.GetTimestamp();

        // Rearrange the stack for a protected call with the traceback handler:
        // [name, fn] -> [fn] -> [fn, traceback] -> [traceback, fn].
        luaL_checkstack(luaState, 2, null);
        lua_rotate(luaState, 1, 1);
        lua_pop(luaState, 1);
        lua_getfield(luaState, LUA_REGISTRYINDEX, LuaHost.TracebackRegistryKey);
        lua_rotate(luaState, 1, 1);

        var result = lua_pcall(luaState, 0, 0, 1);
        var durationMs = 1000.0 * (Stopwatch.GetTimestamp() - startTimestamp) / Stopwatch.Frequency;

        if (result != LuaResult.Ok)
        {
            var message = lua_tostring(luaState, -1) ?? "unknown Lua error";
            collector.CompleteTest(suite, name, false, message, durationMs);
        }
        else
        {
            collector.CompleteTest(suite, name, true, "", durationMs);
        }

        return 0;
    }

    /// <summary>
    ///     Implements Test.Async(name), which declares an asynchronous test to be completed later.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of results pushed onto the Lua stack.</returns>
    private int Async(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        if (!ResolveSuite(mainState, "Test.Async", out var suite)) return 0;

        if (lua_gettop(luaState) != 1 || !lua_isstring(luaState, 1))
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError(luaState, "Test.Async requires a string name.");
            return 0;
        }

        var name = lua_tostring(luaState, 1) ?? "";
        if (!collector.RegisterTest(suite, name))
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogWarning("Duplicate test {Name}; not registered.", name);
        }

        return 0;
    }

    /// <summary>
    ///     Implements Test.Step(name, fn), which runs a step of an asynchronous test under pcall.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of results pushed onto the Lua stack.</returns>
    private int Step(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        if (!ResolveSuite(mainState, "Test.Step", out var suite)) return 0;

        if (lua_gettop(luaState) != 2 || !lua_isstring(luaState, 1) || !lua_isfunction(luaState, 2))
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError(luaState, "Test.Step requires a string name and a function.");
            return 0;
        }

        var name = lua_tostring(luaState, 1) ?? "";

        // Rearrange the stack for a protected call with the traceback handler:
        // [name, fn] -> [fn] -> [fn, traceback] -> [traceback, fn].
        luaL_checkstack(luaState, 2, null);
        lua_rotate(luaState, 1, 1);
        lua_pop(luaState, 1);
        lua_getfield(luaState, LUA_REGISTRYINDEX, LuaHost.TracebackRegistryKey);
        lua_rotate(luaState, 1, 1);

        var result = lua_pcall(luaState, 0, 0, 1);
        if (result != LuaResult.Ok)
        {
            var message = lua_tostring(luaState, -1) ?? "unknown Lua error";
            collector.CompleteTest(suite, name, false, message);
        }

        return 0;
    }

    /// <summary>
    ///     Implements Test.Pass(name), which completes the named test as passed.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of results pushed onto the Lua stack.</returns>
    private int Pass(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        if (!ResolveSuite(mainState, "Test.Pass", out var suite)) return 0;

        if (lua_gettop(luaState) != 1 || !lua_isstring(luaState, 1))
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError(luaState, "Test.Pass requires a string name.");
            return 0;
        }

        var name = lua_tostring(luaState, 1) ?? "";
        collector.CompleteTest(suite, name, true, "");
        return 0;
    }

    /// <summary>
    ///     Implements Test.Fail(name, [message]), which completes the named test as failed.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of results pushed onto the Lua stack.</returns>
    private int Fail(IntPtr luaState)
    {
        var mainState = LuaUtil.GetMainThread(luaState);
        if (!ResolveSuite(mainState, "Test.Fail", out var suite)) return 0;

        var top = lua_gettop(luaState);
        if (top is < 1 or > 2 || !lua_isstring(luaState, 1))
        {
            scriptingServices.GetScriptLogger(mainState, logger)
                .LogError(luaState, "Test.Fail requires a string name and an optional message.");
            return 0;
        }

        var name = lua_tostring(luaState, 1) ?? "";
        var message = "Test.Fail called without message.";
        if (top == 2 && lua_isstring(luaState, 2)) message = lua_tostring(luaState, 2) ?? message;
        collector.CompleteTest(suite, name, false, message);
        return 0;
    }

    /// <summary>
    ///     Resolves the suite (script) name for the calling Lua host.
    /// </summary>
    /// <param name="mainState">Main thread Lua state.</param>
    /// <param name="functionName">Name of the calling harness function for error reporting.</param>
    /// <param name="suite">Resolved suite name, or empty if the host could not be resolved.</param>
    /// <returns>true if the suite was resolved, false otherwise.</returns>
    private bool ResolveSuite(IntPtr mainState, string functionName, out string suite)
    {
        suite = "";
        if (!scriptManager.TryGetHost(mainState, out var host))
        {
            logger.LogError("No Lua host found for {FunctionName}.", functionName);
            return false;
        }

        suite = host.Name;
        return true;
    }
}
