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
using System.Linq;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.EngineCore.World;
using Sovereign.ServerCore.Systems.Persistence;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Chat processor that handles admin-specific world segment flag commands.
/// </summary>
public class SegmentFlagsChatProcessor : IChatProcessor
{
    /// <summary>
    ///     Command name for /segflag.
    /// </summary>
    private const string SegFlag = "segflag";

    /// <summary>
    ///     Subcommand to add a flag.
    /// </summary>
    private const string Add = "add";

    /// <summary>
    ///     Subcommand to remove a flag.
    /// </summary>
    private const string Remove = "remove";

    /// <summary>
    ///     Subcommand to list flags.
    /// </summary>
    private const string List = "list";

    /// <summary>
    ///     Shorthand for the sender's current segment.
    /// </summary>
    private const string Here = "here";

    /// <summary>
    ///     Maximum number of segments a single flag command may affect.
    /// </summary>
    private const int MaximumBoxSegments = 65536;

    /// <summary>
    ///     Fallback minimum Z coordinate when the config value is missing.
    /// </summary>
    private const int DefaultMinimumZ = -1024;

    /// <summary>
    ///     Fallback maximum Z coordinate when the config value is missing.
    /// </summary>
    private const int DefaultMaximumZ = 1024;

    /// <summary>
    ///     Number of bits used to convert a Z coordinate to a segment index.
    /// </summary>
    private const int SegmentShift = 5;

    private readonly IDataServices dataServices;
    private readonly IEventSender eventSender;
    private readonly ServerChatInternalController internalController;
    private readonly KinematicsComponentCollection kinematics;
    private readonly ILogger<SegmentFlagsChatProcessor> logger;
    private readonly LoggingUtil loggingUtil;
    private readonly PersistenceController persistenceController;
    private readonly PlayerRoleCheck playerRoleCheck;
    private readonly SegmentFlagManager segmentFlagManager;
    private readonly WorldSegmentResolver segmentResolver;

    public SegmentFlagsChatProcessor(SegmentFlagManager segmentFlagManager,
        WorldSegmentResolver segmentResolver, KinematicsComponentCollection kinematics,
        IDataServices dataServices, PersistenceController persistenceController,
        ServerChatInternalController internalController, IEventSender eventSender,
        PlayerRoleCheck playerRoleCheck, LoggingUtil loggingUtil,
        ILogger<SegmentFlagsChatProcessor> logger)
    {
        this.segmentFlagManager = segmentFlagManager;
        this.segmentResolver = segmentResolver;
        this.kinematics = kinematics;
        this.dataServices = dataServices;
        this.persistenceController = persistenceController;
        this.internalController = internalController;
        this.eventSender = eventSender;
        this.playerRoleCheck = playerRoleCheck;
        this.loggingUtil = loggingUtil;
        this.logger = logger;
    }

    public List<ChatCommand> MatchingCommands => new()
    {
        new ChatCommand
        {
            Command = SegFlag,
            HelpSummary =
                "Manage world segment flags: /segflag add|remove <range> <flag>, /segflag list [range].",
            IncludeInHelp = true
        }
    };

    public void ProcessChat(string command, string message, ulong senderEntityId)
    {
        // Only admins may use this command.
        if (!playerRoleCheck.IsPlayerAdmin(senderEntityId))
        {
            logger.LogWarning("{Player} attempted to use segment flag command while not an admin.",
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
            case Add:
                OnSetFlag(remainder, senderEntityId, true);
                break;

            case Remove:
                OnSetFlag(remainder, senderEntityId, false);
                break;

            case List:
                OnListFlags(remainder, senderEntityId);
                break;

            default:
                SendUsage(senderEntityId);
                break;
        }
    }

    /// <summary>
    ///     Handles the /segflag add and /segflag remove subcommands.
    /// </summary>
    /// <param name="message">Remaining message after the subcommand.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    /// <param name="set">Whether to set the flag (true) or clear it (false).</param>
    private void OnSetFlag(string message, ulong senderEntityId, bool set)
    {
        var tokens = message.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 2)
        {
            SendUsage(senderEntityId);
            return;
        }

        var flagName = tokens[^1];
        var rangeTokens = tokens[..^1];

        if (!TryParseFlag(flagName, senderEntityId, out var flag)) return;
        if (!TryParseRange(rangeTokens, senderEntityId, out var min, out var max)) return;
        if (!IsBoxSizeOk(min, max, senderEntityId)) return;

        persistenceController.UpdateSegmentFlags(eventSender, min, max, flag, set);

        var action = set ? "set" : "cleared";
        logger.LogInformation("SEGFLAG COMMAND: {Player}: {Action} flag {Flag} over {Min} to {Max}.",
            loggingUtil.FormatEntity(senderEntityId), action, flag, min, max);
        internalController.SendSystemMessage(
            $"Flag {flag} {action} over segments {FormatBox(min, max)}.", senderEntityId);
    }

    /// <summary>
    ///     Handles the /segflag list subcommand.
    /// </summary>
    /// <param name="message">Remaining message after the subcommand.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnListFlags(string message, ulong senderEntityId)
    {
        var tokens = message.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) tokens = new[] { Here };

        if (!TryParseRange(tokens, senderEntityId, out var min, out var max)) return;

        var flagged = segmentFlagManager.GetFlagsInBox(min, max)
            .OrderBy(entry => entry.segment.X)
            .ThenBy(entry => entry.segment.Y)
            .ThenBy(entry => entry.segment.Z)
            .ToList();

        if (flagged.Count == 0)
        {
            internalController.SendSystemMessage($"No flags in segments {FormatBox(min, max)}.", senderEntityId);
            return;
        }

        foreach (var (segment, flags) in flagged)
            internalController.SendSystemMessage($"{segment}: {flags}", senderEntityId);
    }

    /// <summary>
    ///     Parses a flag name, sending a usage message if it is unknown, None, or composite.
    /// </summary>
    /// <param name="name">Flag name.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    /// <param name="flag">Parsed flag. Only valid if the method returns true.</param>
    /// <returns>true if the flag was parsed, false otherwise.</returns>
    private bool TryParseFlag(string name, ulong senderEntityId, out WorldSegmentFlag flag)
    {
        flag = WorldSegmentFlag.None;

        if (name.Contains(',')
            || !Enum.TryParse<WorldSegmentFlag>(name, true, out var parsed)
            || !Enum.IsDefined(typeof(WorldSegmentFlag), parsed)
            || parsed == WorldSegmentFlag.None)
        {
            var validNames = string.Join(", ", Enum.GetNames<WorldSegmentFlag>()
                .Where(n => n != nameof(WorldSegmentFlag.None)));
            internalController.SendSystemMessage($"Unknown flag '{name}'. Valid flags: {validNames}.",
                senderEntityId);
            return false;
        }

        flag = parsed;
        return true;
    }

    /// <summary>
    ///     Parses a segment range in one of the supported forms, sending a usage message on failure.
    /// </summary>
    /// <param name="tokens">Range tokens.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    /// <param name="min">Minimum (inclusive) corner of the box. Only valid if the method returns true.</param>
    /// <param name="max">Maximum (inclusive) corner of the box. Only valid if the method returns true.</param>
    /// <returns>true if the range was parsed, false otherwise.</returns>
    private bool TryParseRange(string[] tokens, ulong senderEntityId, out GridPosition min, out GridPosition max)
    {
        min = default;
        max = default;

        if (tokens.Length == 1 && tokens[0].Equals(Here, StringComparison.OrdinalIgnoreCase))
        {
            if (!kinematics.TryGetValue(senderEntityId, out var senderKinematics))
            {
                internalController.SendSystemMessage("You must be positioned in the world to use 'here'.",
                    senderEntityId);
                return false;
            }

            var segment = segmentResolver.GetWorldSegmentForPosition(senderKinematics.Position);
            min = segment;
            max = segment;
            return true;
        }

        if (tokens.Length == 4)
        {
            if (!TryParseCoordinates(tokens, senderEntityId, out var values)) return false;

            min = new GridPosition(Math.Min(values[0], values[2]), Math.Min(values[1], values[3]),
                GetMinimumSegmentZ());
            max = new GridPosition(Math.Max(values[0], values[2]), Math.Max(values[1], values[3]),
                GetMaximumSegmentZ());
            return true;
        }

        if (tokens.Length == 6)
        {
            if (!TryParseCoordinates(tokens, senderEntityId, out var values)) return false;

            min = new GridPosition(Math.Min(values[0], values[3]), Math.Min(values[1], values[4]),
                Math.Min(values[2], values[5]));
            max = new GridPosition(Math.Max(values[0], values[3]), Math.Max(values[1], values[4]),
                Math.Max(values[2], values[5]));
            return true;
        }

        SendUsage(senderEntityId);
        return false;
    }

    /// <summary>
    ///     Parses a token list into integers, sending an error message on failure.
    /// </summary>
    /// <param name="tokens">Tokens to parse.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    /// <param name="values">Parsed values. Only valid if the method returns true.</param>
    /// <returns>true if all tokens parsed, false otherwise.</returns>
    private bool TryParseCoordinates(string[] tokens, ulong senderEntityId, out int[] values)
    {
        values = new int[tokens.Length];
        for (var i = 0; i < tokens.Length; ++i)
            if (!int.TryParse(tokens[i], out values[i]))
            {
                internalController.SendSystemMessage("Coordinates must be integers.", senderEntityId);
                return false;
            }

        return true;
    }

    /// <summary>
    ///     Checks that the given box does not exceed the maximum allowed segment count.
    /// </summary>
    /// <param name="min">Minimum (inclusive) corner of the box.</param>
    /// <param name="max">Maximum (inclusive) corner of the box.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    /// <returns>true if the box is acceptable, false otherwise.</returns>
    private bool IsBoxSizeOk(GridPosition min, GridPosition max, ulong senderEntityId)
    {
        long sizeX = (long)max.X - min.X + 1;
        long sizeY = (long)max.Y - min.Y + 1;
        long sizeZ = (long)max.Z - min.Z + 1;

        if (sizeX > MaximumBoxSegments || sizeY > MaximumBoxSegments || sizeZ > MaximumBoxSegments
            || sizeX * sizeY * sizeZ > MaximumBoxSegments)
        {
            internalController.SendSystemMessage(
                $"Requested box is too large; at most {MaximumBoxSegments} segments are allowed.",
                senderEntityId);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Gets the minimum world segment Z index covering the configured minimum Z coordinate.
    /// </summary>
    /// <returns>Minimum segment Z index.</returns>
    private int GetMinimumSegmentZ()
    {
        var minZ = dataServices.TryGetGlobalInt(GlobalConfigKeys.MinimumZ, out var value)
            ? value
            : DefaultMinimumZ;
        return minZ >> SegmentShift;
    }

    /// <summary>
    ///     Gets the maximum world segment Z index covering the configured maximum Z coordinate.
    /// </summary>
    /// <returns>Maximum segment Z index.</returns>
    private int GetMaximumSegmentZ()
    {
        var maxZ = dataServices.TryGetGlobalInt(GlobalConfigKeys.MaximumZ, out var value)
            ? value
            : DefaultMaximumZ;
        return maxZ >> SegmentShift;
    }

    /// <summary>
    ///     Sends the command usage message.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void SendUsage(ulong senderEntityId)
    {
        internalController.SendSystemMessage(
            "Usage: /segflag add|remove <x1 y1 [z1] x2 y2 [z2] | here> <flag>, /segflag list [range]",
            senderEntityId);
    }

    /// <summary>
    ///     Formats a box for display.
    /// </summary>
    /// <param name="min">Minimum (inclusive) corner of the box.</param>
    /// <param name="max">Maximum (inclusive) corner of the box.</param>
    /// <returns>Formatted box.</returns>
    private static string FormatBox(GridPosition min, GridPosition max)
    {
        return $"{min} to {max}";
    }
}
