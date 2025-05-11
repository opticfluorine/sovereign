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

using Sovereign.EngineUtil.Attributes;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Provides the "chat" Lua module for sending messages to players.
/// </summary>
[ScriptableLibrary("chat")]
public class ServerChatScripting
{
    private readonly ServerChatInternalController internalController;

    public ServerChatScripting(ServerChatInternalController internalController)
    {
        this.internalController = internalController;
    }

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
}