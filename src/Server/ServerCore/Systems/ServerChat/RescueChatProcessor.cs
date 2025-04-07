// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Systems.Movement;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Provides the /rescue command in chat which warps the player to the spawn location.
/// </summary>
public class RescueChatProcessor : IChatProcessor
{
    private readonly IEventSender eventSender;
    private readonly ServerChatInternalController internalController;
    private readonly ILogger<RescueChatProcessor> logger;
    private readonly LoggingUtil loggingUtil;
    private readonly MovementController movementController;

    public RescueChatProcessor(IEventSender eventSender, MovementController movementController,
        ServerChatInternalController internalController, ILogger<RescueChatProcessor> logger,
        LoggingUtil loggingUtil)
    {
        this.eventSender = eventSender;
        this.movementController = movementController;
        this.internalController = internalController;
        this.logger = logger;
        this.loggingUtil = loggingUtil;
    }

    public List<ChatCommand> MatchingCommands => new()
    {
        new ChatCommand
            { Command = "rescue", HelpSummary = "Rescue yourself to the spawn point.", IncludeInHelp = true }
    };

    public void ProcessChat(string command, string message, ulong senderEntityId)
    {
        // TODO Select per-player spawn point.
        var spawnPoint = new Vector3(0, 0, 1);

        logger.LogInformation("{Player} is requesting rescue to {SpawnPoint}.",
            loggingUtil.FormatEntity(senderEntityId), spawnPoint);

        movementController.Teleport(eventSender, senderEntityId, spawnPoint);
        internalController.SendSystemMessage("You have been rescued to the spawn point.", senderEntityId);
    }
}