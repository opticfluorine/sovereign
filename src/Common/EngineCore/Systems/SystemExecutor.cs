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
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;

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

    private readonly ILogger<SystemExecutor> logger;

    /// <summary>
    ///     Systems managed by this executor.
    /// </summary>
    private readonly List<ISystem> systems = new();

    private readonly Terminator terminator;

    /// <summary>
    ///     Executor loop iteration count.
    /// </summary>
    private uint iterationCount;

    public SystemExecutor(IEventLoop eventLoop, IEngineConfiguration engineConfiguration,
        ILogger<SystemExecutor> logger, Terminator terminator)
    {
        this.eventLoop = eventLoop;
        this.engineConfiguration = engineConfiguration;
        this.logger = logger;
        this.terminator = terminator;
    }

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
    public void Execute(CancellationToken cancellationToken)
    {
        /* Ensure that at least one system is being managed. */
        if (systems.Count == 0)
        {
            logger.LogInformation("No systems to manage, this SystemExecutor will terminate.");
            return;
        }

        InitializeSystems();
        ExecuteSystems(cancellationToken);
        CleanupSystems();
    }

    /// <summary>
    ///     Initializes all systems managed by this executor.
    /// </summary>
    private void InitializeSystems()
    {
        logger.LogInformation(FormatStartupLogMessage());

        foreach (var system in systems) system.Initialize();
    }

    /// <summary>
    ///     Executes the systems continuously until the main loop is terminated.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to signal shutdown.</param>
    private void ExecuteSystems(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && !terminator.TerminationToken.IsCancellationRequested)
        {
            // Execute all systems.
            foreach (var system in systems)
                try
                {
                    system.ExecuteOnce();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Unhandled exception in SystemExecutor.");
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
        logger.LogInformation("SystemExecutor is shutting down.");

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