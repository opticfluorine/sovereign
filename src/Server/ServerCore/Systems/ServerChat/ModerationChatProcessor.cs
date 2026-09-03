// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Components.Validators;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.Timing;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Chat processor that handles moderator-specific moderation commands.
/// </summary>
public class ModerationChatProcessor : IChatProcessor
{
    /// <summary>
    ///     Command name for /muteglobal.
    /// </summary>
    private const string MuteGlobal = "muteglobal";

    /// <summary>
    ///     Command name for /unmuteglobal.
    /// </summary>
    private const string UnmuteGlobal = "unmuteglobal";

    /// <summary>
    ///     Command name for /mute.
    /// </summary>
    private const string Mute = "mute";

    /// <summary>
    ///     Command name for /unmute.
    /// </summary>
    private const string Unmute = "unmute";

    /// <summary>
    ///     Command name for /listmutes.
    /// </summary>
    private const string ListMutes = "listmutes";

    private readonly ILogger<ModerationChatProcessor> logger;
    private readonly LoggingUtil loggingUtil;
    private readonly ModerationStateManager moderationStateManager;
    private readonly NameComponentCollection names;
    private readonly NameComponentValidator nameValidator;
    private readonly PlayerNameComponentIndexer playerNameIndex;
    private readonly PlayerRoleCheck playerRoleCheck;
    private readonly IOptions<ModerationOptions> moderationOptions;
    private readonly ServerChatInternalController internalController;
    private readonly ISystemTimer timer;

    public ModerationChatProcessor(ModerationStateManager moderationStateManager, PlayerRoleCheck playerRoleCheck,
        PlayerNameComponentIndexer playerNameIndex, NameComponentValidator nameValidator,
        NameComponentCollection names, ServerChatInternalController internalController,
        IOptions<ModerationOptions> moderationOptions, LoggingUtil loggingUtil, ISystemTimer timer,
        ILogger<ModerationChatProcessor> logger)
    {
        this.moderationStateManager = moderationStateManager;
        this.playerRoleCheck = playerRoleCheck;
        this.playerNameIndex = playerNameIndex;
        this.nameValidator = nameValidator;
        this.names = names;
        this.internalController = internalController;
        this.moderationOptions = moderationOptions;
        this.loggingUtil = loggingUtil;
        this.timer = timer;
        this.logger = logger;
    }

    public List<ChatCommand> MatchingCommands => new()
    {
        new ChatCommand { Command = MuteGlobal, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = UnmuteGlobal, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = Mute, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = Unmute, HelpSummary = "", IncludeInHelp = false },
        new ChatCommand { Command = ListMutes, HelpSummary = "", IncludeInHelp = false }
    };

    public void ProcessChat(string command, string message, ulong senderEntityId)
    {
        if (!playerRoleCheck.IsPlayerModerator(senderEntityId))
        {
            logger.LogWarning("{Player} attempted to use moderation command {Command} while not a moderator.",
                loggingUtil.FormatEntity(senderEntityId), command);
            internalController.SendSystemMessage("Only moderators may use this command.", senderEntityId);
            return;
        }

        logger.LogInformation("MODERATION COMMAND: {Player}: /{Command} {Message}",
            loggingUtil.FormatEntity(senderEntityId), command, message);

        switch (command)
        {
            case MuteGlobal:
                OnMute(message, senderEntityId, ChatMuteScope.Global);
                break;

            case UnmuteGlobal:
                OnUnmute(message, senderEntityId, ChatMuteScope.Global);
                break;

            case Mute:
                OnMute(message, senderEntityId, ChatMuteScope.All);
                break;

            case Unmute:
                OnUnmute(message, senderEntityId, ChatMuteScope.All);
                break;

            case ListMutes:
                OnListMutes(senderEntityId);
                break;
        }
    }

    /// <summary>
    ///     Handles the /muteglobal and /mute commands.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    /// <param name="scope">Chat scope to mute.</param>
    private void OnMute(string message, ulong senderEntityId, ChatMuteScope scope)
    {
        var args = message.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 0)
        {
            SendMuteUsage(scope, senderEntityId);
            return;
        }

        var playerName = args[0];
        if (!nameValidator.IsValid(playerName))
        {
            internalController.SendSystemMessage("Invalid name.", senderEntityId);
            return;
        }

        var timeoutMinutes = moderationOptions.Value.DefaultMuteTimeoutMinutes;
        if (args.Length == 2 && (!int.TryParse(args[1], out timeoutMinutes) || timeoutMinutes <= 0))
        {
            SendMuteUsage(scope, senderEntityId);
            return;
        }

        if (!playerNameIndex.TryGetPlayerByName(playerName, out var playerEntityId))
        {
            internalController.SendSystemMessage("Player does not exist or is not logged in.", senderEntityId);
            return;
        }

        moderationStateManager.Mute(playerEntityId, scope, TimeSpan.FromMinutes(timeoutMinutes));

        var scopeName = DescribeScope(scope);
        internalController.SendSystemMessage($"You are muted from {scopeName} for {timeoutMinutes} minutes.",
            playerEntityId);
        internalController.SendSystemMessage(
            $"Player {playerName} has been muted from {scopeName} for {timeoutMinutes} minutes.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /unmuteglobal and /unmute commands.
    /// </summary>
    /// <param name="message">Remaining message.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    /// <param name="scope">Chat scope to unmute.</param>
    private void OnUnmute(string message, ulong senderEntityId, ChatMuteScope scope)
    {
        var playerName = message.Trim();
        if (!nameValidator.IsValid(playerName))
        {
            internalController.SendSystemMessage("Invalid name.", senderEntityId);
            return;
        }

        if (!playerNameIndex.TryGetPlayerByName(playerName, out var playerEntityId))
        {
            internalController.SendSystemMessage("Player does not exist or is not logged in.", senderEntityId);
            return;
        }

        if (!moderationStateManager.Unmute(playerEntityId, scope))
        {
            internalController.SendSystemMessage("Player is not muted.", senderEntityId);
            return;
        }

        var scopeName = DescribeScope(scope);
        internalController.SendSystemMessage($"You are no longer muted from {scopeName}.", playerEntityId);
        internalController.SendSystemMessage($"Player {playerName} is no longer muted from {scopeName}.",
            senderEntityId);
    }

    /// <summary>
    ///     Handles the /listmutes command.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnListMutes(ulong senderEntityId)
    {
        var activeMutes = moderationStateManager.GetActiveMutes();
        if (activeMutes.Count == 0)
        {
            internalController.SendSystemMessage("No active mutes.", senderEntityId);
            return;
        }

        var now = timer.GetTime();
        foreach (var mute in activeMutes)
        {
            var name = names.GetComponentForEntity(mute.EntityId, true)
                .OrElseDefault(mute.EntityId.ToString("X"));
            var remaining = FormatRemaining(TimeSpan.FromMicroseconds(
                Math.Max(0L, (long)(mute.ExpiryTime - now))));
            internalController.SendSystemMessage($"{name} - {DescribeScope(mute.Scope)} - {remaining} remaining",
                senderEntityId);
        }
    }

    /// <summary>
    ///     Sends a usage message for the /muteglobal or /mute command.
    /// </summary>
    /// <param name="scope">Mute scope.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void SendMuteUsage(ChatMuteScope scope, ulong senderEntityId)
    {
        var command = scope == ChatMuteScope.Global ? MuteGlobal : Mute;
        internalController.SendSystemMessage($"Usage: /{command} <player> [timeout_minutes]", senderEntityId);
    }

    /// <summary>
    ///     Describes the given mute scope in text suitable for chat messages.
    /// </summary>
    /// <param name="scope">Mute scope.</param>
    /// <returns>Scope description.</returns>
    private static string DescribeScope(ChatMuteScope scope)
    {
        return scope == ChatMuteScope.All ? "all chat" : "global chat";
    }

    /// <summary>
    ///     Formats a remaining mute duration for display.
    /// </summary>
    /// <param name="remaining">Remaining duration.</param>
    /// <returns>Formatted duration.</returns>
    private static string FormatRemaining(TimeSpan remaining)
    {
        var totalSeconds = (long)remaining.TotalSeconds;
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        return $"{minutes}m {seconds}s";
    }
}
