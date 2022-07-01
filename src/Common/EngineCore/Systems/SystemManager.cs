/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Castle.Core.Logging;
using System.Threading;
using System.Text;

namespace Sovereign.EngineCore.Systems
{

    /// <summary>
    /// Manages the collection of System objects.
    /// </summary>
    public class SystemManager : IStartable
    {

        /// <summary>
        /// Number of system executor threads.
        /// </summary>
        private const int EXECUTOR_COUNT = 2;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Systems to be managed.
        /// </summary>
        private readonly IList<ISystem> systemList;

        /// <summary>
        /// System executor factory.
        /// </summary>
        private readonly ISystemExecutorFactory systemExecutorFactory;

        /// <summary>
        /// Executors.
        /// </summary>
        private readonly IList<SystemExecutor> executors = new List<SystemExecutor>();

        /// <summary>
        /// Executor threads.
        /// </summary>
        private readonly IList<Thread> threads = new List<Thread>();

        /// <summary>
        /// Creates a new SystemManager with the given update step.
        /// </summary>
        /// <param name="systemList">Systems to be managed.</param>
        /// <param name="systemExecutorFactory">System executor factory.</param>
        public SystemManager(IList<ISystem> systemList,
            ISystemExecutorFactory systemExecutorFactory)
        {
            this.systemList = systemList;
            this.systemExecutorFactory = systemExecutorFactory;
        }

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
            foreach (var thread in threads)
            {
                thread.Join();
            }

            /* Release the executor resources. */
            foreach (var executor in executors)
            {
                systemExecutorFactory.Release(executor);
            }
        }

        /// <summary>
        /// Creates the SystemExecutors.
        /// </summary>
        private void CreateExecutors()
        {
            for (int i = 0; i < EXECUTOR_COUNT; ++i)
            {
                var executor = systemExecutorFactory.Create();
                executors.Add(executor);
            }
        }

        /// <summary>
        /// Partitions the systems across the SystemExecutors.
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
        /// Runs the SystemExecutors in their own threads.
        /// </summary>
        private void RunExecutors()
        {
            int count = 0;
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
}
