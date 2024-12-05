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

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sovereign.Scripting.Lua;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Responsible for loading scripts.
/// </summary>
public class ScriptLoader
{
    private readonly IServerConfigurationManager configManager;
    private readonly ILogger<ScriptLoader> logger;
    private readonly IEnumerable<ILuaLibrary> luaLibraries;

    public ScriptLoader(ILogger<ScriptLoader> logger, IServerConfigurationManager configManager,
        IEnumerable<ILuaLibrary> luaLibraries)
    {
        this.logger = logger;
        this.configManager = configManager;
        this.luaLibraries = luaLibraries;
    }

    /// <summary>
    ///     Loads or reloads all scripts.
    /// </summary>
    public void LoadAll()
    {
    }
}