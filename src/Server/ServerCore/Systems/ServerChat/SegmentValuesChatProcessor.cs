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
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Chat processor that handles admin-specific world segment key-value commands.
/// </summary>
public class SegmentValuesChatProcessor : IChatProcessor
{
    /// <summary>
    ///     Command name for /segvalue.
    /// </summary>
    private const string SegValue = "segvalue";

    /// <summary>
    ///     Subcommand to read a key.
    /// </summary>
    private const string Get = "get";

    /// <summary>
    ///     Subcommand to write a key.
    /// </summary>
    private const string Set = "set";

    /// <summary>
    ///     Subcommand to remove a key.
    /// </summary>
    private const string Clear = "clear";

    /// <summary>
    ///     Number of tokens before the key: x, y, z.
    /// </summary>
    private const int CoordinateCount = 3;

    /// <summary>
    ///     Number of tokens before the value in a set subcommand.
    /// </summary>
    private const int SetValueIndex = 4;

    private readonly ISegmentKeyValueController controller;
    private readonly IDataServices dataServices;
    private readonly ServerChatInternalController internalController;
    private readonly ILogger<SegmentValuesChatProcessor> logger;
    private readonly LoggingUtil loggingUtil;
    private readonly PlayerRoleCheck playerRoleCheck;

    public SegmentValuesChatProcessor(ISegmentKeyValueController controller, IDataServices dataServices,
        ServerChatInternalController internalController, PlayerRoleCheck playerRoleCheck,
        LoggingUtil loggingUtil, ILogger<SegmentValuesChatProcessor> logger)
    {
        this.controller = controller;
        this.dataServices = dataServices;
        this.internalController = internalController;
        this.playerRoleCheck = playerRoleCheck;
        this.loggingUtil = loggingUtil;
        this.logger = logger;
    }

    public List<ChatCommand> MatchingCommands => new()
    {
        new ChatCommand
        {
            Command = SegValue,
            HelpSummary =
                "Manage world segment key-value data: /segvalue get|set|clear <x> <y> <z> <key> [value [type]].",
            IncludeInHelp = false
        }
    };

    public void ProcessChat(string command, string message, ulong senderEntityId)
    {
        // Only admins may use this command.
        if (!playerRoleCheck.IsPlayerAdmin(senderEntityId))
        {
            logger.LogWarning("{Player} attempted to use segment value command while not an admin.",
                loggingUtil.FormatEntity(senderEntityId));
            internalController.SendSystemMessage("Only admins may use this command.", senderEntityId);
            return;
        }

        var parts = message.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            SendUsage(senderEntityId);
            return;
        }

        var subcommand = parts[0].ToLowerInvariant();
        var remainder = parts.Length > 1 ? parts[1] : "";

        switch (subcommand)
        {
            case Get:
                OnGet(remainder, senderEntityId);
                break;

            case Set:
                OnSet(remainder, senderEntityId);
                break;

            case Clear:
                OnClear(remainder, senderEntityId);
                break;

            default:
                SendUsage(senderEntityId);
                break;
        }
    }

    /// <summary>
    ///     Handles the /segvalue get subcommand.
    /// </summary>
    /// <param name="message">Remaining message after the subcommand.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnGet(string message, ulong senderEntityId)
    {
        var tokens = SplitTokens(message);
        if (tokens.Length != SetValueIndex)
        {
            SendUsage(senderEntityId);
            return;
        }

        if (!TryParseSegmentAndKey(tokens, senderEntityId, out var segment, out var key)) return;

        if (controller.TryGetValue(segment, key, out var value))
            internalController.SendSystemMessage($"{value.Type} {value}", senderEntityId);
        else
            internalController.SendSystemMessage($"{segment} key '{key}' is not set.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /segvalue set subcommand.
    /// </summary>
    /// <param name="message">Remaining message after the subcommand.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnSet(string message, ulong senderEntityId)
    {
        var tokens = SplitTokens(message);
        if (tokens.Length < SetValueIndex + 1 || tokens.Length > SetValueIndex + 2)
        {
            SendUsage(senderEntityId);
            return;
        }

        if (!TryParseSegmentAndKey(tokens, senderEntityId, out var segment, out var key)) return;

        if (dataServices.IsKeyReadOnly(key))
        {
            internalController.SendSystemMessage($"Key '{key}' is reserved and cannot be modified.", senderEntityId);
            return;
        }

        if (!DataCommandArgs.TryParseSetValue(tokens, SetValueIndex, out var value))
        {
            SendUsage(senderEntityId);
            return;
        }

        controller.SetValue(segment, key, value);

        logger.LogInformation("SEGVALUE COMMAND: {Player}: set {Segment} key {Key} to {Type} {Value}.",
            loggingUtil.FormatEntity(senderEntityId), segment, key, value.Type, value);
        internalController.SendSystemMessage($"{segment} key '{key}' set to {value.Type} {value}.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /segvalue clear subcommand.
    /// </summary>
    /// <param name="message">Remaining message after the subcommand.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnClear(string message, ulong senderEntityId)
    {
        var tokens = SplitTokens(message);
        if (tokens.Length != SetValueIndex)
        {
            SendUsage(senderEntityId);
            return;
        }

        if (!TryParseSegmentAndKey(tokens, senderEntityId, out var segment, out var key)) return;

        if (dataServices.IsKeyReadOnly(key))
        {
            internalController.SendSystemMessage($"Key '{key}' is reserved and cannot be modified.", senderEntityId);
            return;
        }

        controller.RemoveKey(segment, key);

        logger.LogInformation("SEGVALUE COMMAND: {Player}: cleared {Segment} key {Key}.",
            loggingUtil.FormatEntity(senderEntityId), segment, key);
        internalController.SendSystemMessage($"{segment} key '{key}' cleared.", senderEntityId);
    }

    /// <summary>
    ///     Parses the x, y, z segment indices and the key from the leading tokens.
    /// </summary>
    /// <param name="tokens">Command tokens.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    /// <param name="segment">Parsed segment. Only valid if the method returns true.</param>
    /// <param name="key">Parsed key. Only valid if the method returns true.</param>
    /// <returns>true if the segment and key were parsed, false otherwise.</returns>
    private bool TryParseSegmentAndKey(string[] tokens, ulong senderEntityId, out GridPosition segment,
        out string key)
    {
        segment = default;
        key = "";

        if (tokens.Length < CoordinateCount + 1)
        {
            SendUsage(senderEntityId);
            return false;
        }

        if (!int.TryParse(tokens[0], out var x) || !int.TryParse(tokens[1], out var y)
                                                || !int.TryParse(tokens[2], out var z))
        {
            internalController.SendSystemMessage("Coordinates must be integers.", senderEntityId);
            return false;
        }

        segment = new GridPosition(x, y, z);
        key = tokens[CoordinateCount];
        return true;
    }

    /// <summary>
    ///     Splits a message into whitespace-separated tokens.
    /// </summary>
    /// <param name="message">Message to split.</param>
    /// <returns>Tokens.</returns>
    private static string[] SplitTokens(string message)
    {
        return message.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    ///     Sends the command usage message.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void SendUsage(ulong senderEntityId)
    {
        internalController.SendSystemMessage(
            "Usage: /segvalue get|set|clear <x> <y> <z> <key> [value [type]]", senderEntityId);
    }
}
