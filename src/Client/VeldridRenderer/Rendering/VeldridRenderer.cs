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
using Castle.Core.Logging;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Scenes;
using Sovereign.VeldridRenderer.Rendering.Resources;
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

    /// <summary>
    ///     Dispose flag.
    /// </summary>
    private bool isDisposed;

    public VeldridRenderer(VeldridDevice device, VeldridResourceManager resourceManager,
        SceneManager sceneManager, VeldridSceneConsumer sceneConsumer)
    {
        this.device = device;
        this.resourceManager = resourceManager;
        this.sceneManager = sceneManager;
        this.sceneConsumer = sceneConsumer;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

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
            sceneConsumer.Dispose();
            resourceManager.Dispose();
            device.Dispose();
            isDisposed = true;
        }
    }

    public void Render()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");
        if (resourceManager.CommandList == null)
            throw new InvalidOperationException("Command list not ready.");

        try
        {
            /* Prepare for rendering. */
            var commandList = resourceManager.CommandList;
            commandList.Begin();
            commandList.SetFramebuffer(device.Device.SwapchainFramebuffer);
            commandList.ClearColorTarget(0, RgbaFloat.Black);

            /* Do scene processing to fill buffers for GPU. */
            var scene = sceneManager.ActiveScene;
            scene.BeginScene();
            sceneConsumer.ConsumeScene(scene);
            scene.EndScene();

            /* Render and present the next frame. */
            commandList.End();
            device.Device.SubmitCommands(commandList);
            device.Device.WaitForIdle();
            device.Device.SwapBuffers();
        }
        catch (Exception e)
        {
            Logger.Error("Error during rendering.", e);
        }
    }
}