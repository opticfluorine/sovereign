// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using Microsoft.Extensions.Logging;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace Sovereign.Scripting.Lua;

/// <summary>
///     Logger extensions to support Lua scripting.
/// </summary>
public static class LuaLoggingExtensions
{
    /// <summary>
    ///     Logs a critical error including a Lua traceback.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="luaState">Lua state.</param>
    /// <param name="message">Log message.</param>
    /// <param name="args">Arguments to the log message.</param>
    public static void LogCritical(this ILogger logger, IntPtr luaState, string message, params object[] args)
    {
        logger.LogCritical(message, args);
        logger.LogCritical("{Traceback}", GetTraceback(luaState));
    }

    /// <summary>
    ///     Logs a critical error including a Lua traceback.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="e">Exception.</param>
    /// <param name="luaState">Lua state.</param>
    /// <param name="message">Log message.</param>
    /// <param name="args">Arguments to the log message.</param>
    public static void LogCritical(this ILogger logger, Exception e, IntPtr luaState, string message,
        params object[] args)
    {
        logger.LogCritical(e, message, args);
        logger.LogCritical("{Traceback}", GetTraceback(luaState));
    }

    /// <summary>
    ///     Logs an error including a Lua traceback.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="luaState">Lua state.</param>
    /// <param name="message">Log message.</param>
    /// <param name="args">Arguments to the log message.</param>
    public static void LogError(this ILogger logger, IntPtr luaState, string message, params object[] args)
    {
        // Retrieve a Lua stack trace.
        logger.LogError(message, args);
        logger.LogError("{Traceback}", GetTraceback(luaState));
    }

    /// <summary>
    ///     Logs an error including a Lua traceback.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="e">Exception.</param>
    /// <param name="luaState">Lua state.</param>
    /// <param name="message">Log message.</param>
    /// <param name="args">Arguments to the log message.</param>
    public static void LogError(this ILogger logger, Exception e, IntPtr luaState, string message,
        params object[] args)
    {
        logger.LogError(e, message, args);
        logger.LogError("{Traceback}", GetTraceback(luaState));
    }

    /// <summary>
    ///     Logs a warning including a Lua traceback.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="luaState">Lua state.</param>
    /// <param name="message">Log message.</param>
    /// <param name="args">Arguments to the log message.</param>
    public static void LogWarning(this ILogger logger, IntPtr luaState, string message, params object[] args)
    {
        // Retrieve a Lua stack trace.
        logger.LogWarning(message, args);
        logger.LogWarning("{Traceback}", GetTraceback(luaState));
    }

    /// <summary>
    ///     Logs a warning including a Lua traceback.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="e">Exception.</param>
    /// <param name="luaState">Lua state.</param>
    /// <param name="message">Log message.</param>
    /// <param name="args">Arguments to the log message.</param>
    public static void LogWarning(this ILogger logger, Exception e, IntPtr luaState, string message,
        params object[] args)
    {
        logger.LogWarning(e, message, args);
        logger.LogWarning("{Traceback}", GetTraceback(luaState));
    }

    /// <summary>
    ///     Logs information including a Lua traceback.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="luaState">Lua state.</param>
    /// <param name="message">Log message.</param>
    /// <param name="args">Arguments to the log message.</param>
    public static void LogInformation(this ILogger logger, IntPtr luaState, string message, params object[] args)
    {
        // Retrieve a Lua stack trace.
        logger.LogInformation(message, args);
        logger.LogInformation("{Traceback}", GetTraceback(luaState));
    }

    /// <summary>
    ///     Logs information including a Lua traceback.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="e">Exception.</param>
    /// <param name="luaState">Lua state.</param>
    /// <param name="message">Log message.</param>
    /// <param name="args">Arguments to the log message.</param>
    public static void LogInformation(this ILogger logger, Exception e, IntPtr luaState, string message,
        params object[] args)
    {
        logger.LogInformation(e, message, args);
        logger.LogInformation("{Traceback}", GetTraceback(luaState));
    }

    /// <summary>
    ///     Logs a debug message including a Lua traceback.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="luaState">Lua state.</param>
    /// <param name="message">Log message.</param>
    /// <param name="args">Arguments to the log message.</param>
    public static void LogDebug(this ILogger logger, IntPtr luaState, string message, params object[] args)
    {
        // Retrieve a Lua stack trace.
        logger.LogDebug(message, args);
        logger.LogDebug("{Traceback}", GetTraceback(luaState));
    }

    /// <summary>
    ///     Logs debug information including a Lua traceback.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="e">Exception.</param>
    /// <param name="luaState">Lua state.</param>
    /// <param name="message">Log message.</param>
    /// <param name="args">Arguments to the log message.</param>
    public static void LogDebug(this ILogger logger, Exception e, IntPtr luaState, string message,
        params object[] args)
    {
        logger.LogDebug(e, message, args);
        logger.LogDebug("{Traceback}", GetTraceback(luaState));
    }

    /// <summary>
    ///     Logs a trace message including a Lua traceback.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="luaState">Lua state.</param>
    /// <param name="message">Log message.</param>
    /// <param name="args">Arguments to the log message.</param>
    public static void LogTrace(this ILogger logger, IntPtr luaState, string message, params object[] args)
    {
        // Retrieve a Lua stack trace.
        logger.LogTrace(message, args);
        logger.LogTrace("{Traceback}", GetTraceback(luaState));
    }

    /// <summary>
    ///     Logs trace information including a Lua traceback.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="e">Exception.</param>
    /// <param name="luaState">Lua state.</param>
    /// <param name="message">Log message.</param>
    /// <param name="args">Arguments to the log message.</param>
    public static void LogTrace(this ILogger logger, Exception e, IntPtr luaState, string message,
        params object[] args)
    {
        logger.LogTrace(e, message, args);
        logger.LogTrace("{Traceback}", GetTraceback(luaState));
    }

    /// <summary>
    ///     Gets a Lua traceback for the given Lua state.
    /// </summary>
    /// <param name="luaState">Lua state.</param>
    /// <returns>Traceback.</returns>
    private static string GetTraceback(IntPtr luaState)
    {
        luaL_checkstack(luaState, 1, null);
        lua_getfield(luaState, LUA_REGISTRYINDEX, LuaHost.TracebackRegistryKey);
        var result = lua_pcall(luaState, 0, 1, 0);
        if (result != LuaResult.Ok)
        {
            return "Error getting traceback";
        }

        var traceback = lua_tostring(luaState, -1);
        lua_pop(luaState, 1);
        return traceback;
    }
}