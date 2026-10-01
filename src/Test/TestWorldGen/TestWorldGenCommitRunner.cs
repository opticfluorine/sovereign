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

using System.IO;
using Sovereign.EngineCore.Components.Types;
using Sovereign.WorldGen;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests of the commit job runner: seed validation, registry checks, the player
///     interlock, and successful completion.
/// </summary>
public class TestWorldGenCommitRunner : IDisposable
{
    /// <summary>
    ///     Entity ID of the committing player.
    /// </summary>
    private const ulong SenderEntityId = 0x7FFF000000000042;

    /// <summary>
    ///     Root directory of the test staging area.
    /// </summary>
    private readonly string stagingRoot;

    public TestWorldGenCommitRunner()
    {
        stagingRoot = Path.Combine(Path.GetTempPath(), "worldgen-commit-test",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stagingRoot);
    }

    public void Dispose()
    {
        Directory.Delete(stagingRoot, true);
    }

    [Fact]
    public void Commit_WithoutStagedPlan_ReportsNoStagedPlan()
    {
        var fixture = CreateFixture();

        fixture.Runner.BeginCommit(null, force: false, SenderEntityId);

        Assert.Contains("No staged world generation plan",
            SingleMessage(fixture.Sender), StringComparison.Ordinal);
        Assert.Equal(WorldGenerationJobStatus.Idle, fixture.Services.JobStatus);
    }

    [Fact]
    public void Commit_WrongSeed_ReportsMismatch()
    {
        var fixture = CreateFixture();
        StagePlan(fixture, seed: 7);

        fixture.Runner.BeginCommit(8, force: false, SenderEntityId);

        Assert.Contains("No staged plan with seed 8", SingleMessage(fixture.Sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Commit_StraddlingRegisteredWorld_IsRefused()
    {
        var fixture = CreateFixture();
        StagePlan(fixture, seed: 7, originX: 0, originY: 0);
        fixture.Registry.Seed(Entry(96, 0, width: 64, height: 64, seed: 99));

        fixture.Runner.BeginCommit(null, force: false, SenderEntityId);

        Assert.Contains("straddles registered world 99", SingleMessage(fixture.Sender),
            StringComparison.Ordinal);
        Assert.Empty(fixture.Writer.Requests);
    }

    [Fact]
    public void Commit_ContainingRegisteredWorld_RequiresReplace()
    {
        var fixture = CreateFixture();
        StagePlan(fixture, seed: 7, originX: 0, originY: 0);
        fixture.Registry.Seed(Entry(16, 16, width: 32, height: 32, seed: 99));

        fixture.Runner.BeginCommit(null, force: false, SenderEntityId);

        Assert.Contains("fully contains registered world 99", SingleMessage(fixture.Sender),
            StringComparison.Ordinal);
        Assert.Contains("/worldgen replace 7 99", SingleMessage(fixture.Sender),
            StringComparison.Ordinal);
        Assert.Empty(fixture.Writer.Requests);
    }

    [Fact]
    public void Commit_SubscribedPlayerInFootprint_RefusedWithoutForce()
    {
        var fixture = CreateFixture();
        StagePlan(fixture, seed: 7);
        fixture.Subscriptions.Subscribe(new GridPosition { X = 0, Y = 0, Z = 0 }, 1000);
        fixture.Subscriptions.Subscribe(new GridPosition { X = 1, Y = 0, Z = 0 }, 1001);

        fixture.Runner.BeginCommit(null, force: false, SenderEntityId);

        var message = SingleMessage(fixture.Sender);
        Assert.Contains("Commit refused", message);
        Assert.Contains("2 player(s)", message);
        Assert.Contains("(0, 0, 0)", message);
        Assert.Contains("--force", message);
        Assert.Empty(fixture.Writer.Requests);
        Assert.Equal(WorldGenerationJobStatus.Idle, fixture.Services.JobStatus);
    }

    [Fact]
    public async Task Commit_SubscribedPlayerInFootprint_ProceedsWithForce()
    {
        var fixture = CreateFixture();
        StagePlan(fixture, seed: 7);
        fixture.Subscriptions.Subscribe(new GridPosition { X = 0, Y = 0, Z = 0 }, 1000);

        fixture.Runner.BeginCommit(null, force: true, SenderEntityId);
        await WaitUntil(() => fixture.Services.JobStatus == WorldGenerationJobStatus.Idle,
            "commit completion");

        var messages = SentMessages(fixture.Sender);
        Assert.Contains(messages, m => m.Contains("commit started", StringComparison.Ordinal));
        Assert.Contains(messages,
            m => m.Contains("1 subscribed player(s)", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Commit_HappyPath_WritesRegistryAndReportsStats()
    {
        var fixture = CreateFixture();
        StagePlan(fixture, seed: 7);

        fixture.Runner.BeginCommit(7, force: false, SenderEntityId);

        await WaitUntil(() => fixture.Services.JobStatus == WorldGenerationJobStatus.Idle,
            "commit completion");

        var world = Assert.Single(fixture.Registry.Worlds);
        Assert.Equal((ulong)7, world.Seed);
        Assert.Equal("test", world.Profile);

        var reply = LastMessage(fixture.Sender);
        Assert.Contains("World generation committed: seed 7", reply);
        Assert.Contains("12 segments written", reply);
        Assert.Contains("3 decorations created", reply);
        Assert.False(Directory.Exists(Path.Combine(stagingRoot, "staging_7", "segments")),
            "Commit should consume the staging directory.");
    }

    [Fact]
    public async Task Replace_MatchingRegisteredWorld_DeletesAndWrites()
    {
        var fixture = CreateFixture();
        StagePlan(fixture, seed: 7);
        fixture.Registry.Seed(Entry(0, 0, width: 64, height: 64, seed: 99));

        fixture.Runner.BeginReplace(7, 99, force: false, SenderEntityId);

        await WaitUntil(() => fixture.Writer.Requests.Count > 0, "replace start");
        var request = fixture.Writer.Requests[0];
        Assert.NotNull(request.ReplaceWorld);
        Assert.Equal((ulong)99, request.ReplaceWorld.Seed);

        await WaitUntil(() => fixture.Services.JobStatus == WorldGenerationJobStatus.Idle,
            "replace completion");
        Assert.Equal((ulong)99, fixture.Registry.Worlds[0].Seed);
        Assert.Equal(2, fixture.Registry.Worlds.Count);
    }

    [Fact]
    public void Replace_UnknownOldSeed_IsRefused()
    {
        var fixture = CreateFixture();
        StagePlan(fixture, seed: 7);

        fixture.Runner.BeginReplace(7, 12345, force: false, SenderEntityId);

        Assert.Contains("No registered world with seed 12345", SingleMessage(fixture.Sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Replace_WrongStagedSeed_IsRefused()
    {
        var fixture = CreateFixture();
        StagePlan(fixture, seed: 7);
        fixture.Registry.Seed(Entry(0, 0, width: 64, height: 64, seed: 99));

        fixture.Runner.BeginReplace(8, 99, force: false, SenderEntityId);

        Assert.Contains("No staged plan with seed 8", SingleMessage(fixture.Sender),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Commit_WhileJobRunning_IsRefused()
    {
        var fixture = CreateFixture();
        fixture.StateManager.SetJobStatus(WorldGenerationJobStatus.Planning, "busy");

        fixture.Runner.BeginCommit(null, force: false, SenderEntityId);

        Assert.Contains("already running", SingleMessage(fixture.Sender),
            StringComparison.Ordinal);
        Assert.Empty(fixture.Writer.Requests);
    }

    /// <summary>
    ///     Creates a commit runner fixture with test doubles.
    /// </summary>
    /// <returns>Fixture.</returns>
    private CommitFixture CreateFixture()
    {
        var sender = new FakeEventSender();
        var scratch = new WorldGenScratch(Options.Create(new WorldGenOptions()));
        var stateManager = new WorldGenStateManager(NullLogger<WorldGenStateManager>.Instance);
        var services = new WorldGenerationServices(stateManager);
        var registry = new FakeWorldGenRegistryStore();
        var writer = new FakeWorldGenCommitWriter();
        var subscriptions = new FakeSegmentSubscriptionProbe();
        var runner = new WorldGenCommitRunner(stateManager, services, scratch, registry, writer,
            subscriptions, new WorldManagementController(), sender,
            new ServerChatInternalController(sender), Options.Create(new WorldGenOptions()),
            NullLogger<WorldGenCommitRunner>.Instance);
        var system = new WorldGenerationSystem(new EventCommunicator(), new FakeEventLoop(),
            stateManager, WorldGenFixture.BuildPlanRunner(stateManager, services,
                new StubWorldGenPipeline(), sender, scratch), runner, scratch,
            NullLogger<WorldGenerationSystem>.Instance);
        return new CommitFixture(runner, system, services, stateManager, sender, registry,
            writer, subscriptions);
    }

    /// <summary>
    ///     Records a staged plan with an intact staging directory.
    /// </summary>
    /// <param name="fixture">Fixture.</param>
    /// <param name="seed">Plan seed.</param>
    /// <param name="originX">Origin X.</param>
    /// <param name="originY">Origin Y.</param>
    private void StagePlan(CommitFixture fixture, ulong seed, int originX = 0, int originY = 0)
    {
        var staging = Path.Combine(stagingRoot, $"staging_{seed}");
        Directory.CreateDirectory(Path.Combine(staging, "segments"));
        fixture.Services.RecordCompletedPlan(new WorldGenPlan
        {
            Seed = seed,
            ProfileName = "test",
            Profile = TestProfiles.CreateSmall128(),
            OriginX = originX,
            OriginY = originY,
            ResolvedTemplates = TestResolvedTemplates.ForProfile(TestProfiles.CreateSmall128()),
            StagingDirectory = staging,
            Terrain = new Sovereign.WorldGen.Terrain.TerrainMap
            {
                Width = 64,
                Height = 64,
                Heights = new int[64, 64],
                IsOcean = new bool[64, 64],
                IsCliff = new bool[64, 64],
                IsBeach = new bool[64, 64],
                IsRiver = new bool[64, 64],
                RiverWidth = new int[64, 64],
                IsBank = new bool[64, 64],
                IsLake = new bool[64, 64],
                LakeSurfaceZ = new int[64, 64]
            },
            Statistics = new Sovereign.WorldGen.Output.PlanStatistics
            {
                Width = 64,
                Height = 64,
                LandCells = 4096,
                WaterCells = 0,
                RiverCount = 0,
                LakeCount = 0,
                LongestStraightRiverRun = 0,
                TerrainMs = 0,
                HydrologyMs = 0,
                BiomesMs = 0,
                PreviewMs = 0,
                TotalMs = 0
            },
            PreviewPath = Path.Combine(stagingRoot, "preview.png")
        });
    }

    /// <summary>
    ///     Creates a registry entry.
    /// </summary>
    /// <param name="originX">Origin X.</param>
    /// <param name="originY">Origin Y.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="seed">Seed.</param>
    /// <returns>Entry.</returns>
    private static WorldGenRegistryEntry Entry(int originX, int originY, int width, int height,
        ulong seed)
    {
        return new WorldGenRegistryEntry
        {
            Seed = seed,
            Profile = "test",
            OriginX = originX,
            OriginY = originY,
            Width = width,
            Height = height,
            MinSegmentZ = -2,
            MaxSegmentZ = 0,
            CommittedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Extracts the single system message sent through the sender.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <returns>Message text.</returns>
    private static string SingleMessage(FakeEventSender sender)
    {
        lock (sender)
        {
            var messages = SentMessages(sender);
            var message = Assert.Single(messages);
            return message;
        }
    }

    /// <summary>
    ///     Extracts the most recent system message sent through the sender.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <returns>Message text.</returns>
    private static string LastMessage(FakeEventSender sender)
    {
        var messages = SentMessages(sender);
        Assert.NotEmpty(messages);
        return messages[^1];
    }

    /// <summary>
    ///     Extracts system chat messages sent through the sender.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <returns>Messages.</returns>
    private static List<string> SentMessages(FakeEventSender sender)
    {
        var messages = new List<string>();
        foreach (var ev in sender.SentEvents)
        {
            if (ev.EventDetails is Sovereign.EngineCore.Events.Details.SystemChatEventDetails
                details)
            {
                messages.Add(details.Message);
            }
        }

        return messages;
    }

    /// <summary>
    ///     Waits until the condition holds or the timeout elapses.
    /// </summary>
    /// <param name="condition">Condition.</param>
    /// <param name="what">Description for the timeout message.</param>
    /// <returns>Task that completes when the condition holds.</returns>
    private static async Task WaitUntil(Func<bool> condition, string what)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            if (condition()) return;
            await Task.Delay(10);
        }

        Assert.Fail($"Timed out waiting for {what}.");
    }

    /// <summary>
    ///     Commit runner test fixture.
    /// </summary>
    /// <param name="Runner">Runner under test.</param>
    /// <param name="System">Job slot system.</param>
    /// <param name="Services">Services.</param>
    /// <param name="Sender">Recording sender.</param>
    /// <param name="Registry">Registry fake.</param>
    /// <param name="Writer">Writer fake.</param>
    /// <param name="Subscriptions">Subscription fake.</param>
    private sealed record CommitFixture(WorldGenCommitRunner Runner,
        WorldGenerationSystem System, WorldGenerationServices Services,
        WorldGenStateManager StateManager, FakeEventSender Sender,
        FakeWorldGenRegistryStore Registry, FakeWorldGenCommitWriter Writer,
        FakeSegmentSubscriptionProbe Subscriptions);
}
