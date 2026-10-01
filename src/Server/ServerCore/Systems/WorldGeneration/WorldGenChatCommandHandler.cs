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
    ///     Option forcing a commit despite subscribed players.
    /// </summary>
    private const string ForceOption = "--force";

    /// <summary>
    ///     Usage summary for the plan subcommand.
    /// </summary>
    private const string PlanUsage = "Usage: /worldgen plan <seed> [--profile <name>] [--at <x>,<y>]";

    /// <summary>
    ///     Usage summary for the commit subcommand.
    /// </summary>
    private const string CommitUsage = "Usage: /worldgen commit [seed] [--force]";

    /// <summary>
    ///     Usage summary for the replace subcommand.
    /// </summary>
    private const string ReplaceUsage = "Usage: /worldgen replace <stagedSeed> <oldSeed> [--force]";

    /// <summary>
    ///     Usage summary shown when the subcommand cannot be recognized.
    /// </summary>
    private const string GeneralUsage = "Usage: /worldgen <plan|status|preview|commit|replace|abort>";

    private readonly WorldGenerationController controller;
    private readonly WorldGenerationServices services;
    private readonly ServerChatInternalController internalController;

    public WorldGenChatCommandHandler(WorldGenerationController controller,
        WorldGenerationServices services, ServerChatInternalController internalController)
    {
        this.controller = controller;
        this.services = services;
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

        controller.Plan(seed, profileName, origin, senderEntityId);
    }

    /// <summary>
    ///     Handles the status subcommand.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnStatus(ulong senderEntityId)
    {
        var message = services.LastStatusMessage is null
            ? $"World generation status: {services.JobStatus}."
            : $"World generation status: {services.JobStatus} ({services.LastStatusMessage}).";
        internalController.SendSystemMessage(message, senderEntityId);
    }

    /// <summary>
    ///     Handles the preview subcommand by re-reporting the last completed plan's preview.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnPreview(ulong senderEntityId)
    {
        var plan = services.LastCompletedPlan;
        internalController.SendSystemMessage(plan is null
            ? "No world generation plan has completed this server session."
            : $"Last plan preview (seed {plan.Seed}): {plan.PreviewPath}", senderEntityId);
    }

    /// <summary>
    ///     Handles the commit subcommand.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnCommit(string[] args, ulong senderEntityId)
    {
        ulong? seed = null;
        var force = false;

        var i = 1;
        while (i < args.Length)
        {
            if (args[i] == ForceOption)
            {
                force = true;
                ++i;
            }
            else if (i == 1 && TryParseSeed(args[i], out var parsedSeed))
            {
                seed = parsedSeed;
                ++i;
            }
            else
            {
                SendUsage(CommitUsage, senderEntityId);
                return;
            }
        }

        controller.Commit(seed, force, senderEntityId);
    }

    /// <summary>
    ///     Handles the replace subcommand.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnReplace(string[] args, ulong senderEntityId)
    {
        var force = false;
        if (args.Length < 3 || !TryParseSeed(args[1], out var stagedSeed)
            || !TryParseSeed(args[2], out var oldSeed))
        {
            SendUsage(ReplaceUsage, senderEntityId);
            return;
        }

        for (var i = 3; i < args.Length; ++i)
        {
            if (args[i] != ForceOption)
            {
                SendUsage(ReplaceUsage, senderEntityId);
                return;
            }

            force = true;
        }

        controller.Replace(stagedSeed, oldSeed, force, senderEntityId);
    }

    /// <summary>
    ///     Handles the abort subcommand.
    /// </summary>
    /// <param name="senderEntityId">Sender entity ID.</param>
    private void OnAbort(ulong senderEntityId)
    {
        var reply = controller.Abort() switch
        {
            WorldGenAbortOutcome.Requested =>
                "Cancellation requested; the job will stop at its next checkpoint.",
            WorldGenAbortOutcome.AlreadyRequested =>
                "Cancellation was already requested; waiting for the job to unwind.",
            _ => "No world generation job is running; it may have already completed."
        };
        internalController.SendSystemMessage(reply, senderEntityId);
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
