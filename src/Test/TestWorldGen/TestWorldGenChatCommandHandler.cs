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
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("status", SenderEntityId);

        Assert.Contains("Idle", GetSingleSystemMessage(sender), StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Status_ReportsRunningPhase()
    {
        var (handler, sender, system, _) = CreateHandler();
        system.SetJobStatus(WorldGenerationJobStatus.Planning, "Terrain: shaping surface");

        handler.Handle("status", SenderEntityId);

        var message = GetSingleSystemMessage(sender);
        Assert.Contains("Planning", message);
        Assert.Contains("Terrain", message);
    }

    [Fact]
    public void Handle_Plan_StartsJob()
    {
        var (handler, sender, _, services) = CreateHandler();

        handler.Handle("plan 1", SenderEntityId);

        Assert.Contains(SentMessages(sender), m => m.Contains("World generation started"));
        WaitUntil(() => services.LastCompletedPlan is not null, "plan completion");
        Assert.Contains(SentMessages(sender), m => m.Contains("Preview:"));
    }

    [Fact]
    public void Handle_Plan_WithOptions_ParsesAndStartsJob()
    {
        var (handler, sender, _, services) = CreateHandler();

        handler.Handle("plan 42 --profile default --at 10,-20", SenderEntityId);

        Assert.Contains(SentMessages(sender), m => m.Contains("World generation started"));
        WaitUntil(() => services.LastCompletedPlan is not null, "plan completion");
        Assert.Equal(10, services.LastCompletedPlan!.OriginX);
        Assert.Equal(-20, services.LastCompletedPlan.OriginY);
    }

    [Fact]
    public void Handle_Plan_UnknownProfile_ReportsFailure()
    {
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("plan 1 --profile no-such-profile", SenderEntityId);

        Assert.Contains(SentMessages(sender), m => m.Contains("Worldgen plan failed"));
    }

    [Fact]
    public void Handle_Plan_MissingSeed_ReportsUsage()
    {
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("plan", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_Plan_BadAtOption_ReportsUsage()
    {
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("plan 1 --at bogus", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_Commit_WithoutPlan_ReportsNoStagedPlan()
    {
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("commit", SenderEntityId);

        Assert.Contains("No staged world generation plan", GetSingleSystemMessage(sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Commit_WithSeedWithoutPlan_ReportsNoStagedPlan()
    {
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("commit 7", SenderEntityId);

        Assert.Contains("No staged world generation plan", GetSingleSystemMessage(sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Commit_BadSeed_ReportsUsage()
    {
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("commit xyz", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_Preview_WithoutPlan_ReportsError()
    {
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("preview", SenderEntityId);

        Assert.Contains("No world generation plan", GetSingleSystemMessage(sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Preview_AfterPlan_ReportsPreviewPath()
    {
        var (handler, sender, _, services) = CreateHandler();

        handler.Handle("plan 1", SenderEntityId);
        WaitUntil(() => services.LastCompletedPlan is not null, "plan completion");
        sender.Reset();

        handler.Handle("preview", SenderEntityId);

        Assert.Contains(services.LastCompletedPlan!.PreviewPath, GetSingleSystemMessage(sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Replace_WithoutPlan_ReportsNoStagedPlan()
    {
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("replace 1 2", SenderEntityId);

        Assert.Contains("No staged world generation plan", GetSingleSystemMessage(sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Replace_BadArgs_ReportsUsage()
    {
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("replace 1", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_Abort_RespondsNotYetImplemented()
    {
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("abort", SenderEntityId);

        var message = GetSingleSystemMessage(sender);
        Assert.Contains("abort", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not yet implemented", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Handle_UnknownSubcommand_ReportsUsage()
    {
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("bogus", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_EmptyMessage_ReportsUsage()
    {
        var (handler, sender, _, _) = CreateHandler();

        handler.Handle("   ", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    /// <summary>
    ///     Creates a handler backed by a real system and job runner with a stub pipeline.
    /// </summary>
    /// <returns>Handler, recording event sender, the job slot system, and the services.</returns>
    private static (WorldGenChatCommandHandler Handler, FakeEventSender Sender,
        WorldGenerationSystem System, WorldGenerationServices Services) CreateHandler()
    {
        var sender = new FakeEventSender();
        var scratch = new WorldGenScratch(Options.Create(new WorldGenOptions()));
        var system = new WorldGenerationSystem(new EventCommunicator(), new FakeEventLoop(),
            scratch, NullLogger<WorldGenerationSystem>.Instance);
        var services = new WorldGenerationServices(system);
        var loader = new ProfileLoader(Path.Combine(AppContext.BaseDirectory, "Data", "Worldgen"));
        var runner = new WorldGenPlanJobRunner(system, services, new StubWorldGenPipeline(), loader,
            new ProfileValidator(), new WorldGenTemplateResolver(new FakeWorldGenTemplateSource()),
            scratch, new ServerChatInternalController(sender),
            NullLogger<WorldGenPlanJobRunner>.Instance);
        var commitRunner = CreateCommitRunner(system, services, scratch, sender);
        var handler = new WorldGenChatCommandHandler(
            new WorldGenerationController(runner, commitRunner), services,
            new ServerChatInternalController(sender));

        return (handler, sender, system, services);
    }

    /// <summary>
    ///     Creates a commit runner backed by test doubles.
    /// </summary>
    /// <param name="system">Job slot system.</param>
    /// <param name="services">World generation services.</param>
    /// <param name="scratch">Scratch resolver.</param>
    /// <param name="sender">Recording event sender.</param>
    /// <returns>Commit runner.</returns>
    private static WorldGenCommitRunner CreateCommitRunner(WorldGenerationSystem system,
        WorldGenerationServices services, WorldGenScratch scratch, FakeEventSender sender)
    {
        return new WorldGenCommitRunner(system, services, scratch,
            new FakeWorldGenRegistryStore(), new FakeWorldGenCommitWriter(),
            new FakeSegmentSubscriptionProbe(), new WorldManagementController(), sender,
            new ServerChatInternalController(sender), Options.Create(new WorldGenOptions()),
            NullLogger<WorldGenCommitRunner>.Instance);
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
