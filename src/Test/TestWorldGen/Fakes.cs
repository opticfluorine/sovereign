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
using System.Threading;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Sovereign.WorldGen;
using Sovereign.WorldGen.Output;
using Sovereign.WorldGen.Terrain;

namespace TestWorldGen;

/// <summary>
///     Event loop fake that records registered systems without dispatching events.
/// </summary>
internal sealed class FakeEventLoop : IEventLoop
{
    public void UpdateSystemTime(ulong systemTime)
    {
    }

    public int PumpEventLoop()
    {
        return 0;
    }

    public void RegisterEventSender(IEventSender eventSender)
    {
    }

    public void UnregisterEventSender(IEventSender eventSender)
    {
    }

    public void RegisterSystem(ISystem system)
    {
    }

    public void UnregisterSystem(ISystem system)
    {
    }
}

/// <summary>
///     Event sender fake that records sent events.
/// </summary>
internal sealed class FakeEventSender : IEventSender
{
    /// <summary>
    ///     Lock guarding the recorded event list; senders may run on background threads.
    /// </summary>
    private readonly Lock accessLock = new();

    /// <summary>
    ///     Events that have been sent through this fake.
    /// </summary>
    private readonly List<Event> sentEvents = new();

    /// <summary>
    ///     Snapshot of the recorded events, safe to read while a background job is sending.
    /// </summary>
    public IReadOnlyList<Event> SentEvents
    {
        get
        {
            lock (accessLock)
            {
                return new List<Event>(sentEvents);
            }
        }
    }

    public void SendEvent(Event ev)
    {
        lock (accessLock)
        {
            sentEvents.Add(ev);
        }
    }

    /// <summary>
    ///     Clears all recorded events.
    /// </summary>
    public void Reset()
    {
        lock (accessLock)
        {
            sentEvents.Clear();
        }
    }

    public bool TryGetOutgoingEvent(out Event? ev)
    {
        ev = null;
        return false;
    }
}

/// <summary>
///     World generation pipeline test double that reports phases and returns a minimal plan.
/// </summary>
internal sealed class StubWorldGenPipeline : IWorldGenPipeline
{
    /// <summary>
    ///     Lock guarding test-visible state; <see cref="Plan"/> runs on a background task.
    /// </summary>
    private readonly Lock accessLock = new();

    /// <summary>
    ///     Optional gate that the pipeline waits on before completing.
    /// </summary>
    public ManualResetEventSlim? Gate { get; init; }

    private bool started;

    /// <summary>
    ///     Whether the pipeline has been invoked.
    /// </summary>
    public bool Started
    {
        get
        {
            lock (accessLock)
            {
                return started;
            }
        }
    }

    private readonly List<string> seenPhases = new();

    /// <summary>
    ///     Phases reported through the progress callback.
    /// </summary>
    public IReadOnlyList<string> SeenPhases
    {
        get
        {
            lock (accessLock)
            {
                return new List<string>(seenPhases);
            }
        }
    }

    public WorldGenPlan Plan(WorldGenProfile profile, string profileName, ulong seed, int originX,
        int originY, string previewPath, string stagingDirectory,
        WorldGenResolvedTemplates resolvedTemplates, Action<string>? progress,
        System.Threading.CancellationToken cancellationToken = default)
    {
        List<string> phases = new() { "Terrain", "Hydrology", "Preview", "Assembly" };
        lock (accessLock)
        {
            started = true;
            seenPhases.AddRange(phases);
        }

        foreach (var phase in phases)
        {
            progress?.Invoke(phase);
        }

        Gate?.Wait(TimeSpan.FromSeconds(10));

        return new WorldGenPlan
        {
            Seed = seed,
            ProfileName = profileName,
            Profile = profile,
            OriginX = originX,
            OriginY = originY,
            ResolvedTemplates = resolvedTemplates,
            StagingDirectory = stagingDirectory,
            Terrain = new TerrainMap
            {
                Width = 1,
                Height = 1,
                Heights = new int[1, 1],
                IsOcean = new bool[1, 1],
                IsCliff = new bool[1, 1],
                IsBeach = new bool[1, 1],
                IsRiver = new bool[1, 1],
                RiverWidth = new int[1, 1],
                IsBank = new bool[1, 1],
                IsLake = new bool[1, 1],
                LakeSurfaceZ = new int[1, 1]
            },
            Statistics = new PlanStatistics
            {
                Width = 1,
                Height = 1,
                LandCells = 0,
                WaterCells = 1,
                RiverCount = 0,
                LakeCount = 0,
                LongestStraightRiverRun = 0,
                TerrainMs = 0,
                HydrologyMs = 0,
                BiomesMs = 0,
                PreviewMs = 0,
                TotalMs = 0
            },
            PreviewPath = previewPath
        };
    }
}

/// <summary>
///     Builds populated template name indexers for tests.
/// </summary>
internal static class TestTemplateIndexers
{
    /// <summary>
    ///     Creates a populated template name indexer from the given name-to-ID map.
    /// </summary>
    /// <param name="templates">Template names mapped to template entity IDs.</param>
    /// <returns>Populated indexer.</returns>
    public static TemplateNameComponentIndexer Create(IReadOnlyDictionary<string, ulong> templates)
    {
        var entityTable = new EntityTable();
        var componentManager = new ComponentManager(new EntityNotifier());
        var names = new NameComponentCollection(entityTable, componentManager);
        var indexer = new TemplateNameComponentIndexer(names, new TemplateNameComponentFilter(names));
        foreach (var (name, id) in templates)
        {
            names.AddComponent(id, name);
        }

        names.ApplyComponentUpdates();
        return indexer;
    }

    /// <summary>
    ///     Creates an indexer covering the templates referenced by the shipped default
    ///     profile.
    /// </summary>
    /// <returns>Populated indexer.</returns>
    public static TemplateNameComponentIndexer CreateDefault()
    {
        var names = new[]
        {
            "Bedrock", "Water", "Shale", "Granite", "Basalt", "Gravel", "Sand",
            "Grass", "Dirt", "Sandstone", "Snow", "Oak Tree", "Pine Tree", "Cactus",
            "Acacia Tree", "Boulder", "Dead Bush"
        };
        var templates = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < names.Length; ++i)
        {
            templates[names[i]] = 0x7FFE000000000000UL + (ulong)i;
        }

        return Create(templates);
    }
}

/// <summary>
///     Worldgen world registry store fake with in-memory entries.
/// </summary>
internal sealed class FakeWorldGenRegistryStore : IWorldGenWorldRegistryStore
{
    private readonly Lock accessLock = new();
    private readonly List<WorldGenRegistryEntry> worlds = new();

    public IReadOnlyList<WorldGenRegistryEntry> Worlds
    {
        get
        {
            lock (accessLock)
            {
                return new List<WorldGenRegistryEntry>(worlds);
            }
        }
    }

    public IReadOnlyList<WorldGenRegistryEntry> LoadWorlds()
    {
        lock (accessLock)
        {
            return new List<WorldGenRegistryEntry>(worlds);
        }
    }

    public void AppendWorld(WorldGenRegistryEntry entry)
    {
        lock (accessLock)
        {
            worlds.Add(entry);
        }
    }

    /// <summary>
    ///     Seeds a registered world directly, bypassing the store semantics.
    /// </summary>
    /// <param name="entry">World to seed.</param>
    public void Seed(WorldGenRegistryEntry entry)
    {
        lock (accessLock)
        {
            worlds.Add(entry);
        }
    }
}

/// <summary>
///     Commit writer fake that records requests without touching any database.
/// </summary>
internal sealed class FakeWorldGenCommitWriter : IWorldGenCommitWriter
{
    private readonly Lock accessLock = new();
    private readonly List<WorldGenCommitRequest> requests = new();

    /// <summary>
    ///     Optional gate the writer waits on before completing.
    /// </summary>
    public ManualResetEventSlim? Gate { get; init; }

    /// <summary>
    ///     Recorded commit requests, in call order.
    /// </summary>
    public IReadOnlyList<WorldGenCommitRequest> Requests
    {
        get
        {
            lock (accessLock)
            {
                return new List<WorldGenCommitRequest>(requests);
            }
        }
    }

    public WorldGenCommitStats Execute(WorldGenCommitRequest request)
    {
        lock (accessLock)
        {
            requests.Add(request);
        }

        Gate?.Wait(TimeSpan.FromSeconds(10));
        request.Progress?.Invoke(1, 1);
        return new WorldGenCommitStats
        {
            SegmentsWritten = 12,
            DecorationsCreated = 3,
            DecorationsDeleted = 0,
            WallMs = 5
        };
    }
}

/// <summary>
///     Segment subscription probe fake with per-segment subscriber sets.
/// </summary>
internal sealed class FakeSegmentSubscriptionProbe : ISegmentSubscriptionProbe
{
    private readonly Dictionary<GridPosition, HashSet<ulong>> subscribers = new();

    /// <summary>
    ///     Subscribes a player to a segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="playerEntityId">Player entity ID.</param>
    public void Subscribe(GridPosition segmentIndex, ulong playerEntityId)
    {
        if (!subscribers.TryGetValue(segmentIndex, out var set))
        {
            set = new HashSet<ulong>();
            subscribers[segmentIndex] = set;
        }

        set.Add(playerEntityId);
    }

    public IReadOnlySet<ulong> GetSubscribersForWorldSegment(GridPosition segmentIndex)
    {
        return subscribers.TryGetValue(segmentIndex, out var set)
            ? set
            : new HashSet<ulong>();
    }
}

/// <summary>
///     World generation pipeline test double that parks on a gate and then honors the
///     cancellation token, mimicking a real mid-pipeline abort.
/// </summary>
internal sealed class CancellableStubPipeline : IWorldGenPipeline
{
    /// <summary>Set when the pipeline has reached its parked phase.</summary>
    public ManualResetEventSlim Entered { get; } = new(false);

    /// <summary>Release gate for the parked phase.</summary>
    public ManualResetEventSlim Release { get; } = new(false);

    /// <summary>Staging directory the pipeline was invoked with.</summary>
    public string? StagingDirectorySeen { get; private set; }

    public WorldGenPlan Plan(WorldGenProfile profile, string profileName, ulong seed, int originX,
        int originY, string previewPath, string stagingDirectory,
        WorldGenResolvedTemplates resolvedTemplates, Action<string>? progress,
        System.Threading.CancellationToken cancellationToken = default)
    {
        StagingDirectorySeen = stagingDirectory;
        progress?.Invoke("Terrain");
        Entered.Set();
        Release.Wait(TimeSpan.FromSeconds(10));
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Invoke("Hydrology");

        return new WorldGenPlan
        {
            Seed = seed,
            ProfileName = profileName,
            Profile = profile,
            OriginX = originX,
            OriginY = originY,
            ResolvedTemplates = resolvedTemplates,
            StagingDirectory = stagingDirectory,
            Terrain = new TerrainMap
            {
                Width = 1,
                Height = 1,
                Heights = new int[1, 1],
                IsOcean = new bool[1, 1],
                IsCliff = new bool[1, 1],
                IsBeach = new bool[1, 1],
                IsRiver = new bool[1, 1],
                RiverWidth = new int[1, 1],
                IsBank = new bool[1, 1],
                IsLake = new bool[1, 1],
                LakeSurfaceZ = new int[1, 1]
            },
            Statistics = new PlanStatistics
            {
                Width = 1,
                Height = 1,
                LandCells = 0,
                WaterCells = 1,
                RiverCount = 0,
                LakeCount = 0,
                LongestStraightRiverRun = 0,
                TerrainMs = 0,
                HydrologyMs = 0,
                BiomesMs = 0,
                PreviewMs = 0,
                TotalMs = 0
            },
            PreviewPath = previewPath
        };
    }
}

/// <summary>
///     Commit writer test double that parks on a gate after its first batch and then
///     honors the request's cancellation token.
/// </summary>
internal sealed class GatedCancellableCommitWriter : IWorldGenCommitWriter
{
    /// <summary>Set when the writer has committed its first batch.</summary>
    public ManualResetEventSlim Entered { get; } = new(false);

    /// <summary>Release gate for the parked phase.</summary>
    public ManualResetEventSlim Release { get; } = new(false);

    /// <summary>Number of Execute invocations.</summary>
    public int Calls { get; private set; }

    /// <summary>Phases reported through the request's phase callback.</summary>
    public List<string> Phases { get; } = new();

    public WorldGenCommitStats Execute(WorldGenCommitRequest request)
    {
        ++Calls;
        request.Phase?.Invoke("writing segments");
        request.Progress?.Invoke(1, 2);
        Entered.Set();
        Release.Wait(TimeSpan.FromSeconds(10));
        request.CancellationToken.ThrowIfCancellationRequested();

        request.Phase?.Invoke("writing decorations");
        request.Progress?.Invoke(2, 2);
        return new WorldGenCommitStats
        {
            SegmentsWritten = 1,
            DecorationsCreated = 0,
            DecorationsDeleted = 0,
            WallMs = 1
        };
    }
}

/// <summary>
///     World generation pipeline test double that always throws.
/// </summary>
internal sealed class ThrowingWorldGenPipeline : IWorldGenPipeline
{
    private readonly Exception exception;

    public ThrowingWorldGenPipeline(Exception exception)
    {
        this.exception = exception;
    }

    public WorldGenPlan Plan(WorldGenProfile profile, string profileName, ulong seed, int originX,
        int originY, string previewPath, string stagingDirectory,
        WorldGenResolvedTemplates resolvedTemplates, Action<string>? progress,
        System.Threading.CancellationToken cancellationToken = default)
    {
        throw exception;
    }
}
