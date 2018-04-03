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

using log4net;

namespace Engine8.EngineCore.Systems.Display
{

    /// <summary>
    /// System that manages the display.
    /// </summary>
    public class DisplaySystem : ISystem
    {

        /// <summary>
        /// Class-level logger.
        /// </summary>
        private static readonly ILog LOG = LogManager.GetLogger(typeof(DisplaySystem));

        /// <summary>
        /// Main display manager.
        /// </summary>
        private DisplayManager DisplayManager { get; set; }

        /// <summary>
        /// System manager.
        /// </summary>
        private SystemManager SystemManager;

        public void InitializeSystem(SystemManager systemManager)
        {
            LOG.Info("Initializing Display system");

            // Store dependencies
            SystemManager = systemManager;

            // Create display manager
            LOG.Debug("Creating DisplayManager");
            DisplayManager = new DisplayManager();
            DisplayManager.StartDisplay(1024, 768, false);
            LOG.Debug("DisplayManager created");

            LOG.Info("Display system initialization complete");
        }

        public void CleanupSystem()
        {
            LOG.Info("Cleaning up Display system");

            // Stop display manager
            LOG.Debug("Stopping DisplayManager");
            DisplayManager.StopDisplay();
            LOG.Debug("DisplayManager stopped");

            LOG.Info("Display system cleanup complete");
        }

        public void DoUpdate(ulong systemTime)
        {

        }

        public void DoRender(ulong timeToNextUpdate)
        {

        }

    }

}
