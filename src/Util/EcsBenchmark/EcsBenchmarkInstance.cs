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

using System.Numerics;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.ServerCore.Timing;

namespace Sovereign.EcsBenchmark;

/// <summary>
///     Simple benchmark for ECS performance that looks at bulk updates of a kinematics component collection.
/// </summary>
public class EcsBenchmarkInstance
{
    /// <summary>
    ///     How many component update cycles to perform in the warmup phase.
    /// </summary>
    private const int WarmupCycleCount = 1000;

    /// <summary>
    ///     How many component update cycles to perform in the measurement phase.
    /// </summary>
    private const int MeasuredCycleCount = 500;

    private readonly KinematicComponentCollection kinematics;

    private readonly List<ulong> runtimesUs = new(MeasuredCycleCount);
    private readonly ServerSystemTimer timer = new();

    public EcsBenchmarkInstance(int size)
    {
        // Stand up a kinematics component collection with the requested number of components.
        var entityTable = new EntityTable();
        var entityNotifier = new EntityNotifier();
        var componentManager = new ComponentManager(entityNotifier);
        kinematics = new KinematicComponentCollection(entityTable, componentManager);

        for (var i = 0; i < size; ++i)
            kinematics.AddComponent((ulong)i, new Kinematics
            {
                Position = Vector3.Zero,
                Velocity = Vector3.One
            });

        kinematics.ApplyComponentUpdates();
        kinematics.OnBeginDirectAccess += OnDirectAccess;
    }

    public RuntimeStatistics Run()
    {
        Warmup();
        Measurement();
        return BuildStatistics();
    }

    private void Warmup()
    {
        for (var i = 0; i < WarmupCycleCount; ++i) kinematics.ApplyComponentUpdates();
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
            var startTimeUs = timer.GetTime();
            kinematics.ApplyComponentUpdates();
            var endTimeUs = timer.GetTime();

            var elapsedUs = endTimeUs - startTimeUs;
            runtimesUs.Add(elapsedUs);
        }

        GC.EndNoGCRegion();
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

    private int OnDirectAccess(int[] modifiedIndices)
    {
        var componentList = kinematics.Components;
        var count = kinematics.ComponentCount;
        var directMods = 0;
        for (var i = 0; i < count; ++i)
        {
            // Don't bother with a timestep for the benchmark, just accumulate the position subcomponent.
            // The benchmark runs about 6.6% faster in Release using + instead of +=.
            componentList[i].Position = componentList[i].Position + componentList[i].Velocity;
            modifiedIndices[directMods++] = i;
        }

        return directMods;
    }

    public class RuntimeStatistics
    {
        public ulong MaxRuntimeUs;
        public double MeanRuntimeUs;
        public ulong MinRuntimeUs;
        public int RunCount;
    }
}