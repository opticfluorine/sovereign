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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.WorldGen;
using Sovereign.WorldGen.Output;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Runs world generation commit and replace jobs against the single job slot. Cheap
///     validation and the player interlock run synchronously on the caller; the batched
///     database writes run on a background task. A failed job always returns the slot to
///     Idle, and re-running an interrupted commit is safe by construction.
/// </summary>
public sealed class WorldGenCommitRunner
{
    private readonly WorldGenerationSystem system;
    private readonly WorldGenerationServices services;
    private readonly WorldGenScratch scratch;
    private readonly IWorldGenWorldRegistryStore registryStore;
    private readonly IWorldGenCommitWriter writer;
    private readonly ISegmentSubscriptionProbe subscriptionProbe;
    private readonly WorldManagementController worldController;
    private readonly IEventSender eventSender;
    private readonly ServerChatInternalController chat;
    private readonly WorldGenOptions options;
    private readonly ILogger<WorldGenCommitRunner> logger;

    private readonly object jobLock = new();

    public WorldGenCommitRunner(WorldGenerationSystem system, WorldGenerationServices services,
        WorldGenScratch scratch, IWorldGenWorldRegistryStore registryStore,
        IWorldGenCommitWriter writer, ISegmentSubscriptionProbe subscriptionProbe,
        WorldManagementController worldController, IEventSender eventSender,
        ServerChatInternalController chat, IOptions<WorldGenOptions> options,
        ILogger<WorldGenCommitRunner> logger)
    {
        this.system = system;
        this.services = services;
        this.scratch = scratch;
        this.registryStore = registryStore;
        this.writer = writer;
        this.subscriptionProbe = subscriptionProbe;
        this.worldController = worldController;
        this.eventSender = eventSender;
        this.chat = chat;
        this.options = options.Value;
        this.logger = logger;
    }

    /// <summary>
    ///     Begins committing the staged plan.
    /// </summary>
    /// <param name="seed">Expected seed of the staged plan, or null to skip the check.</param>
    /// <param name="force">Whether to proceed despite subscribed players.</param>
    /// <param name="senderEntityId">Entity ID of the committing player.</param>
    public void BeginCommit(ulong? seed, bool force, ulong senderEntityId)
    {
        var (plan, contained) = FindStagedPlan(seed, senderEntityId);
        if (plan is null) return;

        if (contained is not null)
        {
            Refuse(senderEntityId,
                $"Commit refused: the footprint fully contains registered world {contained.Seed} " +
                $"at ({contained.OriginX}, {contained.OriginY}). Use /worldgen replace {plan.Seed} " +
                $"{contained.Seed} instead.");
            return;
        }

        BeginWrite(plan, null, force, senderEntityId, "commit");
    }

    /// <summary>
    ///     Begins committing the staged plan, replacing a registered world.
    /// </summary>
    /// <param name="stagedSeed">Expected seed of the staged plan.</param>
    /// <param name="oldSeed">Seed of the registered world to replace.</param>
    /// <param name="force">Whether to proceed despite subscribed players.</param>
    /// <param name="senderEntityId">Entity ID of the committing player.</param>
    public void BeginReplace(ulong stagedSeed, ulong oldSeed, bool force, ulong senderEntityId)
    {
        var plan = services.LastCompletedPlan;
        if (plan is null || !Directory.Exists(plan.StagingDirectory))
        {
            Refuse(senderEntityId, "No staged world generation plan to commit; use /worldgen plan first.");
            return;
        }

        if (plan.Seed != stagedSeed)
        {
            Refuse(senderEntityId,
                $"No staged plan with seed {stagedSeed}; the staged plan has seed {plan.Seed}.");
            return;
        }

        var old = registryStore.LoadWorlds().FirstOrDefault(entry => entry.Seed == oldSeed);
        if (old is null)
        {
            Refuse(senderEntityId, $"No registered world with seed {oldSeed}.");
            return;
        }

        var relation = WorldGenWorldRegistry.Classify(old, plan.OriginX, plan.OriginY,
            plan.Profile.Width, plan.Profile.Height);
        if (relation != WorldGenFootprintRelation.Contains)
        {
            Refuse(senderEntityId,
                $"Replace refused: the staged footprint does not fully contain registered " +
                $"world {oldSeed} at ({old.OriginX}, {old.OriginY}).");
            return;
        }

        BeginWrite(plan, old, force, senderEntityId, "replace");
    }

    /// <summary>
    ///     Validates the staged plan, the registry, and the player interlock, then starts
    ///     the background write.
    /// </summary>
    /// <param name="plan">Staged plan to write.</param>
    /// <param name="replacedWorld">Registered world to replace, or null.</param>
    /// <param name="force">Whether to proceed despite subscribed players.</param>
    /// <param name="senderEntityId">Entity ID of the committing player.</param>
    /// <param name="verb">Command verb for progress replies.</param>
    private void BeginWrite(WorldGenPlan plan, WorldGenRegistryEntry? replacedWorld, bool force,
        ulong senderEntityId, string verb)
    {
        lock (jobLock)
        {
            if (system.JobStatus != WorldGenerationJobStatus.Idle)
            {
                chat.SendSystemMessage(
                    "A world generation job is already running; use /worldgen status for progress.",
                    senderEntityId);
                return;
            }

            var width = plan.Profile.Width;
            var height = plan.Profile.Height;
            var (minSegmentZ, maxSegmentZ) = SegmentAssembler.SegmentZRange(plan.Profile);

            // Player interlock: refuse while any player is subscribed to a segment inside
            // the affected footprint, unless --force was given.
            var (subscriberCount, exampleSegment) = CountFootprintSubscribers(
                plan.OriginX, plan.OriginY, width, height, minSegmentZ, maxSegmentZ);
            if (subscriberCount > 0 && !force)
            {
                Refuse(senderEntityId,
                    $"Commit refused: {subscriberCount} player(s) are subscribed to segments " +
                    $"inside the footprint (example: segment ({exampleSegment.X}, " +
                    $"{exampleSegment.Y}, {exampleSegment.Z})). Use --force to proceed.");
                return;
            }

            var replaceNote = replacedWorld is not null
                ? $" Replacing world {replacedWorld.Seed}."
                : "";
            var forceNote = subscriberCount > 0
                ? $" {subscriberCount} subscribed player(s) inside the footprint were overridden."
                : "";
            system.BeginJob(WorldGenerationJobStatus.Committing, "commit starting");
            chat.SendSystemMessage(
                $"World generation {verb} started: seed {plan.Seed}, footprint {width}x{height} " +
                $"at ({plan.OriginX}, {plan.OriginY}).{replaceNote}{forceNote}", senderEntityId);

            var token = system.JobCancellationToken;
            Task.Run(() => RunCommit(plan, replacedWorld, senderEntityId, token));
        }
    }

    /// <summary>
    ///     Runs the commit on a background task: batched writes, registry append,
    ///     segment invalidation, and the final reply.
    /// </summary>
    /// <param name="plan">Staged plan to write.</param>
    /// <param name="replacedWorld">Registered world to replace, or null.</param>
    /// <param name="senderEntityId">Entity ID of the committing player.</param>
    private void RunCommit(WorldGenPlan plan, WorldGenRegistryEntry? replacedWorld,
        ulong senderEntityId, CancellationToken token)
    {
        try
        {
            var progressChat = new ProgressChat(system, chat, senderEntityId);
            var stats = writer.Execute(new WorldGenCommitRequest
            {
                StagingDirectory = plan.StagingDirectory,
                BatchSize = options.CommitBatchSize,
                ReplaceWorld = replacedWorld,
                Progress = progressChat.OnBatchProgress,
                Phase = progressChat.OnPhase,
                CancellationToken = token
            });

            registryStore.AppendWorld(new WorldGenRegistryEntry
            {
                Seed = plan.Seed,
                Profile = plan.ProfileName,
                OriginX = plan.OriginX,
                OriginY = plan.OriginY,
                Width = plan.Profile.Width,
                Height = plan.Profile.Height,
                MinSegmentZ = SegmentAssembler.SegmentZRange(plan.Profile).MinSegmentZ,
                MaxSegmentZ = SegmentAssembler.SegmentZRange(plan.Profile).MaxSegmentZ,
                CommittedAtUtc = DateTime.UtcNow
            });

            worldController.UnloadWorldSegments(eventSender, FootprintSegments(plan).ToList());
            scratch.DeleteSessionDirectory(plan.StagingDirectory);

            system.EndJob("Commit complete");
            chat.SendSystemMessage(
                $"World generation committed: seed {plan.Seed}; {stats.SegmentsWritten} " +
                $"segments written, {stats.DecorationsCreated} decorations created" +
                (stats.DecorationsDeleted > 0
                    ? $", {stats.DecorationsDeleted} replaced decorations deleted"
                    : "") +
                $" in {stats.WallMs / 1000.0:F1} s.", senderEntityId);
        }
        catch (OperationCanceledException)
        {
            var phase = system.LastStatusMessage ?? "commit";
            logger.LogInformation("World generation commit aborted at {Phase} (seed {Seed}).",
                phase, plan.Seed);
            system.EndJob($"Commit aborted at {phase}");
            chat.SendSystemMessage(
                $"World generation commit aborted at {phase}; the batched writes are " +
                "idempotent, so re-run /worldgen commit to finish.", senderEntityId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "World generation commit failed (seed {Seed}).", plan.Seed);
            system.EndJob($"Commit failed: {e.Message}");
            chat.SendSystemMessage($"World generation commit failed: {e.Message}", senderEntityId);
        }
    }

    /// <summary>
    ///     Finds the staged plan to commit, rejecting the request when no intact staged
    ///     plan exists or the seed does not match.
    /// </summary>
    /// <param name="seed">Expected seed, or null to skip the check.</param>
    /// <param name="senderEntityId">Entity ID of the committing player.</param>
    /// <returns>The staged plan, or null if the request was refused, and the contained
    ///     registered world requiring a replace, or null.</returns>
    private (WorldGenPlan? Plan, WorldGenRegistryEntry? Contained) FindStagedPlan(
        ulong? seed, ulong senderEntityId)
    {
        lock (jobLock)
        {
            if (system.JobStatus != WorldGenerationJobStatus.Idle)
            {
                chat.SendSystemMessage(
                    "A world generation job is already running; use /worldgen status for progress.",
                    senderEntityId);
                return (Plan: (WorldGenPlan?)null, Contained: (WorldGenRegistryEntry?)null);
            }

            var staged = services.LastCompletedPlan;
            if (staged is null || !Directory.Exists(staged.StagingDirectory))
            {
                Refuse(senderEntityId,
                    "No staged world generation plan to commit; use /worldgen plan first.");
                return (Plan: (WorldGenPlan?)null, Contained: (WorldGenRegistryEntry?)null);
            }

            if (seed is not null && seed != staged.Seed)
            {
                Refuse(senderEntityId,
                    $"No staged plan with seed {seed}; the staged plan has seed {staged.Seed}.");
                return (Plan: (WorldGenPlan?)null, Contained: (WorldGenRegistryEntry?)null);
            }

            // Registry check: a straddling footprint is refused outright; a contained
            // registered world must be replaced explicitly.
            var width = staged.Profile.Width;
            var height = staged.Profile.Height;
            foreach (var world in registryStore.LoadWorlds())
            {
                var relation = WorldGenWorldRegistry.Classify(world, staged.OriginX,
                    staged.OriginY, width, height);
                switch (relation)
                {
                    case WorldGenFootprintRelation.Straddles:
                        Refuse(senderEntityId,
                            $"Commit refused: the footprint straddles registered world " +
                            $"{world.Seed} at ({world.OriginX}, {world.OriginY}).");
                        return (Plan: (WorldGenPlan?)null, Contained: (WorldGenRegistryEntry?)null);
                    case WorldGenFootprintRelation.Contains:
                        return (staged, world);
                }
            }

            return (staged, null);
        }
    }

    /// <summary>
    ///     Sends a refusal reply and resets the job slot to Idle.
    /// </summary>
    /// <param name="senderEntityId">Entity ID of the committing player.</param>
    /// <param name="message">Refusal reason.</param>
    private void Refuse(ulong senderEntityId, string message)
    {
        system.EndJob("Commit refused");
        chat.SendSystemMessage(message, senderEntityId);
    }

    /// <summary>
    ///     Counts the distinct players subscribed to any segment inside the footprint.
    /// </summary>
    /// <param name="originX">World X coordinate of the footprint origin.</param>
    /// <param name="originY">World Y coordinate of the footprint origin.</param>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <param name="minSegmentZ">Lowest footprint segment Z index.</param>
    /// <param name="maxSegmentZ">Highest footprint segment Z index.</param>
    /// <returns>Distinct subscriber count and an example subscribed segment.</returns>
    private (int Count, GridPosition Example) CountFootprintSubscribers(int originX, int originY,
        int width, int height, int minSegmentZ, int maxSegmentZ)
    {
        var players = new HashSet<ulong>();
        var example = GridPosition.Zero;
        foreach (var segment in FootprintSegments(originX, originY, width, height,
                     minSegmentZ, maxSegmentZ))
        {
            foreach (var player in subscriptionProbe.GetSubscribersForWorldSegment(segment))
            {
                if (players.Add(player) && players.Count == 1) example = segment;
            }
        }

        return (players.Count, example);
    }

    /// <summary>
    ///     Enumerates every world segment of a plan footprint.
    /// </summary>
    /// <param name="plan">Plan whose footprint to enumerate.</param>
    /// <returns>Segment indices.</returns>
    private static IEnumerable<GridPosition> FootprintSegments(WorldGenPlan plan)
    {
        var (minSegmentZ, maxSegmentZ) = SegmentAssembler.SegmentZRange(plan.Profile);
        return FootprintSegments(plan.OriginX, plan.OriginY, plan.Profile.Width,
            plan.Profile.Height, minSegmentZ, maxSegmentZ);
    }

    /// <summary>
    ///     Enumerates every world segment of a footprint box.
    /// </summary>
    /// <param name="originX">World X coordinate of the footprint origin.</param>
    /// <param name="originY">World Y coordinate of the footprint origin.</param>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <param name="minSegmentZ">Lowest footprint segment Z index.</param>
    /// <param name="maxSegmentZ">Highest footprint segment Z index.</param>
    /// <returns>Segment indices in scan order.</returns>
    private static IEnumerable<GridPosition> FootprintSegments(int originX, int originY,
        int width, int height, int minSegmentZ, int maxSegmentZ)
    {
        for (var sz = minSegmentZ; sz <= maxSegmentZ; ++sz)
        for (var sy = originY >> 5; sy <= (originY + height - 1) >> 5; ++sy)
        for (var sx = originX >> 5; sx <= (originX + width - 1) >> 5; ++sx)
        {
            yield return new GridPosition { X = sx, Y = sy, Z = sz };
        }
    }

    /// <summary>
    ///     Reports commit progress to the job status and, at the 25/50/75% milestones, to
    ///     chat. Status messages carry the current write phase.
    /// </summary>
    private sealed class ProgressChat
    {
        private readonly WorldGenerationSystem system;
        private readonly ServerChatInternalController chat;
        private readonly ulong senderEntityId;
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private string phase = "starting";
        private int lastMilestone;

        public ProgressChat(WorldGenerationSystem system, ServerChatInternalController chat,
            ulong senderEntityId)
        {
            this.system = system;
            this.chat = chat;
            this.senderEntityId = senderEntityId;
        }

        /// <summary>
        ///     Called when the writer begins a new write phase.
        /// </summary>
        /// <param name="phase">Short phase label.</param>
        public void OnPhase(string phase)
        {
            this.phase = phase;
            system.SetJobStatus(WorldGenerationJobStatus.Committing, $"commit {phase}");
        }

        /// <summary>
        ///     Called after each committed batch.
        /// </summary>
        /// <param name="completedBatches">Completed batch count.</param>
        /// <param name="totalBatches">Total batch count.</param>
        public void OnBatchProgress(int completedBatches, int totalBatches)
        {
            if (totalBatches <= 0) return;

            var percent = completedBatches * 100 / totalBatches;
            system.SetJobStatus(WorldGenerationJobStatus.Committing,
                $"commit {percent}% ({phase})");
            var milestone = percent / 25;
            if (milestone > lastMilestone && percent < 100)
            {
                lastMilestone = milestone;
                chat.SendSystemMessage(
                    $"World generation commit {percent}% complete ({phase}) " +
                    $"({clock.ElapsedMilliseconds / 1000.0:F1} s).", senderEntityId);
            }
        }
    }
}
