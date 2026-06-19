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
using Sovereign.EngineUtil.Attributes;
using Sovereign.Scripting.Lua;
using Sovereign.ServerCore.Systems.Scripting;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Provides the "chat" Lua module for sending messages to players.
/// </summary>
[ScriptableLibrary("Chat")]
public class ServerChatScripting(
    ServerChatInternalController internalController,
    ScriptChatCallbacks callbacks,
    ScriptingServices scriptingServices)
{
    private readonly ScriptChatCallbacks callbacks = callbacks;
    private readonly ScriptingServices scriptingServices = scriptingServices;

    [ScriptableFunction("SendSystemMessage")]
    public void SendSystemMessage(ulong targetEntityId, string message)
    {
        internalController.SendSystemMessage(message, targetEntityId);
    }

    [ScriptableFunction("SendToPlayer")]
    public void SendMessageToPlayer(ulong playerEntityId, uint color, string message)
    {
        internalController.SendMessageToPlayer(playerEntityId, message, color);
    }

    [ScriptableFunction("SendToAll")]
    public void SendMessageToAll(uint color, string message)
    {
        internalController.SendMessageToAll(message, color);
    }

    [ScriptableFunction("AddCommand")]
    public void AddCommand(IntPtr luaState, string command, [ScriptableCallback] int callback)
    {
        if (!scriptingServices.TryGetHostForState(luaState, out var host))
            throw new Exception("No host found for script.");

        if (!callbacks.TryAddCallbackCommand(command, host, callback))
        {
            host.Logger.LogError(luaState, "Chat command /{Command} is already registered.", command);
        }
    }
}