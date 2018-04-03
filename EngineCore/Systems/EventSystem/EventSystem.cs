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

using System;
using log4net;

namespace Engine8.EngineCore.Systems.EventSystem
{

    /// <summary>
    /// State-updating system that pumps the event loop.
    /// </summary>
    public class EventSystem : ISystem
    {

        /// <summary>
        /// Class-level log.
        /// </summary>
        private static readonly ILog LOG = LogManager.GetLogger(typeof(EventSystem));

        /// <summary>
        /// Main event loop.
        /// </summary>
        private MainEventLoop EventLoop;

        /// <summary>
        /// The system manager that owns this system.
        /// </summary>
        private SystemManager SystemManager;

        public void InitializeSystem(SystemManager manager)
        {
            /* Set the dependencies. */
            SystemManager = manager;

            /* Create the event loop. */
            EventLoop = new MainEventLoop();

            /* Register the dispatcher and loop with the manager. */
            try
            {
                SystemManager.RegisterEventFramework(EventLoop);
            }
            catch (InvalidOperationException)
            {
                LOG.Error("Another system already registered an event framework.");
                throw new SystemInitializationException("Event framework already registered");
            }
        }

        public void CleanupSystem()
        {
            
        }

        public void DoUpdate(ulong systemTime)
        {
            /* 
             * Advance system time and dispatch events.
             * The queue will be cleared of any immediately-generated
             * events through calls from the main loop.
             */
            EventLoop.PumpEventLoop(systemTime);
        }

        /* This service does not perform rendering. */
        public void DoRender(ulong timeSinceUpdate)
        {
            throw new NotImplementedException();
        }

    }

}
