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

namespace Sovereign.ClientCore.Systems.ClientChat;

/// <summary>
///     Public read API for client-side chat.
/// </summary>
public class ClientChatServices
{
    private readonly ChatHistoryManager chatHistoryManager;

    public ClientChatServices(ChatHistoryManager chatHistoryManager)
    {
        this.chatHistoryManager = chatHistoryManager;
    }

    /// <summary>
    ///     Read-only list of chat history ordered oldest to newest by time received.
    /// </summary>
    public IReadOnlyList<ChatHistoryEntry> ChatHistory => chatHistoryManager.History;
}