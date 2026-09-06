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
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
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
using Sovereign.EngineCore.Systems.Data;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.Persistence.Players;
using Sovereign.ServerCore.Systems.Scripting;
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
    ///     Command name for /reloadentity.
    /// </summary>
    private const string ReloadEntity = "reloadentity";

    /// <summary>
    ///     Command name for /reloadtemplate.
    /// </summary>
    private const string ReloadTemplate = "reloadtemplate";

    /// <summary>
    ///     Command name for /getvalue.
    /// </summary>
    private const string GetValue = "getvalue";

    /// <summary>
    ///     Command name for /setvalue.
    /// </summary>
    private const string SetValue = "setvalue";

    /// <summary>
    ///     Command name for /getentityvalue.
    /// </summary>
    private const string GetEntityValue = "getentityvalue";

    /// <summary>
    ///     Command name for /setentityvalue.
    /// </summary>
    private const string SetEntityValue = "setentityvalue";

    private readonly AdminTagCollection admins;
    private readonly BlockController blockController;
    private readonly IBlockServices blockServices;
    private readonly BlockTemplateNameComponentIndexer blockTemplateNames;
    private readonly IDataController dataController;
    private readonly IDataServices dataServices;
    private readonly EntityTable entityTable;
    private readonly IEventSender eventSender;
    private readonly ServerChatInternalController internalController;
    private readonly ILogger<AdminChatProcessor> logger;
    private readonly LoggingUtil loggingUtil;
    private readonly NameComponentCollection names;
    private readonly NameComponentValidator nameValidator;
    private readonly PersistencePlayerServices persistencePlayerServices;
    private readonly PlayerNameComponentIndexer playerNameIndex;
    private readonly PlayerRoleCheck playerRoleCheck;
    private readonly ScriptingController scriptingController;
    private readonly ScriptingServices scriptingServices;
    private readonly WorldManagementController worldManagementController;

    public AdminChatProcessor(AdminTagCollection admins, ServerChatInternalController internalController,
        PlayerRoleCheck playerRoleCheck, PlayerNameComponentIndexer playerNameIndex,
        NameComponentValidator nameValidator, PersistencePlayerServices persistencePlayerServices,
        LoggingUtil loggingUtil, NameComponentCollection names, WorldManagementController worldManagementController,
        IEventSender eventSender, BlockController blockController, IBlockServices blockServices,
        BlockTemplateNameComponentIndexer blockTemplateNames, EntityTable entityTable,
        IDataController dataController, IDataServices dataServices,
        ILogger<AdminChatProcessor> logger, ScriptingController scriptingController,
        ScriptingServices scriptingServices)
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
        this.entityTable = entityTable;
        this.dataController = dataController;
        this.dataServices = dataServices;
        this.logger = logger;
        this.scriptingController = scriptingController;
        this.scriptingServices = scriptingServices;
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
        new ChatCommand { Command = ReloadEntity, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = ReloadTemplate, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = GetValue, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = SetValue, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = GetEntityValue, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = SetEntityValue, HelpSummary = "", IncludeInHelp = false }
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

            case ReloadEntity:
                OnReloadEntity(message, senderEntityId);
                break;

            case ReloadTemplate:
                OnReloadTemplate(message, senderEntityId);
                break;

            case GetValue:
                OnGetValue(message, senderEntityId);
                break;

            case SetValue:
                OnSetValue(message, senderEntityId);
                break;

            case GetEntityValue:
                OnGetEntityValue(message, senderEntityId);
                break;

            case SetEntityValue:
                OnSetEntityValue(message, senderEntityId);
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
    ///     Handles the /reloadentity command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnReloadEntity(string message, ulong senderEntityId)
    {
        var args = message.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (args.Length != 1)
        {
            logger.LogWarning("{Player} used /reloadentity with bad parameters.",
                loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("Usage: /reloadentity entity_id", senderEntityId);
            return;
        }

        // Parse the hex-encoded entity ID. Short IDs are offsets from the first persisted entity ID.
        var arg = args[0];
        if (!ulong.TryParse(arg, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
        {
            logger.LogWarning("{Player} used /reloadentity with a non-hex entity ID.",
                loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("Entity ID must be hex-encoded.", senderEntityId);
            return;
        }

        var entityId = arg.Length <= 12 ? EntityConstants.FirstPersistedEntityId + parsed : parsed;
        if (!EntityUtil.IsRegularEntity(entityId))
        {
            logger.LogWarning("{Player} tried to reload non-regular entity {EntityId:X16}.",
                loggingUtil.FormatEntity(senderEntityId), entityId);
            internalController.SendSystemMessage("Entity ID must refer to a regular entity.", senderEntityId);
            return;
        }

        if (!entityTable.Exists(entityId))
        {
            logger.LogWarning("{Player} tried to reload entity {EntityId:X16} which is not loaded.",
                loggingUtil.FormatEntity(senderEntityId), entityId);
            internalController.SendSystemMessage("Entity is not currently loaded.", senderEntityId);
            return;
        }

        scriptingController.ReloadEntity(eventSender, entityId);
        internalController.SendSystemMessage($"Entity {entityId:X16} will be reloaded.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /reloadtemplate command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnReloadTemplate(string message, ulong senderEntityId)
    {
        var args = message.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (args.Length != 1)
        {
            logger.LogWarning("{Player} used /reloadtemplate with bad parameters.",
                loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("Usage: /reloadtemplate template_rel_id", senderEntityId);
            return;
        }

        // Parse the decimal relative template entity ID, matching the /addblock convention.
        ulong templateId;
        try
        {
            templateId = EntityConstants.FirstTemplateEntityId + ulong.Parse(args[0]);
        }
        catch (Exception)
        {
            logger.LogWarning("{Player} used /reloadtemplate with a bad template_rel_id.",
                loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("template_rel_id must correspond to a template entity.",
                senderEntityId);
            return;
        }

        if (!EntityUtil.IsTemplateEntity(templateId))
        {
            logger.LogWarning("{Player} tried to use a non-template entity as template entity.",
                loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("template_rel_id must correspond to a template entity.",
                senderEntityId);
            return;
        }

        if (!entityTable.Exists(templateId))
        {
            logger.LogWarning("{Player} tried to reload template {TemplateId:X16} which is not loaded.",
                loggingUtil.FormatEntity(senderEntityId), templateId);
            internalController.SendSystemMessage("Template is not loaded.", senderEntityId);
            return;
        }

        // Snapshot the live instance set so the count reported here matches the reload request.
        var count = entityTable.GetInstancesOfTemplate(templateId).Count;
        if (count == 0)
        {
            logger.LogWarning("{Player} tried to reload template {TemplateId:X16} which has no loaded instances.",
                loggingUtil.FormatEntity(senderEntityId), templateId);
            internalController.SendSystemMessage("No loaded entities have the given template.", senderEntityId);
            return;
        }

        scriptingController.ReloadTemplate(eventSender, templateId);
        internalController.SendSystemMessage(
            count == 1 ? "1 entity will be reloaded." : $"{count} entities will be reloaded.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /getvalue command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnGetValue(string message, ulong senderEntityId)
    {
        var key = message.Trim();
        if (key.Length == 0)
        {
            internalController.SendSystemMessage("Usage: /getvalue key", senderEntityId);
            return;
        }

        if (dataServices.TryGetGlobal(key, out var value))
            internalController.SendSystemMessage(value, senderEntityId);
        else
            internalController.SendSystemMessage("Key not found.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /setvalue command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnSetValue(string message, ulong senderEntityId)
    {
        var args = message.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 0)
        {
            internalController.SendSystemMessage("Usage: /setvalue key [value]", senderEntityId);
            return;
        }

        var key = args[0];
        if (DataKeyConstraints.IsKeyReadOnly(key))
        {
            logger.LogWarning("{Player} tried to set read-only key {Key} via /setvalue.",
                loggingUtil.FormatEntity(senderEntityId), key);
            internalController.SendSystemMessage("Key is read-only.", senderEntityId);
            return;
        }

        if (args.Length == 1)
        {
            dataController.RemoveGlobalSync(key);
            internalController.SendSystemMessage($"Key {key} deleted.", senderEntityId);
            return;
        }

        dataController.SetGlobalSync(key, args[1]);
        internalController.SendSystemMessage($"Key {key} set to {args[1]}.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /getentityvalue command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnGetEntityValue(string message, ulong senderEntityId)
    {
        var args = message.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (args.Length != 2)
        {
            internalController.SendSystemMessage("Usage: /getentityvalue entity_id key", senderEntityId);
            return;
        }

        if (!ValidateEntityForDataCommand(args[0], senderEntityId, out var entityId)) return;

        var key = args[1];
        if (dataServices.TryGetEntityKeyValueLocal(entityId, key, out var value))
        {
            internalController.SendSystemMessage(value, senderEntityId);
            return;
        }

        if (entityTable.TryGetTemplate(entityId, out var templateId) &&
            dataServices.TryGetEntityKeyValueLocal(templateId, key, out value))
        {
            internalController.SendSystemMessage($"{value} (inherited from template {templateId:X16})",
                senderEntityId);
            return;
        }

        internalController.SendSystemMessage("Key not found.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /setentityvalue command.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnSetEntityValue(string message, ulong senderEntityId)
    {
        var args = message.Split(' ', 3, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (args.Length < 2)
        {
            internalController.SendSystemMessage("Usage: /setentityvalue entity_id key [value]", senderEntityId);
            return;
        }

        if (!ValidateEntityForDataCommand(args[0], senderEntityId, out var entityId)) return;

        var key = args[1];
        if (DataKeyConstraints.IsKeyReadOnly(key))
        {
            logger.LogWarning("{Player} tried to set read-only key {Key} on entity {EntityId:X16} via /setentityvalue.",
                loggingUtil.FormatEntity(senderEntityId), key, entityId);
            internalController.SendSystemMessage("Key is read-only.", senderEntityId);
            return;
        }

        if (args.Length == 2)
        {
            dataController.RemoveEntityKeyValueSync(entityId, key);
            internalController.SendSystemMessage($"Key {key} deleted on entity {entityId:X16}.", senderEntityId);
            return;
        }

        dataController.SetEntityKeyValueSync(entityId, key, args[2]);
        internalController.SendSystemMessage($"Key {key} on entity {entityId:X16} set to {args[2]}.", senderEntityId);
    }

    /// <summary>
    ///     Parses a hex-encoded entity ID, interpreting short IDs as offsets from the first persisted
    ///     entity ID.
    /// </summary>
    /// <param name="arg">Hex-encoded entity ID argument.</param>
    /// <param name="entityId">Parsed entity ID, or zero if the method returns false.</param>
    /// <returns>true if the entity ID was parsed, false otherwise.</returns>
    private bool TryParseEntityId(string arg, out ulong entityId)
    {
        if (!ulong.TryParse(arg, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
        {
            entityId = 0;
            return false;
        }

        entityId = arg.Length <= 12 ? EntityConstants.FirstPersistedEntityId + parsed : parsed;
        return true;
    }

    /// <summary>
    ///     Validates that the given entity ID argument refers to a loaded regular entity that is
    ///     eligible for key-value data commands, sending a system message to the sender if not.
    /// </summary>
    /// <param name="arg">Hex-encoded entity ID argument.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    /// <param name="entityId">Parsed entity ID, or zero if the method returns false.</param>
    /// <returns>true if the entity is valid for key-value data commands, false otherwise.</returns>
    private bool ValidateEntityForDataCommand(string arg, ulong senderEntityId, out ulong entityId)
    {
        if (!TryParseEntityId(arg, out entityId))
        {
            internalController.SendSystemMessage("Entity ID must be hex-encoded.", senderEntityId);
            return false;
        }

        if (entityId is >= EntityConstants.FirstBlockEntityId and <= EntityConstants.LastBlockEntityId)
        {
            logger.LogWarning("{Player} used a key-value data command on block entity {EntityId:X16}.",
                loggingUtil.FormatEntity(senderEntityId), entityId);
            internalController.SendSystemMessage("Entity key-value data cannot be used on block entities.",
                senderEntityId);
            return false;
        }

        if (!EntityUtil.IsRegularEntity(entityId))
        {
            logger.LogWarning("{Player} used a key-value data command on non-regular entity {EntityId:X16}.",
                loggingUtil.FormatEntity(senderEntityId), entityId);
            internalController.SendSystemMessage("Entity ID must refer to a regular entity.", senderEntityId);
            return false;
        }

        if (!entityTable.Exists(entityId))
        {
            logger.LogWarning("{Player} used a key-value data command on entity {EntityId:X16} which is not loaded.",
                loggingUtil.FormatEntity(senderEntityId), entityId);
            internalController.SendSystemMessage("Entity is not currently loaded.", senderEntityId);
            return false;
        }

        return true;
    }
}
