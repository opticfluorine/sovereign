﻿/*
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

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineCore.World;
using System.Collections.Generic;
using System.Threading;

namespace Sovereign.EngineCore.Main
{

    /// <summary>
    /// Runs the engine.
    /// </summary>
    public class EngineBase : IEngineBase
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Event loop.
        /// </summary>
        private readonly IEventLoop eventLoop;

        /// <summary>
        /// Time manager.
        /// </summary>
        private readonly TimeManager timeManager;

        /// <summary>
        /// World manager.
        /// </summary>
        private readonly WorldManager worldManager;

        private readonly IList<IMainLoopAction> mainLoopActions;
        private readonly EventDescriptions eventDescriptions;

        /// <summary>
        /// If the main loop processes less than this many events in a single pass,
        /// the main thread will sleep instead of yielding its timeslice. This reduces
        /// CPU utilization during light workloads in exchange for a temporary increase
        /// in main loop latency.
        /// </summary>
        /// <remarks>
        /// This can be set to zero to disable.
        /// </remarks>
        private const int ThreadSleepEventLimit = 2;

        /// <summary>
        /// Main loop cycle count.
        /// </summary>
        private ulong cycleCount = 0;

        public EngineBase(IEventLoop eventLoop, TimeManager timeManager,
            WorldManager worldManager, IList<IMainLoopAction> mainLoopActions,
            ConsoleEventAdapter eventAdapter, EventDescriptions eventDescriptions)
        {
            this.eventLoop = eventLoop;
            this.timeManager = timeManager;
            this.worldManager = worldManager;
            this.mainLoopActions = mainLoopActions;
            this.eventDescriptions = eventDescriptions;
        }

        public void Run()
        {
            /* Output diagnostic info. */
            LogDiagnostics();

            /* Start the engine. */
            Logger.Info("EngineBase is starting.");
            Startup();
            Logger.Info("EngineBase is started.");

            /* Run the engine. */
            RunEngine();

            /* Stop the engine. */
            Logger.Info("EngineBase is stopping.");
            Shutdown();
            Logger.Info("EngineBase is stopped.");
        }

        /// <summary>
        /// Starts the engine.
        /// </summary>
        private void Startup()
        {

        }

        /// <summary>
        /// Runs the engine.
        /// </summary>
        private void RunEngine()
        {
            /* Enter the main loop. */
            while (!eventLoop.Terminated)
            {
                /* Process any timed actions on the main thread. */
                timeManager.AdvanceTime();

                /* Drive the event loop. */
                var eventsProcessed = eventLoop.PumpEventLoop();

                /* Perform any main loop actions that are ready. */
                PerformMainLoopActions();

                /* Yield to avoid consuming 100% CPU. */
                Thread.Sleep(eventsProcessed < ThreadSleepEventLimit ? 1 : 0);
            }
        }

        /// <summary>
        /// Stops the engine.
        /// </summary>
        private void Shutdown()
        {

        }

        /// <summary>
        /// Performs any main loop actions that are ready for execution.
        /// </summary>
        private void PerformMainLoopActions()
        {
            cycleCount++;
            foreach (IMainLoopAction action in mainLoopActions)
            {
                if (cycleCount % action.CycleInterval == 0)
                {
                    action.Execute();
                }
            }
        }

        /// <summary>
        /// Logs startup diagnostic information.
        /// </summary>
        private void LogDiagnostics()
        {
            ThreadPool.GetMaxThreads(out int workerThreads, out int completionPortThreads);
            Logger.DebugFormat("Maximum worker threads = {0}.", workerThreads);
            Logger.DebugFormat("Maximum I/O completion threads = {0}.", completionPortThreads);

            eventDescriptions.LogDebugInfo();
        }

    }

}
