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

using System.Reflection;
using System.Runtime.InteropServices;

namespace Sovereign.Scripting.Lua;

/// <summary>
///     P/Invoke bindings and macro reimplementations for LuaJIT 2.1.
/// </summary>
/// <remarks>
///     These are partial bindings created from the LuaJIT 2.1 lua.h and lauxlib.h headers.
///     LuaJIT exposes the Lua 5.1 API with LuaJIT extensions; for documentation of these
///     functions, refer to the Lua 5.1 Reference Manual and the LuaJIT extensions page.
///     Several Lua 5.4-only functions do not exist in LuaJIT and are reimplemented here
///     in managed code on top of the Lua 5.1 API.
/// </remarks>
public static partial class LuaBindings
{
    public delegate int LuaCFunction(IntPtr luaState);

    public enum LuaGcWhat
    {
        Stop = 0,
        Restart = 1,
        Collect = 2,
        Count = 3,
        CountB = 4,
        Step = 5,
        SetPause = 6,
        SetStepMul = 7,
        IsRunning = 9
    }

    public enum LuaResult
    {
        Ok = 0,
        Yield = 1,
        ErrRun = 2,
        ErrSyntax = 3,
        ErrMem = 4,
        ErrErr = 5
    }

    public enum LuaType
    {
        None = -1,
        Nil = 0,
        Boolean = 1,
        LightUserData = 2,
        Number = 3,
        String = 4,
        Table = 5,
        Function = 6,
        UserData = 7,
        Thread = 8
    }

    public const int LUA_MULTRET = -1;

    public const int LUA_REGISTRYINDEX = -10000;
    public const int LUA_GLOBALSINDEX = -10002;

    /// <summary>
    ///     Lua registry key under which the pointer of the main thread state is stored.
    /// </summary>
    public const string MainThreadRegistryKey = "sovereign_mainthread";

    private const string LibName = "luajit-5.1";

    static LuaBindings()
    {
        NativeLibrary.SetDllImportResolver(typeof(LuaBindings).Assembly, ResolveLuaLibrary);
    }

    [LibraryImport(LibName)]
    public static partial IntPtr luaL_newstate();

    [LibraryImport(LibName)]
    public static partial void lua_close(IntPtr luaState);

    //
    // Basic Stack Manipulation
    //

    public static int lua_absindex(IntPtr luaState, int idx)
    {
        return idx > 0 || idx <= LUA_REGISTRYINDEX ? idx : lua_gettop(luaState) + idx + 1;
    }

    [LibraryImport(LibName)]
    public static partial int lua_gettop(IntPtr luaState);

    [LibraryImport(LibName)]
    public static partial void lua_settop(IntPtr luaState, int idx);

    public static void lua_pop(IntPtr luaState, int n)
    {
        lua_settop(luaState, -n - 1);
    }

    [LibraryImport(LibName)]
    public static partial void lua_pushvalue(IntPtr luaState, int idx);

    /// <summary>
    ///     Rotates the elements between the given index and the top of the stack by n positions.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="idx">Stack index of the first element to rotate.</param>
    /// <param name="n">Number of positions to rotate; may be negative.</param>
    public static void lua_rotate(IntPtr luaState, int idx, int n)
    {
        var absIdx = lua_absindex(luaState, idx);
        var count = lua_gettop(luaState) - absIdx + 1;
        if (count <= 1) return;

        // Normalize the rotation amount into [0, count).
        n %= count;
        if (n < 0) n += count;
        if (n == 0) return;

        // Save copies of the top n elements; they will move to the bottom.
        var oldTop = lua_gettop(luaState);
        for (var i = 0; i < n; ++i)
            lua_pushvalue(luaState, absIdx + count - n + i);

        // Shift the remaining elements upward by n positions. Iterating downward ensures
        // that each destination is always above the not-yet-copied sources.
        for (var i = count - n - 1; i >= 0; --i)
            lua_copy(luaState, absIdx + i, absIdx + n + i);

        // Move the saved copies into the bottom n slots.
        for (var i = 0; i < n; ++i)
            lua_copy(luaState, oldTop + 1 + i, absIdx + i);

        lua_pop(luaState, n);
    }

    [LibraryImport(LibName)]
    public static partial void lua_copy(IntPtr luaState, int fromidx, int toidx);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_checkstack(IntPtr luaState, int n);

    //
    // Access Functions
    //

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_isnumber(IntPtr luaState, int idx);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_isstring(IntPtr luaState, int idx);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_iscfunction(IntPtr luaState, int idx);

    /// <summary>
    ///     Determines if the value at the given stack index is an integer-valued number.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="idx">Stack index.</param>
    /// <returns>true if the value is an integral number, false otherwise.</returns>
    public static bool lua_isinteger(IntPtr luaState, int idx)
    {
        if (lua_type(luaState, idx) != LuaType.Number) return false;
        var value = lua_tonumber(luaState, idx);
        return !double.IsNaN(value) && !double.IsInfinity(value) && value == Math.Truncate(value);
    }

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_isuserdata(IntPtr luaState, int idx);

    public static bool lua_isboolean(IntPtr luaState, int idx)
    {
        return lua_type(luaState, idx) == LuaType.Boolean;
    }

    public static bool lua_istable(IntPtr luaState, int idx)
    {
        return lua_type(luaState, idx) == LuaType.Table;
    }

    public static bool lua_isfunction(IntPtr luaState, int idx)
    {
        return lua_type(luaState, idx) == LuaType.Function;
    }

    /// <summary>
    ///     Determines if the value at the given stack index is a light userdata.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="idx">Stack index.</param>
    /// <returns>true if the value is a light userdata, false otherwise.</returns>
    public static bool lua_islightuserdata(IntPtr luaState, int idx)
    {
        return lua_type(luaState, idx) == LuaType.LightUserData;
    }

    [LibraryImport(LibName)]
    public static partial LuaType lua_type(IntPtr luaState, int idx);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial string lua_typename(IntPtr luaState, LuaType tp);

    [LibraryImport(LibName)]
    public static partial double lua_tonumberx(IntPtr luaState, int idx, IntPtr isnum);

    public static double lua_tonumber(IntPtr luaState, int idx)
    {
        return lua_tonumberx(luaState, idx, IntPtr.Zero);
    }

    [LibraryImport(LibName)]
    public static partial long lua_tointegerx(IntPtr luaState, int idx, IntPtr isnum);

    public static long lua_tointeger(IntPtr luaState, int idx)
    {
        return lua_tointegerx(luaState, idx, IntPtr.Zero);
    }

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_toboolean(IntPtr luaState, int idx);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr lua_tolstring(IntPtr luaState, int idx, IntPtr len);

    public static string lua_tostring(IntPtr luaState, int idx)
    {
        return Marshal.PtrToStringUTF8(lua_tolstring(luaState, idx, IntPtr.Zero)) ?? "";
    }

    public static uint lua_rawlen(IntPtr luaState, int idx)
    {
        return (uint)LuaObjLenNative(luaState, idx);
    }

    [LibraryImport(LibName)]
    public static partial IntPtr lua_touserdata(IntPtr luaState, int idx);

    [LibraryImport(LibName)]
    public static partial IntPtr lua_tothread(IntPtr luaState, int idx);

    [LibraryImport(LibName)]
    public static partial IntPtr lua_topointer(IntPtr luaState, int idx);

    //
    // Comparison Functions
    //

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_rawequal(IntPtr luaState, int idx1, int idx2);

    //
    // Push Functions (C# -> stack)
    //

    [LibraryImport(LibName)]
    public static partial void lua_pushnil(IntPtr luaState);

    [LibraryImport(LibName)]
    public static partial void lua_pushnumber(IntPtr luaState, double n);

    [LibraryImport(LibName)]
    public static partial void lua_pushinteger(IntPtr luaState, long n);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial void lua_pushlstring(IntPtr luaState, string s, long len);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial void lua_pushstring(IntPtr luaState, string s);

    [LibraryImport(LibName)]
    public static partial void lua_pushcclosure(IntPtr luaState, LuaCFunction fn, int n);

    public static void lua_pushcfunction(IntPtr luaState, LuaCFunction fn)
    {
        lua_pushcclosure(luaState, fn, 0);
    }

    [LibraryImport(LibName)]
    public static partial void lua_pushboolean(IntPtr luaState, [MarshalAs(UnmanagedType.Bool)] bool b);

    [LibraryImport(LibName)]
    public static partial void lua_pushlightuserdata(IntPtr luaState, IntPtr p);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_pushthread(IntPtr luaState);

    //
    // Get Functions (Lua -> stack)
    //

    public static LuaType lua_getglobal(IntPtr luaState, string name)
    {
        return lua_getfield(luaState, LUA_GLOBALSINDEX, name);
    }

    public static LuaType lua_gettable(IntPtr luaState, int idx)
    {
        LuaGetTableNative(luaState, idx);
        return lua_type(luaState, -1);
    }

    public static LuaType lua_getfield(IntPtr luaState, int idx, string k)
    {
        LuaGetFieldNative(luaState, idx, k);
        return lua_type(luaState, -1);
    }

    public static LuaType lua_geti(IntPtr luaState, int idx, int i)
    {
        var absIdx = lua_absindex(luaState, idx);
        lua_pushinteger(luaState, i);
        return lua_gettable(luaState, absIdx);
    }

    public static LuaType lua_rawget(IntPtr luaState, int idx)
    {
        LuaRawGetNative(luaState, idx);
        return lua_type(luaState, -1);
    }

    public static LuaType lua_rawgeti(IntPtr luaState, int idx, int i)
    {
        LuaRawGetiNative(luaState, idx, i);
        return lua_type(luaState, -1);
    }

    [LibraryImport(LibName)]
    public static partial void lua_createtable(IntPtr luaState, int narr, int nrec);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_getmetatable(IntPtr luaState, int objindex);

    //
    // Set Functions (Stack -> Lua)
    //

    public static void lua_setglobal(IntPtr luaState, string name)
    {
        lua_setfield(luaState, LUA_GLOBALSINDEX, name);
    }

    [LibraryImport(LibName)]
    public static partial void lua_settable(IntPtr luaState, int idx);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial void lua_setfield(IntPtr luaState, int idx, string k);

    /// <summary>
    ///     Sets t[n] = v where v is the top of the stack, popping the value from the stack.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="idx">Stack index of the table.</param>
    /// <param name="n">Integer key.</param>
    public static void lua_seti(IntPtr luaState, int idx, int n)
    {
        var absIdx = lua_absindex(luaState, idx);
        lua_pushinteger(luaState, n);
        lua_pushvalue(luaState, -2);
        lua_settable(luaState, absIdx);
        lua_pop(luaState, 1);
    }

    [LibraryImport(LibName)]
    public static partial void lua_rawset(IntPtr luaState, int idx);

    [LibraryImport(LibName)]
    public static partial void lua_rawseti(IntPtr luaState, int idx, int n);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_setmetatable(IntPtr luaState, int objindex);

    //
    // Load and Call Functions
    //

    [LibraryImport(LibName)]
    public static partial void lua_call(IntPtr luaState, int nargs, int nresults);

    [LibraryImport(LibName)]
    public static partial LuaResult lua_pcall(IntPtr luaState, int nargs, int nresults, int errfunc);

    [LibraryImport(LibName)]
    public static partial int lua_next(IntPtr luaState, int index);

    public static void lua_len(IntPtr luaState, int index)
    {
        lua_pushinteger(luaState, (long)LuaObjLenNative(luaState, index));
    }

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial LuaResult luaL_loadfilex(IntPtr luaState, string filename, string? mode);

    //
    // Garbage Collection
    //

    [LibraryImport(LibName)]
    public static partial int lua_gc(IntPtr luaState, LuaGcWhat what, int data);

    //
    // Auxiliary Library Functions
    //

    public static LuaResult luaL_loadfile(IntPtr luaState, string filename)
    {
        return luaL_loadfilex(luaState, filename, null);
    }

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial LuaResult luaL_loadstring(IntPtr luaState, string s);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial void luaL_checkstack(IntPtr luaState, int sz, string? msg);

    [LibraryImport(LibName)]
    public static partial int luaL_ref(IntPtr luaState, int t);

    [LibraryImport(LibName)]
    public static partial void luaL_unref(IntPtr luaState, int t, int r);

    //
    // A note on error functions:
    // lua_error, luaL_argerror, and luaL_typeerror are not supported by these bindings.
    // These functions perform a longjmp which would cross a managed frame if called from
    // within managed code invoked from Lua; this will crash the .NET CLR.
    // 
    // See https://learn.microsoft.com/en-us/dotnet/standard/native-interop/exceptions-interoperability
    // for additional information.
    //
    public static int lua_error(IntPtr luaState)
    {
        throw new NotSupportedException();
    }

    public static void luaL_argerror(IntPtr luaState, int arg, string extramsg)
    {
        throw new NotSupportedException();
    }

    public static void luaL_typeerror(IntPtr luaState, int arg, string tname)
    {
        throw new NotSupportedException();
    }

    [LibraryImport(LibName)]
    public static partial void luaL_openlibs(IntPtr luaState);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial void luaL_traceback(IntPtr luaState, IntPtr traceState, string? message, int level);

    //
    // Native stubs for functions that are reimplemented as managed wrappers above.
    // The public wrappers take their names; these bind to the real entry points.
    //

    [LibraryImport(LibName, EntryPoint = "lua_getfield", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void LuaGetFieldNative(IntPtr luaState, int idx, string k);

    [LibraryImport(LibName, EntryPoint = "lua_gettable")]
    private static partial void LuaGetTableNative(IntPtr luaState, int idx);

    [LibraryImport(LibName, EntryPoint = "lua_rawget")]
    private static partial void LuaRawGetNative(IntPtr luaState, int idx);

    [LibraryImport(LibName, EntryPoint = "lua_rawgeti")]
    private static partial void LuaRawGetiNative(IntPtr luaState, int idx, int i);

    [LibraryImport(LibName, EntryPoint = "lua_objlen")]
    private static partial ulong LuaObjLenNative(IntPtr luaState, int idx);

    /// <summary>
    ///     Attempts to load the LuaJIT native library by its known names, falling back to the
    ///     default probing behavior if none are found.
    /// </summary>
    /// <param name="libraryName">Requested library name.</param>
    /// <returns>Loaded library handle, or Zero to fall back to default probing.</returns>
    private static IntPtr ResolveLuaLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibName) return IntPtr.Zero;

        if (OperatingSystem.IsLinux())
        {
            if (NativeLibrary.TryLoad("libluajit-5.1.so.2", assembly, searchPath, out var handle)) return handle;
            if (NativeLibrary.TryLoad("libluajit-5.1.so", assembly, searchPath, out handle)) return handle;
        }
        else if (OperatingSystem.IsWindows())
        {
            if (NativeLibrary.TryLoad("luajit-5.1.dll", assembly, searchPath, out var handle)) return handle;
            if (NativeLibrary.TryLoad("lua51.dll", assembly, searchPath, out handle)) return handle;
            if (NativeLibrary.TryLoad("luajit.dll", assembly, searchPath, out handle)) return handle;
        }

        return IntPtr.Zero;
    }
}
