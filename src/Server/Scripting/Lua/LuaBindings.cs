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

    [LibraryImport(LibName)]
    public static partial LuaType lua_type(IntPtr luaState, int idx);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial string lua_typename(IntPtr luaState, LuaType tp);

    [LibraryImport(LibName)]
    public static partial float lua_tonumberx(IntPtr luaState, int idx, ref int isnum);

    [LibraryImport(LibName)]
    public static partial int lua_tointegerx(IntPtr luaState, int idx, ref int isnum);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool lua_toboolean(IntPtr luaState, int idx);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial string lua_tolstring(IntPtr luaState, int idx, ref long len);

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
    public static partial void lua_pushnumber(IntPtr luaState, float n);

    [LibraryImport(LibName)]
    public static partial void lua_pushinteger(IntPtr luaState, int n);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr lua_pushlstring(IntPtr luaState, string s, long len);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr lua_pushstring(IntPtr luaState, string s);

    [LibraryImport(LibName)]
    public static partial void lua_pushcclosure(IntPtr luaState, LuaCFunction fn, int n);

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
}