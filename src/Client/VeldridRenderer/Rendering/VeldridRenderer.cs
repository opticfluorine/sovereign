/*
 * Sovereign Engine
 * Copyright (c) 2021 opticfluorine
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
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Scenes;
using Sovereign.VeldridRenderer.Rendering.Gui;
using Sovereign.VeldridRenderer.Rendering.Resources;
using Sovereign.VeldridRenderer.Rendering.Scenes.Game;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering;

/// <summary>
///     Renderer implementation using the Veldrid graphics library.
/// </summary>
public class VeldridRenderer : IRenderer
{
    /// <summary>
    ///     Veldrid graphics device.
    /// </summary>
    private readonly VeldridDevice device;

    private readonly GuiRenderer guiRenderer;
    private readonly ILogger<VeldridRenderer> logger;

    /// <summary>
    ///     Veldrid resource manager.
    /// </summary>
    private readonly VeldridResourceManager resourceManager;

    /// <summary>
    ///     Scene consumer.
    /// </summary>
    private readonly VeldridSceneConsumer sceneConsumer;

    /// <summary>
    ///     Scene manager.
    /// </summary>
    private readonly SceneManager sceneManager;

    private readonly WorldRenderer worldRenderer;

    /// <summary>
    ///     Dispose flag.
    /// </summary>
    private bool isDisposed;

    public VeldridRenderer(VeldridDevice device, VeldridResourceManager resourceManager,
        SceneManager sceneManager, VeldridSceneConsumer sceneConsumer, WorldRenderer worldRenderer,
        GuiRenderer guiRenderer, ILogger<VeldridRenderer> logger)
    {
        this.device = device;
        this.resourceManager = resourceManager;
        this.sceneManager = sceneManager;
        this.sceneConsumer = sceneConsumer;
        this.worldRenderer = worldRenderer;
        this.guiRenderer = guiRenderer;
        this.logger = logger;
    }

    public void Initialize(IVideoAdapter videoAdapter)
    {
        try
        {
            /* Attempt to create the rendering device. */
            device.CreateDevice();

            /* Install main resources. */
            resourceManager.InitializeBaseResources();

            /* Initialize all scenes. */
            sceneConsumer.Initialize();

            // Initialize additional rendering layers.
            guiRenderer.Initialize();
        }
        catch (Exception e)
        {
            throw new RendererInitializationException("Failed to initialize renderer.", e);
        }
    }

    public void Cleanup()
    {
        if (!isDisposed)
        {
            guiRenderer.Dispose();
            sceneConsumer.Dispose();
            resourceManager.Dispose();
            device.Dispose();
            isDisposed = true;
        }
    }

    public void ReloadResources()
    {
        resourceManager.ReloadTextures();
        worldRenderer.ReloadResources();
        guiRenderer.ReloadResources();
    }

    public void Render()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");
        if (resourceManager.CommandList == null)
            throw new InvalidOperationException("Command list not ready.");

        var commandList = resourceManager.CommandList;
        try
        {
            /* Prepare for rendering. */
            commandList.Begin();
            commandList.SetFramebuffer(device.Device.SwapchainFramebuffer);
            commandList.ClearColorTarget(0, RgbaFloat.Black);
            commandList.ClearDepthStencil(1.0f);

            /* Do scene processing to fill buffers for GPU. */
            var scene = sceneManager.ActiveScene;
            scene.BeginScene();
            sceneConsumer.ConsumeScene(scene);
            scene.EndScene();

            // GUI rendering from Dear ImGui.
            guiRenderer.RenderGui(commandList);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error during rendering.");
        }

        /* Render and present the next frame. */
        commandList.End();
        device.Device.SubmitCommands(commandList);
        device.Device.WaitForIdle();
        device.Device.SwapBuffers();
        guiRenderer.EndFrame();
    }
}