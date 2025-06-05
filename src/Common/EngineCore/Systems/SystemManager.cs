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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Configuration;

namespace Sovereign.EngineCore.Systems;

/// <summary>
///     Manages the collection of System objects.
/// </summary>
public class SystemManager : BackgroundService
{
    private readonly List<ExecutorScope> executors = new();
    private readonly List<Task> executorTasks = new();
    private readonly ILogger<SystemManager> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IEnumerable<ISystem> systems;
    private readonly PerformanceOptions performanceOptions;

    public SystemManager(IEnumerable<ISystem> systems,
        ILogger<SystemManager> logger, IServiceScopeFactory serviceScopeFactory, 
        IOptions<PerformanceOptions> performanceOptions)
    {
        this.systems = systems;
        this.logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;
        this.performanceOptions = performanceOptions.Value;
    }

    public override void Dispose()
    {
        foreach (var scope in executors)
        {
            scope.ServiceScope.Dispose();
        }

        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SystemManager started.");

        /* Create executors. */
        CreateExecutors();

        /* Partition systems across the executors. */
        PartitionSystems();

        /* Run the executors. */
        await RunExecutors(stoppingToken);

        logger.LogInformation("SystemManager stopped. Exiting application.");
        Environment.Exit(0);
    }

    /// <summary>
    ///     Creates the SystemExecutors.
    /// </summary>
    private void CreateExecutors()
    {
        for (var i = 0; i < performanceOptions.SystemExecutorCount; ++i)
        {
            var scope = serviceScopeFactory.CreateScope();
            executors.Add(new ExecutorScope
            {
                ServiceScope = scope,
                SystemExecutor = scope.ServiceProvider.GetService<SystemExecutor>() ??
                                 throw new Exception("Got null executor.")
            });
        }
    }

    /// <summary>
    ///     Partitions the systems across the SystemExecutors.
    /// </summary>
    private void PartitionSystems()
    {
        /* Iterate over the systems in order of decreasing workload. */
        var rankedSystems = systems.OrderByDescending(system => system.WorkloadEstimate);
        var count = 0;
        foreach (var system in rankedSystems)
        {
            /* Cycle through the executors round-robin style. */
            var executor = executors[count % performanceOptions.SystemExecutorCount].SystemExecutor;
            executor.AddSystem(system);

            count++;
        }
    }

    /// <summary>
    ///     Runs the SystemExecutors in their own threads.
    /// </summary>
    private Task RunExecutors(CancellationToken shutdownToken)
    {
        foreach (var executor in executors)
        {
            executorTasks.Add(Task.Run(() => executor.SystemExecutor.Execute(shutdownToken)));
        }

        return Task.WhenAll(executorTasks);
    }

    /// <summary>
    ///     Executor scope wrapper.
    /// </summary>
    private readonly record struct ExecutorScope : IDisposable
    {
        public required SystemExecutor SystemExecutor { get; init; }
        public required IServiceScope ServiceScope { get; init; }

        public void Dispose()
        {
            ServiceScope.Dispose();
        }
    }
}