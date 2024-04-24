/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using Castle.Core;
using Castle.Core.Logging;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Events;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;

namespace Sovereign.ClientCore.Rendering;

/// <summary>
///     Manages rendering services on the main thread.
/// </summary>
public class RenderingManager : IStartable
{
    private readonly AdapterSelector adapterSelector;
    private readonly ClientConfigurationManager configManager;
    private readonly DisplayModeSelector displayModeSelector;
    private readonly CommonGuiManager guiManager;

    private readonly MainDisplay mainDisplay;
    private readonly IRenderer renderer;
    private readonly RenderingResourceManager resourceManager;
    private readonly SDLEventAdapter sdlEventAdapter;

    /// <summary>
    ///     Selected video adapter.
    /// </summary>
    private IVideoAdapter? selectedAdapter;

    /// <summary>
    ///     Selected display mode.
    /// </summary>
    private IDisplayMode? selectedDisplayMode;

    public RenderingManager(MainDisplay mainDisplay, AdapterSelector adapterSelector,
        DisplayModeSelector displayModeSelector, IRenderer renderer,
        RenderingResourceManager resourceManager, ClientConfigurationManager configManager,
        SDLEventAdapter sdlEventAdapter, CommonGuiManager guiManager)
    {
        this.mainDisplay = mainDisplay;
        this.adapterSelector = adapterSelector;
        this.displayModeSelector = displayModeSelector;
        this.renderer = renderer;
        this.resourceManager = resourceManager;
        this.configManager = configManager;
        this.sdlEventAdapter = sdlEventAdapter;
        this.guiManager = guiManager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Error handler.
    /// </summary>
    public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

    public void Start()
    {
        try
        {
            SelectConfiguration();
            CreateMainDisplay();
            InitializeGui();
            LoadResources();
            InitializeRenderer();
        }
        catch (FatalErrorException)
        {
            Environment.Exit(1);
        }
        catch (Exception e)
        {
            Logger.Fatal("Unhandled exception in RenderingManager.Start().", e);
            Environment.Exit(1);
        }
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
    ///     Renders the next frame.
    /// </summary>
    public void Render()
    {
        guiManager.NewFrame();
        renderer.Render();
    }

    /// <summary>
    ///     Creates the main display.
    /// </summary>
    private void CreateMainDisplay()
    {
        if (selectedDisplayMode == null)
        {
            var msg = "Attempted to create main display without a display mode.";
            Logger.Fatal(msg);
            ErrorHandler.Error(msg);
            throw new FatalErrorException(msg);
        }

        try
        {
            /* Create the main window. */
            mainDisplay.Show(selectedDisplayMode, configManager.ClientConfiguration.Display.Fullscreen);
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
    ///     Initializes the renderer.
    /// </summary>
    private void InitializeRenderer()
    {
        if (selectedAdapter == null)
        {
            var msg = "Attempted to initialize renderer without a display adapter.";
            Logger.Fatal(msg);
            ErrorHandler.Error(msg);
            throw new FatalErrorException(msg);
        }

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
    ///     Initializes the GUI.
    /// </summary>
    private void InitializeGui()
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
    ///     Selects the renderer configuration.
    /// </summary>
    private void SelectConfiguration()
    {
        selectedAdapter = adapterSelector.SelectAdapter();
        selectedDisplayMode = displayModeSelector.SelectDisplayMode(selectedAdapter);
    }

    /// <summary>
    ///     Loads resources used by rendering.
    /// </summary>
    private void LoadResources()
    {
        resourceManager.InitializeResources();
    }

    /// <summary>
    ///     Cleans up resources used by rendering.
    /// </summary>
    private void CleanupResources()
    {
        resourceManager.CleanupResources();
    }
}