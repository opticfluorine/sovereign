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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.Scripting.Lua;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Manages the active set of scripts.
/// </summary>
public class ScriptManager : IDisposable
{
    private readonly Dictionary<string, LuaHost> hostsByName = new();
    private readonly Dictionary<IntPtr, LuaHost> hostsByState = new();
    private readonly ILogger<ScriptManager> logger;
    private readonly List<LuaHost> scriptHosts = new();

    public ScriptManager(ILogger<ScriptManager> logger)
    {
        this.logger = logger;
    }

    public void Dispose()
    {
        foreach (var host in scriptHosts) host.Dispose();
    }

    /// <summary>
    ///     Unloads all currently loaded scripts.
    /// </summary>
    public void UnloadAll()
    {
        foreach (var host in scriptHosts) host.Dispose();
        hostsByState.Clear();
        hostsByName.Clear();
        scriptHosts.Clear();
    }

    /// <summary>
    ///     Unloads a currently loaded script.
    /// </summary>
    /// <param name="host">Lua host associated with the script to be unloaded.</param>
    public void Unload(LuaHost host)
    {
        hostsByState.Remove(host.LuaState);
        hostsByName.Remove(host.Name);
        for (var i = scriptHosts.Count - 1; i >= 0; --i)
        {
            if (scriptHosts[i].LuaState != host.LuaState) continue;
            scriptHosts.RemoveAt(i);
            break;
        }

        host.Dispose();
    }

    /// <summary>
    ///     Loads a list of hosted scripts.
    /// </summary>
    public void Load(List<LuaHost> scripts)
    {
        foreach (var host in scripts) Load(host);
    }

    /// <summary>
    ///     Loads the given hosted script.
    /// </summary>
    /// <param name="host">Hosted script.</param>
    public void Load(LuaHost host)
    {
        scriptHosts.Add(host);
        hostsByState[host.LuaState] = host;
        hostsByName[host.Name] = host;

        // Now that host registration is complete, execute the pending script asynchronously.
        Task.Run(() =>
        {
            try
            {
                host.ExecuteLoadedScript();
            }
            catch (LuaException e)
            {
                logger.LogError("{Error}", e.Message);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while executing loaded script.");
            }
        });
    }

    /// <summary>
    ///     Gets the host associated with the given Lua state handle.
    /// </summary>
    /// <param name="luaState">Lua state handle.</param>
    /// <param name="host">Associated host.</param>
    /// <returns>true if a matching host was found, false otherwise.</returns>
    public bool TryGetHost(IntPtr luaState, [NotNullWhen(true)] out LuaHost? host)
    {
        return hostsByState.TryGetValue(luaState, out host);
    }

    /// <summary>
    ///     Gets the host associated with the given script by name.
    /// </summary>
    /// <param name="name">Script name.</param>
    /// <param name="host">Script host.</param>
    /// <returns>true if a matching host was found, false otherwise.</returns>
    public bool TryGetHost(string name, [NotNullWhen(true)] out LuaHost? host)
    {
        return hostsByName.TryGetValue(name, out host);
    }

    /// <summary>
    ///     Gets the names of all currently loaded scripts.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetScriptNames()
    {
        return hostsByName.Keys;
    }
}