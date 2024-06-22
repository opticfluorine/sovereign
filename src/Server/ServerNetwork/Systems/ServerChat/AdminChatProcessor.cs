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

using System;
using System.Collections.Generic;
using Castle.Core.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Validators;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.Persistence.Players;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.ServerCore.Systems.WorldManagement;

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

    /// <summary>
    ///     Command name for /addblock.
    /// </summary>
    private const string AddBlock = "addblock";

    /// <summary>
    ///     Command name for /removeblock.
    /// </summary>
    private const string RemoveBlock = "removeblock";

    private readonly AdminTagCollection admins;
    private readonly BlockController blockController;
    private readonly BlockServices blockServices;
    private readonly BlockTemplateNameComponentIndexer blockTemplateNames;
    private readonly IEventSender eventSender;
    private readonly ServerChatInternalController internalController;
    private readonly LoggingUtil loggingUtil;
    private readonly NameComponentCollection names;
    private readonly NameComponentValidator nameValidator;
    private readonly PersistencePlayerServices persistencePlayerServices;
    private readonly PlayerNameComponentIndexer playerNameIndex;
    private readonly PlayerRoleCheck playerRoleCheck;
    private readonly WorldManagementController worldManagementController;

    public AdminChatProcessor(AdminTagCollection admins, ServerChatInternalController internalController,
        PlayerRoleCheck playerRoleCheck, PlayerNameComponentIndexer playerNameIndex,
        NameComponentValidator nameValidator, PersistencePlayerServices persistencePlayerServices,
        LoggingUtil loggingUtil, NameComponentCollection names, WorldManagementController worldManagementController,
        IEventSender eventSender, BlockController blockController, BlockServices blockServices,
        BlockTemplateNameComponentIndexer blockTemplateNames)
    {
        this.admins = admins;
        this.internalController = internalController;
        this.playerRoleCheck = playerRoleCheck;
        this.playerNameIndex = playerNameIndex;
        this.nameValidator = nameValidator;
        this.persistencePlayerServices = persistencePlayerServices;
        this.loggingUtil = loggingUtil;
        this.names = names;
        this.worldManagementController = worldManagementController;
        this.eventSender = eventSender;
        this.blockController = blockController;
        this.blockServices = blockServices;
        this.blockTemplateNames = blockTemplateNames;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public List<ChatCommand> MatchingCommands => new()
    {
        new ChatCommand { Command = AddAdmin, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = RemoveAdmin, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = AddBlock, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = RemoveBlock, HelpSummary = "", IncludeInHelp = false }
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
        else if (command == AddBlock)
            OnAddBlock(message, senderEntityId);
        else if (command == RemoveBlock)
            OnRemoveBlock(message, senderEntityId);
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
            // Modify the player directly in memory, then trigger a resync to update the client role states.
            admins.TagEntity(playerEntityId);
            worldManagementController.ResyncPositionedEntity(eventSender, playerEntityId);
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
        internalController.SendSystemMessage($"Player {playerName} is now admin.", senderEntityId);
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
            return;
        }

        // Check if the affected player is online.
        if (playerNameIndex.TryGetPlayerByName(playerName, out var playerEntityId))
        {
            // Modify the player directly in memory, then trigger a resync to update the client role states.
            admins.UntagEntity(playerEntityId);
            worldManagementController.ResyncPositionedEntity(eventSender, playerEntityId);
            internalController.SendSystemMessage("You are no longer an admin.", playerEntityId);
        }
        else
        {
            // Player offline or doesn't exist, ensure no admin role if player exists.
            persistencePlayerServices.RemoveAdminForPlayer(playerName);
        }

        Logger.InfoFormat("Player {0} is no longer admin (or already was not); change made by {1}.", playerName,
            loggingUtil.FormatEntity(senderEntityId));
        internalController.SendSystemMessage($"Player {playerName} is no longer admin.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /addblock command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnAddBlock(string message, ulong senderEntityId)
    {
        var args = message.Split(' ', 4, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (args.Length != 4)
        {
            Logger.WarnFormat("{0} used /addblock with bad parameters.", loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("Usage: /addblock x y z (template_rel_id | template_name)",
                senderEntityId);
            return;
        }

        // Parse block position.
        GridPosition blockPosition;
        try
        {
            var x = int.Parse(args[0]);
            var y = int.Parse(args[1]);
            var z = int.Parse(args[2]);
            blockPosition = new GridPosition { X = x, Y = y, Z = z };
        }
        catch (Exception)
        {
            Logger.WarnFormat("{0} used /addblock with bad coordinates.", loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("x, y, and z must be integers.", senderEntityId);
            return;
        }

        // Parse template entity specification.
        ulong templateEntityId = 0;
        try
        {
            templateEntityId = EntityConstants.FirstTemplateEntityId + ulong.Parse(args[3]);
            if (templateEntityId is not (>= EntityConstants.FirstTemplateEntityId
                and <= EntityConstants.LastTemplateEntityId))
            {
                Logger.WarnFormat("{0} tried to use a non-template entity as template entity.",
                    loggingUtil.FormatEntity(senderEntityId));
                internalController.SendSystemMessage("template_rel_id must correspond to a template entity.",
                    senderEntityId);
                return;
            }

            if (!blockServices.IsEntityBlock(templateEntityId))
            {
                Logger.WarnFormat("{0} tried to use a non-block template entity for block creation.",
                    loggingUtil.FormatEntity(senderEntityId));
                internalController.SendSystemMessage("template_rel_id must correspond to a template entity.",
                    senderEntityId);
                return;
            }
        }
        catch (Exception)
        {
            // If argument isn't a ulong, treat it as the name of a block template entity.
            if (!blockTemplateNames.TryGetByName(args[3], out templateEntityId))
            {
                Logger.WarnFormat("{0} tried to add block type '{1}' which was not found.",
                    loggingUtil.FormatEntity(senderEntityId), args[3]);
                internalController.SendSystemMessage("Unrecognized block template name.", senderEntityId);
                return;
            }
        }

        // Check for block existence.
        if (blockServices.BlockExistsAtPosition(blockPosition))
        {
            // Block already exists, can't add new.
            Logger.WarnFormat("{0} tried to add block where one already exists.",
                loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("Block already exists at requested position.", senderEntityId);
            return;
        }

        // Add block.
        blockController.AddBlock(eventSender, new BlockRecord
        {
            Position = blockPosition,
            TemplateEntityId = templateEntityId
        });
    }

    /// <summary>
    ///     Handles the /removeblock command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnRemoveBlock(string message, ulong senderEntityId)
    {
        var args = message.Split(' ', 3);
        if (args.Length != 3)
        {
            Logger.WarnFormat("{0} used /removeblock with bad parameters.", loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("Usage: /removeblock x y z",
                senderEntityId);
            return;
        }

        // Parse block position.
        GridPosition blockPosition;
        try
        {
            var x = int.Parse(args[0]);
            var y = int.Parse(args[1]);
            var z = int.Parse(args[2]);
            blockPosition = new GridPosition { X = x, Y = y, Z = z };
        }
        catch (Exception)
        {
            Logger.WarnFormat("{0} used /addblock with bad coordinates.", loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("x, y, and z must be integers.", senderEntityId);
            return;
        }

        if (!blockServices.BlockExistsAtPosition(blockPosition))
        {
            Logger.WarnFormat("{0} used /removeblock with no block at position.",
                loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("No block at position.", senderEntityId);
        }

        // Remove block.
        blockController.RemoveBlockAtPosition(eventSender, blockPosition);
    }
}