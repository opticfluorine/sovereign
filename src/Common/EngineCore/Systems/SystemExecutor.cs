/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;

namespace Sovereign.EngineCore.Systems;

/// <summary>
///     Responsible for executing a group of systems.
/// </summary>
public class SystemExecutor
{
    /// <summary>
    ///     An executor thread will sleep if it processes less than this number
    ///     of events in a single pass. This reduces system load under light
    ///     conditions in exchange for a temporary increase in event latency.
    /// </summary>
    private const int ThreadYieldEventLimit = 0;

    /// <summary>
    ///     Event loop.
    /// </summary>
    private readonly IEventLoop eventLoop;

    /// <summary>
    ///     Systems managed by this executor.
    /// </summary>
    private readonly List<ISystem> systems = new();

    public SystemExecutor(IEventLoop eventLoop)
    {
        this.eventLoop = eventLoop;
    }

    public ILogger Log { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Adds a system to the executor.
    /// </summary>
    /// <param name="system">System to be managed.</param>
    public void AddSystem(ISystem system)
    {
        systems.Add(system);
    }

    /// <summary>
    ///     Executes the systems. This should be invoked from its own thread.
    /// </summary>
    public void Execute()
    {
        /* Ensure that at least one system is being managed. */
        if (systems.Count == 0)
        {
            Log.Info("No systems to manage, this SystemExecutor will terminate.");
            return;
        }

        InitializeSystems();
        ExecuteSystems();
        CleanupSystems();
    }

    /// <summary>
    ///     Initializes all systems managed by this executor.
    /// </summary>
    private void InitializeSystems()
    {
        Log.Info(FormatStartupLogMessage());

        foreach (var system in systems) system.Initialize();
    }

    /// <summary>
    ///     Executes the systems continuously until the main loop is terminated.
    /// </summary>
    private void ExecuteSystems()
    {
        while (!eventLoop.Terminated)
        {
            // Keep track of events processed for later load balancing.
            var eventsProcessed = 0;

            // Execute all systems.
            foreach (var system in systems)
                try
                {
                    eventsProcessed += system.ExecuteOnce();
                }
                catch (Exception e)
                {
                    Log.Error("Unhandled exception in system.", e);
                }

            // Check if this thread's workload is light.
            if (eventsProcessed < ThreadYieldEventLimit)
                // This thread's current workload is light, so yield the thread
                // to the OS. This reduces our utilization in exchange for a
                // temporary increase in event latency on this thread.
                Thread.Sleep(1);
        }
    }

    /// <summary>
    ///     Cleans up all systems managed by this executor.
    /// </summary>
    private void CleanupSystems()
    {
        Log.Info("SystemExecutor is shutting down.");

        foreach (var system in systems) system.Cleanup();
    }

    /// <summary>
    ///     Formats a log message summarizing the executor loadout.
    /// </summary>
    /// <returns>Log message.</returns>
    private string FormatStartupLogMessage()
    {
        var sb = new StringBuilder();

        sb.Append("SystemExecutor is starting with the following systems:");
        foreach (var system in systems)
            sb.Append("\n - ").Append(system.GetType().Name)
                .Append(" (Workload = ").Append(system.WorkloadEstimate)
                .Append(")");

        return sb.ToString();
    }
}