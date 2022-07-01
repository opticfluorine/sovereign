/*
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

using Castle.Core;
using Castle.Core.Logging;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using System;
using Sovereign.ClientCore.Events;
using Sovereign.ClientCore.Rendering.Gui;

namespace Sovereign.ClientCore.Rendering
{

    /// <summary>
    /// Manages rendering services on the main thread.
    /// </summary>
    public class RenderingManager : IStartable
    {
        public ILogger Logger { private get; set; } = NullLogger.Instance;

        private readonly MainDisplay mainDisplay;
        private readonly AdapterSelector adapterSelector;
        private readonly DisplayModeSelector displayModeSelector;
        private readonly IRenderer renderer;
        private readonly RenderingResourceManager resourceManager;
        private readonly IClientConfiguration clientConfiguration;
        private readonly SDLEventAdapter sdlEventAdapter;
        private readonly CommonGuiManager guiManager;

        /// <summary>
        /// Error handler.
        /// </summary>
        public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

        /// <summary>
        /// Selected video adapter.
        /// </summary>
        private IVideoAdapter selectedAdapter;

        /// <summary>
        /// Selected display mode.
        /// </summary>
        private IDisplayMode selectedDisplayMode;

        public RenderingManager(MainDisplay mainDisplay, AdapterSelector adapterSelector,
            DisplayModeSelector displayModeSelector, IRenderer renderer,
            RenderingResourceManager resourceManager, IClientConfiguration clientConfiguration,
            SDLEventAdapter sdlEventAdapter, CommonGuiManager guiManager)
        {
            this.mainDisplay = mainDisplay;
            this.adapterSelector = adapterSelector;
            this.displayModeSelector = displayModeSelector;
            this.renderer = renderer;
            this.resourceManager = resourceManager;
            this.clientConfiguration = clientConfiguration;
            this.sdlEventAdapter = sdlEventAdapter;
            this.guiManager = guiManager;
        }

        public void Start()
        {
            /* Create the main display. */
            CreateMainDisplay();

            /* Initialize the GUI. */
            InitializeGUI();

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

            /* Clean up GUI resources. */
            guiManager.Dispose();

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
                mainDisplay.Show(selectedDisplayMode, clientConfiguration.Fullscreen);
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
        /// Initializes the GUI.
        /// </summary>
        private void InitializeGUI()
        {
            try
            {
                guiManager.Initialize();
            }
            catch (Exception e)
            {
                /* Fatal error - can't initialize the GUI. */
                var msg = "Failed to initialize the GUI.";
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
