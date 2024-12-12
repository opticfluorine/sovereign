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

[ScriptableLibrary("chat")]
public class ServerChatScripting
{
    private readonly ServerChatInternalController internalController;

    public ServerChatScripting(ServerChatInternalController internalController)
    {
        this.internalController = internalController;
    }

    [ScriptableFunction("send_system_message")]
    public void SendSystemMessage(ulong targetEntityId, string message)
    {
        internalController.SendSystemMessage(message, targetEntityId);
    }

    [ScriptableFunction("send_to_player")]
    public void SendMessageToPlayer(ulong playerEntityId, byte red, byte green, byte blue, string message)
    {
        internalController.SendMessageToPlayer(playerEntityId, message, red, green, blue);
    }

    [ScriptableFunction("send_to_all")]
    public void SendMessageToAll(byte red, byte green, byte blue, string message)
    {
        internalController.SendMessageToAll(message, red, green, blue);
    }
}