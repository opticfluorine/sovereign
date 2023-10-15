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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Castle.Core;
using Castle.Core.Logging;

namespace Sovereign.EngineCore.Systems;

/// <summary>
///     Manages the collection of System objects.
/// </summary>
public class SystemManager : IStartable
{
    /// <summary>
    ///     Number of system executor threads.
    /// </summary>
    private const int EXECUTOR_COUNT = 1;

    /// <summary>
    ///     Executors.
    /// </summary>
    private readonly List<SystemExecutor> executors = new();

    /// <summary>
    ///     System executor factory.
    /// </summary>
    private readonly ISystemExecutorFactory systemExecutorFactory;

    /// <summary>
    ///     Systems to be managed.
    /// </summary>
    private readonly IList<ISystem> systemList;

    /// <summary>
    ///     Executor threads.
    /// </summary>
    private readonly List<Thread> threads = new();

    /// <summary>
    ///     Creates a new SystemManager with the given update step.
    /// </summary>
    /// <param name="systemList">Systems to be managed.</param>
    /// <param name="systemExecutorFactory">System executor factory.</param>
    public SystemManager(IList<ISystem> systemList,
        ISystemExecutorFactory systemExecutorFactory)
    {
        this.systemList = systemList;
        this.systemExecutorFactory = systemExecutorFactory;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void Start()
    {
        Logger.Info("Starting SystemManager.");

        /* Create executors. */
        CreateExecutors();

        /* Partition systems across the executors. */
        PartitionSystems();

        /* Run the executors. */
        RunExecutors();
    }

    public void Stop()
    {
        Logger.Info("Stopping SystemManager.");

        /* Join on the executor threads. */
        foreach (var thread in threads) thread.Join();

        /* Release the executor resources. */
        foreach (var executor in executors) systemExecutorFactory.Release(executor);
    }

    /// <summary>
    ///     Creates the SystemExecutors.
    /// </summary>
    private void CreateExecutors()
    {
        for (var i = 0; i < EXECUTOR_COUNT; ++i)
        {
            var executor = systemExecutorFactory.Create();
            executors.Add(executor);
        }
    }

    /// <summary>
    ///     Partitions the systems across the SystemExecutors.
    /// </summary>
    private void PartitionSystems()
    {
        /* Iterate over the systems in order of decreasing workload. */
        var rankedSystems = systemList.OrderByDescending(system => system.WorkloadEstimate);
        var count = 0;
        foreach (var system in rankedSystems)
        {
            /* Cycle through the executors round-robin style. */
            var executor = executors[count % EXECUTOR_COUNT];
            executor.AddSystem(system);

            count++;
        }
    }

    /// <summary>
    ///     Runs the SystemExecutors in their own threads.
    /// </summary>
    private void RunExecutors()
    {
        var count = 0;
        var sb = new StringBuilder();
        foreach (var executor in executors)
        {
            /* Generate a name for the executor. */
            sb.Clear();
            sb.Append("Executor ").Append(count);

            /* Start the executor thread. */
            var executorThread = new Thread(executor.Execute)
            {
                Name = sb.ToString()
            };
            executorThread.Start();
            threads.Add(executorThread);

            count++;
        }
    }
}