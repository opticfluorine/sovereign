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

using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sovereign.Scripting.Lua;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Manages script callbacks that implement custom chat commands.
/// </summary>
public sealed class ScriptChatCallbacks
{
    private readonly ConcurrentDictionary<string, (LuaHost host, int cbRef, ChatCommandFlags flags)> callbackCommands =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Attempts to register a new callback command.
    /// </summary>
    /// <param name="command">Command (case-insensitive).</param>
    /// <param name="host">Script host.</param>
    /// <param name="callbackRef">Callback function Lua reference.</param>
    /// <param name="flags">Behaviour flags controlling how the callback is invoked.</param>
    /// <returns>true if successfully registered, false otherwise.</returns>
    public bool TryAddCallbackCommand(string command, LuaHost host, int callbackRef, ChatCommandFlags flags)
    {
        return callbackCommands.TryAdd(command, (host, callbackRef, flags));
    }

    /// <summary>
    ///     Attempts to handle a chat command via a script callback.
    /// </summary>
    /// <param name="command">Chat command (case-insensitive).</param>
    /// <param name="message">Chat message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    /// <returns>true if the message was handled, false otherwise.</returns>
    public bool TryHandleCommand(string command, string message, ulong senderEntityId)
    {
        if (!callbackCommands.TryGetValue(command, out var cb)) return false;

        var (host, cbRef, flags) = cb;
        try
        {
            if ((flags & ChatCommandFlags.CommaSeparatedArgs) > 0)
            {
                var args = message.Split(',').Select(s => s.Trim()).ToList();
                host.CallRefFunction(cbRef, command, args, senderEntityId);
            }
            else
            {
                host.CallRefFunction(cbRef, command, message, senderEntityId);
            }
        }
        catch (LuaException e)
        {
            host.Logger.LogError(e, "Failed to invoke callback for chat command /{Command}.", command);
            return false;
        }

        return true;
    }
}