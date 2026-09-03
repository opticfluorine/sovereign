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
using System.Linq;
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Components.Validators;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.Persistence.Bans;
using Sovereign.Persistence.Players;
using Sovereign.ServerCore.Components;
using Sovereign.ServerCore.Systems.Scripting;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.ServerNetwork.Network.ServerNetwork;

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

    /// <summary>
    ///     Command name for /reloadallscripts.
    /// </summary>
    private const string ReloadAllScripts = "reloadallscripts";

    /// <summary>
    ///     Command name for /reloadscript.
    /// </summary>
    private const string ReloadScript = "reloadscript";

    /// <summary>
    ///     Command name for /loadnewscripts.
    /// </summary>
    private const string LoadNewScripts = "loadnewscripts";

    /// <summary>
    ///     Command name for /listscripts.
    /// </summary>
    private const string ListScripts = "listscripts";

    /// <summary>
    ///     Command name for /ban.
    /// </summary>
    private const string Ban = "ban";

    /// <summary>
    ///     Command name for /unban.
    /// </summary>
    private const string Unban = "unban";

    /// <summary>
    ///     Command name for /listbans.
    /// </summary>
    private const string ListBans = "listbans";

    private readonly AccountComponentCollection accounts;
    private readonly AdminTagCollection admins;
    private readonly AccountServices accountServices;
    private readonly BlockController blockController;
    private readonly IBlockServices blockServices;
    private readonly BlockTemplateNameComponentIndexer blockTemplateNames;
    private readonly IEventSender eventSender;
    private readonly ServerChatInternalController internalController;
    private readonly ILogger<AdminChatProcessor> logger;
    private readonly LoggingUtil loggingUtil;
    private readonly NameComponentCollection names;
    private readonly NameComponentValidator nameValidator;
    private readonly PersistenceBanServices persistenceBanServices;
    private readonly PersistencePlayerServices persistencePlayerServices;
    private readonly PlayerNameComponentIndexer playerNameIndex;
    private readonly PlayerRoleCheck playerRoleCheck;
    private readonly ScriptingController scriptingController;
    private readonly ScriptingServices scriptingServices;
    private readonly WorldManagementController worldManagementController;
    private readonly ServerNetworkController networkController;

    public AdminChatProcessor(AdminTagCollection admins, ServerChatInternalController internalController,
        PlayerRoleCheck playerRoleCheck, PlayerNameComponentIndexer playerNameIndex,
        NameComponentValidator nameValidator, PersistencePlayerServices persistencePlayerServices,
        LoggingUtil loggingUtil, NameComponentCollection names, WorldManagementController worldManagementController,
        IEventSender eventSender, BlockController blockController, IBlockServices blockServices,
        BlockTemplateNameComponentIndexer blockTemplateNames, ILogger<AdminChatProcessor> logger,
        ScriptingController scriptingController, ScriptingServices scriptingServices,
        PersistenceBanServices persistenceBanServices, AccountServices accountServices,
        ServerNetworkController networkController, AccountComponentCollection accounts)
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
        this.logger = logger;
        this.scriptingController = scriptingController;
        this.scriptingServices = scriptingServices;
        this.persistenceBanServices = persistenceBanServices;
        this.accountServices = accountServices;
        this.networkController = networkController;
        this.accounts = accounts;
    }

    public List<ChatCommand> MatchingCommands => new()
    {
        new ChatCommand { Command = AddAdmin, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = RemoveAdmin, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = AddBlock, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = RemoveBlock, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = ReloadAllScripts, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = ReloadScript, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = LoadNewScripts, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = ListScripts, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = Ban, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = Unban, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = ListBans, HelpSummary = "", IncludeInHelp = false }
    };

    public void ProcessChat(string command, string message, ulong senderEntityId)
    {
        // Must be an admin to use admin commands.
        if (!playerRoleCheck.IsPlayerAdmin(senderEntityId))
        {
            logger.LogWarning("{Player} attempted to use admin command {Command} while not admin.",
                loggingUtil.FormatEntity(senderEntityId), command);
            internalController.SendSystemMessage("Only admins may use this command.", senderEntityId);
            return;
        }

        // Record admin commands in log for audit.
        logger.LogInformation("ADMIN COMMAND: {Player}: /{Command} {Message}", loggingUtil.FormatEntity(senderEntityId),
            command, message);

        // Admin role verified, dispatch to specific handlers.
        switch (command)
        {
            case AddAdmin:
                OnAddAdmin(message, senderEntityId);
                break;

            case RemoveAdmin:
                OnRemoveAdmin(message, senderEntityId);
                break;

            case AddBlock:
                OnAddBlock(message, senderEntityId);
                break;

            case RemoveBlock:
                OnRemoveBlock(message, senderEntityId);
                break;

            case ReloadAllScripts:
                OnReloadAllScripts(senderEntityId);
                break;

            case ReloadScript:
                OnReloadScript(message, senderEntityId);
                break;

            case LoadNewScripts:
                OnLoadNewScripts(senderEntityId);
                break;

            case ListScripts:
                OnListScripts(senderEntityId);
                break;

            case Ban:
                OnBan(message, senderEntityId);
                break;

            case Unban:
                OnUnban(message, senderEntityId);
                break;

            case ListBans:
                OnListBans(senderEntityId);
                break;
        }
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
            worldManagementController.ResyncEntityTree(eventSender, playerEntityId);
            internalController.SendSystemMessage("You are now an admin.", playerEntityId);
        }
        else if (!persistencePlayerServices.TryAddAdminForPlayer(playerName))
        {
            // Player doesn't exist.
            logger.LogError("Cannot make player {Name} admin: player does not exist.", playerName);
            internalController.SendSystemMessage("Player does not exist.", senderEntityId);
            return;
        }

        logger.LogInformation("Player {Name} is now admin; change made by {Admin}.", playerName,
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
            logger.LogWarning("{Player} tried to remove own admin role; request denied.", playerName);
            internalController.SendSystemMessage("Cannot remove own admin role.", senderEntityId);
            return;
        }

        // Check if the affected player is online.
        if (playerNameIndex.TryGetPlayerByName(playerName, out var playerEntityId))
        {
            // Modify the player directly in memory, then trigger a resync to update the client role states.
            admins.UntagEntity(playerEntityId);
            worldManagementController.ResyncEntityTree(eventSender, playerEntityId);
            internalController.SendSystemMessage("You are no longer an admin.", playerEntityId);
        }
        else
        {
            // Player offline or doesn't exist, ensure no admin role if player exists.
            persistencePlayerServices.RemoveAdminForPlayer(playerName);
        }

        logger.LogInformation("Player {Player} is no longer admin (or already was not); change made by {Admin}.",
            playerName, loggingUtil.FormatEntity(senderEntityId));
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
            logger.LogWarning("{Player} used /addblock with bad parameters.", loggingUtil.FormatEntity(senderEntityId));
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
            logger.LogWarning("{Player} used /addblock with bad coordinates.",
                loggingUtil.FormatEntity(senderEntityId));
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
                logger.LogWarning("{Player} tried to use a non-template entity as template entity.",
                    loggingUtil.FormatEntity(senderEntityId));
                internalController.SendSystemMessage("template_rel_id must correspond to a template entity.",
                    senderEntityId);
                return;
            }

            if (!blockServices.IsEntityBlock(templateEntityId))
            {
                logger.LogWarning("{Player} tried to use a non-block template entity for block creation.",
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
                logger.LogWarning("{Player} tried to add block type '{Type}' which was not found.",
                    loggingUtil.FormatEntity(senderEntityId), args[3]);
                internalController.SendSystemMessage("Unrecognized block template name.", senderEntityId);
                return;
            }
        }

        // Check for block existence.
        if (blockServices.BlockExistsAtPosition(blockPosition))
        {
            // Block already exists, can't add new.
            logger.LogWarning("{Player} tried to add block where one already exists.",
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
            logger.LogWarning("{Player} used /removeblock with bad parameters.",
                loggingUtil.FormatEntity(senderEntityId));
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
            logger.LogWarning("{Player} used /addblock with bad coordinates.",
                loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("x, y, and z must be integers.", senderEntityId);
            return;
        }

        if (!blockServices.BlockExistsAtPosition(blockPosition))
        {
            logger.LogWarning("{Player} used /removeblock with no block at position.",
                loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("No block at position.", senderEntityId);
        }

        // Remove block.
        blockController.RemoveBlockAtPosition(eventSender, blockPosition);
    }

    /// <summary>
    ///     Handles the /reloadallscripts command.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnReloadAllScripts(ulong senderEntityId)
    {
        scriptingController.ReloadAllScripts(eventSender);
        internalController.SendSystemMessage("All scripts requested to reload.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /reloadscript command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnReloadScript(string message, ulong senderEntityId)
    {
        if (!scriptingServices.IsScriptLoaded(message))
        {
            internalController.SendSystemMessage($"Script {message} is not currently loaded.", senderEntityId);
            return;
        }

        scriptingController.ReloadScript(eventSender, message);
        internalController.SendSystemMessage($"Script {message} will be reloaded.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /loadnewscripts command.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnLoadNewScripts(ulong senderEntityId)
    {
        scriptingController.LoadNewScripts(eventSender);
        internalController.SendSystemMessage("Any new scripts will be loaded.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /listscripts command.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnListScripts(ulong senderEntityId)
    {
        internalController.SendSystemMessage("Currently loaded scripts:", senderEntityId);
        foreach (var name in scriptingServices.GetLoadedScripts().Order())
            internalController.SendSystemMessage($"  - {name}", senderEntityId);
    }

    /// <summary>
    ///     Handles the /ban command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnBan(string message, ulong senderEntityId)
    {
        // Parse arguments, do basic validation.
        var args = message.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (args.Length is < 1 or > 2)
        {
            internalController.SendSystemMessage("Usage: /ban player [duration_in_days]", senderEntityId);
            return;
        }

        var playerName = args[0];
        if (!nameValidator.IsValid(playerName))
        {
            internalController.SendSystemMessage("Invalid name.", senderEntityId);
            return;
        }

        int? durationDays = null;
        if (args.Length == 2)
        {
            if (!int.TryParse(args[1], out var duration) || duration <= 0)
            {
                internalController.SendSystemMessage("Duration must be a positive number of days.",
                    senderEntityId);
                return;
            }

            durationDays = duration;
        }

        // Resolve the banned player's account.
        if (!TryResolveAccount(playerName, senderEntityId, out var accountId)) return;

        // Record the ban.
        persistenceBanServices.AddBan(accountId, playerName, names[senderEntityId], durationDays);
        logger.LogInformation("Player {Name} (account {AccountId}) has been banned; ban created by {Admin}.",
            playerName, accountId, loggingUtil.FormatEntity(senderEntityId));

        // Kick the account's connection if the account is logged in.
        if (accountServices.TryGetConnectionIdForAccount(accountId, out var connectionId))
        {
            if (accountServices.TryGetPlayerForAccount(accountId, out var playerEntityId))
                internalController.SendSystemMessage("You have been banned from this server.", playerEntityId);
            networkController.Disconnect(eventSender, connectionId);
        }

        // Confirm to the admin.
        internalController.SendSystemMessage(durationDays.HasValue
            ? $"Account of player {playerName} has been banned for {durationDays.Value} day(s)."
            : $"Account of player {playerName} has been banned permanently.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /unban command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnUnban(string message, ulong senderEntityId)
    {
        // Parse arguments, do basic validation.
        var playerName = message.Trim();
        if (!nameValidator.IsValid(playerName))
        {
            internalController.SendSystemMessage("Invalid name.", senderEntityId);
            return;
        }

        // Resolve the player's account.
        if (!TryResolveAccount(playerName, senderEntityId, out var accountId)) return;

        // Remove any active bans.
        var removed = persistenceBanServices.RemoveBansForAccount(accountId);
        logger.LogInformation("Removed {Count} ban(s) for player {Name}; request made by {Admin}.",
            removed, playerName, loggingUtil.FormatEntity(senderEntityId));

        internalController.SendSystemMessage(removed > 0
            ? $"Removed {removed} ban(s)."
            : "No active bans for that player.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /listbans command.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnListBans(ulong senderEntityId)
    {
        var bans = persistenceBanServices.GetActiveBans();
        if (bans.Count == 0)
        {
            internalController.SendSystemMessage("No active bans.", senderEntityId);
            return;
        }

        internalController.SendSystemMessage("Active bans:", senderEntityId);
        foreach (var ban in bans)
        {
            var duration = ban.DurationDays.HasValue ? $"{ban.DurationDays.Value} day(s)" : "permanent";
            internalController.SendSystemMessage(
                $"  - {ban.Username} (banned as {ban.PlayerName}, {duration}, created {ban.CreatedUtc:u} by {ban.AdminName})",
                senderEntityId);
        }
    }

    /// <summary>
    ///     Resolves a player name to an account ID, checking online players first
    ///     and then falling back to the database.
    /// </summary>
    /// <param name="playerName">Player name (matched case-insensitively).</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    /// <param name="accountId">Resolved account ID. Only valid if the method returns true.</param>
    /// <returns>true if the account was resolved, false otherwise.</returns>
    private bool TryResolveAccount(string playerName, ulong senderEntityId, out Guid accountId)
    {
        // Check if the player is online.
        if (playerNameIndex.TryGetPlayerByName(playerName, out var playerEntityId)
            && accounts.HasComponentForEntity(playerEntityId))
        {
            accountId = accounts[playerEntityId];
            return true;
        }

        // Player is offline; resolve through the database.
        if (persistenceBanServices.TryGetAccountForPlayerName(playerName, out accountId)) return true;

        logger.LogWarning("Cannot resolve account for player {Name}; player does not exist.", playerName);
        internalController.SendSystemMessage("Player does not exist.", senderEntityId);
        return false;
    }
}