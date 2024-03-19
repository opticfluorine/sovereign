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

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Interface for server-side chat message processors.
/// </summary>
public interface IChatProcessor
{
    /// <summary>
    ///     List of case-insensitive command names that will route to this processor.
    /// </summary>
    List<ChatCommand> MatchingCommands { get; }

    /// <summary>
    ///     Processes a chat command that was routed to this chat processor.
    /// </summary>
    /// <param name="command">Command name. Will be a lowercase string.</param>
    /// <param name="message">Remaining portion of the message following the command invocation.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    void ProcessChat(string command, string message, ulong senderEntityId);
}