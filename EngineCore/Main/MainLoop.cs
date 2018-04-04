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

using Engine8.EngineCore.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.EngineCore.Main
{

    /// <summary>
    /// Main loop that drives the systems.
    /// </summary>
    public class MainLoop
    {

        /// <summary>
        /// Flag indicating that the loop is to exit.
        /// </summary>
        private bool IsDone { get; set; }

        /// <summary>
        /// SystemManager governed by this main loop.
        /// </summary>
        private readonly SystemManager SystemManager;

        /// <summary>
        /// Creates a main loop to govern the given SystemManager.
        /// </summary>
        /// <param name="systemManager">SystemManager to be governed.</param>
        public MainLoop(SystemManager systemManager)
        {
            SystemManager = systemManager;
        }

        /// <summary>
        /// Called to execute the main loop.
        /// </summary>
        public void Execute()
        {
            /* Reset the termination flag prior to entering the loop. */
            IsDone = false;

            /* Loop... */
            while (!IsDone)
            {

                /* Yield to the OS to avoid 100% CPU usage when not needed. */
                
            }
        }

    }
}
