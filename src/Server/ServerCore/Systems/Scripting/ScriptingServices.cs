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

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Sovereign.Scripting.Lua;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Public services exposed by the Scripting system.
/// </summary>
public class ScriptingServices
{
    private readonly ScriptManager scriptManager;

    public ScriptingServices(ScriptManager scriptManager)
    {
        this.scriptManager = scriptManager;
    }

    /// <summary>
    ///     Gets the Lua host associated with the given Lua state if known.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="host">Associated Lua host, or null if unknown.</param>
    /// <returns>Returns true if the Lua host was found, false otherwise.</returns>
    public bool TryGetHostForState(IntPtr luaState, [NotNullWhen(true)] out LuaHost? host)
    {
        return scriptManager.TryGetHost(luaState, out host);
    }

    /// <summary>
    ///     Utility method that selects the appropriate logger for the given Lua state.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <param name="fallback">Fallback logger to use if the script logger cannot be found.</param>
    /// <returns>Logger.</returns>
    public ILogger GetScriptLogger(IntPtr luaState, ILogger fallback)
    {
        return TryGetHostForState(luaState, out var host) ? host.Logger : fallback;
    }

    /// <summary>
    ///     Checks whether the given script is currently loaded.
    /// </summary>
    /// <param name="name">Script name.</param>
    /// <returns>true if loaded, false otherwise.</returns>
    public bool IsScriptLoaded(string name)
    {
        return scriptManager.TryGetHost(name, out _);
    }
}