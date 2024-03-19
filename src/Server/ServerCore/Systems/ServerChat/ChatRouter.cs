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
using System.Diagnostics.CodeAnalysis;
using Castle.Core.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.World;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Routes incoming chat messages to the appropriate chat processor.
/// </summary>
public class ChatRouter
{
    /// <summary>
    ///     Name of the help command.
    /// </summary>
    private const string HelpCommand = "help";

    private readonly ChatHelpManager helpManager;
    private readonly ServerChatInternalController internalController;
    private readonly LoggingUtil loggingUtil;
    private readonly PositionComponentCollection positions;

    /// <summary>
    ///     Map from lowercase command to corresponding chat processor.
    /// </summary>
    private readonly Dictionary<string, IChatProcessor> processorsByCommand = new();

    private readonly WorldSegmentResolver resolver;

    public ChatRouter(IList<IChatProcessor> processors, ServerChatInternalController internalController,
        PositionComponentCollection positions, WorldSegmentResolver resolver, LoggingUtil loggingUtil,
        ChatHelpManager helpManager)
    {
        this.internalController = internalController;
        this.positions = positions;
        this.resolver = resolver;
        this.loggingUtil = loggingUtil;
        this.helpManager = helpManager;

        // Build lookup table.
        foreach (var proc in processors)
        {
            foreach (var command in proc.MatchingCommands)
            {
                var lowerCommand = command.Command.ToLower();
                if (processorsByCommand.TryGetValue(lowerCommand, out var otherProc))
                {
                    Logger.WarnFormat("Command {0} already registered by {1}; {2} will not be used for this command.",
                        command.Command, otherProc.GetType(), proc.GetType());
                    continue;
                }

                processorsByCommand[lowerCommand] = proc;
            }
        }
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Routes an incoming chat message to the appropriate chat processor.
    /// </summary>
    /// <param name="details">Chat message details.</param>
    public void RouteChatMessage(ChatEventDetails details)
    {
        if (TryTokenizeCommand(details, out var command, out var remainder))
        {
            if (command.Equals(HelpCommand))
            {
                helpManager.SendHelp(details.SenderEntityId);
            }
            else if (processorsByCommand.TryGetValue(command, out var processor))
            {
                processor.ProcessChat(command, remainder, details.SenderEntityId);
            }
            else
            {
                internalController.SendSystemMessage("Unrecognized command.", details.SenderEntityId);
            }
        }
        else
        {
            RouteLocalChat(details);
        }
    }

    /// <summary>
    ///     Routes a local chat message.
    /// </summary>
    /// <param name="details">Chat message details.</param>
    private void RouteLocalChat(ChatEventDetails details)
    {
        // Determine local world segment.
        if (!positions.HasComponentForEntity(details.SenderEntityId))
        {
            Logger.ErrorFormat("Tried to send local chat for unpositioned entity {0}.", details.SenderEntityId);
            return;
        }

        Logger.InfoFormat("{0}: {1}", loggingUtil.FormatEntity(details.SenderEntityId), details.Message);
        var segmentIndex = resolver.GetWorldSegmentForPosition(positions[details.SenderEntityId]);
        internalController.SendLocalChat(details.Message, details.SenderEntityId, segmentIndex);
    }

    /// <summary>
    ///     Tries to tokenize a message into a command.
    /// </summary>
    /// <param name="details">Chat message details.</param>
    /// <param name="command">If message is a command, the name of the command.</param>
    /// <param name="remainder">If message is a command, the rest of the message after the command name.</param>
    /// <returns>true if a command, false otherwise.</returns>
    private bool TryTokenizeCommand(ChatEventDetails details, [NotNullWhen(true)] out string? command,
        [NotNullWhen(true)] out string? remainder)
    {
        command = null;
        remainder = null;

        var message = details.Message.TrimEnd();
        if (message.Length < 2 || message[0] != ChatConstants.CommandPrefix) return false;

        // Message starts with the command prefix and has at least one other non-whitespace character,
        // so it is a command.
        var firstSpace = message.IndexOf(' ');
        command = (firstSpace < 0 ? message.Substring(1) : message.Substring(1, firstSpace - 1)).ToLower();
        remainder = firstSpace < 0 ? "" : message.Substring(firstSpace).Trim();
        return true;
    }
}