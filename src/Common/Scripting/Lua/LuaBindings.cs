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

using System.Runtime.InteropServices;

namespace Sovereign.Scripting.Lua;

/// <summary>
///     P/Invoke bindings and macro reimplementations for Lua 5.4.
/// </summary>
/// <remarks>
///     These are partial bindings created from the Lua 5.4 lua.h and lauxlib.h headers.
///     For documentation of these functions, refer to the Lua 5.4 Reference Manual.
/// </remarks>
public static partial class LuaBindings
{
    public delegate int LuaCFunction(IntPtr luaState);

    public delegate int LuaKFunction(IntPtr luaState, int status, IntPtr ctx);

    public delegate string LuaReader(IntPtr luaState, IntPtr ud, ref long sz);

    public delegate int LuaWriter(IntPtr luaState, IntPtr p, long sz, IntPtr ud);

    public enum LuaArithOp
    {
        Add = 0,
        Sub = 1,
        Mul = 2,
        Mod = 3,
        Pow = 4,
        Div = 5,
        IDiv = 6,
        BAnd = 7,
        BOr = 8,
        BXor = 9,
        Shl = 10,
        Shr = 11,
        Unm = 12,
        BNot = 13
    }

    public enum LuaCompareOp
    {
        Eq = 0,
        Lt = 1,
        Le = 2
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

    public const int LUA_REGISTRYINDEX = -1001000;

    private const string LibName = "lua5.4";

    [LibraryImport(LibName)]
    public static partial IntPtr luaL_newstate();

    [LibraryImport(LibName)]
    public static partial void lua_close(IntPtr luaState);

    //
    // Basic Stack Manipulation
    //

    [LibraryImport(LibName)]
    public static partial int lua_absindex(IntPtr luaState, int idx);

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

    [LibraryImport(LibName)]
    public static partial void lua_rotate(IntPtr luaState, int idx, int n);

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

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_isinteger(IntPtr luaState, int idx);

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

    [LibraryImport(LibName)]
    public static partial uint lua_rawlen(IntPtr luaState, int idx);

    [LibraryImport(LibName)]
    public static partial IntPtr lua_touserdata(IntPtr luaState, int idx);

    [LibraryImport(LibName)]
    public static partial IntPtr lua_tothread(IntPtr luaState, int idx);

    [LibraryImport(LibName)]
    public static partial IntPtr lua_topointer(IntPtr luaState, int idx);

    //
    // Comparison and Arithmetic Functions
    //

    [LibraryImport(LibName)]
    public static partial void lua_arith(IntPtr luaState, LuaArithOp op);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_rawequal(IntPtr luaState, int idx1, int idx2);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_compare(IntPtr luaState, int idx1, int idx2, LuaCompareOp op);

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
    public static partial IntPtr lua_pushlstring(IntPtr luaState, string s, long len);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr lua_pushstring(IntPtr luaState, string s);

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

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial LuaType lua_getglobal(IntPtr luaState, string name);

    [LibraryImport(LibName)]
    public static partial LuaType lua_gettable(IntPtr luaState, int idx);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial LuaType lua_getfield(IntPtr luaState, int idx, string k);

    [LibraryImport(LibName)]
    public static partial LuaType lua_geti(IntPtr luaState, int idx, int i);

    [LibraryImport(LibName)]
    public static partial LuaType lua_rawget(IntPtr luaState, int idx);

    [LibraryImport(LibName)]
    public static partial LuaType lua_rawgeti(IntPtr luaState, int idx, int i);

    [LibraryImport(LibName)]
    public static partial LuaType lua_rawgetp(IntPtr luaState, int idx, IntPtr p);

    [LibraryImport(LibName)]
    public static partial void lua_createtable(IntPtr luaState, int narr, int nrec);

    [LibraryImport(LibName)]
    public static partial IntPtr lua_newuserdatauv(IntPtr luaState, long sz, int nuvalue);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_getmetatable(IntPtr luaState, int objindex);

    [LibraryImport(LibName)]
    public static partial LuaType lua_getiuservalue(IntPtr luaState, int index, int n);

    //
    // Set Functions (Stack -> Lua)
    //

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial void lua_setglobal(IntPtr luaState, string name);

    [LibraryImport(LibName)]
    public static partial void lua_settable(IntPtr luaState, int idx);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial void lua_setfield(IntPtr luaState, int idx, string k);

    [LibraryImport(LibName)]
    public static partial void lua_seti(IntPtr luaState, int idx, int n);

    [LibraryImport(LibName)]
    public static partial void lua_rawset(IntPtr luaState, int idx);

    [LibraryImport(LibName)]
    public static partial void lua_rawseti(IntPtr luaState, int idx, int n);

    [LibraryImport(LibName)]
    public static partial void lua_rawsetp(IntPtr luaState, int idex, IntPtr p);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_setmetatable(IntPtr luaState, int objindex);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_setiuservalue(IntPtr luaState, int idx, int n);

    //
    // Load and Call Functions
    //

    [LibraryImport(LibName)]
    public static partial void lua_callk(IntPtr luaState, int nargs, int nresults, IntPtr ctx, LuaKFunction? k);

    public static void lua_call(IntPtr luaState, int nargs, int nresults)
    {
        lua_callk(luaState, nargs, nresults, IntPtr.Zero, null);
    }

    [LibraryImport(LibName)]
    public static partial LuaResult lua_pcallk(IntPtr luaState, int nargs, int nresults, int errfunc, IntPtr ctx,
        LuaKFunction? k);

    public static LuaResult lua_pcall(IntPtr luaState, int nargs, int nresults, int errfunc)
    {
        return lua_pcallk(luaState, nargs, nresults, errfunc, IntPtr.Zero, null);
    }

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial LuaResult lua_load(IntPtr luaState, LuaReader reader, IntPtr dt, string chunkname,
        string mode);

    [LibraryImport(LibName)]
    public static partial LuaResult lua_dump(IntPtr luaState, LuaWriter writer, IntPtr data,
        [MarshalAs(UnmanagedType.Bool)] bool strip);

    [LibraryImport(LibName)]
    public static partial int lua_next(IntPtr luaState, int index);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial LuaResult luaL_loadfilex(IntPtr luaState, string filename, string? mode);

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
}