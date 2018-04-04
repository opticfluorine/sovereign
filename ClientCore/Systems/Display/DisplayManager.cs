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

namespace Engine8.ClientCore.Systems.Display
{

    /// <summary>
    /// Top-level manager for the display components including the
    /// main window, the Direct3D manager, etc.
    /// </summary>
    class DisplayManager
    {

        /// <summary>
        /// Class-level logger.
        /// </summary>
        private static readonly ILog LOG = LogManager.GetLogger(typeof(DisplayManager));

        public DisplayManager()
        {
            
        }

        /// <summary>
        /// Starts the display with the given resolution.
        /// </summary>
        /// <param name="width">Display width in pixels.</param>
        /// <param name="height">Display height in pixels.</param>
        /// <param name="fullscreen">Whether to run in fullscreen mode.</param>
        public void StartDisplay(int width, int height, bool fullscreen)
        {
            // Create the main window
            CreateMainWindow(width, height, fullscreen);
        }

        /// <summary>
        /// Stops the display.
        /// </summary>
        public void StopDisplay()
        {

        }

        private void CreateMainWindow(int width, int height, bool fullscreen)
        {
            // Create and display main window
            LOG.Debug("Creating main window");
            // TODO
            LOG.Debug("Main window created");
        }

    }

}
