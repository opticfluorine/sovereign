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

using Castle.Core;
using Castle.Core.Logging;
using Sovereign.ClientCore.Logging;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.EngineCore.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering
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
        /// Rendering resource manager.
        /// </summary>
        private readonly RenderingResourceManager resourceManager;

        /// <summary>
        /// Selected video adapter.
        /// </summary>
        private IVideoAdapter selectedAdapter;
        
        /// <summary>
        /// Selected display mode.
        /// </summary>
        private IDisplayMode selectedDisplayMode;

        public RenderingManager(MainDisplay mainDisplay, AdapterSelector adapterSelector,
            DisplayModeSelector displayModeSelector, IRenderer renderer, RenderingResourceManager resourceManager)
        {
            this.mainDisplay = mainDisplay;
            this.adapterSelector = adapterSelector;
            this.displayModeSelector = displayModeSelector;
            this.renderer = renderer;
            this.resourceManager = resourceManager;
        }

        public void Start()
        {
            /* Create the main display. */
            CreateMainDisplay();

            /* Load resources used by the renderer. */
            LoadResources();

            /* Initialize the renderer. */
            InitializeRenderer();
        }

        public void Stop()
        {
            /* Release resources used by the renderer. */
            CleanupResources();

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
        /// Renders the next frame.
        /// </summary>
        public void Render()
        {
            renderer.Render();
        }

        /// <summary>
        /// Creates the main display.
        /// </summary>
        private void CreateMainDisplay()
        {
            try
            {
                /* Configure the display. */
                SelectConfiguration();

                /* Create the main window. */
                mainDisplay.Show(selectedDisplayMode, false);
            }
            catch (Exception e)
            {
                /* Fatal error - can't create the main display. */
                var msg = "Failed to create the main display.";
                Logger.Fatal(msg, e);
                ErrorHandler.Error(e.Message);
                throw new FatalErrorException(msg, e);
            }
        }

        /// <summary>
        /// Initializes the renderer.
        /// </summary>
        private void InitializeRenderer()
        {
            try
            {
                renderer.Initialize(selectedAdapter);
            }
            catch (Exception e)
            {
                /* Fatal error - can't initialize the renderer. */
                var msg = "Failed to initialize the renderer.";
                Logger.Fatal(msg, e);
                ErrorHandler.Error(e.Message);
                throw new FatalErrorException(msg, e);
            }
        }

        /// <summary>
        /// Selects the renderer configuration.
        /// </summary>
        private void SelectConfiguration()
        {
            selectedAdapter = adapterSelector.SelectAdapter();
            selectedDisplayMode = displayModeSelector.SelectDisplayMode(selectedAdapter);
        }

        /// <summary>
        /// Loads resources used by rendering.
        /// </summary>
        private void LoadResources()
        {
            resourceManager.InitializeResources();
        }

        /// <summary>
        /// Cleans up resources used by rendering.
        /// </summary>
        private void CleanupResources()
        {
            resourceManager.CleanupResources();
        }

    }

}
