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
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Events;

namespace Sovereign.EngineCore.Systems;

/// <summary>
///     Responsible for executing a group of systems.
/// </summary>
public class SystemExecutor
{
    private readonly IEngineConfiguration engineConfiguration;

    /// <summary>
    ///     Event loop.
    /// </summary>
    private readonly IEventLoop eventLoop;

    /// <summary>
    ///     Systems managed by this executor.
    /// </summary>
    private readonly List<ISystem> systems = new();

    /// <summary>
    ///     Executor loop iteration count.
    /// </summary>
    private uint iterationCount;

    public SystemExecutor(IEventLoop eventLoop, IEngineConfiguration engineConfiguration)
    {
        this.eventLoop = eventLoop;
        this.engineConfiguration = engineConfiguration;
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
            // Execute all systems.
            foreach (var system in systems)
                try
                {
                    system.ExecuteOnce();
                }
                catch (Exception e)
                {
                    Log.Error("Unhandled exception in system.", e);
                }

            iterationCount++;
            if (engineConfiguration.ExecutorThreadSleepInterval > 0 &&
                iterationCount == engineConfiguration.ExecutorThreadSleepInterval)
            {
                iterationCount = 0;
                Thread.Sleep(1);
            }
            else
            {
                Thread.Yield();
            }
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