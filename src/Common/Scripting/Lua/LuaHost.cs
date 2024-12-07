// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using Microsoft.Extensions.Logging;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.Scripting.Lua;

/// <summary>
///     Execution host for a Lua script.
/// </summary>
public class LuaHost : IDisposable
{
    private const int QuickLookupSizeHint = 32;
    private readonly ILogger logger;

    private readonly int quickLookupStackPosition;
    private string library = "";
    private uint nextQuickLookupIndex = 1;

    public LuaHost(ILogger logger)
    {
        this.logger = logger;

        // Create basic Lua environment.
        LuaState = luaL_newstate();
        luaL_openlibs(LuaState);
        InstallUtilLibrary();

        // Create the Quick Lookup table for fast C#-to-Lua callback access.
        // No need to push it to global state - it will permanently live on the stack for fast access.
        lua_createtable(LuaState, QuickLookupSizeHint, 0);
        quickLookupStackPosition = lua_gettop(LuaState);
    }

    /// <summary>
    ///     Lua state handle.
    /// </summary>
    public IntPtr LuaState { get; }

    public void Dispose()
    {
        lua_close(LuaState);
    }

    /// <summary>
    ///     Starts a new library.
    /// </summary>
    /// <param name="name">Library name.</param>
    public void StartLibrary(string name)
    {
        library = name;

        luaL_checkstack(LuaState, 1, null);

        lua_createtable(LuaState, 0, 0);
        lua_setglobal(LuaState, library);

        // Place library name at top of stack while the library is open.
        lua_getglobal(LuaState, library);
    }

    /// <summary>
    ///     Adds a function to the current library.
    /// </summary>
    /// <param name="name">Function name.</param>
    /// <param name="func">Function.</param>
    public void AddLibraryFunction(string name, LuaCFunction func)
    {
        luaL_checkstack(LuaState, 2, null);

        lua_getglobal(LuaState, library);
        lua_pushcfunction(LuaState, func);
        lua_setfield(LuaState, -2, name);
    }

    /// <summary>
    ///     Ends the current library, if any.
    /// </summary>
    public void EndLibrary()
    {
        library = "";
        lua_pop(LuaState, 1);
    }

    /// <summary>
    ///     Pops the function from the top of the Lua stack and adds it to the quick
    ///     lookup table.
    /// </summary>
    /// <returns>Quick lookup index for later retrieval.</returns>
    public uint AddQuickLookupFunction()
    {
        lua_seti(LuaState, quickLookupStackPosition, (int)nextQuickLookupIndex);
        return nextQuickLookupIndex++;
    }

    /// <summary>
    ///     Calls the Lua function at the given position of the quick lookup table.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="par">Parameters.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if index is out of range.</exception>
    /// <exception cref="LuaException">Thrown if the quick lookup table is corrupt.</exception>
    public void CallQuickLookupFunction<TParams, TResult>(uint index, TParams par)
    {
        if (index < 1 || index >= nextQuickLookupIndex) throw new ArgumentOutOfRangeException("index");

        luaL_checkstack(LuaState, 1, null); // TODO space for args/results

        var ftype = lua_geti(LuaState, quickLookupStackPosition, (int)index);
        if (ftype != LuaType.Function) throw new LuaException("Quick lookup table has non-function value.");

        // TODO handle args

        var result = lua_pcall(LuaState, 0, 0, 0);
        if (result != LuaResult.Ok)
            // TODO handle error
            throw new LuaException("Error while calling function.");

        // TODO handle results
    }

    /// <summary>
    ///     Asynchronously calls the Lua function at the given position of the quick lookup table.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="par"></param>
    /// <typeparam name="TParams"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public Task CallQuickLookupFunctionAsync<TParams, TResult>(uint index, TParams par)
    {
        return Task.Run(() => CallQuickLookupFunction<TParams, TResult>(index, par));
    }

    /// <summary>
    ///     Loads and executes a script file.
    /// </summary>
    /// <param name="filename">Path to script file.</param>
    public void LoadScript(string filename)
    {
        luaL_loadfile(LuaState, filename);
        lua_pcall(LuaState, 0, LUA_MULTRET, 0);
    }

    /// <summary>
    ///     Installs the per-script util library into the host.
    /// </summary>
    private void InstallUtilLibrary()
    {
        try
        {
            StartLibrary("util");
            AddLibraryFunction("log_trace", UtilLogTrace);
            AddLibraryFunction("log_debug", UtilLogDebug);
            AddLibraryFunction("log_info", UtilLogInformation);
            AddLibraryFunction("log_warn", UtilLogWarn);
            AddLibraryFunction("log_error", UtilLogError);
            AddLibraryFunction("log_crit", UtilLogCritical);
        }
        finally
        {
            EndLibrary();
        }
    }

    /// <summary>
    ///     Provides the util.log_info Lua function which logs a trace message.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always 0.</returns>
    private int UtilLogTrace(IntPtr luaState)
    {
        try
        {
            if (lua_gettop(luaState) != 1) throw new LuaException("Requires one argument.");
            if (!lua_isstring(luaState, 1)) throw new LuaException("First argument must be a string.");
            var msg = lua_tostring(luaState, 1);

            logger.LogTrace(msg);
        }
        catch (Exception e)
        {
            // Log error but continue the script as-is.
            logger.LogError(e, "Error in util.log_trace.");
        }

        return (int)LuaResult.Ok;
    }

    /// <summary>
    ///     Provides the util.log_info Lua function which logs a debug message.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always 0.</returns>
    private int UtilLogDebug(IntPtr luaState)
    {
        try
        {
            if (lua_gettop(luaState) != 1) throw new LuaException("Requires one argument.");
            if (!lua_isstring(luaState, 1)) throw new LuaException("First argument must be a string.");
            var msg = lua_tostring(luaState, 1);

            logger.LogDebug(msg);
        }
        catch (Exception e)
        {
            // Log error but continue the script as-is.
            logger.LogError(e, "Error in util.log_debug.");
        }

        return (int)LuaResult.Ok;
    }

    /// <summary>
    ///     Provides the util.log_info Lua function which logs an informational message.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always 0.</returns>
    private int UtilLogInformation(IntPtr luaState)
    {
        try
        {
            if (lua_gettop(luaState) != 1) throw new LuaException("Requires one argument.");
            if (!lua_isstring(luaState, 1)) throw new LuaException("First argument must be a string.");
            var msg = lua_tostring(luaState, 1);

            logger.LogInformation(msg);
        }
        catch (Exception e)
        {
            // Log error but continue the script as-is.
            logger.LogError(e, "Error in util.log_info.");
        }

        return (int)LuaResult.Ok;
    }

    /// <summary>
    ///     Provides the util.log_info Lua function which logs a warning message.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always 0.</returns>
    private int UtilLogWarn(IntPtr luaState)
    {
        try
        {
            if (lua_gettop(luaState) != 1) throw new LuaException("Requires one argument.");
            if (!lua_isstring(luaState, 1)) throw new LuaException("First argument must be a string.");
            var msg = lua_tostring(luaState, 1);

            logger.LogWarning(msg);
        }
        catch (Exception e)
        {
            // Log error but continue the script as-is.
            logger.LogError(e, "Error in util.log_warn.");
        }

        return (int)LuaResult.Ok;
    }

    /// <summary>
    ///     Provides the util.log_info Lua function which logs an error message.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always 0.</returns>
    private int UtilLogError(IntPtr luaState)
    {
        try
        {
            if (lua_gettop(luaState) != 1) throw new LuaException("Requires one argument.");
            if (!lua_isstring(luaState, 1)) throw new LuaException("First argument must be a string.");
            var msg = lua_tostring(luaState, 1);

            logger.LogError(msg);
        }
        catch (Exception e)
        {
            // Log error but continue the script as-is.
            logger.LogError(e, "Error in util.log_error.");
        }

        return (int)LuaResult.Ok;
    }

    /// <summary>
    ///     Provides the util.log_info Lua function which logs a critical message.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Always 0.</returns>
    private int UtilLogCritical(IntPtr luaState)
    {
        try
        {
            if (lua_gettop(luaState) != 1) throw new LuaException("Requires one argument.");
            if (!lua_isstring(luaState, 1)) throw new LuaException("First argument must be a string.");
            var msg = lua_tostring(luaState, 1);

            logger.LogCritical(msg);
        }
        catch (Exception e)
        {
            // Log error but continue the script as-is.
            logger.LogError(e, "Error in util.log_crit.");
        }

        return (int)LuaResult.Ok;
    }
}