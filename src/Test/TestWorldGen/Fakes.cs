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
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
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
        int originY, string previewPath, Action<string>? progress)
    {
        List<string> phases = new() { "Terrain", "Hydrology", "Preview" };
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
            OriginX = originX,
            OriginY = originY,
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
        int originY, string previewPath, Action<string>? progress)
    {
        throw exception;
    }
}
