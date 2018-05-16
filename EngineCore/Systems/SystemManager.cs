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

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Castle.Core;
using Castle.Core.Logging;

namespace Engine8.EngineCore.Systems
{

    /// <summary>
    /// Manages the collection of System objects.
    /// </summary>
    public class SystemManager : IStartable
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Systems to be managed.
        /// </summary>
        private readonly IList<ISystem> systemList;

        /// <summary>
        /// Running tasks.
        /// </summary>
        private readonly IList<Task> tasks = new List<Task>();

        /// <summary>
        /// Creates a new SystemManager with the given update step.
        /// </summary>
        /// <param name="systemList">Systems to be managed.</param>
        public SystemManager(IList<ISystem> systemList)
        {
            this.systemList = systemList;
        }

        public void Start()
        {
            Logger.Info("Starting SystemManager.");

            var taskQuery = from system in systemList
                            select RunSingleSystem(system);
        }

        public void Stop()
        {
            Logger.Info("Stopping SystemManager.");
        }

        /// <summary>
        /// Runs a single system in a separate thread.
        /// </summary>
        /// <param name="system">System to run.</param>
        private Task RunSingleSystem(ISystem system)
        {
            var task = Task.Run(() =>
           {
               system.Initialize();
               system.Run();
               system.Cleanup();
           });
            return task;
        }

    }
}
