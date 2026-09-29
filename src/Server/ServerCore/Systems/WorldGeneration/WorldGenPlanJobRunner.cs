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
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.WorldGen;
using Sovereign.WorldGen.Output;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Runs world generation plan jobs against the single job slot. Profile loading and
///     validation happen synchronously on the caller; the pipeline runs on a background task.
///     A failed job always returns the slot to Idle.
/// </summary>
public sealed class WorldGenPlanJobRunner
{
    /// <summary>
    ///     Name of the profile used when none is specified.
    /// </summary>
    private const string DefaultProfileName = "default";

    private readonly WorldGenerationSystem system;
    private readonly WorldGenerationServices services;
    private readonly IWorldGenPipeline pipeline;
    private readonly ProfileLoader profileLoader;
    private readonly ProfileValidator profileValidator;
    private readonly WorldGenTemplateResolver templateResolver;
    private readonly WorldGenScratch scratch;
    private readonly ServerChatInternalController chat;
    private readonly ILogger<WorldGenPlanJobRunner> logger;

    /// <summary>
    ///     Guards the synchronous phase of job startup so that two plans cannot race for the
    ///     job slot.
    /// </summary>
    private readonly object jobLock = new();

    public WorldGenPlanJobRunner(WorldGenerationSystem system, WorldGenerationServices services,
        IWorldGenPipeline pipeline, ProfileLoader profileLoader, ProfileValidator profileValidator,
        WorldGenTemplateResolver templateResolver, WorldGenScratch scratch,
        ServerChatInternalController chat, ILogger<WorldGenPlanJobRunner> logger)
    {
        this.system = system;
        this.services = services;
        this.pipeline = pipeline;
        this.profileLoader = profileLoader;
        this.profileValidator = profileValidator;
        this.templateResolver = templateResolver;
        this.scratch = scratch;
        this.chat = chat;
        this.logger = logger;
    }

    /// <summary>
    ///     Begins a world generation plan job. The job slot must be Idle; profile issues are
    ///     reported synchronously, and the pipeline runs on a background task.
    /// </summary>
    /// <param name="seed">World generation seed.</param>
    /// <param name="profileName">Profile name, or null for the default profile.</param>
    /// <param name="origin">Origin of the generated world region, or null for the default origin.</param>
    /// <param name="senderEntityId">Entity to reply to.</param>
    public void BeginPlan(ulong seed, string? profileName, GridPosition? origin, ulong senderEntityId)
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

            var name = profileName ?? DefaultProfileName;
            system.SetJobStatus(WorldGenerationJobStatus.Planning, $"Loading profile \"{name}\"");

            WorldGenProfile profile;
            try
            {
                profile = profileLoader.Load(name);
            }
            catch (ProfileLoadException e)
            {
                system.SetJobStatus(WorldGenerationJobStatus.Idle, "Profile load failed");
                chat.SendSystemMessage($"Worldgen plan failed: {e.Message}", senderEntityId);
                return;
            }

            if (!ValidateProfile(profile, senderEntityId)) return;

            // Template resolution happens synchronously on the caller against the live
            // template entity set, before any background work starts.
            WorldGenResolvedTemplates resolvedTemplates;
            try
            {
                resolvedTemplates = templateResolver.Resolve(profile, name);
            }
            catch (WorldGenTemplateResolutionException e)
            {
                system.SetJobStatus(WorldGenerationJobStatus.Idle, "Template resolution failed");
                chat.SendSystemMessage($"Worldgen plan failed: {e.Message}", senderEntityId);
                return;
            }

            var originX = origin?.X ?? 0;
            var originY = origin?.Y ?? 0;
            var previewPath = scratch.ResolvePreviewPath(seed);
            var stagingDirectory = scratch.CreateSessionDirectory(seed);

            system.SetJobStatus(WorldGenerationJobStatus.Planning, "Starting generation");
            chat.SendSystemMessage(
                $"World generation started: seed {seed}, profile \"{name}\", origin ({originX}, {originY}).",
                senderEntityId);

            Task.Run(() => RunJob(seed, name, profile, originX, originY, previewPath,
                stagingDirectory, resolvedTemplates, senderEntityId));
        }
    }

    /// <summary>
    ///     Validates a profile, reporting each issue to the sender. Errors abort the job.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="senderEntityId">Entity to reply to.</param>
    /// <returns>true if generation may proceed, false if the profile has errors.</returns>
    private bool ValidateProfile(WorldGenProfile profile, ulong senderEntityId)
    {
        var hasErrors = false;
        foreach (var issue in profileValidator.Validate(profile))
        {
            chat.SendSystemMessage($"Profile {issue.Severity.ToString().ToLowerInvariant()}: {issue.Message}",
                senderEntityId);
            hasErrors |= issue.Severity == ProfileValidationSeverity.Error;
        }

        if (!hasErrors) return true;

        system.SetJobStatus(WorldGenerationJobStatus.Idle, "Profile validation failed");
        chat.SendSystemMessage("Worldgen plan aborted: the profile has errors.", senderEntityId);
        return false;
    }

    /// <summary>
    ///     Runs the plan pipeline on a background task. Any failure is logged, returns the job
    ///     slot to Idle, and replies with a short error line; the slot is never left stuck.
    /// </summary>
    /// <param name="seed">World generation seed.</param>
    /// <param name="profileName">Profile name.</param>
    /// <param name="profile">Validated profile.</param>
    /// <param name="originX">World X coordinate of the footprint origin.</param>
    /// <param name="originY">World Y coordinate of the footprint origin.</param>
    /// <param name="previewPath">Path for the preview image.</param>
    /// <param name="stagingDirectory">Staging directory of the staged plan.</param>
    /// <param name="resolvedTemplates">Profile template names resolved against the live
    ///     template entity set.</param>
    /// <param name="senderEntityId">Entity to reply to.</param>
    private void RunJob(ulong seed, string profileName, WorldGenProfile profile, int originX, int originY,
        string previewPath, string stagingDirectory,
        WorldGenResolvedTemplates resolvedTemplates, ulong senderEntityId)
    {
        try
        {
            var plan = pipeline.Plan(profile, profileName, seed, originX, originY, previewPath,
                stagingDirectory, resolvedTemplates,
                phase => system.SetJobStatus(WorldGenerationJobStatus.Planning, phase));
            WritePlanManifest(plan);

            // Send the completion reply before recording the plan: waiters use the recorded
            // plan as the signal that the job finished, so all completion chat must already
            // be visible (and thread-safely recorded) when LastCompletedPlan becomes set.
            var reply = plan.Statistics.Format() + "\nPreview: " + plan.PreviewPath;
            foreach (var cavePreviewPath in plan.CavePreviewPaths)
            {
                reply += "\nPreview: " + cavePreviewPath;
            }

            chat.SendSystemMessage(reply, senderEntityId);
            system.SetJobStatus(WorldGenerationJobStatus.Idle, "Plan complete");
            services.RecordCompletedPlan(plan);
        }
        catch (Exception e)
        {
            scratch.DeleteSessionDirectory(stagingDirectory);
            logger.LogError(e, "World generation plan failed (seed {Seed}).", seed);
            system.SetJobStatus(WorldGenerationJobStatus.Idle, $"Plan failed: {e.Message}");
            chat.SendSystemMessage($"World generation failed: {e.Message}", senderEntityId);
        }
    }

    /// <summary>
    ///     Writes the staged plan manifest (seed, profile, origin, dimensions, resolved
    ///     template IDs, and statistics) to the staging directory.
    /// </summary>
    /// <param name="plan">Completed plan to describe.</param>
    private static void WritePlanManifest(WorldGenPlan plan)
    {
        var manifest = new StagedPlanManifest
        {
            Seed = plan.Seed,
            ProfileName = plan.ProfileName,
            OriginX = plan.OriginX,
            OriginY = plan.OriginY,
            Width = plan.Profile.Width,
            Height = plan.Profile.Height,
            MinSegmentZ = SegmentAssembler.SegmentZRange(plan.Profile).MinSegmentZ,
            MaxSegmentZ = SegmentAssembler.SegmentZRange(plan.Profile).MaxSegmentZ,
            ResolvedTemplates = plan.ResolvedTemplates.IdsByName,
            Statistics = plan.Statistics.Format(),
            PreviewPath = plan.PreviewPath,
            CavePreviewPaths = plan.CavePreviewPaths
        };

        var path = Path.Combine(plan.StagingDirectory, "plan.json");
        File.WriteAllText(path, JsonSerializer.Serialize(manifest,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            }));
    }

    /// <summary>
    ///     Contents of a staged plan's plan.json manifest.
    /// </summary>
    private sealed class StagedPlanManifest
    {
        /// <summary>
        ///     Root seed of the staged plan.
        /// </summary>
        public ulong Seed { get; init; }

        /// <summary>
        ///     Name of the profile used to generate the plan.
        /// </summary>
        public string ProfileName { get; init; } = "";

        /// <summary>
        ///     World X coordinate of the footprint origin.
        /// </summary>
        public int OriginX { get; init; }

        /// <summary>
        ///     World Y coordinate of the footprint origin.
        /// </summary>
        public int OriginY { get; init; }

        /// <summary>
        ///     Width of the footprint in blocks.
        /// </summary>
        public int Width { get; init; }

        /// <summary>
        ///     Height of the footprint in blocks.
        /// </summary>
        public int Height { get; init; }

        /// <summary>
        ///     Lowest world segment Z index occupied by the plan.
        /// </summary>
        public int MinSegmentZ { get; init; }

        /// <summary>
        ///     Highest world segment Z index occupied by the plan.
        /// </summary>
        public int MaxSegmentZ { get; init; }

        /// <summary>
        ///     Resolved template entity IDs by template name.
        /// </summary>
        public IReadOnlyDictionary<string, ulong> ResolvedTemplates { get; init; }
            = new Dictionary<string, ulong>();

        /// <summary>
        ///     Formatted plan statistics.
        /// </summary>
        public string Statistics { get; init; } = "";

        /// <summary>
        ///     Absolute path of the rendered preview image.
        /// </summary>
        public string PreviewPath { get; init; } = "";

        /// <summary>
        ///     Absolute paths of the rendered cave preview images.
        /// </summary>
        public IReadOnlyList<string> CavePreviewPaths { get; init; } = new List<string>();
    }
}
