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

using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.Scripting.Lua;

/// <summary>
///     Execution host for a Lua script.
/// </summary>
public class LuaHost : IDisposable
{
    private string library = "";

    public LuaHost()
    {
        LuaState = luaL_newstate();
        luaL_openlibs(LuaState);
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
}