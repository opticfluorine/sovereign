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

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.Scripting.Lua;

/// <summary>
///     Execution host for a Lua script.
/// </summary>
/// <remarks>
///     This class is thread-safe, however deadlocks may occur if the thread holding the lock
///     is busy (e.g. with an infinite loop or blocking call in an executing Lua script).
/// </remarks>
public class LuaHost : IDisposable
{
    private const int QuickLookupSizeHint = 32;
    private const string QuickLookupKey = "sovereign_ql";
    private readonly List<GCHandle> bindings = new();
    private readonly List<string> functionNames = new();
    private readonly Lock opsLock = new();
    private readonly int quickLookupStackPosition;
    private readonly int tracebackStackPosition;
    private string library = "";
    private uint nextQuickLookupIndex = 1;

    public LuaHost(ILogger logger)
    {
        Logger = logger;

        // Create basic Lua environment.
        LuaState = luaL_newstate();
        lua_gc(LuaState, LuaGcWhat.Gen, GcDefaultMinorMul, GcDefaultMajorMul);
        luaL_openlibs(LuaState);
        InstallUtilLibrary();

        luaL_checkstack(LuaState, 4, null);

        // Install traceback error handler.
        lua_getglobal(LuaState, "debug");
        lua_getfield(LuaState, -1, "traceback");
        tracebackStackPosition = lua_gettop(LuaState);

        // Create the Quick Lookup table for fast C#-to-Lua callback access.
        // No need to push it to global state - it will permanently live on the stack for fast access.
        // Also set the table into the registry for lookup in other stacks.
        lua_createtable(LuaState, QuickLookupSizeHint, 0);
        quickLookupStackPosition = lua_gettop(LuaState);
        lua_pushnil(LuaState);
        lua_copy(LuaState, quickLookupStackPosition, -1);
        lua_setfield(LuaState, LUA_REGISTRYINDEX, QuickLookupKey);

#if !DEBUG
        // Remove unwanted default modules.
        lua_pushnil(LuaState);
        lua_setglobal(LuaState, "debug");
#endif
    }

    /// <summary>
    ///     Entity parameter hints indexed by function name.
    /// </summary>
    public ConcurrentDictionary<string, List<string>> EntityParameterHintsByFunction { get; } = new();

    /// <summary>
    ///     List of global functions provided by the script.
    /// </summary>
    public IReadOnlyList<string> FunctionNames => functionNames;

    /// <summary>
    ///     Logger for this host.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    ///     Flag indicating whether this host has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    ///     Lua state handle.
    /// </summary>
    public IntPtr LuaState { get; }

    /// <summary>
    ///     Name of this Lua host.
    /// </summary>
    public string Name { get; set; } = "";

    public void Dispose()
    {
        lock (opsLock)
        {
            lua_close(LuaState);
            foreach (var binding in bindings)
                if (binding.IsAllocated)
                    binding.Free();
            IsDisposed = true;
        }
    }

    /// <summary>
    ///     Starts a new library.
    /// </summary>
    /// <param name="name">Library name.</param>
    public void BeginLibrary(string name)
    {
        lock (opsLock)
        {
            library = name;

            luaL_checkstack(LuaState, 1, null);

            lua_createtable(LuaState, 0, 0);
            lua_setglobal(LuaState, library);
        }
    }

    /// <summary>
    ///     Adds a function to the current library.
    /// </summary>
    /// <param name="name">Function name.</param>
    /// <param name="func">Function.</param>
    public void AddLibraryFunction(string name, LuaCFunction func)
    {
        lock (opsLock)
        {
            luaL_checkstack(LuaState, 2, null);

            lua_getglobal(LuaState, library);
            bindings.Add(GCHandle.Alloc(func));
            lua_pushcfunction(LuaState, func);
            lua_setfield(LuaState, -2, name);
            lua_pop(LuaState, 1);
        }
    }

    /// <summary>
    ///     Pushes a function onto the top of the Lua stack.
    /// </summary>
    /// <param name="func">Function.</param>
    public void PushFunction(LuaCFunction func)
    {
        lock (opsLock)
        {
            luaL_checkstack(LuaState, 1, null);

            bindings.Add(GCHandle.Alloc(func));
            lua_pushcfunction(LuaState, func);
        }
    }

    /// <summary>
    ///     Adds an integer-valued constant to the current library.
    /// </summary>
    /// <param name="name">Constant name.</param>
    /// <param name="value">Value.</param>
    public void AddLibraryConstant(string name, long value)
    {
        lock (opsLock)
        {
            luaL_checkstack(LuaState, 2, null);

            lua_getglobal(LuaState, library);
            lua_pushinteger(LuaState, value);
            lua_setfield(LuaState, -2, name);
            lua_pop(LuaState, 1);
        }
    }

    /// <summary>
    ///     Ends the current library, if any.
    /// </summary>
    public void EndLibrary()
    {
        lock (opsLock)
        {
            library = "";
        }
    }

    /// <summary>
    ///     Pops the function from the top of the Lua stack and adds it to the quick
    ///     lookup table. This should only be called from within a protected Lua
    ///     context where the managed opsLock is already held by the caller.
    /// </summary>
    /// <returns>Quick lookup index for later retrieval.</returns>
    public uint AddQuickLookupFunction()
    {
        // For registering functions, grab the table from the registry since we are probably
        // in a separate stack tied to a managed function call (e.g. a callback registration method).
        luaL_checkstack(LuaState, 1, null);
        lua_getfield(LuaState, LUA_REGISTRYINDEX, QuickLookupKey);
        lua_rotate(LuaState, -2, 1);
        lua_seti(LuaState, -2, (int)nextQuickLookupIndex);
        lua_pop(LuaState, 1);
        return nextQuickLookupIndex++;
    }

    /// <summary>
    ///     Calls the Lua function at the given position of the quick lookup table.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="setupFunction">
    ///     Function to call to push any arguments onto the stack.
    ///     Return value indicates the number of arguments.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if index is out of range.</exception>
    /// <exception cref="LuaException">Thrown if the quick lookup table is corrupt.</exception>
    public void CallQuickLookupFunction(uint index, Func<int> setupFunction)
    {
        lock (opsLock)
        {
            if (index < 1 || index >= nextQuickLookupIndex) throw new ArgumentOutOfRangeException("index");

            // Push function onto stack.
            luaL_checkstack(LuaState, 1, null);
            var ftype = lua_geti(LuaState, quickLookupStackPosition, (int)index);
            if (ftype != LuaType.Function) throw new LuaException("Quick lookup table has non-function value.");

            // Call the setup function to push any arguments onto the stack.
            var nargs = setupFunction();

            // Invoke function in a protected context.
            Validate(lua_pcall(LuaState, nargs, 0, tracebackStackPosition));
        }
    }

    /// <summary>
    ///     Calls a named function in the script.
    /// </summary>
    /// <param name="functionName">Function name.</param>
    /// <param name="configureArgs">
    ///     Function that sets up arguments using the given ArgumentBuilder and returns the number of
    ///     arguments.
    /// </param>
    public void CallNamedFunction(string functionName, Func<ArgumentsBuilder, int> configureArgs)
    {
        lock (opsLock)
        {
            // Locate the function.
            luaL_checkstack(LuaState, 2, null);
            lua_getglobal(LuaState, functionName);
            if (lua_type(LuaState, -1) != LuaType.Function)
            {
                lua_pop(LuaState, 1);
                throw new LuaException($"Function '{functionName}' not found.");
            }

            // Configure arguments.
            var nargs = configureArgs.Invoke(new ArgumentsBuilder(this));

            // Call function.
            Validate(lua_pcall(LuaState, nargs, 0, tracebackStackPosition));
        }
    }

    /// <summary>
    ///     Loads and executes the given string of Lua code.
    /// </summary>
    /// <param name="luaCode">Code to execute.</param>
    /// <exception cref="LuaException">Thrown if the Lua library reports an error.</exception>
    public void LoadAndExecuteString(string luaCode)
    {
        lock (opsLock)
        {
            luaL_checkstack(LuaState, 1, null);

            Validate(luaL_loadstring(LuaState, luaCode));
            Validate(lua_pcall(LuaState, 0, 0, tracebackStackPosition));
        }
    }

    /// <summary>
    ///     Loads a script file (but does not execute it).
    /// </summary>
    /// <param name="filename">Path to script file.</param>
    public void LoadScript(string filename)
    {
        lock (opsLock)
        {
            Validate(luaL_loadfile(LuaState, filename));
        }
    }

    /// <summary>
    ///     Executes an already loaded script.
    /// </summary>
    public void ExecuteLoadedScript()
    {
        lock (opsLock)
        {
            // Scan the global table for functions present before the script is executed.
            var baselineFunctions = new HashSet<string>();
            ScanFunctions(baselineFunctions);

            Validate(lua_pcall(LuaState, 0, LUA_MULTRET, tracebackStackPosition));

            // Re-scan to find new functions added by the script.
            var newFunctions = new HashSet<string>();
            ScanFunctions(newFunctions);
            functionNames.Clear();
            functionNames.AddRange(newFunctions.Except(baselineFunctions));
            functionNames.Sort();
        }
    }

    /// <summary>
    ///     Executes some other scripting action in the context of this host.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    public void ExecuteOther(Action<ExecutionControls> action)
    {
        lock (opsLock)
        {
            action.Invoke(new ExecutionControls(this));
        }
    }

    /// <summary>
    ///     Adds an entity key-value parameter hint for the given function.
    /// </summary>
    /// <param name="functionName">Function name.</param>
    /// <param name="parameterName">Parameter name.</param>
    public void AddEntityParameterHint(string functionName, string parameterName)
    {
        lock (opsLock)
        {
            if (!EntityParameterHintsByFunction.TryGetValue(functionName, out var parameters))
            {
                parameters = new List<string>();
                EntityParameterHintsByFunction[functionName] = parameters;
            }

            if (!parameters.Contains(parameterName))
                parameters.Add(parameterName);
        }
    }

    /// <summary>
    ///     Installs the per-script util library into the host.
    /// </summary>
    private void InstallUtilLibrary()
    {
        try
        {
            BeginLibrary("util");
            AddLibraryFunction("LogTrace", UtilLogTrace);
            AddLibraryFunction("LogDebug", UtilLogDebug);
            AddLibraryFunction("LogInfo", UtilLogInformation);
            AddLibraryFunction("LogWarn", UtilLogWarn);
            AddLibraryFunction("LogError", UtilLogError);
            AddLibraryFunction("LogCrit", UtilLogCritical);
            AddLibraryFunction("ToBool", UtilStringToBool);
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

            Logger.LogTrace(msg);
        }
        catch (Exception e)
        {
            // Log error but continue the script as-is.
            Logger.LogError(e, "Error in util.log_trace.");
        }

        return 0;
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

            Logger.LogDebug(msg);
        }
        catch (Exception e)
        {
            // Log error but continue the script as-is.
            Logger.LogError(e, "Error in util.log_debug.");
        }

        return 0;
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

            Logger.LogInformation(msg);
        }
        catch (Exception e)
        {
            // Log error but continue the script as-is.
            Logger.LogError(e, "Error in util.log_info.");
        }

        return 0;
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

            Logger.LogWarning(msg);
        }
        catch (Exception e)
        {
            // Log error but continue the script as-is.
            Logger.LogError(e, "Error in util.log_warn.");
        }

        return 0;
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

            Logger.LogError(msg);
        }
        catch (Exception e)
        {
            // Log error but continue the script as-is.
            Logger.LogError(e, "Error in util.log_error.");
        }

        return 0;
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

            Logger.LogCritical(msg);
        }
        catch (Exception e)
        {
            // Log error but continue the script as-is.
            Logger.LogError(e, "Error in util.log_crit.");
        }

        return 0;
    }

    /// <summary>
    ///     util.to_bool Lua function. Converts string to bool.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Number of return values.</returns>
    private int UtilStringToBool(IntPtr luaState)
    {
        try
        {
            if (lua_gettop(luaState) != 1) throw new LuaException("util.to_bool requires one argument.");
            if (!lua_isstring(luaState, -1)) throw new LuaException("util.to_bool expects a string argument.");

            luaL_checkstack(luaState, 1, null);
            var input = lua_tostring(luaState, -1);
            if (bool.TryParse(input, out var result))
                lua_pushboolean(luaState, result);
            else
                throw new LuaException($"util.to_bool: cannot convert string '{input}' to bool.");

            return 1;
        }
        catch (LuaException e)
        {
            Logger.LogError("{Error}", e.Message);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error in util.to_bool.");
        }

        return 0;
    }

    /// <summary>
    ///     Convenience method that validates a Lua call, throwing an exception on failure.
    /// </summary>
    /// <param name="result">Result of Lua call.</param>
    /// <exception cref="LuaException">Thrown if the Lua call failed for any reason.</exception>
    private void Validate(LuaResult result)
    {
        if (result == LuaResult.Ok) return;

        var err = lua_tostring(LuaState, -1);
        throw new LuaException(err);
    }

    /// <summary>
    ///     Scans the global table for functions.
    /// </summary>
    private void ScanFunctions(HashSet<string> nameSet)
    {
        nameSet.Clear();

        luaL_checkstack(LuaState, 2, null);
        lua_rawgeti(LuaState, LUA_REGISTRYINDEX, LUA_RIDX_GLOBALS); // push global table
        lua_pushnil(LuaState); // init key for loop
        while (lua_next(LuaState, -2) != 0)
        {
            if (lua_isfunction(LuaState, -1))
                nameSet.Add(lua_tostring(LuaState, -2));
            lua_pop(LuaState, 1); // pop value, keep key for next iteration
        }

        lua_pop(LuaState, 1); // pop global table

        functionNames.Sort();
    }

    /// <summary>
    ///     Builder for adding arguments to a Lua function call.
    /// </summary>
    public readonly struct ArgumentsBuilder
    {
        private readonly LuaHost host;

        public ArgumentsBuilder(LuaHost host)
        {
            this.host = host;
        }

        /// <summary>
        ///     Pushes an integer orgument onto the function call.
        /// </summary>
        /// <param name="argument"></param>
        public void AddInteger(long argument)
        {
            luaL_checkstack(host.LuaState, 1, null);
            lua_pushinteger(host.LuaState, argument);
        }
    }

    /// <summary>
    ///     Controls for arbitrary Lua execution.
    /// </summary>
    public readonly struct ExecutionControls
    {
        private readonly LuaHost host;

        public ExecutionControls(LuaHost host)
        {
            this.host = host;
        }

        /// <summary>
        ///     Validating wrapper around lua_pcall.
        /// </summary>
        /// <param name="nargs">Argument count.</param>
        /// <param name="nresults">Result count.</param>
        public void PCall(int nargs, int nresults)
        {
            host.Validate(lua_pcall(host.LuaState, nargs, nresults, host.tracebackStackPosition));
        }
    }
}