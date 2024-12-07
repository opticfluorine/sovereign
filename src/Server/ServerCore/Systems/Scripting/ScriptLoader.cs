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
using System.IO;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly ILoggerFactory loggerFactory;
    private readonly IEnumerable<ILuaLibrary> luaLibraries;
    private readonly IServiceScopeFactory scopeFactory;

    public ScriptLoader(ILogger<ScriptLoader> logger, IServerConfigurationManager configManager,
        IEnumerable<ILuaLibrary> luaLibraries, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    {
        this.logger = logger;
        this.configManager = configManager;
        this.luaLibraries = luaLibraries;
        this.scopeFactory = scopeFactory;
        this.loggerFactory = loggerFactory;
    }

    /// <summary>
    ///     Loads and hosts all scripts.
    /// </summary>
    public List<LuaHost> LoadAll()
    {
        var config = configManager.ServerConfiguration.Scripting;

        var files = Directory.EnumerateFiles(config.ScriptDirectory, "*.lua",
            new EnumerationOptions
            {
                MatchCasing = MatchCasing.CaseInsensitive,
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
                MaxRecursionDepth = (int)config.MaxDirectoryDepth
            });

        var hosts = new List<LuaHost>();
        foreach (var file in files)
            try
            {
                hosts.Add(HostScript(file));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to load script {File}.", file);
            }

        return hosts;
    }

    /// <summary>
    ///     Loads and hosts the given Lua script.
    /// </summary>
    /// <param name="file">Path to script file.</param>
    /// <returns>Hosted script.</returns>
    private LuaHost HostScript(string file)
    {
        logger.LogInformation("Loading script {File}.", file);

        var host = GetNewHost(file);
        host.LoadScript(file);

        return host;
    }

    /// <summary>
    ///     Creates a new Lua host and configures it for integration with the engine.
    /// </summary>
    /// <returns>Configured Lua host.</returns>
    private LuaHost GetNewHost(string filename)
    {
        var scriptName = Path.GetFileNameWithoutExtension(filename);
        var loggerCat = $"Script ({scriptName})";
        var hostLogger = loggerFactory.CreateLogger(loggerCat);

        var host = new LuaHost(hostLogger);
        foreach (var library in luaLibraries) library.Install(host);

        return host;
    }
}