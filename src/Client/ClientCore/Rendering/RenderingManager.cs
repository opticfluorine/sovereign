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
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Events;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;

namespace Sovereign.ClientCore.Rendering;

/// <summary>
///     Manages rendering services on the main thread.
/// </summary>
public class RenderingManager : IDisposable
{
    private readonly AdapterSelector adapterSelector;
    private readonly ClientConfigurationManager configManager;
    private readonly DisplayModeSelector displayModeSelector;
    private readonly CommonGuiManager guiManager;
    private readonly ILogger<RenderingManager> logger;

    private readonly MainDisplay mainDisplay;
    private readonly IRenderer renderer;
    private readonly RenderingResourceManager resourceManager;
    private readonly SDLEventAdapter sdlEventAdapter;
    private readonly ClientStateServices stateServices;

    private bool initialized;

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
        CommonGuiManager guiManager, ClientStateServices stateServices,
        ILogger<RenderingManager> logger, SDLEventAdapter sdlEventAdapter)
    {
        this.mainDisplay = mainDisplay;
        this.adapterSelector = adapterSelector;
        this.displayModeSelector = displayModeSelector;
        this.renderer = renderer;
        this.resourceManager = resourceManager;
        this.configManager = configManager;
        this.guiManager = guiManager;
        this.stateServices = stateServices;
        this.logger = logger;
        this.sdlEventAdapter = sdlEventAdapter;
    }

    /// <summary>
    ///     Error handler.
    /// </summary>
    public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

    public void Dispose()
    {
        CleanupResources();
        renderer.Cleanup();
        guiManager.Dispose();
        mainDisplay.Close();
    }

    /// <summary>
    ///     Initializes the renderer.
    /// </summary>
    private void Initialize()
    {
        try
        {
            SelectConfiguration();
            CreateMainDisplay();
            InitializeGui();
            LoadResources();
            InitializeRenderer();

            initialized = true;
        }
        catch (FatalErrorException)
        {
            Environment.Exit(1);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Unhandled exception in RenderingManager.");
            Environment.Exit(1);
        }
    }

    /// <summary>
    ///     Renders the next frame.
    /// </summary>
    public void Render()
    {
        if (!initialized) Initialize();

        if (stateServices.CheckAndClearFlagValue(ClientStateFlag.ReloadClientResources))
        {
            LoadResources();
            renderer.ReloadResources();
        }

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
            logger.LogCritical(msg);
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
            logger.LogCritical(msg, e);
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
            logger.LogCritical(msg);
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
            logger.LogCritical(e, msg);
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
            logger.LogCritical(msg, e);
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
    ///     Loads (or reloads) resources used by rendering.
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