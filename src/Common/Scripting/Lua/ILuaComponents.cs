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

namespace Sovereign.Scripting.Lua;

/// <summary>
///     Interface for Lua component collection bindings.
/// </summary>
public interface ILuaComponents
{
    /// <summary>
    ///     Installs the component collection into a Lua host.
    /// </summary>
    /// <param name="luaHost"></param>
    /// <remarks>
    ///     The 'components' table must be at the top of the Lua stack when this is called.
    ///     The Lua stack must be unchanged when this method returns to the caller (however,
    ///     it may be mutated within the method call as long as it is restored afterwards).
    /// </remarks>
    void Install(LuaHost luaHost);
}