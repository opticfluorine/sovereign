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
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.WorldGen;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests for the /worldgen abort flow: plan aborts settle to Idle with the staging
///     directory cleaned, commit aborts leave a consistent database and re-commit cleanly,
///     and abort-vs-completion races never wedge the job slot.
/// </summary>
public class TestWorldGenAbort
{
    /// <summary>
    ///     Sender entity ID used in tests.
    /// </summary>
    private const ulong SenderEntityId = 0x1234;

    [Fact]
    public void AbortPlan_MidPipeline_SettlesIdleCleansStagingAndReports()
    {
        var pipeline = new CancellableStubPipeline();
        var (controller, system, services, sender, jobSender, _, _) = CreateController(pipeline);

        controller.Plan(jobSender, 12345, "default", null, SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);
        WaitUntil(() => pipeline.Entered.IsSet, "pipeline start");

        controller.Abort(jobSender);
        WorldGenFixture.Pump(system, jobSender);
        Assert.Equal(WorldGenerationJobStatus.Cancelling, services.JobStatus);

        pipeline.Release.Set();
        WaitUntil(() => services.JobStatus == WorldGenerationJobStatus.Idle, "abort settle");

        Assert.Equal(WorldGenerationJobStatus.Idle, services.JobStatus);
        Assert.Contains("Plan aborted at", services.LastStatusMessage);
        Assert.NotNull(pipeline.StagingDirectorySeen);
        Assert.False(
            Directory.Exists(Path.Combine(pipeline.StagingDirectorySeen!, "segments")),
            "An aborted plan's staged segments must be cleaned up by the runner.");
        Assert.Contains(SentMessages(sender), m => m.Contains("aborted at"));

        // The slot is usable again immediately after the abort settles.
        controller.Plan(jobSender, 777, "default", null, SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);
        WaitUntil(() => services.LastCompletedPlan?.Seed == 777UL, "next plan");
        Assert.Equal(WorldGenerationJobStatus.Idle, services.JobStatus);
    }

    [Fact]
    public void AbortCommit_MidBatch_SettlesIdleAndReCommitCompletes()
    {
        var writer = new GatedCancellableCommitWriter();
        var (controller, system, services, sender, jobSender, registry, commitWriter) =
            CreateController(new StubWorldGenPipeline(), writer);

        // Stage a plan first; the slot must have settled to Idle before the commit is
        // offered, since the plan task records the plan just after its EndJob call.
        controller.Plan(jobSender, 555, "default", null, SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);
        WaitUntil(() => services.LastCompletedPlan is not null
            && services.JobStatus == WorldGenerationJobStatus.Idle, "plan completion");

        controller.Commit(jobSender, 555, false, SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);
        WaitUntil(() => writer.Entered.IsSet, "commit start");
        controller.Abort(jobSender);
        WorldGenFixture.Pump(system, jobSender);

        writer.Release.Set();
        WaitUntil(() => services.JobStatus == WorldGenerationJobStatus.Idle, "commit abort settle");

        Assert.Equal(WorldGenerationJobStatus.Idle, services.JobStatus);
        Assert.Contains("Commit aborted at", services.LastStatusMessage);
        Assert.Contains(SentMessages(sender),
            m => m.Contains("commit aborted") && m.Contains("idempotent"));
        Assert.Empty(registry.Worlds);
        Assert.True(Directory.Exists(services.LastCompletedPlan!.StagingDirectory),
            "An aborted commit keeps the staged plan for a re-commit.");

        // Re-commit completes and registers the world.
        writer.Release.Set();
        controller.Commit(jobSender, 555, false, SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);
        WaitUntil(() => registry.Worlds.Count == 1
            && services.JobStatus == WorldGenerationJobStatus.Idle, "re-commit");
        Assert.Equal(WorldGenerationJobStatus.Idle, services.JobStatus);
        Assert.Contains(SentMessages(sender), m => m.Contains("World generation committed"));
    }

    [Fact]
    public void Abort_WhenIdle_ReportsNoJob()
    {
        var gate = new ManualResetEventSlim(false);
        var (handler, sender, jobSender, system, _, _) = CreateHandler(gate);

        handler.Handle("abort", SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);

        Assert.Contains(SentMessages(sender),
            m => m.Contains("No world generation job is running"));
        gate.Set();
    }

    [Fact]
    public void Abort_CompletingJob_IsCoherentAndNeverStuck()
    {
        // A pipeline that finishes before observing the token: the abort request races the
        // completion. Either the job completes or aborts; either way the slot ends Idle and
        // never wedges.
        for (var i = 0; i < 20; ++i)
        {
            var pipeline = new StubWorldGenPipeline();
            var (controller, system, services, _, jobSender, _, _) = CreateController(pipeline);

            controller.Plan(jobSender, 1000UL + (ulong)i, "default", null, SenderEntityId);
            WorldGenFixture.Pump(system, jobSender);
            controller.Abort(jobSender);
            WorldGenFixture.Pump(system, jobSender);
            WaitUntil(() => services.JobStatus == WorldGenerationJobStatus.Idle, "settle");

            Assert.Equal(WorldGenerationJobStatus.Idle, services.JobStatus);
            Assert.True(services.LastCompletedPlan is not null
                        || services.LastStatusMessage!.Contains("aborted"),
                "The job must either complete or abort, never wedge the slot.");
        }
    }

    [Fact]
    public void Abort_Twice_RequestsCancellationTwice()
    {
        var pipeline = new CancellableStubPipeline();
        var (controller, system, services, _, jobSender, _, _) = CreateController(pipeline);

        controller.Plan(jobSender, 12345, "default", null, SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);
        WaitUntil(() => pipeline.Entered.IsSet, "pipeline start");

        controller.Abort(jobSender);
        WorldGenFixture.Pump(system, jobSender);
        Assert.Equal(WorldGenerationJobStatus.Cancelling, services.JobStatus);

        controller.Abort(jobSender);
        WorldGenFixture.Pump(system, jobSender);

        pipeline.Release.Set();
        WaitUntil(() => services.JobStatus == WorldGenerationJobStatus.Idle, "abort settle");
    }

    /// <summary>
    ///     Creates a chat command handler backed by a gated stub pipeline, for reply
    ///     assertions through the chat path.
    /// </summary>
    /// <param name="gate">Gate the stub pipeline parks on.</param>
    /// <returns>Handler, chat sender, controller event sender, system, state manager,
    ///     and services.</returns>
    private static (WorldGenChatCommandHandler Handler, FakeEventSender Sender,
        FakeEventSender JobSender, WorldGenerationSystem System, WorldGenStateManager StateManager,
        WorldGenerationServices Services) CreateHandler(ManualResetEventSlim gate)
    {
        var sender = new FakeEventSender();
        var jobSender = new FakeEventSender();
        var scratch = new WorldGenScratch(Options.Create(new WorldGenOptions()));
        var stateManager = new WorldGenStateManager(NullLogger<WorldGenStateManager>.Instance);
        var services = new WorldGenerationServices(stateManager);
        var loader = new ProfileLoader(Path.Combine(AppContext.BaseDirectory, "Data", "Worldgen"));
        var runner = new WorldGenPlanJobRunner(stateManager, services,
            new StubWorldGenPipeline { Gate = gate }, loader,
            new ProfileValidator(),
            new WorldGenTemplateResolver(TestTemplateIndexers.CreateDefault()),
            scratch, new ServerChatInternalController(sender),
            NullLogger<WorldGenPlanJobRunner>.Instance);
        var commitRunner = WorldGenFixture.BuildCommitRunner(stateManager, services, scratch, sender);
        var system = WorldGenFixture.BuildSystem(stateManager, runner, commitRunner, scratch);
        var handler = new WorldGenChatCommandHandler(
            new WorldGenerationController(), services,
            new ServerChatInternalController(sender), jobSender);
        return (handler, sender, jobSender, system, stateManager, services);
    }

    /// <summary>
    ///     Creates a controller with the given pipeline and default fakes.
    /// </summary>
    /// <param name="pipeline">Pipeline test double.</param>
    /// <returns>Controller, system, services, chat sender, job sender, and registry store.</returns>
    private static (WorldGenerationController Controller, WorldGenerationSystem System,
        WorldGenerationServices Services, FakeEventSender Sender, FakeEventSender JobSender,
        FakeWorldGenRegistryStore Registry, IWorldGenCommitWriter Writer) CreateController(
            IWorldGenPipeline pipeline)
    {
        return CreateController(pipeline, new FakeWorldGenCommitWriter());
    }

    /// <summary>
    ///     Creates a controller with the given pipeline and commit writer.
    /// </summary>
    /// <param name="pipeline">Pipeline test double.</param>
    /// <param name="writer">Commit writer test double.</param>
    /// <returns>Controller, system, services, chat sender, job sender, and registry store.</returns>
    private static (WorldGenerationController Controller, WorldGenerationSystem System,
        WorldGenerationServices Services, FakeEventSender Sender, FakeEventSender JobSender,
        FakeWorldGenRegistryStore Registry, IWorldGenCommitWriter Writer) CreateController(
            IWorldGenPipeline pipeline, IWorldGenCommitWriter writer)
    {
        var sender = new FakeEventSender();
        var jobSender = new FakeEventSender();
        var scratch = new WorldGenScratch(Options.Create(new WorldGenOptions()));
        var stateManager = new WorldGenStateManager(NullLogger<WorldGenStateManager>.Instance);
        var services = new WorldGenerationServices(stateManager);
        var registry = new FakeWorldGenRegistryStore();
        var planRunner = WorldGenFixture.BuildPlanRunner(stateManager, services, pipeline, sender,
            scratch);
        var commitRunner = new WorldGenCommitRunner(stateManager, services, scratch, registry,
            writer, new FakeSegmentSubscriptionProbe(), new WorldManagementController(), sender,
            new ServerChatInternalController(sender), Options.Create(new WorldGenOptions()),
            NullLogger<WorldGenCommitRunner>.Instance);
        var system = WorldGenFixture.BuildSystem(stateManager, planRunner, commitRunner, scratch);
        var controller = new WorldGenerationController();
        return (controller, system, services, sender, jobSender, registry, writer);
    }

    /// <summary>
    ///     Waits until the condition holds or the timeout elapses.
    /// </summary>
    /// <param name="condition">Condition to wait for.</param>
    /// <param name="what">Description for the timeout message.</param>
    private static void WaitUntil(Func<bool> condition, string what)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            if (condition()) return;
            Thread.Sleep(10);
        }

        Assert.Fail($"Timed out waiting for {what}.");
    }

    /// <summary>
    ///     Extracts the system chat messages sent through the sender.
    /// </summary>
    /// <param name="sender">Recording sender.</param>
    /// <returns>Messages in send order.</returns>
    private static List<string> SentMessages(FakeEventSender sender)
    {
        var messages = new List<string>();
        foreach (var ev in sender.SentEvents)
        {
            if (ev.EventDetails is SystemChatEventDetails details)
            {
                messages.Add(details.Message);
            }
        }

        return messages;
    }
}
