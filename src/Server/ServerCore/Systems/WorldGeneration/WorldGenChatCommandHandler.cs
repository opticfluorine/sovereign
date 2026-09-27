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
using System.Globalization;
using Sovereign.EngineCore.Components.Types;
using Sovereign.ServerCore.Systems.ServerChat;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Handles the /worldgen chat command on behalf of the admin chat processor.
/// </summary>
public sealed class WorldGenChatCommandHandler
{
    /// <summary>
    ///     Subcommand to request a new world generation plan.
    /// </summary>
    private const string PlanSubcommand = "plan";

    /// <summary>
    ///     Subcommand to report the current job status.
    /// </summary>
    private const string StatusSubcommand = "status";

    /// <summary>
    ///     Subcommand to preview the pending plan.
    /// </summary>
    private const string PreviewSubcommand = "preview";

    /// <summary>
    ///     Subcommand to commit the pending plan.
    /// </summary>
    private const string CommitSubcommand = "commit";

    /// <summary>
    ///     Subcommand to replace the current world with a new plan.
    /// </summary>
    private const string ReplaceSubcommand = "replace";

    /// <summary>
    ///     Subcommand to abort the current job.
    /// </summary>
    private const string AbortSubcommand = "abort";

    /// <summary>
    ///     Option specifying the world generation profile name.
    /// </summary>
    private const string ProfileOption = "--profile";

    /// <summary>
    ///     Option specifying the world region origin.
    /// </summary>
    private const string AtOption = "--at";

    /// <summary>
    ///     Usage summary for the plan subcommand.
    /// </summary>
    private const string PlanUsage = "Usage: /worldgen plan <seed> [--profile <name>] [--at <x>,<y>]";

    /// <summary>
    ///     Usage summary for the commit subcommand.
    /// </summary>
    private const string CommitUsage = "Usage: /worldgen commit [seed]";

    /// <summary>
    ///     Usage summary for the replace subcommand.
    /// </summary>
    private const string ReplaceUsage = "Usage: /worldgen replace <seed> <seed>";

    /// <summary>
    ///     Usage summary shown when the subcommand cannot be recognized.
    /// </summary>
    private const string GeneralUsage = "Usage: /worldgen <plan|status|preview|commit|replace|abort>";

    private readonly WorldGenerationController controller;
    private readonly ServerChatInternalController internalController;

    public WorldGenChatCommandHandler(WorldGenerationController controller,
        ServerChatInternalController internalController)
    {
        this.controller = controller;
        this.internalController = internalController;
    }

    /// <summary>
    ///     Handles a /worldgen chat command.
    /// </summary>
    /// <param name="message">Remaining message following the command invocation.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    public void Handle(string message, ulong senderEntityId)
    {
        var args = message.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 0)
        {
            SendUsage(GeneralUsage, senderEntityId);
            return;
        }

        switch (args[0].ToLowerInvariant())
        {
            case PlanSubcommand:
                OnPlan(args, senderEntityId);
                break;

            case StatusSubcommand:
                OnStatus(senderEntityId);
                break;

            case PreviewSubcommand:
                OnPreview(senderEntityId);
                break;

            case CommitSubcommand:
                OnCommit(args, senderEntityId);
                break;

            case ReplaceSubcommand:
                OnReplace(args, senderEntityId);
                break;

            case AbortSubcommand:
                OnAbort(senderEntityId);
                break;

            default:
                SendUsage(GeneralUsage, senderEntityId);
                break;
        }
    }

    /// <summary>
    ///     Handles the plan subcommand.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnPlan(string[] args, ulong senderEntityId)
    {
        if (args.Length < 2 || !TryParseSeed(args[1], out var seed))
        {
            SendUsage(PlanUsage, senderEntityId);
            return;
        }

        string? profileName = null;
        GridPosition? origin = null;
        var i = 2;
        while (i < args.Length)
        {
            switch (args[i])
            {
                case ProfileOption when i + 1 < args.Length:
                    profileName = args[i + 1];
                    i += 2;
                    break;

                case AtOption when i + 1 < args.Length && TryParsePoint(args[i + 1], out var x, out var y):
                    origin = new GridPosition { X = x, Y = y };
                    i += 2;
                    break;

                default:
                    SendUsage(PlanUsage, senderEntityId);
                    return;
            }
        }

        try
        {
            controller.Plan(seed, profileName, origin);
        }
        catch (NotImplementedException)
        {
            SendNotImplemented("plan", senderEntityId);
        }
    }

    /// <summary>
    ///     Handles the status subcommand.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnStatus(ulong senderEntityId)
    {
        internalController.SendSystemMessage($"World generation status: {controller.Status()}.", senderEntityId);
    }

    /// <summary>
    ///     Handles the preview subcommand.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnPreview(ulong senderEntityId)
    {
        SendNotImplemented("preview", senderEntityId);
    }

    /// <summary>
    ///     Handles the commit subcommand.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnCommit(string[] args, ulong senderEntityId)
    {
        ulong? seed = null;
        if (args.Length >= 2)
        {
            if (args.Length != 2 || !TryParseSeed(args[1], out var parsedSeed))
            {
                SendUsage(CommitUsage, senderEntityId);
                return;
            }

            seed = parsedSeed;
        }

        try
        {
            controller.Commit(seed);
        }
        catch (NotImplementedException)
        {
            SendNotImplemented("commit", senderEntityId);
        }
    }

    /// <summary>
    ///     Handles the replace subcommand.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnReplace(string[] args, ulong senderEntityId)
    {
        if (args.Length != 3 || !TryParseSeed(args[1], out _) || !TryParseSeed(args[2], out _))
        {
            SendUsage(ReplaceUsage, senderEntityId);
            return;
        }

        SendNotImplemented("replace", senderEntityId);
    }

    /// <summary>
    ///     Handles the abort subcommand.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnAbort(ulong senderEntityId)
    {
        try
        {
            controller.Abort();
        }
        catch (NotImplementedException)
        {
            SendNotImplemented("abort", senderEntityId);
        }
    }

    /// <summary>
    ///     Replies that the given subcommand is not yet implemented.
    /// </summary>
    /// <param name="subcommand">Subcommand name.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void SendNotImplemented(string subcommand, ulong senderEntityId)
    {
        internalController.SendSystemMessage($"Worldgen {subcommand} is not yet implemented.", senderEntityId);
    }

    /// <summary>
    ///     Sends a usage summary to the sender.
    /// </summary>
    /// <param name="usage">Usage summary.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void SendUsage(string usage, ulong senderEntityId)
    {
        internalController.SendSystemMessage(usage, senderEntityId);
    }

    /// <summary>
    ///     Tries to parse a world generation seed.
    /// </summary>
    /// <param name="arg">Seed argument.</param>
    /// <param name="seed">Parsed seed, or zero if the method returns false.</param>
    /// <returns>true if the seed was parsed, false otherwise.</returns>
    private static bool TryParseSeed(string arg, out ulong seed)
    {
        return ulong.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out seed);
    }

    /// <summary>
    ///     Tries to parse a comma-separated world region origin.
    /// </summary>
    /// <param name="arg">Origin argument in "x,y" form.</param>
    /// <param name="x">Parsed X coordinate, or zero if the method returns false.</param>
    /// <param name="y">Parsed Y coordinate, or zero if the method returns false.</param>
    /// <returns>true if the origin was parsed, false otherwise.</returns>
    private static bool TryParsePoint(string arg, out int x, out int y)
    {
        x = 0;
        y = 0;
        var parts = arg.Split(',');
        return parts.Length == 2
            && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out x)
            && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out y);
    }
}
