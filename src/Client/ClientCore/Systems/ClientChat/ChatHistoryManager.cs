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
using System.Numerics;
using Sovereign.EngineCore.Components;

namespace Sovereign.ClientCore.Systems.ClientChat;

/// <summary>
///     Manages the client-side chat history.
/// </summary>
public class ChatHistoryManager
{
    /// <summary>
    ///     Chat history, ordered oldest to newest by time received.
    /// </summary>
    private readonly List<ChatHistoryEntry> history = new();

    private readonly NameComponentCollection names;

    public ChatHistoryManager(NameComponentCollection names)
    {
        this.names = names;
    }

    /// <summary>
    ///     Read-only chat history, ordered oldest to newest by time received.
    /// </summary>
    public IReadOnlyList<ChatHistoryEntry> History => history;

    /// <summary>
    ///     Adds a line to the chat history.
    /// </summary>
    /// <param name="type">Chat type.</param>
    /// <param name="message">Chat message.</param>
    /// <param name="originEntityId">Entity ID of the source.</param>
    public void AddChat(ChatType type, string message, ulong originEntityId)
    {
        // Format everything according to chat type.
        var name = names.HasComponentForEntity(originEntityId)
            ? names[originEntityId]
            : ClientChatConstants.UnknownName;
        AddChat(type, message, name);
    }

    /// <summary>
    ///     Adds a line to the chat history.
    /// </summary>
    /// <param name="type">Chat type.</param>
    /// <param name="message">Chat message.</param>
    /// <param name="originName">Name of the source.</param>
    public void AddChat(ChatType type, string message, string originName)
    {
        var fullMessage = $"{originName}: {message}";
        AddChat(type, fullMessage);
    }

    /// <summary>
    ///     Adds a line to the chat history.
    /// </summary>
    /// <param name="type">Chat type.</param>
    /// <param name="message">Chat message.</param>
    public void AddChat(ChatType type, string message)
    {
        if (!ClientChatConstants.ChatTextColors.TryGetValue(type, out var color))
            color = ClientChatConstants.DefaultTextColor;

        // Append the chat history.
        history.Add(new ChatHistoryEntry { Message = message, Color = color });
    }

    /// <summary>
    ///     Adds a line to the chat history.
    /// </summary>
    /// <param name="message">Chat message.</param>
    /// <param name="color">Font color.</param>
    public void AddChat(string message, Vector4 color)
    {
        history.Add(new ChatHistoryEntry { Message = message, Color = color });
    }

    /// <summary>
    ///     Clears the chat history.
    /// </summary>
    public void Clear()
    {
        history.Clear();
    }
}