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

namespace Sovereign.EngineCore.Systems;

/// <summary>
///     Manages the collection of System objects.
/// </summary>
public class SystemManager : IHostedService, IDisposable
{
    /// <summary>
    ///     Number of system executor threads.
    /// </summary>
    private const int ExecutorCount = 1;

    private readonly List<ExecutorScope> executors = new();
    private readonly List<Task> executorTasks = new();
    private readonly ILogger<SystemManager> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly CancellationTokenSource shutdownTokenSource = new();
    private readonly IEnumerable<ISystem> systems;

    public SystemManager(IEnumerable<ISystem> systems,
        ILogger<SystemManager> logger, IServiceScopeFactory serviceScopeFactory)
    {
        this.systems = systems;
        this.logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;
    }

    public void Dispose()
    {
        foreach (var scope in executors)
        {
            scope.ServiceScope.Dispose();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping SystemManager.");
        shutdownTokenSource.Cancel();
        await Task.WhenAll(executorTasks);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting SystemManager.");

        /* Create executors. */
        CreateExecutors();

        /* Partition systems across the executors. */
        PartitionSystems();

        /* Run the executors. */
        await RunExecutors();
    }

    /// <summary>
    ///     Creates the SystemExecutors.
    /// </summary>
    private void CreateExecutors()
    {
        for (var i = 0; i < ExecutorCount; ++i)
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
            var executor = executors[count % ExecutorCount].SystemExecutor;
            executor.AddSystem(system);

            count++;
        }
    }

    /// <summary>
    ///     Runs the SystemExecutors in their own threads.
    /// </summary>
    private async Task RunExecutors()
    {
        foreach (var executor in executors)
        {
            executorTasks.Add(Task.Run(() => executor.SystemExecutor.Execute(shutdownTokenSource.Token)));
        }

        await Task.WhenAll(executorTasks);
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