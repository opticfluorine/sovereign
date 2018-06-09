/*
 * Engine8 Dynamic World MMORPG Engine
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

using Castle.Core.Logging;
using Engine8.EngineCore.Events;
using System.Collections.Generic;
using System.Text;

namespace Engine8.EngineCore.Systems
{

    /// <summary>
    /// Responsible for executing a group of systems.
    /// </summary>
    public class SystemExecutor
    {

        public ILogger Log { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Systems managed by this executor.
        /// </summary>
        private readonly IList<ISystem> systems = new List<ISystem>();

        /// <summary>
        /// Event loop.
        /// </summary>
        private readonly IEventLoop eventLoop;

        public SystemExecutor(IEventLoop eventLoop)
        {
            this.eventLoop = eventLoop;
        }

        /// <summary>
        /// Adds a system to the executor.
        /// </summary>
        /// <param name="system">System to be managed.</param>
        public void AddSystem(ISystem system)
        {
            systems.Add(system);
        }

        /// <summary>
        /// Executes the systems. This should be invoked from its own thread.
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
        /// Initializes all systems managed by this executor.
        /// </summary>
        private void InitializeSystems()
        {
            Log.Info(FormatStartupLogMessage());

            foreach (var system in systems)
            {
                system.Initialize();
            }
        }

        /// <summary>
        /// Executes the systems continuously until the main loop is terminated.
        /// </summary>
        private void ExecuteSystems()
        {
            while (!eventLoop.Terminated)
            {
                foreach (var system in systems)
                {
                    system.ExecuteOnce();
                }
            }
        }

        /// <summary>
        /// Cleans up all systems managed by this executor.
        /// </summary>
        private void CleanupSystems()
        {
            Log.Info("SystemExecutor is shutting down.");

            foreach (var system in systems)
            {
                system.Cleanup();
            }
        }

        /// <summary>
        /// Formats a log message summarizing the executor loadout.
        /// </summary>
        /// <returns>Log message.</returns>
        private string FormatStartupLogMessage()
        {
            var sb = new StringBuilder();

            sb.Append("SystemExecutor is starting with the following systems:");
            foreach (var system in systems)
            {
                sb.Append("\n - ").Append(system.GetType().Name)
                    .Append(" (Workload = ").Append(system.WorkloadEstimate)
                    .Append(")");
            }

            return sb.ToString();
        }

    }

}
