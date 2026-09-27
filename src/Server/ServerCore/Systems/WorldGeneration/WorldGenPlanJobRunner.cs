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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.WorldGen;

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
        WorldGenScratch scratch, ServerChatInternalController chat,
        ILogger<WorldGenPlanJobRunner> logger)
    {
        this.system = system;
        this.services = services;
        this.pipeline = pipeline;
        this.profileLoader = profileLoader;
        this.profileValidator = profileValidator;
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

            var originX = origin?.X ?? 0;
            var originY = origin?.Y ?? 0;
            var previewPath = scratch.ResolvePreviewPath(seed);

            system.SetJobStatus(WorldGenerationJobStatus.Planning, "Starting generation");
            chat.SendSystemMessage(
                $"World generation started: seed {seed}, profile \"{name}\", origin ({originX}, {originY}).",
                senderEntityId);

            Task.Run(() => RunJob(seed, name, profile, originX, originY, previewPath, senderEntityId));
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
    /// <param name="senderEntityId">Entity to reply to.</param>
    private void RunJob(ulong seed, string profileName, WorldGenProfile profile, int originX, int originY,
        string previewPath, ulong senderEntityId)
    {
        try
        {
            var plan = pipeline.Plan(profile, profileName, seed, originX, originY, previewPath,
                phase => system.SetJobStatus(WorldGenerationJobStatus.Planning, phase));
            services.RecordCompletedPlan(plan);
            system.SetJobStatus(WorldGenerationJobStatus.Idle, "Plan complete");
            chat.SendSystemMessage(
                plan.Statistics.Format() + "\nPreview: " + plan.PreviewPath, senderEntityId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "World generation plan failed (seed {Seed}).", seed);
            system.SetJobStatus(WorldGenerationJobStatus.Idle, $"Plan failed: {e.Message}");
            chat.SendSystemMessage($"World generation failed: {e.Message}", senderEntityId);
        }
    }
}
