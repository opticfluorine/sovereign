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
using System.Linq;
using System.Text;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Builds the help message displayed when the player types /help.
/// </summary>
public class ChatHelpManager
{
    /// <summary>
    ///     Help message.
    /// </summary>
    private readonly string helpMessage;

    private readonly ServerChatInternalController internalController;

    public ChatHelpManager(IList<IChatProcessor> processors, ServerChatInternalController internalController)
    {
        this.internalController = internalController;
        var commands = processors
            .SelectMany(proc => proc.MatchingCommands)
            .Where(command => command.IncludeInHelp)
            .OrderBy(command => command.Command);
        var sb = new StringBuilder();
        sb.Append("Available commands:\n");
        foreach (var command in commands)
        {
            sb.Append($"  /{command.Command} - {command.HelpSummary}\n");
        }

        helpMessage = sb.ToString().Trim();
    }

    /// <summary>
    ///     Sends help message as a system message to the given player.
    /// </summary>
    /// <param name="playerEntityId"></param>
    public void SendHelp(ulong playerEntityId)
    {
        internalController.SendSystemMessage(helpMessage, playerEntityId);
    }
}