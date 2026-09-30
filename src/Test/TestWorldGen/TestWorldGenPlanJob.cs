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
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Sovereign.WorldGen;
using Sovereign.WorldGen.Layout;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests for the world generation plan job: happy path status flow, failure handling,
///     and refusal of concurrent jobs.
/// </summary>
public class TestWorldGenPlanJob
{
    /// <summary>
    ///     Sender entity ID used in tests.
    /// </summary>
    private const ulong SenderEntityId = 0x1234;

    [Fact]
    public void BeginPlan_HappyPath_ReportsPhasesReturnsPlanAndReturnsToIdle()
    {
        var pipeline = new StubWorldGenPipeline();
        var (runner, system, services, sender) = CreateRunner(pipeline);

        runner.BeginPlan(12345, "default", null, SenderEntityId);
        WaitUntil(() => services.LastCompletedPlan is not null, "plan completion");

        Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
        Assert.Equal(new[] { "Terrain", "Hydrology", "Preview", "Assembly" }, pipeline.SeenPhases);
        Assert.Equal("Plan complete", system.LastStatusMessage);
        Assert.NotNull(services.LastCompletedPlan);
        Assert.Equal(12345UL, services.LastCompletedPlan!.Seed);
        Assert.Contains(SentMessages(sender), m => m.Contains("World generation started"));
        Assert.Contains(SentMessages(sender), m => m.Contains("Preview:"));
    }

    [Fact]
    public void BeginPlan_PipelineFailure_ReturnsToIdleWithErrorReply()
    {
        var (runner, system, _, sender) = CreateRunner(
            new ThrowingWorldGenPipeline(new InvalidOperationException("boom")));

        runner.BeginPlan(999, "default", null, SenderEntityId);
        WaitUntil(() => system.JobStatus == WorldGenerationJobStatus.Idle, "failure handling");

        Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
        var messages = SentMessages(sender);
        Assert.Contains(messages, m => m.Contains("failed"));
        Assert.Contains(messages, m => m.Contains("boom"));
    }

    [Fact]
    public void BeginPlan_StrictLayoutFailure_ReturnsToIdleWithConsolidatedMessage()
    {
        var (runner, system, _, sender) = CreateRunner(new ThrowingWorldGenPipeline(
            new LayoutValidationException(
                "layout validation failed after 3 attempt(s); unmatched anchors: #1 (0.02, 0.02).")));

        runner.BeginPlan(999, "default", null, SenderEntityId);
        WaitUntil(() => system.JobStatus == WorldGenerationJobStatus.Idle, "strict failure handling");

        Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
        var messages = SentMessages(sender);
        Assert.Contains(messages, m => m.Contains("layout validation failed"));
        Assert.Contains(messages, m => m.Contains("#1"));
    }

    [Fact]
    public void BeginPlan_WhileJobRunning_IsRefused()
    {
        var gate = new ManualResetEventSlim(false);
        var pipeline = new StubWorldGenPipeline { Gate = gate };
        var (runner, system, _, sender) = CreateRunner(pipeline);

        runner.BeginPlan(1, "default", null, SenderEntityId);
        try
        {
            WaitUntil(() => system.JobStatus == WorldGenerationJobStatus.Planning && pipeline.Started,
                "job start");

            runner.BeginPlan(2, "default", null, SenderEntityId);

            Assert.Contains(SentMessages(sender), m => m.Contains("already running"));
            Assert.Equal(WorldGenerationJobStatus.Planning, system.JobStatus);
        }
        finally
        {
            gate.Set();
        }

        WaitUntil(() => system.JobStatus == WorldGenerationJobStatus.Idle, "job completion");
    }

    [Fact]
    public void BeginPlan_ProfileWithErrors_AbortsAndReturnsToIdle()
    {
        using var scope = new TempProfileScope();
        scope.WriteProfile("broken", @"{
  ""width"": 100,
  ""height"": 2048,
  ""seaLevelZ"": 12,
  ""surfaceMaxZ"": 28,
  ""rockFloorZ"": -63,
  ""bedrockZ"": -64,
  ""bedrockTemplate"": ""Bedrock"",
  ""stoneBands"": []
}");
        var (runner, system, _, sender) = CreateRunner(
            new ThrowingWorldGenPipeline(new InvalidOperationException("should not run")),
            scope.DirectoryPath);

        runner.BeginPlan(1, "broken", null, SenderEntityId);

        Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
        var messages = SentMessages(sender);
        Assert.Contains(messages, m => m.Contains("Profile error"));
        Assert.Contains(messages, m => m.Contains("aborted"));
    }

    [Fact]
    public void BeginPlan_ProfileWithWarnings_ReportsWarningsAndRuns()
    {
        using var scope = new TempProfileScope();
        scope.WriteProfile("warned", @"{
  ""width"": 2048,
  ""height"": 2048,
  ""seaLevelZ"": 12,
  ""surfaceMaxZ"": 40,
  ""rockFloorZ"": -63,
  ""bedrockZ"": -64,
  ""bedrockTemplate"": ""Bedrock"",
  ""stoneBands"": [
    { ""fromZ"": -63, ""toZ"": -1, ""template"": ""Basalt"" }
  ]
}");
        var pipeline = new StubWorldGenPipeline();
        var (runner, system, services, sender) = CreateRunner(pipeline, scope.DirectoryPath);

        runner.BeginPlan(1, "warned", null, SenderEntityId);
        WaitUntil(() => services.LastCompletedPlan is not null, "plan completion");

        Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
        Assert.Contains(SentMessages(sender), m => m.Contains("Profile warning"));
    }

    [Fact]
    public void BeginPlan_ProfileLoadFailure_RepliesErrorAndReturnsToIdle()
    {
        var (runner, system, _, sender) = CreateRunner(
            new ThrowingWorldGenPipeline(new InvalidOperationException("should not run")));

        runner.BeginPlan(1, "nonexistent-profile", null, SenderEntityId);

        Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
        Assert.Contains(SentMessages(sender), m => m.Contains("Worldgen plan failed"));
    }

    /// <summary>
    ///     Creates a runner backed by a real system, the given profile directory, and the
    ///     given pipeline test double.
    /// </summary>
    /// <returns>Runner, system, services, and the recording chat sender.</returns>
    private static (WorldGenPlanJobRunner Runner, WorldGenerationSystem System,
        WorldGenerationServices Services, FakeEventSender Sender) CreateRunner(
        IWorldGenPipeline pipeline, string? profileDirectory = null)
    {
        var sender = new FakeEventSender();
        var scratch = new WorldGenScratch(Options.Create(new WorldGenOptions()));
        var system = new WorldGenerationSystem(new EventCommunicator(), new FakeEventLoop(),
            scratch, NullLogger<WorldGenerationSystem>.Instance);
        var services = new WorldGenerationServices(system);
        var loader = new ProfileLoader(profileDirectory ??
            Path.Combine(AppContext.BaseDirectory, "Data", "Worldgen"));
        var runner = new WorldGenPlanJobRunner(system, services, pipeline, loader,
            new ProfileValidator(), new WorldGenTemplateResolver(TestTemplateIndexers.CreateDefault()),
            scratch, new ServerChatInternalController(sender),
            NullLogger<WorldGenPlanJobRunner>.Instance);
        return (runner, system, services, sender);
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
    ///     Temporary directory with profile JSON files, deleted on disposal.
    /// </summary>
    private sealed class TempProfileScope : IDisposable
    {
        public TempProfileScope()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(DirectoryPath);
        }

        /// <summary>
        ///     Path of the temporary directory.
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        ///     Writes a profile JSON file.
        /// </summary>
        public void WriteProfile(string name, string json)
        {
            File.WriteAllText(Path.Combine(DirectoryPath, $"{name}.json"), json);
        }

        public void Dispose()
        {
            Directory.Delete(DirectoryPath, true);
        }
    }
}
