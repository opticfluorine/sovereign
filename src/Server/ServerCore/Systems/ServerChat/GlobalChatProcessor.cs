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
using Castle.Core.Logging;
using Sovereign.EngineCore.Logging;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Processor for global chat messages.
/// </summary>
public class GlobalChatProcessor : IChatProcessor
{
    private const string HelpText = "Send a global chat message.";
    private readonly ServerChatInternalController internalController;
    private readonly LoggingUtil loggingUtil;

    public GlobalChatProcessor(ServerChatInternalController internalController, LoggingUtil loggingUtil)
    {
        this.internalController = internalController;
        this.loggingUtil = loggingUtil;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public List<ChatCommand> MatchingCommands => new()
    {
        new ChatCommand { Command = "g", HelpSummary = HelpText }
    };

    public void ProcessChat(string command, string message, ulong senderEntityId)
    {
        // Ignore empty messages.
        if (message.Length == 0) return;

        // Send message.
        var name = loggingUtil.FormatEntity(senderEntityId);
        Logger.InfoFormat("{0}: {1}", name, message);
        internalController.SendGlobalChat(message, name);
    }
}