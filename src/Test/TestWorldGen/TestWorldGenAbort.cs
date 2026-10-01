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
        var (controller, system, services, sender, _) = CreateController(pipeline);

        controller.Plan(12345, "default", null, SenderEntityId);
        WaitUntil(() => pipeline.Entered.IsSet, "pipeline start");

        Assert.Equal(WorldGenAbortOutcome.Requested, controller.Abort());
        Assert.Equal(WorldGenerationJobStatus.Cancelling, system.JobStatus);

        pipeline.Release.Set();
        WaitUntil(() => system.JobStatus == WorldGenerationJobStatus.Idle, "abort settle");

        Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
        Assert.Contains("Plan aborted at", system.LastStatusMessage);
        Assert.NotNull(pipeline.StagingDirectorySeen);
        Assert.False(
            Directory.Exists(Path.Combine(pipeline.StagingDirectorySeen!, "segments")),
            "An aborted plan's staged segments must be cleaned up by the runner.");
        Assert.Contains(SentMessages(sender), m => m.Contains("aborted at"));
        Assert.Contains(SentMessages(sender), m => m.Contains("Cancellation requested"));

        // The slot is usable again immediately after the abort settles.
        controller.Plan(777, "default", null, SenderEntityId);
        WaitUntil(() => services.LastCompletedPlan?.Seed == 777UL, "next plan");
        Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
    }

    [Fact]
    public void AbortCommit_MidBatch_SettlesIdleAndReCommitCompletes()
    {
        var writer = new GatedCancellableCommitWriter();
        var (controller, system, services, sender, registry) =
            CreateController(new StubWorldGenPipeline(), writer);

        // Stage a plan first; the slot must have settled to Idle before the commit is
        // offered, since the plan task records the plan just after its EndJob call.
        controller.Plan(555, "default", null, SenderEntityId);
        WaitUntil(() => services.LastCompletedPlan is not null
            && system.JobStatus == WorldGenerationJobStatus.Idle, "plan completion");

        controller.Commit(555, false, SenderEntityId);
        WaitUntil(() => writer.Entered.IsSet, "commit start");
        Assert.Equal(WorldGenAbortOutcome.Requested, controller.Abort());

        writer.Release.Set();
        WaitUntil(() => system.JobStatus == WorldGenerationJobStatus.Idle, "commit abort settle");

        Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
        Assert.Contains("Commit aborted at", system.LastStatusMessage);
        Assert.Contains(SentMessages(sender),
            m => m.Contains("commit aborted") && m.Contains("idempotent"));
        Assert.Empty(registry.Worlds);
        Assert.True(Directory.Exists(services.LastCompletedPlan!.StagingDirectory),
            "An aborted commit keeps the staged plan for a re-commit.");

        // Re-commit completes and registers the world.
        writer.Release.Set();
        controller.Commit(555, false, SenderEntityId);
        WaitUntil(() => registry.Worlds.Count == 1, "re-commit");
        Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
        Assert.Contains(SentMessages(sender), m => m.Contains("World generation committed"));
    }

    [Fact]
    public void Abort_WhenIdle_ReportsNoJob()
    {
        var (controller, _, _, sender, _) = CreateController(new StubWorldGenPipeline());

        Assert.Equal(WorldGenAbortOutcome.NotRunning, controller.Abort());
        Assert.Equal(WorldGenAbortOutcome.NotRunning, controller.Abort());

        // The chat handler translates the outcome into the user-facing reply.
        var gate = new ManualResetEventSlim(false);
        var (handler, handlerSender, _, _) = CreateHandler(gate);
        handler.Handle("abort", SenderEntityId);
        Assert.Contains(SentMessages(handlerSender),
            m => m.Contains("No world generation job is running"));
        gate.Set();
    }

    [Fact]
    public void Abort_CompletingJob_IsCoherentAndNeverStuck()
    {
        // A pipeline that finishes before observing the token: the abort request races the
        // completion. Either the job completes or aborts; either way the slot ends Idle and
        // the abort reply is coherent.
        for (var i = 0; i < 20; ++i)
        {
            var pipeline = new StubWorldGenPipeline();
            var (controller, system, services, _, _) = CreateController(pipeline);

            controller.Plan(1000UL + (ulong)i, "default", null, SenderEntityId);
            controller.Abort();
            WaitUntil(() => system.JobStatus == WorldGenerationJobStatus.Idle, "settle");

            Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
            Assert.True(services.LastCompletedPlan is not null
                        || system.LastStatusMessage!.Contains("aborted"),
                "The job must either complete or abort, never wedge the slot.");
        }
    }

    [Fact]
    public void Abort_Twice_ReportsAlreadyRequested()
    {
        var pipeline = new CancellableStubPipeline();
        var (controller, system, _, sender, _) = CreateController(pipeline);

        controller.Plan(12345, "default", null, SenderEntityId);
        WaitUntil(() => pipeline.Entered.IsSet, "pipeline start");

        Assert.Equal(WorldGenAbortOutcome.Requested, controller.Abort());
        Assert.Equal(WorldGenAbortOutcome.AlreadyRequested, controller.Abort());

        var gate = new ManualResetEventSlim(false);
        var (handler, handlerSender, handlerSystem, _) = CreateHandler(gate);
        handler.Handle("plan 12345", SenderEntityId);
        WaitUntil(() => handlerSystem.JobStatus == WorldGenerationJobStatus.Planning,
            "second job start");
        handler.Handle("abort", SenderEntityId);
        handler.Handle("abort", SenderEntityId);
        Assert.Contains(SentMessages(handlerSender),
            m => m.Contains("already requested", StringComparison.Ordinal));
        Assert.Contains(SentMessages(handlerSender),
            m => m.Contains("Cancellation requested", StringComparison.Ordinal));
        gate.Set();
        WaitUntil(() => handlerSystem.JobStatus == WorldGenerationJobStatus.Idle,
            "second job settle");

        pipeline.Release.Set();
        WaitUntil(() => system.JobStatus == WorldGenerationJobStatus.Idle, "abort settle");
    }

    /// <summary>
    ///     Creates a chat command handler backed by a gated stub pipeline, for reply
    ///     assertions through the chat path.
    /// </summary>
    /// <param name="gate">Gate the stub pipeline parks on.</param>
    /// <returns>Handler, chat sender, system, and services.</returns>
    private static (WorldGenChatCommandHandler Handler, FakeEventSender Sender,
        WorldGenerationSystem System, WorldGenerationServices Services) CreateHandler(
        ManualResetEventSlim gate)
    {
        var sender = new FakeEventSender();
        var scratch = new WorldGenScratch(Options.Create(new WorldGenOptions()));
        var system = new WorldGenerationSystem(new EventCommunicator(), new FakeEventLoop(),
            scratch, NullLogger<WorldGenerationSystem>.Instance);
        var services = new WorldGenerationServices(system);
        var loader = new ProfileLoader(Path.Combine(AppContext.BaseDirectory, "Data", "Worldgen"));
        var runner = new WorldGenPlanJobRunner(system, services,
            new StubWorldGenPipeline { Gate = gate }, loader,
            new ProfileValidator(),
            new WorldGenTemplateResolver(TestTemplateIndexers.CreateDefault()),
            scratch, new ServerChatInternalController(sender),
            NullLogger<WorldGenPlanJobRunner>.Instance);
        var commitRunner = new WorldGenCommitRunner(system, services, scratch,
            new FakeWorldGenRegistryStore(), new FakeWorldGenCommitWriter(),
            new FakeSegmentSubscriptionProbe(), new WorldManagementController(), sender,
            new ServerChatInternalController(sender), Options.Create(new WorldGenOptions()),
            NullLogger<WorldGenCommitRunner>.Instance);
        var handler = new WorldGenChatCommandHandler(
            new WorldGenerationController(runner, commitRunner, services), services,
            new ServerChatInternalController(sender));
        return (handler, sender, system, services);
    }

    /// <summary>
    ///     Creates a controller with the given pipeline and default fakes.
    /// </summary>
    /// <param name="pipeline">Pipeline test double.</param>
    /// <returns>Controller, system, services, chat sender, and registry store.</returns>
    private static (WorldGenerationController Controller, WorldGenerationSystem System,
        WorldGenerationServices Services, FakeEventSender Sender,
        FakeWorldGenRegistryStore Registry) CreateController(IWorldGenPipeline pipeline)
    {
        return CreateController(pipeline, new FakeWorldGenCommitWriter());
    }

    /// <summary>
    ///     Creates a controller with the given pipeline and commit writer.
    /// </summary>
    /// <param name="pipeline">Pipeline test double.</param>
    /// <param name="writer">Commit writer test double.</param>
    /// <returns>Controller, system, services, chat sender, and registry store.</returns>
    private static (WorldGenerationController Controller, WorldGenerationSystem System,
        WorldGenerationServices Services, FakeEventSender Sender,
        FakeWorldGenRegistryStore Registry) CreateController(IWorldGenPipeline pipeline,
        IWorldGenCommitWriter writer)
    {
        var sender = new FakeEventSender();
        var scratch = new WorldGenScratch(Options.Create(new WorldGenOptions()));
        var system = new WorldGenerationSystem(new EventCommunicator(), new FakeEventLoop(),
            scratch, NullLogger<WorldGenerationSystem>.Instance);
        var services = new WorldGenerationServices(system);
        var loader = new ProfileLoader(Path.Combine(AppContext.BaseDirectory, "Data", "Worldgen"));
        var runner = new WorldGenPlanJobRunner(system, services, pipeline, loader,
            new ProfileValidator(), new WorldGenTemplateResolver(TestTemplateIndexers.CreateDefault()),
            scratch, new ServerChatInternalController(sender),
            NullLogger<WorldGenPlanJobRunner>.Instance);
        var registry = new FakeWorldGenRegistryStore();
        var commitRunner = new WorldGenCommitRunner(system, services, scratch, registry, writer,
            new FakeSegmentSubscriptionProbe(), new WorldManagementController(), sender,
            new ServerChatInternalController(sender), Options.Create(new WorldGenOptions()),
            NullLogger<WorldGenCommitRunner>.Instance);
        var controller = new WorldGenerationController(runner, commitRunner, services);
        return (controller, system, services, sender, registry);
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
