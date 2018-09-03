/*
 * Sovereign Dynamic World MMORPG Engine
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

        /// <summary>
        /// Main loop cycle count.
        /// </summary>
        private ulong cycleCount = 0;

        public EngineBase(IEventLoop eventLoop, TimeManager timeManager,
            WorldManager worldManager, IList<IMainLoopAction> mainLoopActions)
        {
            this.eventLoop = eventLoop;
            this.timeManager = timeManager;
            this.worldManager = worldManager;
            this.mainLoopActions = mainLoopActions;
        }

        public void Run()
        {
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
                eventLoop.PumpEventLoop();

                /* Perform any main loop actions that are ready. */
                PerformMainLoopActions();

                /* Yield to avoid consuming 100% CPU. */
                Thread.Sleep(0);
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
            foreach (IMainLoopAction action in mainLoopActions) {
                if (cycleCount % action.CycleInterval == 0)
                {
                    action.Execute();
                }
            }
        }

    }

}
