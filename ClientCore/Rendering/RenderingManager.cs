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

using Castle.Core;
using Castle.Core.Logging;
using Engine8.ClientCore.Logging;
using Engine8.ClientCore.Rendering.Configuration;
using Engine8.ClientCore.Rendering.Display;
using Engine8.EngineCore.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering
{

    /// <summary>
    /// Manages rendering services on the main thread.
    /// </summary>
    public class RenderingManager : IStartable
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Main display.
        /// </summary>
        private readonly MainDisplay mainDisplay;

        /// <summary>
        /// Video adapter selector.
        /// </summary>
        private readonly AdapterSelector adapterSelector;

        /// <summary>
        /// Display mode selector.
        /// </summary>
        private readonly DisplayModeSelector displayModeSelector;

        /// <summary>
        /// Renderer.
        /// </summary>
        private readonly IRenderer renderer;

        /// <summary>
        /// Selected video adapter.
        /// </summary>
        private IVideoAdapter selectedAdapter;

        /// <summary>
        /// Selected display mode.
        /// </summary>
        private IDisplayMode selectedDisplayMode;

        public RenderingManager(MainDisplay mainDisplay, AdapterSelector adapterSelector,
            DisplayModeSelector displayModeSelector, IRenderer renderer)
        {
            this.mainDisplay = mainDisplay;
            this.adapterSelector = adapterSelector;
            this.displayModeSelector = displayModeSelector;
            this.renderer = renderer;
        }

        public void Start()
        {
            /* Any exceptions in this section are fatal. */
            try
            {
                /* Configure the renderer. */
                SelectConfiguration();

                /* Create the main window. */
                mainDisplay.Show(selectedDisplayMode);

                /* Initialize the renderer. */
                renderer.Initialize(mainDisplay, selectedAdapter);
            }
            catch (Exception e)
            {
                /* Fatal error - rendering could not be started. */
                Logger.Fatal("Failed to start rendering.", e);
                ErrorHandler.Error(e.Message);
                throw new FatalErrorException("Failed to start rendering.", e);
            }
        }

        public void Stop()
        {
            /* Stop the renderer. */
            try
            {
                renderer.Cleanup();
            }
            catch (Exception e)
            {
                Logger.Error("Error while cleaning up the renderer.", e);
            }

            /* Close the main window. */
            mainDisplay.Close();
        }

        /// <summary>
        /// Selects the renderer configuration.
        /// </summary>
        private void SelectConfiguration()
        {
            selectedAdapter = adapterSelector.SelectAdapter();
            selectedDisplayMode = displayModeSelector.SelectDisplayMode(selectedAdapter);
        }

    }

}
