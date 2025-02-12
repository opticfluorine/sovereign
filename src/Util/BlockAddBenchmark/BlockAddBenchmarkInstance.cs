// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.ServerCore.Timing;

namespace BlockAddBenchmark;

public class BlockAddBenchmarkInstance
{
    /// <summary>
    ///     How many component update cycles to perform in the warmup phase.
    /// </summary>
    private const int WarmupCycleCount = 100;

    /// <summary>
    ///     How many component update cycles to perform in the measurement phase.
    /// </summary>
    private const int MeasuredCycleCount = 100;

    private readonly BlockPositionComponentCollection blockPositions;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;

    private readonly List<ulong> runtimesUs = new(MeasuredCycleCount);
    private readonly int size;
    private readonly ServerSystemTimer timer = new();

    public BlockAddBenchmarkInstance(int size)
    {
        this.size = size;

        var entityTable = new EntityTable();
        var entityNotifier = new EntityNotifier();
        var componentManager = new ComponentManager(entityNotifier);
        blockPositions = new BlockPositionComponentCollection(entityTable, componentManager);
        materials = new MaterialComponentCollection(entityTable, componentManager);
        materialModifiers = new MaterialModifierComponentCollection(entityTable, componentManager);
    }

    public RuntimeStatistics Run()
    {
        Warmup();
        Measurement();
        return BuildStatistics();
    }

    private void Warmup()
    {
        for (var i = 0; i < WarmupCycleCount; ++i)
        {
            Reset();
            DoRun();
        }
    }

    private void Measurement()
    {
        runtimesUs.Clear();

        // Force a GC collection before executing the benchmark so that we start from a clean state.
        GC.Collect();

        if (!GC.TryStartNoGCRegion(1000000)) throw new Exception("Failed to halt GC during benchmark.");
        for (var i = 0; i < MeasuredCycleCount; ++i)
        {
            // Run a single timed measurement.
            Reset();
            var startTimeUs = timer.GetTime();
            DoRun();
            var endTimeUs = timer.GetTime();

            var elapsedUs = endTimeUs - startTimeUs;
            runtimesUs.Add(elapsedUs);
        }

        GC.EndNoGCRegion();
    }

    private void Reset()
    {
        blockPositions.Clear();
        materials.Clear();
        materialModifiers.Clear();
    }

    private void DoRun()
    {
        for (var i = 0; i < size; ++i)
        {
            blockPositions.AddComponent((ulong)i, new GridPosition { X = i, Y = 0, Z = 0 });
            materials.AddComponent((ulong)i, 1);
            materialModifiers.AddComponent((ulong)i, 0);

            blockPositions.ApplyComponentUpdates();
            materials.ApplyComponentUpdates();
            materialModifiers.ApplyComponentUpdates();
        }
    }

    private RuntimeStatistics BuildStatistics()
    {
        return new RuntimeStatistics
        {
            MinRuntimeUs = runtimesUs.Min(),
            MaxRuntimeUs = runtimesUs.Max(),
            MeanRuntimeUs = (double)runtimesUs.Aggregate((a, b) => a + b) / MeasuredCycleCount,
            RunCount = MeasuredCycleCount
        };
    }

    public class RuntimeStatistics
    {
        public ulong MaxRuntimeUs;
        public double MeanRuntimeUs;
        public ulong MinRuntimeUs;
        public int RunCount;
    }
}