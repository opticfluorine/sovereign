// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

public static class LuaUtil
{
    /// <summary>
    ///     Gets the LuaState of the main thread for the given state.
    /// </summary>
    /// <param name="luaState">Any LuaState belonging to the same instance as the main thread.</param>
    /// <returns>LuaState for the main thread of the same instance.</returns>
    public static IntPtr GetMainThread(IntPtr luaState)
    {
        luaL_checkstack(luaState, 1, null);
        lua_geti(luaState, LUA_REGISTRYINDEX, LUA_RIDX_MAINTHREAD);
        var mainThread = lua_topointer(luaState, -1);
        lua_pop(luaState, 1);
        return mainThread;
    }
}