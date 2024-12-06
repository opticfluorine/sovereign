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
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sovereign.Scripting.Lua;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Manages the active set of scripts.
/// </summary>
public class ScriptManager : IDisposable
{
    private readonly ScriptLoader loader;
    private readonly ILogger<ScriptManager> logger;
    private List<LuaHost> scriptHosts = new();

    public ScriptManager(ScriptLoader loader, ILogger<ScriptManager> logger)
    {
        this.loader = loader;
        this.logger = logger;
    }

    public void Dispose()
    {
        foreach (var host in scriptHosts) host.Dispose();
    }

    /// <summary>
    ///     Loads or reloads all scripts.
    /// </summary>
    public void Reload()
    {
        logger.LogInformation("Loading scripts...");

        foreach (var host in scriptHosts) host.Dispose();

        scriptHosts = loader.LoadAll();

        logger.LogInformation("Finished loading scripts.");
    }
}