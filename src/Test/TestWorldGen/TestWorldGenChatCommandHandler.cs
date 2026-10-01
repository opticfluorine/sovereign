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
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Sovereign.WorldGen;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for <see cref="WorldGenChatCommandHandler" />.
/// </summary>
public class TestWorldGenChatCommandHandler
{
    /// <summary>
    ///     Sender entity ID used in tests.
    /// </summary>
    private const ulong SenderEntityId = 0x1234;

    [Fact]
    public void Handle_Status_WhenIdle_ReportsIdle()
    {
        var (handler, sender, _, _, _, _) = CreateHandler();

        handler.Handle("status", SenderEntityId);

        Assert.Contains("Idle", GetSingleSystemMessage(sender), StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Status_ReportsRunningPhase()
    {
        var (handler, sender, jobSender, system, stateManager, services) = CreateHandler();
        stateManager.SetJobStatus(WorldGenerationJobStatus.Planning, "Terrain: shaping surface");

        handler.Handle("status", SenderEntityId);

        var message = GetSingleSystemMessage(sender);
        Assert.Contains("Planning", message);
        Assert.Contains("Terrain", message);
    }

    [Fact]
    public void Handle_Plan_StartsJob()
    {
        var (handler, sender, jobSender, system, _, services) = CreateHandler();

        handler.Handle("plan 1", SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);

        Assert.Contains(SentMessages(sender), m => m.Contains("World generation started"));
        WaitUntil(() => services.LastCompletedPlan is not null, "plan completion");
        Assert.Contains(SentMessages(sender), m => m.Contains("Preview:"));
    }

    [Fact]
    public void Handle_Plan_WithOptions_ParsesAndStartsJob()
    {
        var (handler, sender, jobSender, system, _, services) = CreateHandler();

        handler.Handle("plan 42 --profile default --at 10,-20", SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);

        Assert.Contains(SentMessages(sender), m => m.Contains("World generation started"));
        WaitUntil(() => services.LastCompletedPlan is not null, "plan completion");
        Assert.Equal(10, services.LastCompletedPlan!.OriginX);
        Assert.Equal(-20, services.LastCompletedPlan.OriginY);
    }

    [Fact]
    public void Handle_Plan_UnknownProfile_ReportsFailure()
    {
        var (handler, sender, jobSender, system, _, _) = CreateHandler();

        handler.Handle("plan 1 --profile no-such-profile", SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);

        Assert.Contains(SentMessages(sender), m => m.Contains("Worldgen plan failed"));
    }

    [Fact]
    public void Handle_Plan_MissingSeed_ReportsUsage()
    {
        var (handler, sender, _, _, _, _) = CreateHandler();

        handler.Handle("plan", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_Plan_BadAtOption_ReportsUsage()
    {
        var (handler, sender, _, _, _, _) = CreateHandler();

        handler.Handle("plan 1 --at bogus", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_Commit_WithoutPlan_ReportsNoStagedPlan()
    {
        var (handler, sender, jobSender, system, _, _) = CreateHandler();

        handler.Handle("commit", SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);

        Assert.Contains("No staged world generation plan", GetSingleSystemMessage(sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Commit_WithSeedWithoutPlan_ReportsNoStagedPlan()
    {
        var (handler, sender, jobSender, system, _, _) = CreateHandler();

        handler.Handle("commit 7", SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);

        Assert.Contains("No staged world generation plan", GetSingleSystemMessage(sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Commit_BadSeed_ReportsUsage()
    {
        var (handler, sender, _, _, _, _) = CreateHandler();

        handler.Handle("commit xyz", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_Preview_WithoutPlan_ReportsError()
    {
        var (handler, sender, _, _, _, _) = CreateHandler();

        handler.Handle("preview", SenderEntityId);

        Assert.Contains("No world generation plan", GetSingleSystemMessage(sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Preview_AfterPlan_ReportsPreviewPath()
    {
        var (handler, sender, jobSender, system, _, services) = CreateHandler();

        handler.Handle("plan 1", SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);
        WaitUntil(() => services.LastCompletedPlan is not null, "plan completion");
        sender.Reset();

        handler.Handle("preview", SenderEntityId);

        Assert.Contains(services.LastCompletedPlan!.PreviewPath, GetSingleSystemMessage(sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Replace_WithoutPlan_ReportsNoStagedPlan()
    {
        var (handler, sender, jobSender, system, _, _) = CreateHandler();

        handler.Handle("replace 1 2", SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);

        Assert.Contains("No staged world generation plan", GetSingleSystemMessage(sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Replace_BadArgs_ReportsUsage()
    {
        var (handler, sender, _, _, _, _) = CreateHandler();

        handler.Handle("replace 1", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_Abort_WhenIdle_ReportsNoJob()
    {
        var (handler, sender, _, _, _, _) = CreateHandler();

        handler.Handle("abort", SenderEntityId);

        var message = GetSingleSystemMessage(sender);
        Assert.Contains("No world generation job is running", message, StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Abort_WhileRunning_RequestsCancellation()
    {
        var gate = new ManualResetEventSlim(false);
        var (handler, sender, jobSender, system, stateManager, services) = CreateHandler(gate);

        handler.Handle("plan 12345", SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);
        WaitUntil(() => services.JobStatus == WorldGenerationJobStatus.Planning, "job start");

        handler.Handle("abort", SenderEntityId);
        WorldGenFixture.Pump(system, jobSender);
        Assert.Contains(SentMessages(sender),
            m => m.Contains("Cancellation requested", StringComparison.Ordinal));
        Assert.Equal(WorldGenerationJobStatus.Cancelling, services.JobStatus);

        handler.Handle("abort", SenderEntityId);
        Assert.Contains(SentMessages(sender),
            m => m.Contains("Cancellation requested", StringComparison.Ordinal));

        gate.Set();
        WaitUntil(() => services.JobStatus == WorldGenerationJobStatus.Idle, "job completion");
    }

    [Fact]
    public void Handle_UnknownSubcommand_ReportsUsage()
    {
        var (handler, sender, _, _, _, _) = CreateHandler();

        handler.Handle("bogus", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_EmptyMessage_ReportsUsage()
    {
        var (handler, sender, _, _, _, _) = CreateHandler();

        handler.Handle("   ", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    /// <summary>
    ///     Creates a handler backed by a real system and job runner with a stub pipeline.
    /// </summary>
    /// <returns>Handler, recording chat sender, the controller event sender, the job slot
    ///     system, the state manager, and the services.</returns>
    private static (WorldGenChatCommandHandler Handler, FakeEventSender Sender,
        FakeEventSender JobSender, WorldGenerationSystem System, WorldGenStateManager StateManager,
        WorldGenerationServices Services) CreateHandler(ManualResetEventSlim? gate = null)
    {
        var sender = new FakeEventSender();
        var jobSender = new FakeEventSender();
        var scratch = new WorldGenScratch(Options.Create(new WorldGenOptions()));
        var stateManager = new WorldGenStateManager(NullLogger<WorldGenStateManager>.Instance);
        var services = new WorldGenerationServices(stateManager);
        var loader = new ProfileLoader(Path.Combine(AppContext.BaseDirectory, "Data", "Worldgen"));
        var runner = new WorldGenPlanJobRunner(stateManager, services,
            new StubWorldGenPipeline { Gate = gate }, loader,
            new ProfileValidator(), new WorldGenTemplateResolver(TestTemplateIndexers.CreateDefault()),
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
    ///     Waits until the condition holds or the timeout elapses.
    /// </summary>
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

    /// <summary>
    ///     Gets the single system chat message sent through the given sender.
    /// </summary>
    /// <param name="sender">Recording event sender.</param>
    /// <returns>Message text.</returns>
    private static string GetSingleSystemMessage(FakeEventSender sender)
    {
        var ev = Assert.Single(sender.SentEvents);
        var details = Assert.IsType<SystemChatEventDetails>(ev.EventDetails);
        Assert.Equal(SenderEntityId, details.TargetEntityId);
        return details.Message;
    }
}

/// <summary>
///     Assertions for /worldgen command replies.
/// </summary>
internal static class WorldGenReplyAssertions
{
    /// <summary>
    ///     Asserts that the reply indicates the subcommand is not yet implemented.
    /// </summary>
    /// <param name="message">Reply message.</param>
    public static void NotImplementedReply(this string message)
    {
        Assert.Contains("not yet implemented", message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Asserts that the reply is a usage summary.
    /// </summary>
    /// <param name="message">Reply message.</param>
    public static void UsageReply(this string message)
    {
        Assert.Contains("Usage:", message, StringComparison.Ordinal);
    }
}
