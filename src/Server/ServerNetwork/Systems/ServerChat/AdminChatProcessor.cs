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
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Validators;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Player;
using Sovereign.Persistence.Players;
using Sovereign.ServerCore.Systems.ServerChat;

namespace Sovereign.ServerNetwork.Systems.ServerChat;

/// <summary>
///     Chat processor that handles admin-specific commands.
/// </summary>
public class AdminChatProcessor : IChatProcessor
{
    /// <summary>
    ///     Command name for /addadmin.
    /// </summary>
    private const string AddAdmin = "addadmin";

    /// <summary>
    ///     Command name for /removeadmin.
    /// </summary>
    private const string RemoveAdmin = "removeadmin";

    private readonly AdminTagCollection admins;
    private readonly ServerChatInternalController internalController;
    private readonly LoggingUtil loggingUtil;
    private readonly NameComponentCollection names;
    private readonly NameComponentValidator nameValidator;
    private readonly PersistencePlayerServices persistencePlayerServices;
    private readonly PlayerNameComponentIndexer playerNameIndex;
    private readonly PlayerRoleCheck playerRoleCheck;

    public AdminChatProcessor(AdminTagCollection admins, ServerChatInternalController internalController,
        PlayerRoleCheck playerRoleCheck, PlayerNameComponentIndexer playerNameIndex,
        NameComponentValidator nameValidator, PersistencePlayerServices persistencePlayerServices,
        LoggingUtil loggingUtil, NameComponentCollection names)
    {
        this.admins = admins;
        this.internalController = internalController;
        this.playerRoleCheck = playerRoleCheck;
        this.playerNameIndex = playerNameIndex;
        this.nameValidator = nameValidator;
        this.persistencePlayerServices = persistencePlayerServices;
        this.loggingUtil = loggingUtil;
        this.names = names;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public List<ChatCommand> MatchingCommands => new()
    {
        new ChatCommand { Command = AddAdmin, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = RemoveAdmin, HelpSummary = "", IncludeInHelp = false }
    };

    public void ProcessChat(string command, string message, ulong senderEntityId)
    {
        // Must be an admin to use admin commands.
        if (!playerRoleCheck.IsPlayerAdmin(senderEntityId))
        {
            Logger.WarnFormat("{0} attempted to use admin command {1} while not admin.",
                loggingUtil.FormatEntity(senderEntityId), command);
            internalController.SendSystemMessage("Only admins may use this command.", senderEntityId);
            return;
        }

        // Record admin commands in log for audit.
        Logger.InfoFormat("ADMIN COMMAND: {0}: /{1} {2}", loggingUtil.FormatEntity(senderEntityId), command, message);

        // Admin role verified, dispatch to specific handlers.
        if (command == AddAdmin)
            OnAddAdmin(message, senderEntityId);
        else if (command == RemoveAdmin)
            OnRemoveAdmin(message, senderEntityId);
    }

    /// <summary>
    ///     Handles the /addadmin command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnAddAdmin(string message, ulong senderEntityId)
    {
        // Parse arguments, do basic validation.
        var playerName = message.Trim();
        if (!nameValidator.IsValid(playerName))
        {
            internalController.SendSystemMessage("Invalid name.", senderEntityId);
            return;
        }

        // Check if the affected player is online.
        if (playerNameIndex.TryGetPlayerByName(playerName, out var playerEntityId))
        {
            // Modify the player directly in memory.
            admins.TagEntity(playerEntityId);
            internalController.SendSystemMessage("You are now an admin.", playerEntityId);
        }
        else if (!persistencePlayerServices.TryAddAdminForPlayer(playerName))
        {
            // Player doesn't exist.
            Logger.ErrorFormat("Cannot make player {0} admin: player does not exist.", playerName);
            internalController.SendSystemMessage("Player does not exist.", senderEntityId);
            return;
        }

        Logger.InfoFormat("Player {0} is now admin; change made by {1}.", playerName,
            loggingUtil.FormatEntity(senderEntityId));
        internalController.SendSystemMessage("OK.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /removeadmin command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnRemoveAdmin(string message, ulong senderEntityId)
    {
        // Parse arguments, do basic validation.
        var playerName = message.Trim();
        if (!nameValidator.IsValid(playerName))
        {
            internalController.SendSystemMessage("Invalid name.", senderEntityId);
            return;
        }

        // Do not allow players to remove their own admin role.
        if (playerName == names[senderEntityId])
        {
            Logger.WarnFormat("{0} tried to remove own admin role; request denied.", playerName);
            internalController.SendSystemMessage("Cannot remove own admin role.", senderEntityId);
        }

        // Check if the affected player is online.
        if (playerNameIndex.TryGetPlayerByName(playerName, out var playerEntityId))
        {
            // Modify the player directly in memory.
            admins.UntagEntity(playerEntityId);
            internalController.SendSystemMessage("You are no longer an admin.", playerEntityId);
        }
        else
        {
            // Player offline or doesn't exist, ensure no admin role if player exists.
            persistencePlayerServices.RemoveAdminForPlayer(playerName);
        }

        Logger.InfoFormat("Player {0} is no longer admin (or already was not); change made by {1}.", playerName,
            loggingUtil.FormatEntity(senderEntityId));
        internalController.SendSystemMessage("OK.", senderEntityId);
    }
}