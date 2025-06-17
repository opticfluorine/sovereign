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
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Lua;
using Sovereign.Scripting.Lua;
using Sovereign.ServerCore.Configuration;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Responsible for loading scripts.
/// </summary>
public class ScriptLoader
{
    private readonly ScriptingOptions config;
    private readonly ILogger<ScriptLoader> logger;
    private readonly ILoggerFactory loggerFactory;
    private readonly IEnumerable<ILuaComponents> luaComponents;
    private readonly IEnumerable<ILuaLibrary> luaLibraries;

    public ScriptLoader(ILogger<ScriptLoader> logger, IOptions<ScriptingOptions> scriptingOptions,
        IEnumerable<ILuaLibrary> luaLibraries, ILoggerFactory loggerFactory, IEnumerable<ILuaComponents> luaComponents)
    {
        this.logger = logger;
        this.luaLibraries = luaLibraries;
        this.loggerFactory = loggerFactory;
        this.luaComponents = luaComponents;
        config = scriptingOptions.Value;
    }

    /// <summary>
    ///     Loads and hosts all scripts.
    /// </summary>
    public List<LuaHost> LoadAll()
    {
        return LoadScriptList(GetAllScriptFiles());
    }

    /// <summary>
    ///     Loads scripts where the script name satisfies a given predicate.
    /// </summary>
    /// <param name="namePredicate">Predicate acting on script names. Names for which this returns true are loaded.</param>
    /// <returns>Loaded scripts whose names satisfied the given predicate.</returns>
    public List<LuaHost> LoadWhere(Func<string, bool> namePredicate)
    {
        return LoadScriptList(GetAllScriptFiles().Where(f => namePredicate(GetNameFromPath(f))));
    }

    /// <summary>
    ///     Loads the script with the given name.
    /// </summary>
    /// <param name="scriptName">Script name.</param>
    /// <returns></returns>
    public LuaHost Load(string scriptName)
    {
        var scriptPath = Path.Combine(config.ScriptDirectory, $"{scriptName}.lua");
        return HostScript(scriptPath);
    }

    /// <summary>
    ///     Loads the given scripts into a list.
    /// </summary>
    /// <param name="files">Script files to attempt loading.</param>
    /// <returns>List of successfully loaded scripts.</returns>
    private List<LuaHost> LoadScriptList(IEnumerable<string> files)
    {
        var hosts = new List<LuaHost>();
        foreach (var file in files)
            try
            {
                hosts.Add(HostScript(file));
            }
            catch (LuaException e)
            {
                logger.LogError("{Error}", e.Message);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to load script {File}.", file);
            }

        return hosts;
    }

    /// <summary>
    ///     Identifies all script files.
    /// </summary>
    /// <returns>Enumerated script files.</returns>
    private IEnumerable<string> GetAllScriptFiles()
    {
        return Directory.EnumerateFiles(config.ScriptDirectory, "*.lua",
            new EnumerationOptions
            {
                MatchCasing = MatchCasing.CaseInsensitive,
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
                MaxRecursionDepth = (int)config.MaxDirectoryDepth
            });
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
        host.Name = GetNameFromPath(file);
        host.LoadScript(file);

        return host;
    }

    /// <summary>
    ///     Gets the script name for the given script filename.
    /// </summary>
    /// <param name="file">Script filename.</param>
    /// <returns>Script name.</returns>
    private string GetNameFromPath(string file)
    {
        var relPath = Path.GetRelativePath(config.ScriptDirectory, file);
        return relPath.Substring(0, relPath.LastIndexOf('.'));
    }

    /// <summary>
    ///     Creates a new Lua host and configures it for integration with the engine.
    /// </summary>
    /// <returns>Configured Lua host.</returns>
    private LuaHost GetNewHost(string filename)
    {
        var scriptName = Path.GetFileNameWithoutExtension(filename);
        var loggerCat = $".Script ({scriptName})"; // first dot is to help with Serilog context expressions
        var hostLogger = loggerFactory.CreateLogger(loggerCat);

        var host = new LuaHost(hostLogger);
        InstallCommonLibraries(host);
        InstallEventTable(host);
        InstallComponents(host);
        foreach (var library in luaLibraries)
        {
            var startDepth = lua_gettop(host.LuaState);
            library.Install(host);
            var endDepth = lua_gettop(host.LuaState);

            if (startDepth != endDepth)
            {
                hostLogger.LogError("Lua library {Library} did not leave the Lua stack unchanged. " +
                                    "Expected {StartDepth} but got {EndDepth}.",
                    library.GetType().Name, startDepth, endDepth);
            }
        }

        return host;
    }

    /// <summary>
    ///     Installs the event ID table into the given host.
    /// </summary>
    /// <param name="host">Lua host.</param>
    private void InstallEventTable(LuaHost host)
    {
        luaL_checkstack(host.LuaState, 2, null);

        lua_createtable(host.LuaState, 0, 0);
        foreach (var eventId in ScriptableEventSet.Events)
        {
            lua_pushinteger(host.LuaState, (long)eventId);
            lua_setfield(host.LuaState, -2, eventId.ToString());
        }

        lua_setglobal(host.LuaState, "events");
    }

    /// <summary>
    ///     Installs component collection bindings into the host.
    /// </summary>
    /// <param name="host">Lua host.</param>
    private void InstallComponents(LuaHost host)
    {
        luaL_checkstack(host.LuaState, 2, null);

        lua_createtable(host.LuaState, 0, 0);
        foreach (var component in luaComponents)
        {
            component.Install(host);
        }

        lua_setglobal(host.LuaState, "components");
    }

    /// <summary>
    ///     Installs common libraries from ScriptingCommonLibraries.
    /// </summary>
    /// <param name="host">Lua host.</param>
    private void InstallCommonLibraries(LuaHost host)
    {
        host.LoadAndExecuteString(ScriptingCommonLibraries.Color);
    }
}